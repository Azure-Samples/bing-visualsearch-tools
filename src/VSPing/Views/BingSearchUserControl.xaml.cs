using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;
using VSPing.ViewModels;

namespace VSPing.Views
{
    /// <summary>
    /// Interaction logic for BingSearchUserControl.xaml
    /// </summary>
    public partial class BingSearchUserControl : UserControl
    {
        string id;
        public BingSearchUserControl() // Constructor for the class
        {
            InitializeComponent();
            this.id = Guid.NewGuid().ToString();
            Debug.WriteLine($"Constructing BingSearchUserControl id:{id}");
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
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) // This method allows the user to open a hyperlink they click on
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void HyperlinkContextMenu_Copy(object sender, RoutedEventArgs e) // This method allows a user to copy a hyperlink
        {
            FrameworkElement element = sender as FrameworkElement;

            if (element == null)
                return;

            object dc = element.DataContext;

            Clipboard.SetDataObject(dc?.ToString() ?? String.Empty);
        }

        // can only drop data of type ImageInfoViewModel
        private async void searchResultsGrid_Drop(object sender, DragEventArgs e) // This method searches an image that's dragged into the grid
        {
            ImageInfoViewModel iivm = null;
            iivm = e.Data.GetData(typeof(ImageInfoViewModel)) as ImageInfoViewModel;

            if (iivm == null) // If the image has no reachable data, terminate the method
                return;

            await vm.DownloadQueryImageAndResponseFromQueryImageStore(iivm);
        }
    }
}
