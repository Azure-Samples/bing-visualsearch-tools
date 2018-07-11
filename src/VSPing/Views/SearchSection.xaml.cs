using System.Windows;
using System.Windows.Controls;
using VSPing.ViewModels;

namespace VSPing.Views
{
    /// <summary>
    /// Interaction logic for SearchSection.xaml
    /// </summary>
    public partial class SearchSection : UserControl
    {
        public SearchSection() // Constructor for the class
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
        private async void SearchBtn_Click(object sender, RoutedEventArgs e) // This method searches the currently selected image when the button is pressed
        {
            this.SearchBtn.IsEnabled = false;

            // sometimes bindings don't get activited when user presses the Enter but doesn't leave textbox
            VM.QueryImageUrl = this.ImageUrl.Text;
            VM.FilterSite = this.siteFilter.Text;
            VM.Market = this.market.Text;

            await VM.Search();
            this.SearchBtn.IsEnabled = true;
        }
    }
}
