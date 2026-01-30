using System.Windows.Controls;

namespace Local_Study_and_Focus_Companion.View
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainViewModel();   
        }
    }
}
