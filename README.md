UmbracoSocialPluginAdapters adds Facebook and Twitter adapters for the [jQuery Social Stream Plugin](http://www.designchemical.com/blog/index.php/premium-jquery-plugins/jquery-social-stream-plugin/) to your site!
    
Get your [Umbraco package here!](https://our.umbraco.org/projects/website-utilities/umbracosocialpluginadapters/)
	
The following appSettings need to be defined for the feeds to work:

- SocialPlugin_FacebookAppId
- SocialPlugin_FacebookAppSecret
- SocialPlugin_TwitterConsumerKey
- SocialPlugin_TwitterConsumerSecret

It is also necesary to set the correct URLs in the feed configuration in JavaScript, e.g.:

```	
twitter: {
	id: 'some_handle',
	url: '/umbraco/UmbracoSocialPluginAdapters/Twitter/Get'
},
```
