using System.Windows.Controls;
using System;
using System.Windows;
using Local_Study_and_Focus_Companion.ViewModels;

namespace Local_Study_and_Focus_Companion.View
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
