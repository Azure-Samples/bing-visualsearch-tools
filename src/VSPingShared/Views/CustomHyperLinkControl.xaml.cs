using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace VSPing.SharedViews
{
    /// <summary>
    /// Interaction logic for CustomHyperLinkControl.xaml
    /// </summary>
    public partial class CustomHyperLinkControl : UserControl
    {
        public CustomHyperLinkControl()
        {
            InitializeComponent();
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)//this method is called when you click an hyperlink and helps to process the request
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        private void HyperlinkContextMenu_Copy(object sender, RoutedEventArgs e)//this method is the one in charged of handle the right click - copy
        {
            FrameworkElement element = sender as FrameworkElement;

            if (element == null)
                return;

            object dc = element.DataContext;
            Clipboard.SetDataObject(dc?.ToString() ?? String.Empty);
        }
    }
}
