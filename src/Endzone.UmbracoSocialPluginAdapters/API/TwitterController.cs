using System.Configuration;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Endzone.UmbracoSocialPluginAdapters.API
{
    [PluginController(Constants.AreaName)]
    public class TwitterController : UmbracoApiController
    {
        [HttpGet]
        public object Get(string url, string screen_name, string include_rts, string exclude_replies, string q = null, string list_id = null, int count = 20)
        {
            //var oauthAccessToken = ConfigurationManager.AppSettings[Constants.TwitterOAuthAccessToken];
            //var oauthAccessTokenSecret = ConfigurationManager.AppSettings[Constants.TwitterOAuthAccessTokenSecret];

            var authSettings = new OAuthTwitterWrapper.AuthenticateSettings()
            {
                OAuthConsumerKey = ConfigurationManager.AppSettings[Constants.TwitterConsumerKey],
                OAuthConsumerSecret = ConfigurationManager.AppSettings[Constants.TwitterConsumerSecret],
                OAuthUrl = "https://api.twitter.com/oauth2/token"
            };
            var searchSettings = new OAuthTwitterWrapper.SearchSettings()
            {
              SearchFormat = "https://api.twitter.com/1.1/{0}"
            };
            var timelineSettings = new OAuthTwitterWrapper.TimeLineSettings();

            switch (url)
            {
                case "timeline":
                    searchSettings.SearchQuery = $"statuses/user_timeline.json?screen_name={screen_name}&amp;include_rts={include_rts}&amp;exclude_replies={exclude_replies}&amp;count={count}";
                    break;

                case "search":
                    searchSettings.SearchQuery = $"search/tweets.json?q={q}&amp;include_rts={include_rts}&amp;count={count}";
                    break;

                case "list":
                    searchSettings.SearchQuery = $"lists/statuses.json?list_id={list_id}&amp;include_rts={include_rts}&amp;count={count}";
                    break;

                default:
                    searchSettings.SearchQuery = $"statuses/user_timeline.json?count={count}";
                    break;
            }

            var twit = new OAuthTwitterWrapper.OAuthTwitterWrapper(authSettings, timelineSettings, searchSettings);
            var json = twit.GetSearch();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(json);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return response;
        }
    }
}
