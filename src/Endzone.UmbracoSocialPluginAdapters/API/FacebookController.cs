using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Endzone.UmbracoSocialPluginAdapters.API
{
    /// <summary>
    /// Standard URL to access this controller is /umbraco/UmbracoSocialPluginAdapters/Facebook/
    /// </summary>
    [PluginController(Constants.AreaName)]
    public class FacebookController : UmbracoApiController
    {
        /// <summary>
        /// Standard URL to access this action is /umbraco/UmbracoSocialPluginAdapters/Facebook/Get.
        /// 
        /// The serialization logic only works with JSON requests. If you use a browser to test this action,
        /// make sure you are sending application/json in your Accept header.
        /// </summary>
        /// <remarks>The id paramater can be a string too, the page name or user alias (untested).</remarks>
        [HttpGet]
        public object Get(string id, string feed = "feed", int limit = 20)
        {
            var appId = ConfigurationManager.AppSettings[Constants.FacebookAppIdKey];
            var appSecret = ConfigurationManager.AppSettings[Constants.FacebookAppSecretKey];

            var pageId = id;

            if (limit > 250)
                limit = 250;

            const string fields = "id,message,picture,link,name,description,type,icon,created_time,from,object_id,likes,comments";

            var authUrl = $"https://graph.facebook.com/oauth/access_token?grant_type=client_credentials&client_id={appId}&client_secret={appSecret}";

            var accessToken = new WebClient().DownloadString(authUrl);
            accessToken = accessToken.Substring(accessToken.IndexOf('=') + 1);

            var pageUrl = $"https://graph.facebook.com/v2.3/{pageId}?key=value&access_token={accessToken}&fields=id,link,name";

            var pageObject = new WebClient().DownloadString(pageUrl);

            dynamic pageDetails = JObject.Parse(pageObject);

            var pageLink = pageDetails.link ?? string.Empty;
            var pageName = pageDetails.name ?? string.Empty;

            var graphUrl = $"https://graph.facebook.com/v2.3/{pageId}/{feed}?key=value&access_token={accessToken}&fields={fields}&limit={limit}";
            var graphObject = new WebClient().DownloadString(graphUrl);
            dynamic parsedJson = JObject.Parse(graphObject);
            var pagefeed = parsedJson.data;

            var response = new
            {
                responseData = new {
                    feed = new {
                        link = string.Empty,
                        entries = new List<dynamic>()
                    }
                }
            };

            foreach (var data in pagefeed)
            {
                string message;
                if (!string.IsNullOrEmpty((string)data.message))
                {
                    message = ((string) data.message).Replace("\n", "<br />");
                }
                else if (!string.IsNullOrEmpty((string)data.story))
                {
                    message = data.story;
                }
                else
                {
                    message = string.Empty;
                }

                if (!string.IsNullOrEmpty((string)data.description))
                {
                    message += " " + data.description;
                }

                string link = data.link ?? string.Empty;
                string image = data.picture ?? string.Empty;
                string type = data.type ?? string.Empty;

                if (type == "status" && !string.IsNullOrEmpty((string)data.story))
                {
                    continue;
                }

                if ((data.object_id == null) && (type != "video"))
                {
                    var picId = image.Split('_');
                    if (picId.Length > 0)
                    {
                        data.object_id = picId[0];
                    }
                }

                if (data.object_id != null)
                {
                    if (!image.Contains("safe_image.php") && Regex.IsMatch((string)data.object_id, @"^\d+$"))
                    {
                        image = $"https://graph.facebook.com/{data.object_id}/picture?type=normal";
                    }
                }

                response.responseData.feed.entries.Add(new {
                    pageLink,
                    pageName,
                    link,
                    content = message,
                    thumb = image,
                    publishedDate = data.created_time //might need to change, original PHP date format: D, d M Y H:i:s O
                });
            }

            return response;
        }
    }
}