using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using VSWebApp.Models;

namespace VSWebApp.Controllers
{
    [Produces("application/json")]
    [Route("api/Search")]

    public class SearchController : Controller
    {
        private AppSettings _appSettings;

        public SearchController(IOptions<VSWebApp.Models.AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        public async Task Post(string mkt = null)
        {
            var baseUri = "https://api.cognitive.microsoft.com/bing/v7.0/images/visualsearch/";
            if(!string.IsNullOrWhiteSpace(mkt))
            {
                baseUri = baseUri + "?mkt=" + mkt;
            }

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, baseUri))
            {
                string accessKey = _appSettings.accessKey;
                HttpContent content = null;
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", accessKey);
                if (Request.HasFormContentType)
                {
                    MultipartFormDataContent mfdc = new MultipartFormDataContent();

                    var form = await Request.ReadFormAsync();

                    foreach (var kvp in form)
                    {
                        var k = kvp.Key;
                        var v = kvp.Value;
                        mfdc.Add(new StringContent(v.ToString()), k);
                    }

                    foreach (var file in form.Files)
                    {
                        mfdc.Add(
                                new StreamContent(file.OpenReadStream()),
                                file.Name,
                                file.FileName
                                );
                    }

                    content = mfdc;
                }
                request.Content = content;
                
                using (var response = await client.SendAsync(request))
                {
                    Response.ContentType = response.Content.Headers.ContentType.MediaType;

                    var stream = await response.Content.ReadAsStreamAsync();
                    stream.CopyTo(Response.Body);

                    Response.ContentLength = stream.Length;
                }
            }
        }
    }
}
