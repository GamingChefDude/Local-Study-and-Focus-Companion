using System.Windows;
using Local_Study_and_Focus_Companion.ViewModels;

namespace Local_Study_and_Focus_Companion
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
