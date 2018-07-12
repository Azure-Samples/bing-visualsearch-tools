using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VSPing.Models
{
    public class KapiSearch
    {
        /// <summary>
        /// class that has basically all the process of the search in bing visual search
        /// This class takes a source image, sends it to the API, and returns the response recieved from the API
        /// </summary>
        public static string KapiEndpointUrl = "https://api.cognitive.microsoft.com/bing/v7.0/images/visualsearch/";
        protected string accessKey = System.Configuration.ConfigurationManager.AppSettings["key"];// You can change the key on App.config
        protected Uri kapiEndpointUrl;        
        protected string subscriptionId;
        protected HttpClient sharedHttpClient;


        public KapiSearch(string kapiEndpointUrl, string subscriptionId, HttpClient sharedHttpClient = null) // Constructor for a KapiSearch object
        {            
            this.kapiEndpointUrl = new Uri (kapiEndpointUrl);
            this.subscriptionId = subscriptionId;
            this.sharedHttpClient = sharedHttpClient;
        }

        protected KapiRequest BuildKapiRequest(SearchRequest searchRequest) // This method fills a KapiRequest from a passed SearchRequest
        {
            KapiRequest kapiRequestObject = new KapiRequest();
            if (searchRequest.ScaledBB.HasValue) // Crops the image if a bounding box was placed on it
            {
                kapiRequestObject.imageInfo.cropArea.top = searchRequest.ScaledBB.Value.ct;
                kapiRequestObject.imageInfo.cropArea.left = searchRequest.ScaledBB.Value.cl;
                kapiRequestObject.imageInfo.cropArea.bottom = searchRequest.ScaledBB.Value.cb;
                kapiRequestObject.imageInfo.cropArea.right = searchRequest.ScaledBB.Value.cr;
            }

            if (string.IsNullOrEmpty(this.subscriptionId) == false) // Checks if the subscription ID exists
                kapiRequestObject.knowledgeRequest = new KapiRequest.KnowledgeRequest() { subscriptionId = subscriptionId };

            if (searchRequest.QueryImageUri != null) // Checks if the queryimage url exists
            {
                kapiRequestObject.imageInfo.url = searchRequest.QueryImageUri.ToString();
            }

            if (searchRequest.CustomProperties.ContainsKey("site")) // If we have specified a Filter Site url, add it to our request
            {
                kapiRequestObject.knowledgeRequest.filters = new KapiRequest.KnowledgeRequest.Filters() { site = searchRequest.CustomProperties["site"] };
            }

            return kapiRequestObject;
        }

        protected MultipartFormDataContent BuildPostPayload(KapiRequest request, SearchRequest searchRequest) // This method builds the entire request as a form 
        {
            MultipartFormDataContent retVal = new MultipartFormDataContent();
            // Part #1 - Add binary image file if using a local image
            if (searchRequest.LocalTransformedQueryImageUri != null)
            {           
                string path = searchRequest.LocalTransformedQueryImageUri.LocalPath;

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamContent sc = new StreamContent(fs);                              
                retVal.Add(
                        sc,         // binay image path
                        "image",    // name = image
                        "image"     // filename = image
                    );
            }

            // Part #2 - Add KnowledgeRequest JSON object
            var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            retVal.Add(new StringContent(requestJson), "knowledgeRequest");

            return retVal;
        }

        protected virtual async Task<KapiResponse> Request(string requestUri, MultipartFormDataContent mfdc) //This method sends the request to the server and recieves a response
        {
            bool perInstanceClient = true;
            HttpClient client = null; 
            if (this.sharedHttpClient != null) // Checks that the client is valid
            {
                client = this.sharedHttpClient;
                perInstanceClient = false;
            }
            else
            {
                client = new HttpClient();
            }
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
                {
                    request.Content = mfdc;
                    if (!string.IsNullOrEmpty(accessKey) && accessKey.Length == 32 ) // Checks for valid length access key
                        request.Headers.Add("Ocp-Apim-Subscription-Key", accessKey);
                    
                    using (var response = await client.SendAsync(request)) // Makes a call to the API and recieves the response
                    {
                        string responseJsonString = string.Empty;
                        responseJsonString = response.Content.ReadAsStringAsync().Result;

                        KapiResponse retVal = new KapiResponse(responseJsonString); // Creates a KapiResponse object from the returned json string
                        retVal.SearchEndpointUrl = requestUri;
                        retVal.Source = SearchResponseSource.LiveQuery;
                        retVal.Status = $"{response.StatusCode.ToString()}";

                        if (response.Headers.Contains("BingAPIs-TraceID"))
                        {
                            retVal.EventId = response.Headers.GetValues("BingAPIs-TraceID").FirstOrDefault();
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            retVal.ExtractSearchResults();
                        }

                        return retVal;
                    }
                }
            }
            finally
            {
                // dispose
                if (perInstanceClient)
                    client.Dispose();
            }
        }

        protected KapiResponse AddJson(KapiRequest kapiRequest, KapiResponse kapiRespose) // This method converts the response into a JSON object
        {
            var root = new JObject();
            root.Add("object",
                    // .NET object --> JSON string --> JObject graph (so that it can be rendered)
                    Newtonsoft.Json.Linq.JObject.Parse(
                        Newtonsoft.Json.JsonConvert.SerializeObject(kapiRequest)
                    ));
            kapiRespose.KapiRequestJson = root;
            return kapiRespose;
        }

        public async Task<KapiResponse> Search(SearchRequest searchRequest)  // This method handles the entire searching process, invoking the above methods
        {
            DateTime startTime = DateTime.Now;

            var endpoint = this.kapiEndpointUrl.ToString();

            if (searchRequest.CustomProperties != null && searchRequest.CustomProperties.ContainsKey("market")) // If we have specified a market parameter, add it to the endpoint url
            {
                endpoint = endpoint + "?mkt=" + searchRequest.CustomProperties["market"];
            }


            var kapiRequest = this.BuildKapiRequest(searchRequest);
            var postBody = this.BuildPostPayload(kapiRequest, searchRequest);
            var retVal = await Request(endpoint, postBody);

            retVal = AddJson(kapiRequest, retVal);
            retVal.Duration = DateTime.Now.Subtract(startTime);

            return retVal;
        }

    }
}