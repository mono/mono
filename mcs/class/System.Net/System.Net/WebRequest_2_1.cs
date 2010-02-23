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

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace System.Net {

	public abstract class WebRequest {

		static IWebRequestCreate default_creator;
		static Dictionary<string, IWebRequestCreate> registred_prefixes;

		public abstract string ContentType { get; set; }
		public abstract WebHeaderCollection Headers { get; set; }
		public abstract string Method { get; set; }
		public abstract Uri RequestUri { get; }

		// custom registered prefixes return null (unless they override this)
		public virtual IWebRequestCreate CreatorInstance { 
			get { return null; }
		}

		public virtual ICredentials Credentials {
			get { throw NotImplemented (); }
			set { throw NotImplemented (); }
		}

		static WebRequest ()
		{
			registred_prefixes = new Dictionary<string, IWebRequestCreate> (StringComparer.OrdinalIgnoreCase);
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
				throw new InvalidOperationException ("Uri is not absolute.");

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

		internal static void RegisterDefaultStack (IWebRequestCreate creator)
		{
			default_creator = creator;
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

		internal void SetupProgressDelegate (Action<long,long> progress)
		{
			FieldInfo fi = GetType ().GetField ("progress", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fi != null)
				fi.SetValue (this, progress);
		}

		static Exception NotImplemented ()
		{
			// hide the "normal" NotImplementedException from corcompare-like tools
			return new NotImplementedException ();
		}
	}
}

#endif
