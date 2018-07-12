using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IUPing.Views
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void HyperlinkContextMenu_Copy(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if (element == null)
                return;

            object dc = element.DataContext;

            //Clipboard.SetData(DataFormats.UnicodeText, dc?.ToString() ?? String.Empty);
            Clipboard.SetDataObject(dc?.ToString() ?? String.Empty);
        }
    }
}
