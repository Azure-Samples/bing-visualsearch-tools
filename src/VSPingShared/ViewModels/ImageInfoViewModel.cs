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
    public class ImageInfoViewModel : BindableBase
    {
        /// <summary>
        /// Handles tags, hash, index, and image information for a query image
        /// </summary>
        public ICommand ImageInfoContextMenuCommand { get; protected set; } // Relay command method that will be called when an image store query image is right clicked and an option is selected
        public IList<Tuple<string, ICommand>> MenuItemNameCommands { get; protected set; } // List of possible options and their associated commands when an image store query image is right clicked

        // Handler that is called after one of the right click options is selected
        // Currently only supports "Run Query" which calls the search on that image
        protected async void InfoImageContextMenuHandler(object o)
        {
            var cmdName = o as string;
            if (cmdName == null) return;
            if (cmdName.Equals("Run Query")) await this.ParentViewModel.DownloadAndSearchQueryImage(this.Url); // If we receive the Run Query commannd

        }

        private string imageHash;
        public string ImageHash { get { return this.imageHash; } set { SetProperty(ref this.imageHash, value); } }

        private string tagsList = null; // Concatenated string of cached tags associated with an instance
        public string TagsList
        {
            get
            {
                
                if (this.tagsList == null) { var asyncFetchTagsTask = FetchTagsAsync(); } // Asynchronously fetch the tags if the tag list is currently null. 
                return this.tagsList;
            }
            set { SetProperty(ref this.tagsList, value); } // Set the tag list to requested value and call on property changed to update the view
        }

        public async Task FetchTagsAsync()
        {
            await ImageInfo.GenerateSearchResponse();

            if (this.ImageInfo.SearchResponse?.Source == VSPing.Models.SearchResponseSource.None) // In the case that there is no cached response for this image
            {
                this.TagsList = "(no cached response)";
            }
            else // If we did find a cached response, update the taglist accordingly
            {

                if (string.IsNullOrEmpty(ImageInfo.TagsList)) this.TagsList = "(response had no tags)";
                else this.TagsList = ImageInfo.TagsList;

                this.ImageHash = (ImageInfo.SearchResponse as VSPing.Models.KapiResponse)?.ImageInsightsToken ?? String.Empty;
            }
        }

        // Accessible properties associated with each instance of the ImageInfoViewModel
        public int Index { get; set; }
        public ImageInfo ImageInfo { get; set; }
        public string Url => ImageInfo.Url;
        public string LastModified => ImageInfo.LastModified;

        private AppViewModel ParentViewModel { get; set; } // AppViewModel to which this instance of the ImageInfoViewModel is attached

        public ImageInfoViewModel(ImageInfo imageInfoModel, int index, AppViewModel parent)
        {
            this.ParentViewModel = parent;
            this.ImageInfo = imageInfoModel;
            this.Index = index;
            this.ImageInfoContextMenuCommand = new RelayCommand(InfoImageContextMenuHandler); // Assigning the handler we have written to the Relay Command variable
            this.TagsList = imageInfoModel.TagsList;

            this.MenuItemNameCommands = new Tuple<string, ICommand>[]
            {
                new Tuple<string, ICommand>("Run Query", ImageInfoContextMenuCommand) // Currently only support the Run Query right click option. Assigns this command to our command handler
            };

        }

        public override string ToString()
        {
            return ImageInfo.Url;
        }

    }
}
