using LiveCharts;
using LiveCharts.Wpf;
using Local_Study_and_Focus_Companion.View;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Globalization;

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

        // Formatters exposed for LiveCharts bindings
        public Func<double, string> YFormatter { get; }
        public Func<ChartPoint, string> PointLabel { get; }

        // Toggle between showing last week only or all time
        private bool _showLastWeekOnly = true;
        public bool ShowLastWeekOnly
        {
            get => _showLastWeekOnly;
            set
            {
                if (_showLastWeekOnly == value) return;
                _showLastWeekOnly = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedRangeLabel));
            }
        }

        public string SelectedRangeLabel => ShowLastWeekOnly ? "Showing: Last Week (click to All Time)" : "Showing: All Time (click to Last Week)";

        public ICommand ToggleStatsRangeCommand { get; }
        public ICommand ToggleViewCommand { get; } // toggles between MainView and StatsView
        public ICommand SwitchViewCommand => ToggleViewCommand; // keep existing binding name usable

        public MainViewModel()
        {
            // Axis/point formatters: convert numeric hours -> hh:mm:ss
            YFormatter = value => TimeSpan.FromHours(value).ToString(@"hh\:mm\:ss");
            PointLabel = point => TimeSpan.FromHours(point.Y).ToString(@"hh\:mm\:ss");

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

            ToggleStatsRangeCommand = new RelayCommand(_ =>
            {
                ShowLastWeekOnly = !ShowLastWeekOnly;
                GetStats();
            });

            ToggleViewCommand = new RelayCommand(_ => ToggleMainStatsView());

            EnsureSaveFolder();
        }

        private void ToggleMainStatsView()
        {
            var main = Application.Current?.MainWindow as MainWindow;
            if (main == null) return;

            var current = main.CurrentView.Content;

            // If currently displaying StatsView, go back to MainView
            if (current is StatsView)
            {
                var mainView = new MainView
                {
                    DataContext = this
                };
                main.CurrentView.Content = mainView;
            }
            else
            {
                // Show stats view (refresh data before showing)
                GetStats();
                var stats = new StatsView
                {
                    DataContext = this
                };
                main.CurrentView.Content = stats;
            }
        }

        private void GetStats()
        {
            // Aggregate total hours per subject from session.csv
            // ShowLastWeekOnly controls whether we filter to the last 7 days or include all time
            try
            {
                var totals = new System.Collections.Generic.Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

                // cutoff: include today and previous 6 days (7 days total) if ShowLastWeekOnly == true
                DateTime cutoff = DateTime.Now.Date.AddDays(-6);

                if (File.Exists(_sessionFile))
                {
                    string[] lines = File.ReadAllLines(_sessionFile);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        string[] parts = line.Split(',');
                        if (parts.Length < 3)
                            continue; // ignore malformed line

                        string dateText = parts[0].Trim();
                        string duration = parts[1].Trim();
                        string subject = parts[2].Trim();
                        if (string.IsNullOrEmpty(subject))
                            subject = "Unknown";

                        // Parse date in the expected format used when saving ("dd-MM-yyyy")
                        if (!DateTime.TryParseExact(dateText, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        {
                            // If the date doesn't match the expected pattern, skip the line
                            continue;
                        }

                        // If last-week-only filter is enabled, skip older entries
                        if (ShowLastWeekOnly && parsedDate.Date < cutoff)
                            continue;

                        // Try parse duration as TimeSpan (expected format "hh:mm:ss" or "h:m:s")
                        if (TimeSpan.TryParse(duration, out TimeSpan span))
                        {
                            if (!totals.ContainsKey(subject))
                                totals[subject] = 0;
                            totals[subject] += span.TotalHours;
                        }
                        else
                        {
                            // Fallback: try splitting by ':' and parse numbers safely
                            var segs = duration.Split(':');
                            if (segs.Length >= 1 && int.TryParse(segs[0], out int h))
                            {
                                double totalHours = h;
                                if (segs.Length >= 2 && int.TryParse(segs[1], out int m))
                                    totalHours += m / 60.0;
                                if (segs.Length >= 3 && int.TryParse(segs[2], out int s))
                                    totalHours += s / 3600.0;

                                if (!totals.ContainsKey(subject))
                                    totals[subject] = 0;
                                totals[subject] += totalHours;
                            }
                            // else ignore this line if we couldn't parse duration
                        }
                    }
                }

                var values = new ChartValues<double>();
                var labels = totals.Keys.ToArray();

                foreach (var key in labels)
                    values.Add(totals[key]);

                SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Time",
                        Values = values,
                        DataLabels = true,
                        LabelPoint = PointLabel
                    }
                };

                Labels = labels;

                OnPropertyChanged(nameof(SeriesCollection));
                OnPropertyChanged(nameof(Labels));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
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
        private string _session_file;
        private string _sessionFile
        {
            get => _session_file;
            set => _session_file = value;
        }

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
