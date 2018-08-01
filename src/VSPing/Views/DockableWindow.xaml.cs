using System;
using System.Collections.Generic;
using System.Windows;
using VSPing.ViewModels;

namespace VSPing.Views
{
    /// <summary>
    /// Interaction logic for DockableWindow.xaml
    /// </summary>
    public partial class DockableWindow : Window
    {
        public DockableWindow() // Constructor for the class
        {
            InitializeComponent();
            this.DataContext = this.FindResource("myAppViewModel"); ;
        }

        private void Window_Activated(object sender, EventArgs e) // This method activates a particular query window
        {
            var vm = this.DataContext as AppViewModel;
        }

        private void DockingManager_ActiveContentChanged(object sender, EventArgs e) // This method updates the query window when content is changed
        {
            object o = sender;
            var vm = this.DataContext as AppViewModel;
            vm.UpdateActiveQueryWindow(this.dockingManager.ActiveContent);
        }

        private void NewQueryWindowMenuItem_Click(object sender, RoutedEventArgs e) // This method opens a new query window
        {
            var vm = this.DataContext as AppViewModel;
            vm.AddNewQueryWindow();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e) // This method closes the application
        {
            Application.Current.Shutdown();
        }

        private void DockingManager_DocumentClosed(object sender, Xceed.Wpf.AvalonDock.DocumentClosedEventArgs e) // This method closes a particular query window
        {
            var vm = this.DataContext as AppViewModel;
            vm.RemoveQueryWindow(e.Document.Content);
        }

        // an event handler that searches an image from a QueryStore that is double clicked
        private async void QueryImageStoreControl_QueryDoubleClick(object sender, VSPing.Utils.GenericEventArgs<object, Dictionary<string, object>> e)
        {
            object data = e.Data;
            var vm = this.DataContext as AppViewModel;
            await vm.DownloadQueryImageAndResponseFromQueryImageStore(data);
        }

        // This is an event handler that generates and returns QueryStore refresh parameters as necessary
        private void QueryImageStoreControl_LoadQueryListClick(object sender, Utils.GenericEventArgs<object, Dictionary<string, object>> e)
        {
            var isvm = e.Data as QueryImageStoreViewModel;

            if (isvm == null)
                return;

            Dictionary<string, object> storeRefreshPropertyBag = null;

            if (isvm.Name == "Local") // Right now, only the local QueryStore needs a file popup, this can be expanded later
            {
                using (var ofd = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog())
                {
                    ofd.IsFolderPicker = true;
                    ofd.Title = "Choose folder containing query images";

                    Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult result = ofd.ShowDialog();

                    if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
                    {
                        storeRefreshPropertyBag = new Dictionary<string, object>();
                        storeRefreshPropertyBag.Add("filePath", ofd.FileName);
                    }
                }
            } 
            else if(isvm.Name == "Azure")
            {
                storeRefreshPropertyBag = new Dictionary<string, object>(); //no parameters for Azure store
            }
            e.ReturnValue = storeRefreshPropertyBag;
        }
    }
}
