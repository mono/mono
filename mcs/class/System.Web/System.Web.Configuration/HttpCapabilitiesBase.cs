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
	Hashtable capabilities;

	public HttpCapabilitiesBase ()
	{
		capabilities = new Hashtable ();
	}

	public virtual string this [string key]
	{
		get { return capabilities [key] as string; }
	}

	public static HttpCapabilitiesBase GetConfigCapabilities (string configKey, HttpRequest request)
	{
		throw new NotImplementedException ();
	}

	internal void SetCapabilities (Hashtable tbl)
	{
		capabilities = tbl;
		Init ();
	}
	
	protected virtual void Init ()
	{
	}
	
}
}

