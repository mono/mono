//
// System.Web.Configuration.HttpCapabilitiesBase
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

namespace System.Web.Configuration {
	using System;
	using System.Collections;
	using System.Web;
	
public class HttpCapabilitiesBase
{
	Hashtable _capabilities;

	public HttpCapabilitiesBase ()
	{
		_capabilities = new Hashtable ();
	}

	public virtual string this [string key]
	{
		get { return _capabilities [key] as string; }
	}

	public static HttpCapabilitiesBase GetConfigCapabilities (string configKey, HttpRequest request)
	{
		throw new NotImplementedException ();
	}

	protected virtual void Init ()
	{
	}
	
}
}

