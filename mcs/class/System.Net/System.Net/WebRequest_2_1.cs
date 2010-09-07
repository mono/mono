//
// System.Net.WebRequest (for 2.1 profile)
//
// Authors:
//	Jb Evain  <jbevain@novell.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008-2010 Novell, Inc (http://www.novell.com)
//

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

#if NET_2_1

using System.Collections.Generic;
using System.IO;

namespace System.Net {

	public abstract class WebRequest {

		const string SystemWindows = "System.Windows, PublicKey=00240000048000009400000006020000002400005253413100040000010001008D56C76F9E8649383049F383C44BE0EC204181822A6C31CF5EB7EF486944D032188EA1D3920763712CCB12D75FB77E9811149E6148E5D32FBAAB37611C1878DDC19E20EF135D0CB2CFF2BFEC3D115810C3D9069638FE4BE215DBF795861920E5AB6F7DB2E2CEEF136AC23D5DD2BF031700AEC232F6C6B1C785B4305C123B37AB";
		const string BrowserStack = "System.Net.Browser.BrowserHttpWebRequestCreator, " + SystemWindows;
		const string ClientStack = "System.Net.Browser.ClientHttpWebRequestCreator, " + SystemWindows;

		static IWebRequestCreate default_creator;
		static IWebRequestCreate browser_creator;
		static IWebRequestCreate client_creator;
		static Dictionary<string, IWebRequestCreate> registred_prefixes;

		internal Action<long,long> progress;

		public abstract string ContentType { get; set; }
		public abstract WebHeaderCollection Headers { get; set; }
		public abstract string Method { get; set; }
		public abstract Uri RequestUri { get; }

		public virtual long ContentLength {
			get { throw NotImplemented (); }
			set { throw NotImplemented (); }
		}

		// custom registered prefixes return null (unless they override this)
		public virtual IWebRequestCreate CreatorInstance { 
			get { return null; }
		}

		public virtual ICredentials Credentials {
			get { throw NotImplemented (); }
			set { throw NotImplemented (); }
		}

		public virtual bool UseDefaultCredentials {
			get { throw NotImplemented (); }
			set { throw NotImplemented (); }
		}

		static WebRequest ()
		{
			registred_prefixes = new Dictionary<string, IWebRequestCreate> (StringComparer.OrdinalIgnoreCase);
			browser_creator = (IWebRequestCreate) Activator.CreateInstance (Type.GetType (BrowserStack));
			client_creator = (IWebRequestCreate) Activator.CreateInstance (Type.GetType (ClientStack));
			default_creator = browser_creator;
		}

		protected WebRequest ()
		{
		}

		public abstract void Abort();
		public abstract IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state);
		public abstract IAsyncResult BeginGetResponse (AsyncCallback callback, object state);
		public abstract Stream EndGetRequestStream (IAsyncResult asyncResult);
		public abstract WebResponse EndGetResponse (IAsyncResult asyncResult);

		internal virtual IAsyncResult BeginGetResponse (AsyncCallback callback, object state, bool policy)
		{
			return BeginGetResponse (callback, state);
		}

		public static WebRequest Create (string requestUriString)
		{
			return Create (new Uri (requestUriString));
		}

		public static WebRequest Create (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			if (!uri.IsAbsoluteUri)
				throw new InvalidOperationException ("This operation is not supported for a relative URI.");

			IWebRequestCreate creator = null;

			// first we look if a domain is registred
			string scheme = uri.Scheme + Uri.SchemeDelimiter;
			string domain = scheme + uri.DnsSafeHost;
			if (!registred_prefixes.TryGetValue (domain, out creator)) {
				// next we look if the protocol is registred (the delimiter '://' is important)
				if (!registred_prefixes.TryGetValue (scheme, out creator)) {
					scheme = uri.Scheme; // without the delimiter
					// then we default to SL
					switch (scheme) {
					case "http":
					case "https":
						creator = default_creator;
						break;
					default:
						registred_prefixes.TryGetValue (scheme, out creator);
						break;
					}
				}
			}

			if (creator == null)
				throw new NotSupportedException (string.Format ("Scheme {0} not supported", scheme));

			return creator.Create (uri);
		}

		public static HttpWebRequest CreateHttp (string requestUriString)
		{
			return CreateHttp (new Uri (requestUriString));
		}

		public static HttpWebRequest CreateHttp (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			if (!uri.IsAbsoluteUri)
				throw new InvalidOperationException ("Uri is not absolute.");

			// we do not check the registred prefixes from CreateHttp and *always* use the client HTTP stack
			switch (uri.Scheme) {
			case "http":
			case "https":
				return (HttpWebRequest) client_creator.Create (uri);
			default:
				throw new NotSupportedException (string.Format ("Scheme {0} not supported", uri.Scheme));
			}
		}

		// We can register for
		// * a protocol (e.g. http) for all requests
		// * a protocol (e.g. https) for a domain
		// * a protocol (e.g. http) for a single request
		//
		// See "How to: Specify Browser or Client HTTP Handling" for more details
		// http://msdn.microsoft.com/en-us/library/dd920295%28VS.95%29.aspx
		public static bool RegisterPrefix (string prefix, IWebRequestCreate creator)
		{
			if (prefix == null)
				throw new ArgumentNullException ("prefix");
			if (creator == null)
				throw new ArgumentNullException ("creator");

			Uri uri;
			if (Uri.TryCreate (prefix, UriKind.Absolute, out uri)) {
				// if a valid URI is supplied then only register the scheme + domain
				prefix = uri.Scheme + Uri.SchemeDelimiter + uri.DnsSafeHost;
			}

			if (registred_prefixes.ContainsKey (prefix))
				return false;

			registred_prefixes.Add (prefix, creator);
			return true;
		}

		static Exception NotImplemented ()
		{
			// hide the "normal" NotImplementedException from corcompare-like tools
			return new NotImplementedException ();
		}
	}
}

#endif
