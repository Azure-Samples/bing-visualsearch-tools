using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VSPing.Models;
using VSPing.Utils;

namespace VSPing.ViewModels
{
    public class QueryImageStoreViewModel : BindableBase
    {
        /// <summary>
        /// The common view model for all query image stores, allws images to be loaded and copy all images to clipboard
        /// </summary>
        protected AppViewModel ParentViewModel { get; set; } // AppViewModel to which this instance is attached
        public IImageStore ImageStore; // IImageStore model object held by this ViewModel instance
        public string Name { get; set; }    
        public ObservableCollection<ImageInfoViewModel> QueryImages { get; set; } // List of image viewmodels currently held in this image store
        protected object queryImagesListSelectedItem; // Currently selected image in this store. Binded to from the view
        public object QueryImagesListSelectedItem { get { return this.queryImagesListSelectedItem; } set { SetProperty(ref this.queryImagesListSelectedItem, value); } }

        public GridViewColumnCollection Columns { get; set; }   //programmatically set

        public virtual void UpdateGridViewColumnList(GridViewColumnCollection col)
        {

        }


        public QueryImageStoreViewModel(IImageStore imageStore, AppViewModel parent)
        {
            this.ImageStore = imageStore;
            this.Name = imageStore.Name;
            this.QueryImages = new ObservableCollection<ImageInfoViewModel>();
            this.ParentViewModel = parent;
        }

        // Called when Load button is clicked in the image store view. 
        // Calls the Refresh function of the IImageStore object with the designated parameters and adds each image viewmodel to the QueryImages list
        //public virtual async Task GetImagesFromStore(string filePath = null)
        public virtual async Task GetImagesFromStore(Dictionary<string, object> storeRefreshParams = null)
        {
            
            this.QueryImages.Clear();
            
            await this.ImageStore.Refresh(storeRefreshParams);

            int index = this.ImageStore.Images.Count;
            foreach (var i in this.ImageStore.Images)
            {
                this.QueryImages.Add(new ImageInfoViewModel(i, index, this.ParentViewModel));
                index--;
            }
            
            
        }

        public virtual async Task<ImageInfoViewModel> GetImage(string url)
        {
            var iivm = this.QueryImages.FirstOrDefault(i => i.Url == url);

            if (iivm != null)
                return iivm;

            //url is not in our ViewModel. Get it from the store
            var ii = await this.ImageStore.GetImage(url);

            if (ii == null)
                return null; //the store doesn't have it

            iivm = new ImageInfoViewModel(ii, this.QueryImages.Count + 1, this.ParentViewModel);

            this.QueryImages.Insert(0, iivm);

            return iivm;
        }

        // Called when the button to copy url's from an image store is clicked.
        // Appends the url of each image currently in this image store on a newline and binds that to the clipboard
        public void CopyAllQueryImageUrlsToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var iivm in this.QueryImages)
            {
                sb.AppendLine($"{iivm.Url}\t{iivm.ImageInfo.LastModified}");
            }

            var text = new DataObject();
            text.SetText(sb.ToString(), TextDataFormat.Text);

            Clipboard.SetDataObject(text);
        }
    }
}
