using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using VSPing.Utils;

namespace VSPing.Models
{
    public class SearchModel : BindableBase
    {
        /// <summary>
        /// This class handles the searching of a query image
        /// </summary>
        public bool BBVisible { get; set; }
        public Rect BB { get; set; }        
        public ScaledBox? ScaledBB { get; set; }
        private KapiSearch KapiSearch { get; }
        public Uri QueryImageUri { get; set; }
        public Uri DownloadedImageUri { get; protected set; }
        public Uri TransformedImageUri { get; protected set; }
 
        public bool IsQueryImageModified { get; protected set; }

        public Dictionary<string, string> CustomProperties;

        public SearchModel()
        {
            this.BBVisible = false;
            this.BB = new Rect(0, 0, 0, 0);
            this.ScaledBB = null;
            this.KapiSearch = new KapiSearch(KapiSearch.KapiEndpointUrl, String.Empty);
            this.IsQueryImageModified = false;
            this.CustomProperties = new Dictionary<string, string>();
        }

        public async Task DownloadQueryImage(string url, bool resizeIfBigger = true)
        {        
            try
            {
                Uri uri = new Uri(url);
                if (uri.IsFile)
                {

                    string correctedImageLocalTempFile = url;
                    bool imageModified = false;

                    if(resizeIfBigger)
                    {
                        var r = ImageEditor.ResizeIfBiggerAndFixOrientation(url);

                        correctedImageLocalTempFile = r.Item1;
                        imageModified = r.Item2;
                    }

                    this.DownloadedImageUri = new Uri(correctedImageLocalTempFile);

                    this.IsQueryImageModified = true; // local files are always "modified", so that we send them with the query 
                }
                else
                {
                    using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                    {
                        var response = await client.GetAsync(uri);
                        var tempFileName = System.IO.Path.GetTempFileName();

                        byte[] bytes = await response.Content.ReadAsByteArrayAsync();

                        System.IO.File.WriteAllBytes(tempFileName, bytes);

                        string correctedImageLocalTempFile = tempFileName;
                        bool imageModified = false;

                        if (resizeIfBigger)
                        {
                            var r = ImageEditor.ResizeIfBiggerAndFixOrientation(tempFileName);

                            correctedImageLocalTempFile = r.Item1;
                            imageModified = r.Item2;
                        }

                        this.DownloadedImageUri = new Uri(correctedImageLocalTempFile);
                        this.IsQueryImageModified = imageModified; // if image was not modified (resized or orientation fixed, we will mark it so. This will control sending image vs image url)
                    }
                }

                this.TransformedImageUri = this.DownloadedImageUri;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }            
        }

        public void RotateQueryImage(double theta)
        {
            var rotatedImage = ImageEditor.RotateImage(this.DownloadedImageUri.LocalPath, (float)(-1.0*theta));
            this.IsQueryImageModified = true;
            this.TransformedImageUri = new Uri(rotatedImage);
        }

        protected SearchRequest CreateSearchRequest()
        {
            SearchRequest retVal = new SearchRequest();
            retVal.ScaledBB = this.ScaledBB;
            
            if (this.IsQueryImageModified)
            {
                retVal.LocalTransformedQueryImageUri = this.TransformedImageUri;
            }
            else
            {
                retVal.QueryImageUri = this.QueryImageUri;
            }
            retVal.CustomProperties = this.CustomProperties;
            return retVal;
        }

        public async Task<BingSearchResponse> Search(Uri imgUri, Dictionary<string, string> parameterDictionary)
        {
            this.QueryImageUri = imgUri;

            this.CustomProperties.Clear();
            foreach (KeyValuePair<string, string> entry in parameterDictionary)
            {
                this.CustomProperties.Add(entry.Key, entry.Value);
            }
           
            BingSearchResponse retVal = null;
            SearchRequest searchRequest = this.CreateSearchRequest();
            retVal = await this.KapiSearch.Search(searchRequest);           
            return retVal;      
        }      
    }
}
