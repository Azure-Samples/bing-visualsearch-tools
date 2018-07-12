using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VSPing.Models;
using VSPing.Utils;

namespace VSPing.ViewModels
{
    public class AppViewModel : BindableBase
    {
        /// <summary>
        /// Creates query windows and images stores for each instance of VSPing, and opening or closing more query windows
        /// </summary>
        public ObservableCollection<BindableBase> QueryWindows { get; set; } // Holds all of the query tabs. Start out by generating three 
        public List<QueryImageStoreViewModel> QueryStores { get; set; } // Holds all of the sources for query images (Local file store and Azure file store)
        protected int queryWindowCount = 0; // Number of query tabs that have been created so far, so that we can accordingly title the tab

        public AppViewModel()
        {
            this.CreateQueryStoresOnStartup();
            this.CreateQueryWindowsOnStartup();
        }

        // Creates three query tabs when the app is first launched
        protected virtual void CreateQueryWindowsOnStartup()
        {
            this.QueryWindows = new ObservableCollection<BindableBase>();
            this.QueryWindows.Add(new MainWindowViewModel(this, $"Query {queryWindowCount++}"));
            this.QueryWindows.Add(new MainWindowViewModel(this, $"Query {queryWindowCount++}"));
            this.QueryWindows.Add(new MainWindowViewModel(this, $"Query {queryWindowCount++}"));
        }

        // Adds the local and azure image stores when the app is first launched
        protected virtual void CreateQueryStoresOnStartup()
        {
            var appmodel = AppModel.GetModel(); // Allows access to the list of IImageStore that we have initialized with local and azure image stores
            this.QueryStores = new List<QueryImageStoreViewModel>();
            foreach (IImageStore store in appmodel.ImageStores)
            {
                QueryStores.Add(new QueryImageStoreViewModel(store, this)); // Create a ImageStoreViewModel from the IImageStore model we get from the static AppModel    
            }

        }

        public virtual void AddNewQueryWindow(string url = null)
        {
            string windowTitle = $"Query {queryWindowCount++}";

            var newWindow = new MainWindowViewModel(this, windowTitle); // Creates a new window with the default query url

            this.QueryWindows.Add(newWindow);
            UpdateActiveQueryWindow(newWindow);
        }

        public void RemoveQueryWindow(object o)
        {
            var mwvm = o as MainWindowViewModel;
            if (mwvm == null) return; // Ensures that the object passed to the function is a query window

            this.QueryWindows.Remove(mwvm);

            if (this.activeQueryWindow == mwvm) UpdateActiveQueryWindow(this.QueryWindows.FirstOrDefault()); // If we are deleting the current active window, adjust the active window variable to the next open one
        }

        protected MainWindowViewModel activeQueryWindow; // The query tab that has last been opened
        protected QueryImageStoreViewModel activeStoreWindow; // The image store tab that has last been opened 
        
        // Called when an object of type MainWindowViewModel or ImageStoreViewModel is opened
        public void UpdateActiveQueryWindow(object activeContent)
        {
            if (activeContent == null)
            {
                this.activeQueryWindow = null;
                return;
            }

            if (activeContent as MainWindowViewModel != null) // Adjust the active query window if we have clicked on a query window
            {
                this.activeQueryWindow = activeContent as MainWindowViewModel; 
            }
            else if (activeContent as QueryImageStoreViewModel != null) // Adjust the active store window if we have clicked on a store window
            {
                this.activeStoreWindow = activeContent as QueryImageStoreViewModel;
            }

            return;
        }

        public virtual async Task DownloadAndSearchQueryImage(string url)
        {
            await this.activeQueryWindow.DownloadAndSearchQueryImage(url);

        }

        // Triggered by double click on an image within the image store
        public async Task DownloadQueryImageAndResponseFromQueryImageStore(object o)
        {
            if (this.activeQueryWindow == null)
            {
                this.AddNewQueryWindow(o.ToString());
            }

            await this.activeQueryWindow.DownloadQueryImageAndResponseFromQueryImageStore(o);
        }

    }
}
