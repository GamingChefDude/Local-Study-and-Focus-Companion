using System.Windows.Controls;

namespace Local_Study_and_Focus_Companion.View
{
    public partial class StatsView : UserControl
    {
        public StatsView()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainViewModel();
        }
    }
}
