using Local_Study_and_Focus_Companion.ViewModels;
using System;
using System.Windows;
using Local_Study_and_Focus_Companion.View;

namespace Local_Study_and_Focus_Companion
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new MainView();
        }
    }
}
