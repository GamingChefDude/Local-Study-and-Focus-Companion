using LiveCharts;
using LiveCharts.Wpf;
using Local_Study_and_Focus_Companion.View;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Local_Study_and_Focus_Companion.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DispatcherTimer _timer;
        private TimeSpan _time;
        private string _noteText = "Write here:";
        private string _subject = "";
        private string _fileName = "";
        private double _fontSize = 12;

        public SeriesCollection SeriesCollection { get; set; } // Main data that gets displayed
        public string[] Labels { get; set; } // Labels on X-Axis to show the subjects

        public MainViewModel()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => Tick();

            // Commands
            StartCommand = new RelayCommand(_ => _timer.Start());
            StopCommand = new RelayCommand(_ => _timer.Stop());
            ResetCommand = new RelayCommand(_ => ResetTimer());
            SaveSessionCommand = new RelayCommand(_ => SaveSession());
            SaveFileCommand = new RelayCommand(_ => SaveFile());
            LoadFileCommand = new RelayCommand(_ => LoadFile());
            FontSizeChangedCommand = new RelayCommand(_ => UpdateFontSize());

            // ShowStatsWindowCommand = new RelayCommand(_ => ShowStatsWindow());

            EnsureSaveFolder();
            GetStats();
        }

        private void GetStats()
        {
            // Example data — hours studied per subject
            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Hours",
                    Values = new ChartValues<double> { 12, 0, 9, 7, 15, 0, 0, 0 }
                }
            };

            Labels = new[] { "Math", "English", "Physics", "Chemistry", "Biology", "History", "Geography", "PE" };
        }

        private void Tick()
        {
            _time = _time.Add(TimeSpan.FromSeconds(1));
            OnPropertyChanged(nameof(Hours));
            OnPropertyChanged(nameof(Minutes));
            OnPropertyChanged(nameof(Seconds));
        }

        private void ResetTimer()
        {
            _timer.Stop();
            _time = TimeSpan.Zero;

            OnPropertyChanged(nameof(Hours));
            OnPropertyChanged(nameof(Minutes));
            OnPropertyChanged(nameof(Seconds));
        }

        public string Hours => _time.Hours.ToString("00");
        public string Minutes => _time.Minutes.ToString("00");
        public string Seconds => _time.Seconds.ToString("00");

        public string NoteText
        {
            get => _noteText;
            set { _noteText = value; OnPropertyChanged(); }
        }

        public string Subject
        {
            get => _subject;
            set { _subject = value; OnPropertyChanged(); }
        }

        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }

        public double NoteFontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(); }
        }

        private string _folderPath;
        private string _sessionFile;

        private void EnsureSaveFolder()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _folderPath = Path.Combine(desktop, "Local Study");
            _sessionFile = Path.Combine(_folderPath, "session.csv");

            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);

            if (!File.Exists(_sessionFile))
                File.Create(_sessionFile).Dispose();
        }

        private void SaveSession()
        {
            if (string.IsNullOrWhiteSpace(Subject))
            {
                System.Windows.MessageBox.Show("Please enter a subject");
                return;
            }

            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string duration = $"{Hours}:{Minutes}:{Seconds}";

            try
            {
                using (StreamWriter fileWriter = File.AppendText(_sessionFile))
                    fileWriter.WriteLine($"{date},{duration},{Subject}");
                System.Windows.MessageBox.Show($"Session saved to {_sessionFile}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void SaveFile()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                System.Windows.MessageBox.Show("Enter a file name first.");
                return;
            }

            string path = Path.Combine(_folderPath, FileName + ".txt");
            File.WriteAllText(path, NoteText);

            System.Windows.MessageBox.Show($"File saved to {path}");
        }

        private void LoadFile()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
                NoteText = File.ReadAllText(dialog.FileName);
        }

        private void UpdateFontSize()
        {
            if (double.TryParse(FontSizeInput, out double size))
                NoteFontSize = size;
        }

        private string _fontSizeInput = "12";
        public string FontSizeInput
        {
            get => _fontSizeInput;
            set
            {
                _fontSizeInput = value;
                OnPropertyChanged();
                UpdateFontSize();   // call automatically
            }
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SaveSessionCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand LoadFileCommand { get; }
        public ICommand FontSizeChangedCommand { get; }
       
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
