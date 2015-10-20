using System.Configuration;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Endzone.UmbracoSocialPluginAdapters.API
{
    [PluginController(Constants.AreaName)]
    public class TwitterController : UmbracoApiController
    {
        [HttpGet]
        public object Get(string url, string q, string list_id, string screen_name, string include_rts, string exclude_replies, int count = 20)
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
                    searchSettings.SearchQuery = string.Format("statuses/user_timeline.json?screen_name={0}&amp;include_rts={1}&amp;exclude_replies={2}&amp;count={3}", screen_name, include_rts, exclude_replies);
                    break;

                case "search":
                    searchSettings.SearchQuery = string.Format("search/tweets.json?q={0}&amp;include_rts={1}&amp;count={3}", q, include_rts, count);
                    break;

                case "list":
                    searchSettings.SearchQuery = string.Format("lists/statuses.json?list_id={0}&amp;include_rts={1}&amp;count={3}", list_id, include_rts, count);
                    break;

                default:
                    searchSettings.SearchQuery = string.Format("statuses/user_timeline.json?count={3}", count);
                    break;
            }

            var twit = new OAuthTwitterWrapper.OAuthTwitterWrapper(authSettings, timelineSettings, searchSettings);
            return twit.GetSearch();
        }
    }
}
