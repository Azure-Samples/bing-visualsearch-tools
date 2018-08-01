using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace VSPing.Models
{
    public interface IImageInfo
    {
        /// <summary>
        /// This interface describes what properties every ImageInfo object has
        /// </summary>
        string Name { get; set; }
        string Url { get; set; }
        string LastModified { get; set; }
        SearchResponse SearchResponse { get; set; }
        string JsonString { get; set; }
    }
    public abstract class ImageInfo: IImageInfo
    {
        /// <summary>
        /// This abstract class extends the IImageInfo interface to hold information that every query store image should have
        /// </summary>
        public string Name { get; set; }
        public string Url { get; set; }
        public string LastModified { get; set; }

        public string JsonString { get; set; }
        public override string ToString()
        {
            return Url.ToString();
        }

        public SearchResponse SearchResponse { get; set; }
        public string[] Tags { get; set; } = new string[] { String.Empty };        
        
        public string TagsList => String.Join(",", this.Tags);

        public abstract Task GenerateSearchResponse();       
    }
    public class FileImageInfo : ImageInfo
    {
        /// <summary>
        /// This class extends the class ImageInfo to pertain to image information for local images
        /// </summary>
        // Downloads cached response stored locally. 
        // Program does not currently save Knowledge API responses locally
        // If you want to add this functionality, save the Knowledge API response in the same folder as your image as "filepath.json"
        public override async Task GenerateSearchResponse() 
        {
            // Try to look for a json response file in the same location with same filename but .json extension
            // if exits attempt to read it as a KAPI JSON response file. If not, fallback.
            var name = this.Url;
            var nameForJsonFile = name.Replace(".jpg", ".json");    //only support JPG at the moment
            if (string.Equals(name,nameForJsonFile) == false && File.Exists(nameForJsonFile)) //If there exists a JSON file with the same name as a jpeg, then continue
            {
                try
                {
                    KapiResponse sr = new KapiResponse();
                    sr.Status = "Cached Response";
                    sr.Source = SearchResponseSource.Cache;

                    string jsonResponse = File.ReadAllText(nameForJsonFile);
                    this.JsonString = jsonResponse;

                    bool parsed = sr.TryParse(jsonResponse);
                    if (parsed == false)
                    {
                        this.SearchResponse = sr;
                        return;
                    }
       
                    sr.Status = "Cached Response";
                    sr.Source = SearchResponseSource.Cache;

                    // we may get a KAPI response of type error. So only process the success (ImageKnowledge) type responses
                    if (sr.ResponseType == KapiResponseType.ImageKnowledge)
                    {
                        sr.ExtractSearchResults();

                        var tagNames = from tag in sr.Tags
                                       let t = tag.DisplayName
                                       let s = tag.Score.HasValue ? $"({tag.Score.Value.ToString("F1")})" : String.Empty
                                       where string.IsNullOrEmpty(tag.DisplayName) == false
                                       select $"{t}{s}";

                        this.Tags = tagNames.ToArray();
                    }

                    this.SearchResponse = sr;
                    return;
                }
                catch (Exception) { }
            } 

            // In the case that cached response is found, we still populate the SearchResponse 
            await Task.Run(() =>
            {
                this.SearchResponse = new SearchResponse();
                this.SearchResponse.SearchEndpointUrl = KapiSearch.KapiEndpointUrl;
                this.SearchResponse.Status = "No Cached Response Available";
                this.SearchResponse.Source = SearchResponseSource.None;
            });
            return;
        }
    }
    public class AzureBlobImageInfo : ImageInfo
    {
        /// <summary>
        /// This class extends the class ImageInfo to pertain to image information for images hosted on Azure
        /// </summary>
        public CloudBlobContainer Container { get; set; }
        public ICloudBlob Blob { get; set; }

        // The Azure Download could be triggered multiple times
        // So we have a private Task which asynchronously download the results using the DownloadAndGenerateSearchResponse().
        // Each caller of the GenerateSearchResponse() is given the same single ton task instance.
        // This way we avoid multiple downloads of the blob
        private Task ActiveGenerateSearchResponse { get; set; }
        public override Task GenerateSearchResponse() //A method that creates search responses for Azure host images
        {
            lock (this)
            {
                if (this.ActiveGenerateSearchResponse == null) //If the image doesn't already have a search response, generate one for it
                {
                    this.ActiveGenerateSearchResponse = this.DownloadAndGenerateSearchResponse();
                }
                return this.ActiveGenerateSearchResponse;
            }
        }

        // Downloads cached response from blob. 
        // Program does not currently save Knowledge API responses to the blob
        // If you want to add this functionality, save the Knowledge API response to your blob as "uri.json"
        private async Task DownloadAndGenerateSearchResponse() 
        {

            if (this.SearchResponse != null) // we have already generated this response no need to try again
            {
                return;
            }
            try
            {
                var sr = new KapiResponse();
                sr.SearchEndpointUrl = KapiSearch.KapiEndpointUrl; // queryUrl for Kapi is constant.
                sr.Status = "No Cached Response Available";
                sr.Source = SearchResponseSource.None;

                var blobUrl = new Uri(this.Url);
                var jsonBlobName = blobUrl.Segments[blobUrl.Segments.Length - 1].Replace(".jpg", ".json");

                var blob =
                    await this.Container.GetBlobReferenceFromServerAsync(jsonBlobName);

                // if no blob exists
                if (blob == null || await blob.ExistsAsync() == false)
                {
                    this.SearchResponse = sr;
                    return;
                }

                MemoryStream memStream = new MemoryStream();
                await blob.DownloadRangeToStreamAsync(memStream, null, null);
                byte[] bytes = memStream.ToArray();
                if (bytes.Length == 0)
                {
                    this.SearchResponse = sr;
                    return;
                }

                string jsonResponse = Encoding.UTF8.GetString(bytes);
                this.JsonString = jsonResponse;
                bool parsed = sr.TryParse(jsonResponse);
                if (parsed == false)
                {
                    this.SearchResponse = sr;
                    return;
                }

                sr.Status = "Cached Response";
                sr.Source = SearchResponseSource.Cache;

                // we may get a KAPI response of type error. So only process the success (ImageKnowledge) type responses
                if (sr.ResponseType == KapiResponseType.ImageKnowledge)
                {
                    sr.ExtractSearchResults();

                    var tagNames = from tag in sr.Tags
                                   let t = tag.DisplayName
                                   let s = tag.Score.HasValue ? $"({tag.Score.Value.ToString("F1")})" : String.Empty
                                   where string.IsNullOrEmpty(tag.DisplayName) == false
                                   select $"{t}{s}";

                    this.Tags = tagNames.ToArray();

                    sr.MiscInfo = new Dictionary<string, Newtonsoft.Json.Linq.JToken>();
                    sr.MiscInfo.Add("Response", sr.KapiResponseJson);
                }

                this.SearchResponse = sr;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
    }
    public interface IImageStore
    {
        /// <summary>
        /// This is a general interface for image stores to extend
        /// </summary>
        string Name { get; set; }
        List<ImageInfo> Images { get; set; }
        Task Refresh(Dictionary<string, object> refreshParams);    
        Task<ImageInfo> GetImage(string url);

    }
    public class FileImageStore : IImageStore
    {
        /// <summary>
        /// This class extends the IImageStore interface to make an image store specially for local files
        /// </summary>
        public string Name { get; set; }
        public List<ImageInfo> Images { get; set; }
        protected readonly string[] imageFileExtensions = new string[] { ".jpg", ".gif", ".png" };

        public FileImageStore() //A constructor for the image store
        {
            this.Name = "Local";
            this.Images = new List<ImageInfo>();
        }

        public async Task Refresh(Dictionary<string, object> refreshParams) //This method populates the store with local image files
        {
            // We expect this particular keyValue refresh param to be present to function
            if (refreshParams.ContainsKey("filePath") == false)
                return; 

            var images =
                await
                Task.Run(() =>
                {
                    var dirInfo = new DirectoryInfo(refreshParams["filePath"] as string);
                    var imageFileInfos = from f in dirInfo.EnumerateFiles()
                                            let lastModified = f.LastWriteTimeUtc.ToLocalTime()
                                            where this.imageFileExtensions.Contains(f.Extension.ToLower())
                                            orderby lastModified descending
                                            select new FileImageInfo()
                                            {
                                                Url = f.FullName,
                                                LastModified = lastModified.ToString("s"),
                                                Name = f.Name
                                            };


                    return imageFileInfos
                            .Cast<ImageInfo>()
                            .ToList();
                });

            this.Images = images;

        }

        public Task<ImageInfo> GetImage(string url)
        {
            return Task.FromResult<ImageInfo>(null);
        }
    }
    public class AzureImageStore : IImageStore
    {
        /// <summary>
        /// This class extends the IImageStore interface to make an image store specially for Azure files
        /// </summary>
        public string Name { get; set; }
        public List<ImageInfo> Images { get; set; }
        private CloudBlobContainer Container { get; set; }
      

        public AzureImageStore() // Constructor for the AzureImageStore
        {
            this.Name = "Azure";            
            this.Images = new List<ImageInfo>();

            try
            {
                string azContainerUrl = System.Configuration.ConfigurationManager.AppSettings["azureContainerSASUrlEscaped"];
                var container = new CloudBlobContainer(new Uri(azContainerUrl));        
                this.Container = container;                

            } catch (Exception)
            {
            }
        }
        public async Task Refresh(Dictionary<string, object> refreshParams = null) // Populates the image store with images from Azure
        {
            //could connect to the container so return as we have nothing to refresh.
            if (this.Container == null)
                return;

            BlobContinuationToken continuationToken = null;

            List<IListBlobItem> blobs = new List<IListBlobItem>();
             
            do
            {
                var response = await this.Container.ListBlobsSegmentedAsync(
                                                "",
                                                useFlatBlobListing: true,
                                                blobListingDetails: BlobListingDetails.Metadata,
                                                maxResults: null,
                                                currentToken: continuationToken,
                                                options: null,
                                                operationContext: null);

                continuationToken = response.ContinuationToken;

                var q = from blobitem in response.Results
                        let blob = blobitem as ICloudBlob
                        where blob.Properties.ContentType.Contains("image")
                        orderby blob.Properties.LastModified descending
                        select blob;

                blobs.AddRange(q);

            } while (continuationToken != null);

            this.Images = (from b in blobs
                           let lastModified = (b as ICloudBlob).Properties.LastModified?.LocalDateTime
                           orderby lastModified descending
                           select (ImageInfo)(new AzureBlobImageInfo()
                           {
                               Name = b.Uri.Segments[b.Uri.Segments.Length - 1],
                               Url = b.Uri.ToString(),
                               LastModified = lastModified?.ToString("s") ?? string.Empty,
                               Blob = b as ICloudBlob,
                               Container = this.Container
                           })).ToList();
        }


        public Task<ImageInfo> GetImage(string url)
        {
            if(this.Container == null)
                return Task.FromResult<ImageInfo>(null);

            var matchingImages = from i in this.Images
                                 where string.Equals(i.Url, url, StringComparison.OrdinalIgnoreCase) == true
                                 select i;


            var ii = matchingImages.FirstOrDefault();

            if (ii != null)
                return Task.FromResult<ImageInfo>(ii);


            if(!Uri.TryCreate(url, UriKind.Absolute, out Uri inUri))    //handle malformed Url
                return Task.FromResult<ImageInfo>(null);

            var blobName = string.Join(String.Empty, inUri.Segments.Skip(2)); //skip "/" (root),"containername/" segments and treat everything else as blob name;

            //search for a blob with his Url
            var blob = this.Container.GetBlockBlobReference(blobName);


            try
            {
                blob.FetchAttributes();
                
            }catch(Exception)
            {
                return Task.FromResult<ImageInfo>(null);
            }

            //blob found, create an ImageInfo and return it.
            ii = (ImageInfo)(new AzureBlobImageInfo()
            {
                Name = blob.Uri.Segments[blob.Uri.Segments.Length - 1],
                Url = blob.Uri.ToString(),
                LastModified = (blob as ICloudBlob).Properties.LastModified?.LocalDateTime.ToString("s") ?? string.Empty,
                Blob = blob as ICloudBlob,
                Container = this.Container
            });

            this.Images.Add(ii);

            return Task.FromResult<ImageInfo>(ii);
        }

       public ImageInfo ContainsUrl(string url, bool strict = false) // Checks if any images in the store come from the passed url
        {
            Uri inUri = null;

            Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out inUri);

            if (inUri == null)
                return null;

            var blobName = inUri.Segments[inUri.Segments.Length - 1];

            var matchingImages = from i in this.Images
                                 where string.Equals(i.Name, blobName, StringComparison.OrdinalIgnoreCase) == true
                                 select i;

            return matchingImages.FirstOrDefault();

        }
    }
}