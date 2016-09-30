using System.Configuration;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using System.Dynamic;
using System.Collections;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Endzone.UmbracoSocialPluginAdapters.API
{
    [PluginController(Constants.AreaName)]
    public class RssController : UmbracoApiController
    {
        [HttpGet]
        public object Get(string id, string type, int? limit, string feed)
        {
            if (string.IsNullOrEmpty(type))
                type = "rss";
            if (!limit.HasValue || limit.Value == 0)
                limit = 20;

            var feed_url = "http://" + id;
            if (type == "pinterest")
            {
                var source = string.IsNullOrEmpty(feed) ? feed : "user";

                if (source == "board") {
                    feed_url = "https://www.pinterest.com/"+ id + ".rss";
                } else {
                    feed_url = "https://www.pinterest.com/" + id + "/feed.rss";
                }
            }
            XmlDocument feedDoc = new XmlDocument();
            XmlReader reader = XmlReader.Create(feed_url);
            feedDoc.Load(reader);


            var feedTitle = feedDoc.SelectSingleNode("//channel/title").InnerText;
            var items = feedDoc.SelectNodes("//channel/item");

            dynamic displayItems = new ExpandoObject();
            displayItems.item = new ArrayList();
            displayItems.feedUrl = feed_url;

            foreach (XmlNode item in items) {
                dynamic displayItem = new ExpandoObject();
                displayItem.feedTitle = feedTitle;
                displayItem.title = item.SelectSingleNode("title") != null ? item.SelectSingleNode("title").InnerText : "feed error";
                displayItem.description = item.SelectSingleNode("description") != null ? item.SelectSingleNode("description").InnerText : "feed error";
                var findImageRegEx = @".* src=\" + "\"" + @"(?<src>.*)\" + "\"";
                Match imageMatch = Regex.Match(displayItem.description, findImageRegEx);
                displayItem.image = imageMatch.Success ? imageMatch.Groups["src"].Value : "";

                //from PHP
                //$clear = trim(preg_replace('/ +/', ' ', preg_replace('[^A-Za-z0-9����������]', ' ', urldecode(html_entity_decode(strip_tags($text))))));
                var text = item.SelectSingleNode("description") != null ? item.SelectSingleNode("description").InnerText : "feed error";
                var cleanedText = HttpUtility.HtmlDecode(text.Trim());
                Regex regHtml = new Regex("<[^>]*>");
                cleanedText = regHtml.Replace(cleanedText, "");

                displayItem.text = cleanedText;

                displayItem.standardimage = item.SelectSingleNode("standardimage")!=null ? item.SelectSingleNode("standardimage").InnerText : "";
                displayItem.link = item.SelectSingleNode("guid") != null ? item.SelectSingleNode("guid").InnerText : "";
                displayItem.publishedDate = item.SelectSingleNode("pubDate") != null ? item.SelectSingleNode("pubDate").InnerText : "";

                displayItems.item.Add(displayItem);
            }

            var json = JsonConvert.SerializeObject(displayItems);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(json);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return response;
        }


    }
}

/*

function dc_get_image($html)
{
	$doc = new DOMDocument();
	@$doc->loadHTML($html);
	$xpath = new DOMXPath($doc);
	$src = $xpath->evaluate("string(//img/@src)"); # "/images/image.jpg"
	return $src;
}
*/
