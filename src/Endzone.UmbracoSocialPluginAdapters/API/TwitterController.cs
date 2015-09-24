using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Endzone.UmbracoSocialPluginAdapters.API
{
    [PluginController(Constants.AreaName)]
    public class TwitterController : UmbracoApiController
    {
        [HttpGet]
        public object Get()
        {
            return 5;
        }
    }
}
