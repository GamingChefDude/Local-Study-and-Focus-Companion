using System;
using System.Windows;
using System.Windows.Threading;

namespace Local_Study_and_Focus_Companion
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        int seconds = 0;

        private void Timer_Tick(object sender, EventArgs e)
        {
            seconds++;
            counter.Content = seconds.ToString();
        }


        public void StartTimer(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        public void StopTimer(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        // leggg til en reset button for timeren 
    }
}
