//
// Mono.Http.GZipWebRequestCreator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Net;

namespace Mono.Http
{
	public class GZipWebRequestCreator : IWebRequestCreate
	{
		public GZipWebRequestCreator ()
		{
		}

		public WebRequest Create (Uri uri)
		{
			string scheme = uri.Scheme;
			if (scheme != "gziphttp")
				throw new ArgumentException ("Must be gziphttp", "uri");

			Uri newuri = new Uri (uri.ToString ().Substring (4));
			WebRequest req = WebRequest.Create (newuri);
			return new GZipWebRequest (req);
		}
	}
}

