using System;
using System.Collections.Generic;
using System.Windows;

namespace VSPing.Models
{
    public struct ScaledBox // Used to turn absolute Rect coordinates for bounding box and put them in [0, 1] range 
    {
        public double ct { get; set; }
        public double cl { get; set; }
        public double cb { get; set; }
        public double cr { get; set; }

        public static ScaledBox From(Size imageSize, Rect bbOnImage) 
        {
            ScaledBox retVal = new ScaledBox();

            retVal.ct = bbOnImage.Top / imageSize.Height; if (retVal.ct < 0) retVal.ct = 0;
            retVal.cl = bbOnImage.Left / imageSize.Width; if (retVal.cl < 0) retVal.cl = 0;
            retVal.cb = bbOnImage.Bottom / imageSize.Height; if (retVal.cb > 1) retVal.cb = 1;
            retVal.cr = bbOnImage.Right / imageSize.Width; if (retVal.cr > 1) retVal.cr = 1;
            return retVal;
        }
    }

    public class SearchRequest
    {
        /// <summary>
        /// This class includes the properties every search request to the Knowledge API will have
        /// </summary>
        public Uri QueryImageUri; // Holds uri is we are sending a url 
        public Uri LocalTransformedQueryImageUri; // Holds uri if we are sending a local image (file uri)
        public ScaledBox? ScaledBB; // Bounding box (if exists)
        public Dictionary<string, string> CustomProperties = new Dictionary<string, string>(); // Currently no properties are used, but can add mkt and safeSearch as described in API docs
    }

    public class SearchResult
    {
        /// <summary>
        /// This class includes the properties every search result from the Knowledge API will have
        /// </summary>
        public string ImageUri { get; set; }
        public string PageUri { get; set; }
        public string PageTitle { get; set; }
        public string PageLanguage { get; set; }
        public string ThumbnailUri { get; set; }
        public string InsightsMetadata { get; set; }
        public override string ToString()
        {
            return ImageUri.ToString();
        }        
    }

    public enum SearchResponseSource
    {
        None,
        LiveQuery,
        Cache
    }
   
    interface ISearchResponse
    {
        /// <summary>
        /// This interface contains a few general properties every search response should include
        /// </summary>
        string SearchEndpointUrl { get; set; }
        string EventId { get; set; }
        string Status { get; set; }
        SearchResponseSource Source { get; set; }
        TimeSpan? Duration { get; set; }
    }

    public class SearchResponse: ISearchResponse
    {
        /// <summary>
        /// This class extends the ISearchResponse interface to include generic search properties
        /// </summary>
        public string SearchEndpointUrl { get; set; }
        public string EventId { get; set; }
        public string Status { get; set; }
        public SearchResponseSource Source { get; set; }
        public TimeSpan? Duration { get; set; }
    }

    public class BingSearchResponse : SearchResponse
    {
        /// <summary>
        /// This class extends the SearchResponse class to add properties exclusive to Bing visual search
        /// </summary>
        public string ImpressionGuid { get; set; }
        public virtual List<SearchResult> VisualSearchResults { get; set; } = new List<SearchResult>();
        public virtual List<SearchResult> ProductSearchResults { get; set; } = new List<SearchResult>();
        public virtual List<SearchResult> PagesIncludingSearchResults { get; set; } = new List<SearchResult>();          
    }
}