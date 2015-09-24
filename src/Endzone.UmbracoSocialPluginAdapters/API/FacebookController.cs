using System.Configuration;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Endzone.UmbracoSocialPluginAdapters.API
{
    [PluginController(Constants.AreaName)]
    public class FacebookController : UmbracoApiController
    {
        [HttpGet]
        public object Get()
        {
            return ConfigurationManager.AppSettings[Constants.FacebookAppIdKey] + ConfigurationManager.AppSettings[Constants.FacebookAppSecretKey];
        }
    }
}