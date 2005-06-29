//
// XmlCachingResolver.cs
//
// Author:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	
// (C) 2003 Ben Maurer
//

using System;
using System.Xml;
using System.Net;
using System.IO;

namespace Mono.Xml {
	public class XmlCachingResolver : XmlUrlResolver {
		static string tmpFolder;
		
		static XmlCachingResolver ()
		{
			tmpFolder = Path.Combine (Path.GetTempPath (), "XmlCachingResolver_Cache");
			Directory.CreateDirectory (tmpFolder);
		}
		#region XmlResolver impl
		ICredentials credentials;
		public override ICredentials Credentials
		{
			set { credentials = value; }
		}
		
		public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			if (ofObjectToReturn == null || ofObjectToReturn == typeof (Stream))
				return GetStream (absoluteUri);
			else
				throw new XmlException ("Unsupported class type: " + ofObjectToReturn);
		}
		
		Stream GetStream (Uri uri)
		{
			// We can handle file:// without all the excess System.Net stuff
			if (uri.Scheme == "file")
				return File.OpenRead (uri.LocalPath);
			
			else {
				WebRequest req = WebRequest.Create (uri);
				if (credentials != null)
					req.Credentials = credentials;
				
				if (req is HttpWebRequest) {
					string url = uri.ToString ();
					
					if (File.Exists (GetCachedPath (url))) {
						// The file is in the cache, lets make sure it is up to date
						HttpWebRequest hreq = req as HttpWebRequest;
						// MS has a bug in their .net that makes 3xx errors (such as NotModified, 304)
						// throw when this is *TRUE* even though their docs say it will throw when *FALSE*
						hreq.AllowAutoRedirect = false;
						hreq.IfModifiedSince = File.GetLastWriteTime (GetCachedPath (url));
						HttpWebResponse hresp = hreq.GetResponse () as HttpWebResponse;
	
						if (hresp.StatusCode != HttpStatusCode.NotModified)
							using (Stream s = hresp.GetResponseStream ())
								AddToCache (url, s);
						
						return GetFromCache (url);
						
					} else {
						// The file has not been cached yet, so lets just get it
						// and add it there.
						using (Stream s = req.GetResponse ().GetResponseStream ())
							AddToCache (url, s);
						return GetFromCache (url);
					}
				} else // Ok, its not a http request, we dont know how to cache this
					return req.GetResponse ().GetResponseStream ();
			}
		}
		#endregion
		
		#region Caching
		
		static void AddToCache (string url, Stream data)
		{
			const int cbBuff = 8192;
			int cb = 0;
			byte [] buff = new byte [cbBuff];
			
			using (FileStream fs = File.Create (GetCachedPath (url))) {
				do {
					cb = data.Read (buff, 0, cbBuff);
					fs.Write (buff, 0, cb);
				} while (cb > 0) ;
			}
		}
		
		static string GetCachedPath (string url)
		{
			// EncodeLocalName will take out all things that would
			// be bad to have in the file system
			return Path.Combine (tmpFolder, XmlConvert.EncodeLocalName (url));
		}
		
		static Stream GetFromCache (string url)
		{
			return File.OpenRead (GetCachedPath (url));
		}
		
		#endregion
		
		// utility method to make reading from this easier.
		public XmlReader GetXmlReader (string url)
		{
			Uri uri = ResolveUri (null, url);
			Stream stream = (Stream)GetEntity (uri, null, typeof (Stream));
			XmlTextReader ret = new XmlTextReader (url, stream);
			ret.XmlResolver = this;
			return ret;
		}
	}
}