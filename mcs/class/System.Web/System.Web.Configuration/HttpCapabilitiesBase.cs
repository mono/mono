//
// System.Web.Configuration.HttpCapabilitiesBase
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2003,2004,2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Security.Permissions;

namespace System.Web.Configuration
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_2_0
	public partial class HttpCapabilitiesBase
#else
	public class HttpCapabilitiesBase
#endif
	{
		internal IDictionary capabilities;

		public HttpCapabilitiesBase () { }

		public virtual string this [string key]
		{
			get { return capabilities [key] as string; }
		}

		internal static string GetUserAgentForDetection (HttpRequest request)
		{
			string ua = null;

#if NET_2_0
			ua = null;
			if (request.Context.CurrentHandler is System.Web.UI.Page)
				ua = ((System.Web.UI.Page) request.Context.CurrentHandler).ClientTarget;
			
			if (String.IsNullOrEmpty (ua)) {
				ua = request.ClientTarget;

				if (String.IsNullOrEmpty (ua))
					ua = request.UserAgent;
			}
#else
			ua = request.ClientTarget;
			if (ua == null || ua.Length == 0)
				ua = request.UserAgent;
#endif
			return ua;
		}
		static HttpBrowserCapabilities GetHttpBrowserCapabilitiesFromBrowscapini(string ua)
		{
			HttpBrowserCapabilities bcap = new HttpBrowserCapabilities();
			bcap.capabilities = CapabilitiesLoader.GetCapabilities (ua);
			return bcap;
		}
		
		public static HttpCapabilitiesBase GetConfigCapabilities (string configKey, HttpRequest request)
		{
			string ua = GetUserAgentForDetection (request);

			HttpBrowserCapabilities bcap = GetHttpBrowserCapabilitiesFromBrowscapini(ua);
#if NET_2_0
			GetConfigCapabilities_called = true;
			if (HttpApplicationFactory.AppBrowsersFiles.Length > 0)
				bcap = HttpApplicationFactory.CapabilitiesProcessor.Process(request, bcap.Capabilities);
#endif
			bcap.useragent = ua;
			bcap.Init ();
			return bcap;
		}

#if NET_2_0
		// Used by unit tests to determine whether GetConfigCapabilities was called.
		static internal bool GetConfigCapabilities_called;
#endif
		protected virtual void Init ()
		{
		}
	}
}


