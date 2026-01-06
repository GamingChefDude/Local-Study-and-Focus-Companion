using System;
using System.Windows;
using System.Windows.Threading;

namespace Local_Study_and_Focus_Companion
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        int timeValue = 1; // time in seconds

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
            // change seconds into minutes
            if (seconds == 60)
            {
                seconds = 0;
                minutes += 1;
                minCounter.Content = minutes.ToString();
            }

            // change minutes into hours
            if (minutes == 60)
            {
                minutes = 0;
                hours += 1;
                hourCounter.Content = minutes.ToString();
            }

            seconds += timeValue; // increment seconds
            sekCounter.Content = seconds.ToString(); // display seconds
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
    }
}
