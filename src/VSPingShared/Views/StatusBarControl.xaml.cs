using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VSPing.ViewModels;

namespace VSPing.SharedViews
{
    /// <summary>
    /// Interaction logic for SearchSection.xaml
    /// </summary>
    public partial class StatusBarControl : UserControl
    {
        public StatusBarControl()
        {
            InitializeComponent();
        }
        //right now the botton bar url isnt implemented
        private void HyperlinkContextMenu_Copy(object sender, RoutedEventArgs e) //This method allows the user to copy the bottom bar URL
        {
            FrameworkElement element = sender as FrameworkElement;

            if (element == null)
                return;

            object dc = element.DataContext;
            Clipboard.SetDataObject(dc?.ToString() ?? String.Empty);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) //This method allows the user to click on and directly navigate to the bottom bar URL
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
