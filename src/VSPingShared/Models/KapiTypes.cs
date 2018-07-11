using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace VSPing.Models
{
    public class ImageBoundingBox
    {
        /// <summary>
        /// This class holds the properties of every bounding box
        /// </summary>
        public class ImageRectangle
        {
            /// <summary>
            /// This class holds the properties of every image rectangle
            /// </summary>
            public class ImagePoint
            {   
                /// <summary>
                /// This class holds the x and y coordinates of a point
                /// </summary>
                public float X { get; set; }
                public float Y { get; set; }
            }

            public ImagePoint TopLeft { get; set; }
            public ImagePoint TopRight { get; set; }
            public ImagePoint BottomLeft { get; set; }
            public ImagePoint BottomRight { get; set; }

            public bool IsBounded =>   TopLeft == null || TopRight == null || BottomLeft == null | BottomRight == null
                                    ||   TopLeft.X != 0 || TopLeft.Y != 0
                                    || TopRight.X != 1 || TopRight.Y != 0
                                    || BottomLeft.X != 0 || BottomLeft.Y != 1
                                    || BottomRight.X != 1 || BottomRight.Y != 1;
        } // returns true if any of this conditions is true

        public ImageRectangle QueryRectangle { get; set; }
        public ImageRectangle DisplayRectangle { get; set; }

        public override string ToString()
        {
            if ( (this.QueryRectangle?.IsBounded ?? false) || (this.DisplayRectangle?.IsBounded ?? false)) // returns an string to check if it's bounded
            {
                return " BBPresent";
            }
            return string.Empty;
        }
    }

    public class cImage
    {
        /// <summary>
        /// This class contains the properties every cImage should have
        /// </summary>
        public string ThumbnailUrl { get; set; }
    }

    public class ImageInsightsToken
    {
        /// <summary>
        /// This class holds information about the imageInsightsToken generated for a particular image
        /// </summary>
        public Dictionary<string, string> Parts { get; private set; } 
        public ImageInsightsToken(string imageInsightsToken)
        {
            if (!string.IsNullOrEmpty(imageInsightsToken))
            {
                // ImageInsights Token is encoded as <key1>_<value1>*<key2>_<value2>*<key3>_<value3>
                var kvp = imageInsightsToken.Split('*').Select(kvpStr => kvpStr.Split('_'));

                this.Parts = kvp.ToDictionary(kvpStr => kvpStr[0], kvpStr => kvpStr[1]);
            }
        }
    }

    public class Provider
    {
        /// <summary>
        /// This class holds information about the provider of an image tag
        /// </summary>
        public string Name { get; set; }
    }

    public class Action
    {
        /// <summary>
        /// This class is a core building block of the response. Each Action has a type, name, and list of urls
        /// </summary>
        public string ActionType { get; set; } = String.Empty;
        public string DisplayName { get; set; } = String.Empty;
        private string websearchUrl;
        public string WebSearchUrl
        {   get { return this.websearchUrl; }
            set {
                this.websearchUrl = value;
                if (string.IsNullOrEmpty(value))
                    return;                    
                if (this.Urls.ContainsKey("WebSearchUrl")) this.Urls.Remove("WebSearchUrl"); this.Urls.Add("WebSearchUrl", value); }
        }

        // return the default Url for this action, if any
        // advanced actions like Url, Address, etc. could have alternate URLs to search.
        public virtual string DefaultUrl
        {
            get { return this.WebSearchUrl; }
        }

        public virtual bool HasDefaultUrl { get { return string.IsNullOrEmpty(this.DefaultUrl) == false; } }
        public Action()
        {
            this.DisplayName = string.Empty;
            this.ActionType = String.Empty;
            this.Urls = new Dictionary<string, string>();
            this.WebSearchUrl = string.Empty;                                    
        }

        public virtual bool IsEmpty() { return false; }
        public Provider[] Provider = new Provider[] { };
        public virtual bool HasThumbnail { get { return false; } }
        public virtual string ThumbnailUrl { get { return null; } }
        public virtual Dictionary<string,string> Urls { get; set; }
        
        protected string GetFirstProviderNameOrDefault() // Name of the provider (the one that provides you the picture)
        {
            return this.Provider.Length > 0 ? this.Provider[0].Name : String.Empty;
        }
                
        public override string ToString()
        {
            return $"{DisplayName} (actiontype:{ActionType}, provider:{GetFirstProviderNameOrDefault()})";
        }

        public virtual void PostProcess() {} // method to be called if needed after actions has been parse, currently empty
    }
 
    public class ImageResultBase
    {
        /// <summary>
        /// This class holds properties all images contained by Actions will have
        /// </summary>
        public class cInsightsMetadata
        {
            public class cOffer // Currently not implemented in the Knowledge API response, but a product results feature soon to come
            {
                public string Name { get; set; }
                public string PriceCurrency { get; set; }
                public double Price { get; set; }               
                public string Availability { get; set; }              
                public override string ToString()
                {
                    return $"{PriceCurrency ?? String.Empty} {Price.ToString("F2")} Availability:({Availability ?? "Unknown"}) {Name ?? String.Empty}";
                }
            }

            public class cAggregateOffer
            {
                public List<cOffer> offers;
            }

            public int ShoppingSourcesCount { get; set; }
            public int RecipeSourcesCount { get; set; }
            public int PagesIncludingCount { get; set; }
            public int AvailableSizesCount { get; set; }
            public cAggregateOffer AggregateOffer { get; set; }

            public override string ToString()
            {
                return $"{AggregateOffer?.offers?[0].ToString() ?? String.Empty}";
            }
        }
        public string WebSearchUrl { get; set; }
        public string Name { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ContentUrl { get; set; }
        public string HostPageUrl { get; set; }
        public cInsightsMetadata InsightsMetadata { get; set; }
        public string ImageInsightsToken { get; set; }
        public ImageResultBase()
        {
            this.Name = String.Empty;
        }  
        public override string ToString()
        {
            return $"{this.Name}";
        }
    }
    public class ImageModuleAction : Action
    {
        /// <summary>
        /// This class contains properties that every image module action should have
        /// </summary>
        public class ImageResultValues
        {
            /// <summary>
            /// This class holds values for image results
            /// </summary>
            public List<ImageResultBase> Value { get; set; }
        }

        public ImageResultValues Data { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Count:{this.Data?.Value?.Count ?? 0}";
        }

        public override bool IsEmpty()
        {
            return (this.Data?.Value?.Count ?? 0)  == 0;
        }
    }

    public class EntityAction : Action // Action which corresponds to an Entity detection by the Satori knowledge base
    {
        /// <summary>
        /// This class extends the Action class to include properties every entity action should have
        /// </summary>
        public class EntityData
        {      
            /// <summary>
            /// This class includes properties every EntityData should have
            /// </summary>
            public cImage Image { get; set; }
            public string _Type { get; set; }
            public string Id { get; set; }
            public string WebSearchurl { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string BingId { get; set; }               
        }

        protected EntityData data;
        public virtual EntityData Data
        {
            get { return this.data; }
            set
            {
                this.data = value; 
            }
        }

        public override bool HasThumbnail
        {
            get
            {
                return string.IsNullOrEmpty(this.Data?.Image?.ThumbnailUrl) == false;
            }
        }
        public override string ThumbnailUrl
        {
            get
            {
                return this.Data?.Image?.ThumbnailUrl;
            }
        }

        public override bool IsEmpty()
        {
            return this.Data == null;
        }
        public override string ToString()
        {   
            return $"{Data.Name} \n(EntityType:{Data._Type ?? "<no type>"} \nBingId:{this.Data.BingId ?? "<no id>"} \nactionType:{this.ActionType} \nprovider:{GetFirstProviderNameOrDefault()})";
        }  
    }

    public class UriAction : Action
    {
        /// <summary>
        /// This class extends the Action class to include properties that every URL Action should have
        /// </summary>
        private Uri url;
        public Uri Url
        {
            get { return this.url; }
            set { this.url = value; if (this.Urls.ContainsKey("Uri")) this.Urls.Remove("Uri"); this.Urls.Add("Uri", value.ToString()); }
        }

        public override string DefaultUrl
        {
            get
            {
                return this.url.AbsoluteUri;
            }
        }

        public override string ToString()
        {
            return $"{this.Url.ToString()} (actionType:{this.ActionType}, provider:{GetFirstProviderNameOrDefault()})";
        }
    }

    public class PostalAddressAction : Action
    {
        /// <summary>
        /// This class extends the Action class to include properties that every postal address action should have
        /// </summary>
        private string toStringValue;

        public struct cLocation
        {
            public Address Address { get; set; }
        }

        public struct Address
        {
            public string Text { get; set; }
        }

        public List<cLocation> Location { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(toStringValue))
                return this.toStringValue;

            int numAddresses = this.Location?.Count ?? 0;

            var addresses = 
                from cLocation l in this.Location ?? Enumerable.Empty<cLocation>()
            select l.Address.Text ?? string.Empty;

            this.toStringValue = $"{string.Join(" | ", addresses)} (actionType: {this.ActionType}, {numAddresses} addresses)";

            return toStringValue;
        }

    }

    public class ActionFactory
    {
        /// <summary>
        /// This class parses action json objects returned by the request into known action classes we have defined
        /// </summary>
        // here it adds the actiontypes in a dictionary of "The Known Action Type" -> the action
        public Dictionary<string, Type> KnownActionTypes = new Dictionary<string, Type>();
        public static ActionFactory actionFactory = null;

        public ActionFactory()
        {
            KnownActionTypes.Add("VisualSearch", typeof(ImageModuleAction));
            KnownActionTypes.Add("ProductVisualSearch", typeof(ImageModuleAction));
            KnownActionTypes.Add("PagesIncluding", typeof(ImageModuleAction));
            KnownActionTypes.Add("Trivia", typeof(ImageModuleAction));
            KnownActionTypes.Add("Entity", typeof(EntityAction));
            KnownActionTypes.Add("Uri", typeof(UriAction));
            KnownActionTypes.Add("PostalAddress", typeof(PostalAddressAction));
        }

        public static ActionFactory GetFactory() // Gets the singleton factory instance
        {
            if (actionFactory == null) actionFactory = new ActionFactory();
            return actionFactory;
        }

        public virtual Action Parse(JToken actionJObject, string actionType)
        {
            Type targetType = KnownActionTypes.ContainsKey(actionType) ? KnownActionTypes[actionType] : null;
            Action action = targetType == null ? null : (Action) actionJObject.ToObject(targetType);
            if (targetType != null) action.PostProcess();
            return action;
        }

    }

    public class Tag // The request returns a list of Tags, in which each Tag has a name, score, and list of corresponding actions, as the most important properties
    {
        /// <summary>
        /// This class dictates the properties that each tag should have
        /// </summary>
        private string toStringValue;
        public cImage Image { get; set; }
        public string DisplayName { get; set; }
        public ImageBoundingBox BoundingBox { get; set; }
        public double? Score { get; set; }
        public List<Action> Actions { get; set; }
        public List<string> Sources { get; set; }
        public bool HasThumbnail { get { return string.IsNullOrEmpty(this.Image?.ThumbnailUrl) == false; } }    
        public IEnumerable<Action> FilteredActions {
            get {
                return
                      this.Actions
                          .Where(action => action.IsEmpty() == false);
                }
        }
        public static Tag FromJson(JObject tagJObject) // Returns a Tag object from the raw json
        {
            Tag retVal = new Tag();
            retVal.DisplayName = tagJObject["displayName"]?.ToString() ?? String.Empty;                      
            retVal.Score = tagJObject["score"]?.ToObject <double>();
            retVal.Image = tagJObject["image"]?.ToObject<cImage>();
            retVal.BoundingBox = tagJObject["boundingBox"]?.ToObject<ImageBoundingBox>();
            retVal.Sources = tagJObject["sources"]?.ToObject<List<string>>();
            retVal.Actions = new List<Action>();

            var actionJObjects = tagJObject["actions"];
                                   
            if (actionJObjects != null) // If there is at least one tag
            {
                var factory = ActionFactory.GetFactory(); // Get the singleton instance of the Action Factory
                foreach (var actionJObject in actionJObjects)
                {
                    string actionType = actionJObject["actionType"]?.ToString() ?? String.Empty;

                    Action action = factory.Parse(actionJObject, actionType);
                    if (action == null) // Not one of the known Action objects for which we have pre-defined objects
                    {
                        action = actionJObject.ToObject<Action>();
                    }
                    retVal.Actions.Add(action);
                }
            }

            return retVal;
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(toStringValue))
            {
                var name = string.IsNullOrEmpty(this.DisplayName) ? "(noname)" : this.DisplayName;
                var bb = this.BoundingBox?.ToString() ?? String.Empty;
                var sources = this.Sources != null ? " sources:" + string.Join(",", this.Sources) : String.Empty;
                var score = this.Score != null ? " score:" + this.Score.Value.ToString("F2") : String.Empty;

                this.toStringValue = $"\"{name}\" (actions:{this.FilteredActions.Count()} {score}{bb}{sources})";
            }

            return this.toStringValue;
            
        }
    }
    
    [JsonObject]
    public class KapiRequest
    {
        /// <summary>
        /// This class contains all properties we will send to the request. JsonObject Attribute signals it can be serialized into JSON
        /// </summary>
        [JsonObject]
        public class ImageInfo
        {
            [JsonObject]
            public class CropArea
            {
                public double top = 0.0;
                public double left = 0.0;
                public double right = 1.0;
                public double bottom = 1.0;
            }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public CropArea cropArea = new CropArea();

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string url;
        }

        [JsonObject]
        public class KnowledgeRequest
        {
            [JsonObject]
            public class Filters
            {
                public string site;
            }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Filters filters;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string subscriptionId;
        }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public KnowledgeRequest knowledgeRequest = new KnowledgeRequest();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public KapiRequest.ImageInfo imageInfo = new ImageInfo();
    }

    public enum KapiResponseType
    {        
        ErrorResponse,
        ImageKnowledge
    }
    public class KapiResponse : BingSearchResponse
    {
        /// <summary>
        /// This class extends the BingSearchResponse class to add properties unique to a Knowledge API Response
        /// </summary>
        /// <param name="jsonObject"></param>
        public KapiResponse(JObject jsonObject) // COnverts the raw json to a KapiResponse object
        {
            TryParse(jsonObject);
        }
        public KapiResponse(string json) : this(Newtonsoft.Json.Linq.JObject.Parse(json))
        { }
        public KapiResponse()
        {
            this.ResponseType = KapiResponseType.ImageKnowledge;
            this.Source = SearchResponseSource.None;
            this.SearchEndpointUrl = KapiSearch.KapiEndpointUrl;       
        }
        public Dictionary<string, JToken> MiscInfo;
        public JToken KapiRequestJson { get; set; }
        public JToken KapiResponseJson { get; set; }
        public KapiResponseType ResponseType { get; private set; }
        public List<Tag> Tags { get; set; }
        public string ImageInsightsToken { get; private set; }
        private void ParseImpressionGuid(JObject jsonObject)
        {
            var pingUri = jsonObject?.SelectToken("$.instrumentation.pingUrlBase")?.ToString();

            if (string.IsNullOrEmpty(pingUri))
                return;
            // IG is the unique id of the ping that's made
            this.ImpressionGuid = pingUri.Substring(
                        pingUri.IndexOf("IG=", StringComparison.OrdinalIgnoreCase) + 3,
                        32);
        }

        private void ParseErrorResponse(JObject jsonObject)
        {
            this.ResponseType = KapiResponseType.ErrorResponse;
            this.ParseImpressionGuid(jsonObject);
        }
        private void ParseSuccessResponse(JObject jsonObject) // Fills all properties based on the JSOn
        {
            this.Tags = this.Tags ?? new List<Tag>();

            this.Tags.Clear();

            this.ResponseType = KapiResponseType.ImageKnowledge;

            this.ParseImpressionGuid(jsonObject);

            foreach (var tagJson in jsonObject["tags"]?.Values<JObject>())
            {
                var tag = Tag.FromJson(tagJson);
                this.Tags.Add(tag);
            }
            
            var str = jsonObject.SelectToken("$.image.imageInsightsToken")?.ToString() ?? String.Empty;
            ImageInsightsToken iit = new ImageInsightsToken(str);

            if (iit.Parts?.ContainsKey("bcid") ?? false) 
            {
                this.ImageInsightsToken = iit.Parts["bcid"];
            } else
            {
                this.ImageInsightsToken = string.Empty;
            }
        }
        public bool TryParse(string jsonString)
        {
            bool parsed = false;

            try
            {
                var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(jsonString);

                parsed = this.TryParse(jsonObject);

            }
            catch (Exception)
            {
                parsed = false;
            }

            return parsed;
            
        }
        public bool TryParse(JObject jsonObject)
        {
            string responseType = jsonObject["_type"]?.Value<string>();

            if (string.IsNullOrEmpty(responseType))
                throw new ApplicationException("unknown type of kapi response received");

            switch (responseType)
            {
                // the two cases:error or if tha parsing is sucesfull
                case "ErrorResponse":
                    ParseErrorResponse(jsonObject);
                    break;
                case "ImageKnowledge":
                    ParseSuccessResponse(jsonObject);
                    break;
                default:
                    throw new ApplicationException("unknown type of kapi response received");
            }

            this.KapiResponseJson = jsonObject;            
            return true;
        }

        protected virtual SearchResult GenerateSearchFromImageResult(ImageResultBase result)
        {
            return new SearchResult()
            {
                ImageUri = result.ContentUrl,
                PageUri = result.HostPageUrl,
                PageTitle = result.Name,
                ThumbnailUri = result.ThumbnailUrl,
                PageLanguage = String.Empty,
                InsightsMetadata = result.InsightsMetadata?.ToString(),
            };
        }

        public virtual void ExtractSearchResults()
        {
            // extract Visual, Product Search results. Because they JSON object/arrays of the same type
            // we just create "actionName",<targetCollection> array, so that we can extract them in the same way         
            var retValSearchResultContainers = new Tuple<string, IList<SearchResult>>[]
            {
                    new Tuple<string, IList<SearchResult>>("VisualSearch",          this.VisualSearchResults),
                    new Tuple<string, IList<SearchResult>>("ProductVisualSearch",   this.ProductSearchResults),
                    new Tuple<string, IList<SearchResult>>("PagesIncluding",        this.PagesIncludingSearchResults),
            };

            // now crack open the result containers
            foreach (var c in retValSearchResultContainers)
            {
                // Step #1 clear the target container
                c.Item2.Clear();

                // Step #2 crack open every Tag.Action of type Tuple.Item1
                var results = this.Tags
                    .Where(tag => tag.DisplayName == String.Empty)
                    .FirstOrDefault()
                    ?.Actions
                    .Where(action => action.ActionType == c.Item1)
                    .Cast<ImageModuleAction>()
                    .FirstOrDefault()
                    ?.Data
                    ?.Value;

                if (results == null) // no results, then continue
                    continue;
               
                // Step #3 copy the results to the right retVal container referenced in Tuple.Item2
                foreach (var result in results)
                {
                    var sr = GenerateSearchFromImageResult(result);
                    c.Item2.Add(sr);
                }
            }
        }
                                     
    }
}