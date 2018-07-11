using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace VSPing.SharedViews
{
    /// <summary>
    /// Interaction logic for TagsActionsControl.xaml
    /// </summary>
    public partial class TagsActionsControl : UserControl
    {
        public TagsActionsControl()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) //This method handles the opening of links from the tags section
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void HyperlinkContextMenu_Copy(object sender, RoutedEventArgs e) //This method handles the copying of links from the tags section
        {
            FrameworkElement element = sender as FrameworkElement;

            if (element == null)
                return;

            object dc = element.DataContext;
            Clipboard.SetDataObject(dc?.ToString() ?? String.Empty);
        }
    }
}
