// System.Xml.XmlUrlResolver.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//	   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) Ximian, Inc.
//

using System.Net;
using System.IO;
using System.Text;
using Mono.Xml.Native;

namespace System.Xml
{
	public class XmlUrlResolver : XmlResolver
	{
		// Field
		ICredentials credential;
		WebClient webClientInternal;
		WebClient webClient {
			get {
				if (webClientInternal == null)
					webClientInternal = new WebClient ();
				return webClientInternal;
			}
		}

		// Constructor
		public XmlUrlResolver ()
			: base ()
		{
		}

		// Properties		
		public override ICredentials Credentials
		{
			set { credential = value; }
		}
		
		// Methods
		public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			if (ofObjectToReturn == null)
				ofObjectToReturn = typeof (Stream);
			if (ofObjectToReturn != typeof (Stream))
				throw new XmlException ("This object type is not supported.");

			if (absoluteUri.Scheme == "file") {
				if (absoluteUri.AbsolutePath == String.Empty)
					throw new ArgumentException ("uri must be absolute.", "absoluteUri");
				return new FileStream (UnescapeRelativeUriBody (absoluteUri.LocalPath), FileMode.Open, FileAccess.Read, FileShare.Read);
			}

			// (MS documentation says) parameter role isn't used yet.
			Stream s = null;
			using (s) {
				WebClient wc = new WebClient ();
				wc.Credentials = credential;
				byte [] data = wc.DownloadData (absoluteUri.ToString ());
				wc.Dispose ();
				return new MemoryStream (data, 0, data.Length);
			}
		}

		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			if (baseUri == null) {
				if (relativeUri == null)
					throw new NullReferenceException ("Either baseUri or relativeUri are required.");
				// Don't ignore such case that relativeUri is in fact absolute uri (e.g. ResolveUri (null, "http://foo.com")).
				if (relativeUri.StartsWith ("http:") ||
					relativeUri.StartsWith ("https:") ||
					relativeUri.StartsWith ("file:"))
					return new Uri (relativeUri);
				else
					// extraneous "/a" is required because current Uri stuff 
					// seems ignorant of difference between "." and "./". 
					// I'd be appleciate if it is fixed with better solution.
					return new Uri (Path.GetFullPath (relativeUri));
//					return new Uri (new Uri (Path.GetFullPath ("./a")), EscapeRelativeUriBody (relativeUri));
			}

			if (relativeUri == null)
				return baseUri;

			return new Uri (baseUri, EscapeRelativeUriBody (relativeUri));
		}

		private string EscapeRelativeUriBody (string src)
		{
			return src.Replace ("<", "%3C")
				.Replace (">", "%3E")
				.Replace ("#", "%23")
				.Replace ("%", "%25")
				.Replace ("\"", "%22");
		}

		private string UnescapeRelativeUriBody (string src)
		{
			return src.Replace ("%3C", "<")
				.Replace ("%3E", ">")
				.Replace ("%23", "#")
				.Replace ("%25", "%")
				.Replace ("%22", "\"");
		}
	}
}
