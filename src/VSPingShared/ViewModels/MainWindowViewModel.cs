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
    public class MainWindowViewModel : BindableBase 
    {
        /// <summary>
        /// In charge of the main window of VSPing, rendering all of the different parts, getting and setting properties of each interactive part of the window
        /// </summary>
        public SearchModel searchModel; // Each MainWindow tab is associated with a SearchModel instance
        public override string ToString()
        {
            return $"VM id:{this.Name}";
        }

        // Constructor that takes in the parent AppViewModel and a name for the tab
        public MainWindowViewModel(AppViewModel parent, string id, string url = "https://chasingthesunandfollowingthestars.files.wordpress.com/2011/07/half-peeled-hongmaodan.jpg")
        {
            this.Name = id;

            var appmodel = AppModel.GetModel(); // Singleton instance of AppModele
            this.searchModel = appmodel.CreateSearchModel(this.Name);
            this.parentAppViewModel = parent;

            SetProperties(url);
            var task = this.DownloadAndSearchQueryImage(this.QueryImageUrl); // Asynchronously calls the Search task on the given url

        }

        protected virtual void SetProperties(string url = "https://chasingthesunandfollowingthestars.files.wordpress.com/2011/07/half-peeled-hongmaodan.jpg")
        {
            this.BBVisibility = Visibility.Hidden;
            this.IsBBChanging = false;
            this.QueryImageUrl = url;
            this.ResponseItemsTabs = new MyObservableCollection();
            this.VisualSearchResults = new MyObservableCollection();
            this.ProductSearchResults = new MyObservableCollection();
            this.PagesIncludingSearchResults = new MyObservableCollection();
            this.Tags = new MyObservableCollection();
            this.FlattendTagsAndActions = new MyObservableCollection();
            this.StatusBarItems = new MyObservableCollection();

            this.RotationSliderPosition = 0;
            this.CanIssueSearch = true;
        }

        protected AppViewModel parentAppViewModel; // AppViewModel instance to which this MainWindow is tied
        public string Name { get; set; } // Name that shows up in the tab of the MainWindow

        private bool canIssueSearch; 
        public bool CanIssueSearch { get { return this.canIssueSearch; } set { SetProperty(ref this.canIssueSearch, value); } } // Whether a new search can be issued in this MainWindow

        private Visibility bbVisibility;
        public Visibility BBVisibility { get { return this.bbVisibility; } set { SetProperty(ref this.bbVisibility, value); } } // Whether a bounding box has needs to be visually shown 

        private bool isBBChanging;
        public bool IsBBChanging { get { return this.isBBChanging; } set { SetProperty(ref this.isBBChanging, value); } } // Whether a bounding box is currently being modified

        private Rect bb;
        public Rect BB { get { return this.bb; } set { SetProperty(ref this.bb, value); } } // Bounding box rectangle object

        public string Market { get; set; } // Market parameter for Kapi

        public string FilterSite { get; set; } // Site parameter for Kapi visual search results

        private string queryImageUrl;

        // Asynchronously downloads the image corresponding to new URL if it has been changed
        public string QueryImageUrl
        {
            get { return this.queryImageUrl; }
            set
            {
                var oldValue = queryImageUrl;
                var changed = SetProperty(ref this.queryImageUrl, value);
                if (changed)
                {
                    var task = DownloadQueryImageAsync(this.queryImageUrl);
                };
            }
        }

        // Query and response-related parameters. Many bound to elements in the view

        private Size queryImageRenderedSize;
        public Size QueryImageRenderedSize { get { return this.queryImageRenderedSize; } set { SetProperty(ref this.queryImageRenderedSize, value); } }

        private string renderQueryImageUrl;
        public string RenderQueryImageUrl { get { return this.renderQueryImageUrl; } set { SetProperty(ref this.renderQueryImageUrl, value); } }

        private string queryStatus;
        public string QueryStatus { get { return this.queryStatus; } set { SetProperty(ref this.queryStatus, value); } }
        private double rotationSliderPosition;
        public double RotationSliderPosition { get { return this.rotationSliderPosition; } set { SetProperty(ref this.rotationSliderPosition, value); } }

        private VSPing.Models.BingSearchResponse searchResponse;
        public VSPing.Models.BingSearchResponse SearchResponse { get { return this.searchResponse; } set { SetProperty(ref this.searchResponse, value); } }
        public MyObservableCollection ResponseItemsTabs { get; set; }
        public MyObservableCollection VisualSearchResults { get; set; }
        public MyObservableCollection ProductSearchResults { get; set; }
        public MyObservableCollection PagesIncludingSearchResults { get; set; }
        public MyObservableCollection Tags { get; set; }
        public MyObservableCollection FlattendTagsAndActions { get; protected set; }
        public MyObservableCollection StatusBarItems { get; protected set; }
        private string eventId;
        public string EventId { get { return this.eventId; } set { SetProperty(ref this.eventId, value); } }
        private string requestLatency;
        public string RequestLatency { get { return this.requestLatency; } set { SetProperty(ref this.requestLatency, value); } }

        private string searchEndpointUrl;
        public string SearchEndpointUrl { get { return this.searchEndpointUrl; } set { SetProperty(ref this.searchEndpointUrl, value); } }
        public string DirectQueryUrl { get; set; }
        public int TagsTabSelectedIndex { get; set; }
        

        // Triggered by the view when dragging on the image begins. Adjusts MainWindow properties accordingly
        public void StartChangingBB(Point startPoint)
        {

            var m = this.searchModel;

            m.BB = new Rect(startPoint, startPoint);

            m.BBVisible = true;

            //
            this.IsBBChanging = true;
            this.BB = m.BB;
            this.BBVisibility = Visibility.Visible;

        }

        // Sets the final bounding box to what the user has dragged
        public void ChangeBB(Size size, Point stopPoint)
        {

            if (this.IsBBChanging == false)
            {
                return;
            }

            var m = this.searchModel;

            m.BB = new Rect(m.BB.Location, stopPoint);

            this.BB = m.BB;
            m.ScaledBB = VSPing.Models.ScaledBox.From(size, m.BB);

        }

        // Update QueryImageUrl and raise notification without using the setter property
        private void UpdateQueryImageUrl(string url)
        {
            this.queryImageUrl = url;   
            this.OnPropertyChanged("QueryImageUrl"); //just raise the notification. 
        }

        // Triggered when an image is dropped onto the query pane and when the query window pane is first loaded with default URL.
        // Ensures that search can be issued, then asynchronously downloads image from url and searches.
        public virtual async Task DownloadAndSearchQueryImage(string url)
        {
            if (this.CanIssueSearch == false)
                return;

            await DownloadQueryImageAsync(url);

            this.UpdateQueryImageUrl(url);

            await Search();
        }

        protected bool CanSearch(object o)
        {
            return this.CanIssueSearch && (o as ImageInfoViewModel != null);
        }

        // Renders from url and tries to retrieve a cached response
        protected virtual async Task DownloadAndRender(ImageInfoViewModel iivm, bool issueQuery, bool shouldResize, string url)
        {
            this.UpdateQueryImageUrl(url);

            await this.DownloadQueryImageAsync(url, shouldResize);

            await this.ShowExistingSearchResponseForUrl(iivm.Url);

            // For cases where we don't have a cached response, we want to directly issue the query.
            if (issueQuery || (this.SearchResponse?.Source ?? SearchResponseSource.None) == SearchResponseSource.None)
            {
                await Search();
            }
        }

        // Asynchronous task to try and get cached response when image is dragged from an ImageStore to the query pane.
        // Logic slightly varies based on whether image is from azure or local
        public async virtual Task DownloadQueryImageAndResponseFromQueryImageStore(object o)
        {
            if (!CanSearch(o)) return;
            ImageInfoViewModel iivm = o as ImageInfoViewModel;


            string url = iivm.Url;
            bool shouldResizeImage = true;
            bool issueQueryIfCachedResponseEmpty = true; 

            
            if (iivm.ImageInfo is FileImageInfo)        
            {
                // For file blobs we have to resize the image, we want to issue a new query
                shouldResizeImage = true;
                issueQueryIfCachedResponseEmpty = true;
            }
            else 
            {
                // For azure blob store blobs we don't resize so that we can reproduce the original query as sent from device
                shouldResizeImage = false;
                issueQueryIfCachedResponseEmpty = false; 
            }
            await iivm.FetchTagsAsync();  // the ImageInfoViewModel may not have been rendered

            await DownloadAndRender(iivm, issueQueryIfCachedResponseEmpty, shouldResizeImage, url);
        }

        // Asynchronously downloads an image based on the url and resets image editing properties
        protected virtual async Task DownloadQueryImageAsync(string url, bool resizeIfBigger = true)
        {
            try
            {
                var m = this.searchModel;
                await m.DownloadQueryImage(url, resizeIfBigger);


                this.RotationSliderPosition = 0;
                this.RemoveBB();

                this.RenderQueryImageUrl = m.TransformedImageUri.ToString();
                this.RotationSliderPosition = 0;

                this.ClearSearchResultsUI();
            }
            catch (Exception)
            {
            }
        }


        public void RotateQueryImage(double theta)
        {
            try
            {
                var m = this.searchModel;
                m.RotateQueryImage(theta);
                this.RenderQueryImageUrl = m.TransformedImageUri.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }


        public void StopChangingBB()
        {
            this.IsBBChanging = false;

            var m = this.searchModel;
            this.BBVisibility = (m.BB.Width > 0 && m.BB.Height > 0) ? Visibility.Visible : Visibility.Hidden;

            if (this.BBVisibility == Visibility.Hidden)
            {
                m.ScaledBB = null;
            }

        }

        // Simulate drawing an empty rect to remove existing BB
        public void RemoveBB()
        {
            Point origin = new Point(0, 0);
            StartChangingBB(origin);
            StopChangingBB();
        }


        protected virtual void ClearSearchResultsUI()
        {
            this.VisualSearchResults.Clear();
            this.ProductSearchResults.Clear();
            this.PagesIncludingSearchResults.Clear();
            this.FlattendTagsAndActions.Clear();
            this.Tags.Clear();
            this.EventId = string.Empty;
            this.SearchResponse = null;

            this.ResponseItemsTabs.Clear();

            this.QueryStatus = string.Empty;
            this.RequestLatency = string.Empty;
            this.StatusBarItems.Clear();
            this.StatusBarItems.Add("Querying");
        }

        protected virtual void RenderStatusBar()
        {
            if (this.SearchResponse == null)
                return;

            this.QueryStatus = this.SearchResponse.Status;
            this.StatusBarItems.Clear();
            this.StatusBarItems.Add(QueryStatus);
            this.SearchEndpointUrl = this.SearchResponse.SearchEndpointUrl;
            this.EventId = this.SearchResponse.EventId;

            string latency = string.Empty;
            if (this.SearchResponse?.Duration?.TotalMilliseconds != null)
            {
                latency = $"{this.SearchResponse.Duration?.TotalMilliseconds.ToString("F1")} ms";
            }
            this.RequestLatency = latency;
            this.StatusBarItems.Add(this.RequestLatency);
        }

        protected virtual void RenderDefaultTagsResults()
        {

            BingSearchResponse bsr = this.SearchResponse as VSPing.Models.BingSearchResponse;
            KapiResponse ksr = this.SearchResponse as VSPing.Models.KapiResponse;

            if (bsr != null) // Signals we got a correct response -> Need to update all of our containers
            {
                foreach (var r in bsr.VisualSearchResults)
                {
                    this.VisualSearchResults.Add(new SearchResultViewModel(r));
                }

                foreach (var r in bsr.ProductSearchResults)
                {
                    this.ProductSearchResults.Add(new SearchResultViewModel(r));
                }

                foreach (var r in bsr.PagesIncludingSearchResults)
                {
                    this.PagesIncludingSearchResults.Add(new SearchResultViewModel(r));
                }

            }
        }

        protected virtual void RenderTagsActionsTabs()
        {
            BingSearchResponse bsr = this.SearchResponse as BingSearchResponse;
            KapiResponse ksr = this.SearchResponse as KapiResponse;

            if (ksr != null && ksr.Tags != null)
            {
                foreach (var t in ksr.Tags)
                {
                    var vm = new TagViewModel(t);
                    this.Tags.Add(vm);

                    this.FlattendTagsAndActions.Add(vm);
                    foreach (var a in vm.FilteredActions)
                    {
                        this.FlattendTagsAndActions.Add(a);
                    }
                }

                this.ResponseItemsTabs.Add(new KeyValuePair<string, object>("Tags", this.FlattendTagsAndActions));
                this.ResponseItemsTabs.Add(new KeyValuePair<string, object>("Response", new MyJToken() { JToken = ksr.KapiResponseJson }));
                this.ResponseItemsTabs.Add(new KeyValuePair<string, object>("Request", new MyJToken() { JToken = ksr.KapiRequestJson }));

                if (ksr.MiscInfo != null)
                {
                    foreach(var kvp in ksr.MiscInfo)
                    {
                        this.ResponseItemsTabs.Add(new KeyValuePair<string, object>(kvp.Key, new MyJToken() { JToken = kvp.Value }));
                    }
                }
                        
            }
        }

        // Fills up MainWindowViewModel properties from the received SearchResponse object
        protected virtual void RenderSearchResponse()
        {
            if (this.SearchResponse == null)
                return;
            RenderStatusBar();
            RenderDefaultTagsResults();
            RenderTagsActionsTabs();
        }

        // Updates visually bound properties, fires asynchronous Search task, renders
        public virtual async Task Search()
        {
            try
            {
                this.ClearSearchResultsUI();
                var m = this.searchModel;

                this.QueryStatus = "Querying";
                this.CanIssueSearch = false;

                Dictionary<string, string> parameterDictionary = new Dictionary<string, string>();

                if (!String.IsNullOrEmpty(this.Market)) parameterDictionary.Add("market", this.Market);
                if (!String.IsNullOrEmpty(this.FilterSite)) parameterDictionary.Add("site", this.FilterSite);

                var response = await m.Search(new Uri(this.QueryImageUrl), parameterDictionary);

                this.SearchResponse = response;

                this.RenderSearchResponse();
            }
            catch (Exception)
            {
                this.QueryStatus = "Error";
            }
            finally // Task has completed -> can again search
            {
                this.CanIssueSearch = true;
            }
        }

        
        protected virtual async Task ShowExistingSearchResponseForUrl(string downloadedQueryImagesUrl)
        {
            this.ClearSearchResultsUI();

            var m = AppModel.GetModel();

            var imageInfo = LookupQueryStores(downloadedQueryImagesUrl); // Find the url within the list of ImageStore images                              

            if (imageInfo == null) // Unlikely as image was detected to be in store but now is not found
            {
                return;
            }

            await imageInfo.GenerateSearchResponse(); // The search response may not have been downloaded yet

            this.SearchResponse = imageInfo.SearchResponse as BingSearchResponse;

            this.RenderSearchResponse();
        }

        // Lookup url in our query stories and return the ImageInfo for it.
        protected ImageInfo LookupQueryStores(string url)
        {            
            foreach (QueryImageStoreViewModel isvm in parentAppViewModel.QueryStores)
            {
                foreach (ImageInfo ii in isvm.ImageStore.Images)
                {
                    if (ii.Url == url)
                        return ii;
                }
            }
            return null;
        }              

        public void CopyTabContentToClipBoard()
        {

            var kapiResponse = this.SearchResponse as KapiResponse;

            if (kapiResponse == null)
                return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(this.QueryImageUrl);

            if (this.TagsTabSelectedIndex == 0) // Want to copy tags tab
            {
                foreach (var t in kapiResponse.Tags)
                {
                    sb.AppendLine(t.ToString());
                    foreach (var a in t.FilteredActions)
                    {
                        sb.Append('\t');
                        sb.AppendLine(a.ToString());
                    }
                }
            }
            else if (this.TagsTabSelectedIndex == 1) // Want to copy json response tab 
            {
                sb.AppendLine(kapiResponse.KapiResponseJson?.ToString() ?? String.Empty);
            }
            else if (this.TagsTabSelectedIndex == 2) // Want to copy json request tab
            {
                sb.AppendLine(kapiResponse.KapiRequestJson?.ToString() ?? String.Empty);
            } 

            Clipboard.SetDataObject(sb.ToString());
        }

        public MainWindowViewModel() { }

    }
}
