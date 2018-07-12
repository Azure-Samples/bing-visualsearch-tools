using System.Windows.Controls;
using VSPing.ViewModels;

namespace VSPing.Views
{
    /// <summary>
    /// Interaction logic for SearchSection.xaml
    /// </summary>
    public partial class TabsSection : UserControl
    {
        public TabsSection() // Constructor for the class
        {
            InitializeComponent();
        }

        private MainWindowViewModel vm;
        private MainWindowViewModel VM 
        {
            get
            {
                if (this.vm == null) // Only creates a new view model if the current one isn't null
                {
                    this.vm = DataContext as MainWindowViewModel;
                }
                return vm;
            }
        }
    }
}
