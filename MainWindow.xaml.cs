using System;
using System.Windows;
using System.Windows.Threading;

namespace Local_Study_and_Focus_Companion
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        int timeValue = 1; // time in seconds
        public MainWindow()
        {
            InitializeComponent(); // initialize components from xaml

            timer.Interval = TimeSpan.FromSeconds(timeValue); // set timer interval
            timer.Tick += Timer_Tick; // add tick event
        }

        int seconds = 0;

        private void Timer_Tick(object sender, EventArgs e)
        {
            seconds += timeValue; // increment seconds
            counter.Content = seconds.ToString(); // display seconds
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
            seconds = 0;
            counter.Content = seconds.ToString();
        }
    }
}
