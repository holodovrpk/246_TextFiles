using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace _246_TextFiles
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Коллекция напоминаний. ObservableCollection автоматически
        // уведомляет UI об изменениях (добавление/удаление элементов).
        ObservableCollection<Reminder> reminders = new ObservableCollection<Reminder>();

        // Таймер, который периодически проверяет, не наступило ли время напоминания.
        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            // Настраиваем таймер: срабатывает каждые 50 секунд.
            // (Проверка раз в 50 сек — грубовато, но для учебного примера ок.)
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(50);
            timer.Tick += Timer_Tick;
        }

        // Обработчик тика таймера — проверяем напоминания.
        private void Timer_Tick(object? sender, EventArgs e)
        {
            int h = DateTime.Now.Hour;   // текущий час
            int m = DateTime.Now.Minute; // текущая минута

            // Проходим по всем напоминаниям
            foreach (Reminder r in reminders)
            {
                // Если время совпало и напоминание ещё не показывалось —
                // показываем MessageBox и помечаем как показанное.
                if (h == r.Hour && m == r.Minute && r.IsShown == false)
                {
                    MessageBox.Show($"{r.Hour}:{r.Minute} - {r.Text}",
                        "Напоминание", MessageBoxButton.OK, MessageBoxImage.Information);
                    r.IsShown = true;
                }
            }

            // Хак для обновления ListBox: сбрасываем и заново задаём источник.
            // (На самом деле при изменении свойства IsShown в Reminder
            //  это не обязательно, если Reminder реализует INotifyPropertyChanged.)
            lbNames.ItemsSource = null;
            lbNames.ItemsSource = reminders;
        }

        // Тестовая кнопка — добавляет одно напоминание вручную.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Добавляем напоминание на 12:10 с текстом-заглушкой.
            reminders.Add(new Reminder
            {
                Hour = 12,
                Minute = 10,
                Text = "jjjjjjjjjjjjjjjjjjjjjj",
                IsShown = false
            });
        }

        // Сохранение списка напоминаний в текстовый файл.
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Диалог выбора файла для сохранения, фильтр — только .txt
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Текстовые файлы (*.txt)|*.txt";

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.FileName;

                // Пишем каждое напоминание в отдельную строку.
                // Поля разделены строкой "%%" — простой самодельный формат.
                using (StreamWriter writer = new StreamWriter(path))
                {
                    foreach (Reminder r in reminders)
                    {
                        writer.WriteLine($"{r.Hour}%%{r.Minute}%%{r.Text}%%{r.IsShown}");
                    }
                }

                MessageBox.Show("Файл сохранён!");
            }
        }

        // Загрузка напоминаний из текстового файла.
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            // Диалог выбора файла для открытия.
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.FileName;

                // Читаем все строки файла.
                string[] t = File.ReadAllLines(path);

                // Каждую строку разбираем обратно в объект Reminder.
                foreach (string line in t)
                {
                    // Разбиваем строку по разделителю "%%":
                    // temp[0] — час, temp[1] — минута, temp[2] — текст, temp[3] — флаг IsShown
                    string[] temp = line.Split("%%");

                    reminders.Add(new Reminder
                    {
                        Hour = Convert.ToInt32(temp[0]),
                        Minute = Convert.ToInt32(temp[1]),
                        Text = temp[2],
                        IsShown = Convert.ToBoolean(temp[3])
                    });
                }

                // Привязываем загруженные данные к ListBox и запускаем таймер.
                lbNames.ItemsSource = reminders;
                timer.Start();

                // MessageBox.Show(reminders.Count.ToString());
                // persons = new ObservableCollection<string>(t);
            }
        }
    }
}