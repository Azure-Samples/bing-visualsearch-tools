using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VSPing.SharedViews
{
    /// <summary>
    /// Interaction logic for ImageResultsListControl.xaml
    /// </summary>
    public partial class ImageResultsListControl : UserControl
    {
        public ImageResultsListControl()
        {
            InitializeComponent();
        }
        private void SearchResultImage_MouseMove(object sender, MouseEventArgs e) //This method allows users to move and copy images in the list view to other sections of the program
        {
            Image img = sender as Image;
            e.Handled = false;
            if (img != null && e.LeftButton == MouseButtonState.Pressed)
            {
                object dataContext = img.DataContext;
                DragDrop.DoDragDrop(
                             img,
                             dataContext.ToString(),
                             DragDropEffects.Copy);

                e.Handled = false;
                System.Diagnostics.Debug.WriteLine("in SearchResultImage_MouseMove");
            }
        }
    }
}
