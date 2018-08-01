using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using VSPing.ViewModels;

namespace VSPing.SharedViews
{
    /// <summary>
    /// Interaction logic for SearchSection.xaml
    /// </summary>
    public partial class ImageModSection : UserControl
    {
        public ImageModSection()
        {
            InitializeComponent();
        }

        private MainWindowViewModel vm;
        private MainWindowViewModel VM
        {
            get
            {
                if (this.vm == null)
                {
                    this.vm = DataContext as MainWindowViewModel;
                }
                return vm;
            }
        }
        private void img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //This method allows the user to start drawing bounding boxes on an image
        {
            Image img = sender as Image;

            VM.StartChangingBB(e.GetPosition((IInputElement)sender));
            e.Handled = false;
            Mouse.Capture((IInputElement)sender);
        }

        private void img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)  //This method allows the user to stop drawing bounding boxes on an image
        {
            VM.StopChangingBB();
            Mouse.Capture(null);
        }

        private void img_MouseMove(object sender, MouseEventArgs e) //This method allows the user to drag a bounding box to a desired size
        {
            Image img = sender as Image;

            VM.ChangeBB(
                new Size(img.ActualWidth, img.ActualHeight),
                e.GetPosition((IInputElement)sender));
            e.Handled = false;
        }

        private void img_SizeChanged(object sender, SizeChangedEventArgs e) //This method acts to update the view when an image is resized
        {
            VM.UpdateQueryImageRenderedSize(e.NewSize);
        }
        private void ScrollViewer_DragEnterOver(object sender, DragEventArgs e) //This method handles images that are dragged over the ImageModSection and allows them to be searched directly
        {     
            var internalType = false; //drag-n-drop where dragged item is an internal type
            if (e.Data.GetFormats().Length == 1) //for internal components participating in drag/drop the list contains exactly 1 item, which is the type name
            {
                var f = e.Data.GetFormats()[0]; //get the one and only formatname which must be a type name
                var o = e.Data.GetData(f); //fetch the object using that type name. 
                if (o is VSPing.ViewModels.ImageInfoViewModel) //if the fetched object is an ImageInfoViewModel, we recognize as internal type
                {
                    internalType = true;
                }

            }


            if (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.UnicodeText) || e.Data.GetDataPresent(DataFormats.FileDrop) || internalType)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }
        private async void ScrollViewer_Drop(object sender, DragEventArgs e) //This method gets data on any images dropped in the ImageModSection
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string str = (string)e.Data.GetData(DataFormats.Text);
                await VM.DownloadAndSearchQueryImage(str);
                e.Handled = true;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                await VM.DownloadAndSearchQueryImage(fileNames[0]);
                e.Handled = true;
            }
            else if (e.Data.GetFormats().Length == 1) //for internal components participating in drag/drop the list contains exactly 1 item, which is the type name
            {
                var f = e.Data.GetFormats()[0]; //get the one and only formatname which must be a type name
                var o = e.Data.GetData(f); //fetch the object using that type name. 
                if (o is VSPing.ViewModels.ImageInfoViewModel) //if the fetched object is an ImageInfoViewModel, we recognize as internal type
                {
                    VSPing.ViewModels.ImageInfoViewModel iivm = o as VSPing.ViewModels.ImageInfoViewModel;
                    await VM.DownloadQueryImageAndResponseFromQueryImageStore(iivm);
                    e.Handled = true;
                }

            }
        }

        private void rotationSlider_Thumb_DragCompleted(object sender, DragCompletedEventArgs e) //This method allows the user to start rotating the image
        {
            Slider slider = sender as Slider;
            VM.RotateQueryImage(slider.Value);
        }

        private void rotationSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) //This method allows the user to stop rotating the image
        {
            Slider slider = sender as Slider;
            VM.RotateQueryImage(slider.Value);
        }
    }
}
