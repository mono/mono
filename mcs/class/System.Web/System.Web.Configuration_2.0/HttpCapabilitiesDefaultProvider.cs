//
// System.Web.Configuration.HttpCapabilitiesDefaultProvider
//
// Authors:
//	Mike Morano (mmorano@mikeandwan.us)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
