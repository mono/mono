//
// System.Web.Configuration.HttpCapabilitiesBase
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Novell, Inc (http://www.novell.com)
//

namespace System.Web.Configuration
{
	using System;
	using System.Collections;
	using System.Web;
	
	public class HttpCapabilitiesBase
	{
		Hashtable capabilities;

		public HttpCapabilitiesBase () { }

		public virtual string this [string key] {
			get { return capabilities [key] as string; }
		}

		public static HttpCapabilitiesBase GetConfigCapabilities (string configKey, HttpRequest request)
		{
			string ua = request.UserAgent;
			HttpBrowserCapabilities bcap = new HttpBrowserCapabilities ();
			bcap.capabilities = CapabilitiesLoader.GetCapabilities (ua);
			bcap.Init ();
			return bcap;
		}

		protected virtual void Init ()
		{
		}
	}
}

