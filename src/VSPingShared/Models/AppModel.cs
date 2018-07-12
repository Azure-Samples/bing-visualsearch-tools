using System.Collections.Generic;
using VSPing.Utils;

namespace VSPing.Models
{
    public class AppModel : BindableBase
    {
        /// <summary>
        /// In charged of establish the different ImageStores that the program can handdle
        /// IE: File, Azure, Etc.
        /// </summary>
        protected static AppModel sModel = null; // Singleton instance of the AppModel
        public static AppModel GetModel()
        {
            if (sModel == null) // Only creates a new instance if one isn't already avaliable
            {
                sModel = new VSPing.Models.AppModel();
            }

            return sModel;
        }
        public List<IImageStore> ImageStores { get; protected set; } // List of image stores from which we can load images to query
        protected Dictionary<string, SearchModel> searchModalDictionary; // One search model per query tab, indexed by the tab name

        public AppModel()
        {
            this.ImageStores = new List<IImageStore>();
            this.ImageStores.Add(new FileImageStore()); // First store is to load images from local folders
            this.ImageStores.Add(new AzureImageStore()); // Second store is to load images from an azure blob. Url to azure blob goes in the App.Config
            this.searchModalDictionary = new Dictionary<string, SearchModel>();
        }

        public virtual SearchModel CreateSearchModel(string id)
        {
            if(!this.searchModalDictionary.ContainsKey(id)) // Checks if an id has already been provided before adding the new one
            {
                SearchModel sm = new SearchModel();
                this.searchModalDictionary.Add(id, sm);
            }

            return this.searchModalDictionary[id];
        }

        public SearchModel GetSearchModel(string id)
        {
            return this.searchModalDictionary[id];
        }
    }
}
