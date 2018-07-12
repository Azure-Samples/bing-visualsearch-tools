using System.Windows;
using System.Windows.Controls;
using VSPing.ViewModels;
using VSPing.Utils;
using Newtonsoft.Json.Linq;

namespace VSPing.SharedViews
{
    /// <summary>
    /// Interaction logic for SearchSection.xaml
    /// </summary>
    public partial class TagsOutputSection : UserControl
    {
        public TagsOutputSection() 
        {
            InitializeComponent();
        }

        private MainWindowViewModel vm;
        private MainWindowViewModel VM
        {
            get //A getter for the above MainWindowViewModel
            {
                if (this.vm == null)
                {
                    this.vm = DataContext as MainWindowViewModel;
                }

                return vm;
            }
        }
        private void TagsCopyButton_Click(object sender, RoutedEventArgs e) //If the button is pressed, this method copies the JSON data of the tags to clipboard
        {
            VM.CopyTabContentToClipBoard();
        }
    }
}
