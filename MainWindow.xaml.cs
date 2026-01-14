using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Local_Study_and_Focus_Companion
{
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer timer = new DispatcherTimer();
        const int timeValue = 1; // time in seconds

        const int sekToMinToHour = 60;
        int seconds = 0;
        int minutes = 0;
        int hours = 0;

        public MainWindow()
        {
            InitializeComponent(); // initialize components from xaml

            timer.Interval = TimeSpan.FromSeconds(timeValue); // set timer interval
            timer.Tick += Timer_Tick; // add tick event
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (seconds != sekToMinToHour || minutes != sekToMinToHour)
            {
                seconds += timeValue; // increment seconds
            }

            // change seconds into minutes
            if (seconds == sekToMinToHour)
            {
                seconds = 0;
                minutes += 1;

                if (minutes < 10)
                {
                    minCounter.Content = "0" + minutes.ToString();
                }
                else
                {
                    minCounter.Content = minutes.ToString();
                }
            }

            // change minutes into hours
            if (minutes == sekToMinToHour)
            {
                minutes = 0;
                hours += 1;
                if (hours < 10)
                {
                    hourCounter.Content = "0" + hours.ToString();
                }
                else
                {
                    hourCounter.Content = hours.ToString();
                }

            }

            // added leading zero of numbers under 10
            if (seconds < 10)
            {
                sekCounter.Content = "0" + seconds.ToString(); // display seconds
            }
            else
            {
                sekCounter.Content = seconds.ToString(); // display seconds
            }
        }


        public void StartTimer(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        public void StopTimer(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        public void ResetTimer(object sender, RoutedEventArgs e)
        {
            timer.Stop();

            // reset values
            seconds = 0;
            minutes = 0;
            hours = 0;
            
            // reset labels
            sekCounter.Content = "00";
            minCounter.Content = "00";
            hourCounter.Content = "00"; 
        }

        public void SizeChanger_Changed(object sender, RoutedEventArgs e)
        {
            noteBox.FontSize = Convert.ToDouble(sizeChanger.Text);
        }

        public void SaveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = fileName.Text;
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, noteBox.Text);
            }
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.Title = "Select a text file";

            if (openFileDialog.ShowDialog() == true)
            {
                noteBox.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }
    }
}
