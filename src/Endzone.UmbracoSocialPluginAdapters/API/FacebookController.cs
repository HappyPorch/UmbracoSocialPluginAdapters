using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Endzone.UmbracoSocialPluginAdapters.API
{
    [PluginController(Constants.AreaName)]
    public class FacebookController : UmbracoApiController
    {
        [HttpGet]
        public object Get(long id, string feed = "feed", int limit = 20)
        {
            var appId = ConfigurationManager.AppSettings[Constants.FacebookAppIdKey];
            var appSecret = ConfigurationManager.AppSettings[Constants.FacebookAppSecretKey];

            var appAccessToken = $"{appId}|{appSecret}";

            var pageId = id;

            if (limit > 250)
                limit = 250;

            const string fields = "id,message,picture,link,name,description,type,icon,created_time,from,object_id,likes,comments";

            var authUrl = $"https://graph.facebook.com/oauth/access_token?grant_type=client_credentials&client_id={appId}&client_secret={appSecret}";

            var graphUrl = $"https://graph.facebook.com/v2.3/{pageId}/{feed}?key=value&access_token={appAccessToken}&fields={fields}&limit={limit}";
            var pageUrl = $"https://graph.facebook.com/v2.3/{pageId}?key=value&access_token={appAccessToken}&fields=id,link,name";

            var pageObject = new WebClient().DownloadString(pageUrl);

            dynamic pageDetails = JObject.Parse(pageObject);

            var pageLink = pageDetails.link ?? string.Empty;
            var pageName = pageDetails.name ?? string.Empty;

            var graphObject = new WebClient().DownloadString(graphUrl);
            dynamic parsedJson = JObject.Parse(graphObject);
            var pagefeed = parsedJson.data;

            var count = 0;

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
                if (!string.IsNullOrEmpty(data.message))
                {
                    message = ((string) data.message).Replace("\n", "<br />");
                }
                else if (!string.IsNullOrEmpty(data.story))
                {
                    message = data.story;
                }
                else
                {
                    message = string.Empty;
                }

                if (!string.IsNullOrEmpty(data.description))
                {
                    message += " " + data.description;
                }

                string link = !string.IsNullOrEmpty(data.link) ? data.link : string.Empty;
                string image = !string.IsNullOrEmpty(data.picture) ? data.picture : string.Empty;
                string type = !string.IsNullOrEmpty(data.type) ? data.type : string.Empty;

                if (type == "status" && !string.IsNullOrEmpty(data.story))
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
                    if (!image.Contains("safe_image.php") && Regex.IsMatch(data.object_id, @"^\d+$"))
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
                    publishedDate = DateTime.Parse(data.created_time) //D, d M Y H:i:s O
                });
            }

            return pageObject;
        }
    }
}