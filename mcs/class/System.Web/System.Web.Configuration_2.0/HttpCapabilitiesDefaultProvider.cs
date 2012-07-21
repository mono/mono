using System;
using System.Collections;


namespace System.Web.Configuration
{
	public class HttpCapabilitiesDefaultProvider : HttpCapabilitiesProvider
	{
		public TimeSpan CacheTime { get; set; }
		public Type ResultType { get; set; }
		public int UserAgentCacheKeyLength { get; set; }
		
		
		public HttpCapabilitiesDefaultProvider()
		{
			UserAgentCacheKeyLength = 64;
		}
		
		
		public HttpCapabilitiesDefaultProvider(HttpCapabilitiesDefaultProvider parent)
		{
			CacheTime = parent.CacheTime;
			ResultType = parent.ResultType;
			UserAgentCacheKeyLength = parent.UserAgentCacheKeyLength;
		}
		
		
		public void AddDependency(string variable)
		{
			throw new NotImplementedException();
		}
		
		
		public virtual void AddRuleList(ArrayList ruleList)
		{
			throw new NotImplementedException();
		}
		
		
		public override HttpBrowserCapabilities GetBrowserCapabilities(HttpRequest request)
		{
			HttpBrowserCapabilities bcap = new HttpBrowserCapabilities();
			bcap.capabilities = HttpCapabilitiesBase.GetConfigCapabilities(null, request).Capabilities;

			return bcap;
		}
	}
}
