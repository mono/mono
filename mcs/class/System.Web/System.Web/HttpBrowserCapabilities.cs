// 
// System.Web.HttpBrowserCapabilities
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
using System;
using System.Web.Configuration;

namespace System.Web {
public class HttpBrowserCapabilities : HttpCapabilitiesBase
{
	public HttpBrowserCapabilities()
	{
	}

	public bool ActiveXControls {
		get { return false; }
	}

	[MonoTODO()]
	public bool AOL {
		get { return false; }
	}

	[MonoTODO()]
	public bool BackgroundSounds {
		get { return false; }
	}

	[MonoTODO()]
	public bool Beta {
		get { return false; }
	}

	[MonoTODO()]
	public string Browser {
		get { return "Not Implemented"; }
	}

	[MonoTODO()]
	public bool CDF {
		get { return false; }
	}

	[MonoTODO()]
	public Version ClrVersion {
		get { return new Version (0, 0); }
	}

	[MonoTODO()]
	public bool Cookies {
		get { return true; }
	}

	[MonoTODO()]
	public bool Crawler {
		get { return false; }
	}

	[MonoTODO()]
	public Version EcmaScriptVersion {
		get { return new Version (0, 0); }
	}

	[MonoTODO()]
	public bool Frames {
		get { return true; }
	}

	[MonoTODO()]
	public bool JavaApplets {
		get { return false; }
	}

	[MonoTODO()]
	public bool JavaScript {
		get { return true; }
	}

	[MonoTODO()]
	public int MajorVersion {
		get { return 0; }
	}

	[MonoTODO()]
	public double MinorVersion {
		get { return 0.0; }
	}

	[MonoTODO()]
	public Version MSDomVersion {
		get { return new Version (0, 0); }
	}

	[MonoTODO()]
	public string Platform {
		get { return "mono::"; }
	}

	[MonoTODO()]
	public bool Tables {
		get { return true; }
	}

	[MonoTODO()]
	public Type TagWriter {
		get { throw new NotImplementedException (); }
	}

	[MonoTODO()]
	public string Type {
		get { return "4"; }
	}

	[MonoTODO()]
	public bool VBScript {
		get { return false; }
	}

	[MonoTODO()]
	public string Version {
		get { return "4.0"; }
	}

	[MonoTODO()]
	public Version W3CDomVersion {
		get { return new Version (0, 0); }
	}

	[MonoTODO()]
	public bool Win16 {
		get { return false; }
	}

	[MonoTODO()]
	public bool Win32 {
		get { return true; }
	}

}
}
