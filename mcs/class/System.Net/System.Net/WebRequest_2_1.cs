//
// System.Net.WebRequest (for 2.1 profile)
//
// Authors:
//	Jb Evain  <jbevain@novell.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008-2009 Novell, Inc (http://www.novell.com)
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

		static Type browser_http_request;
		static Dictionary<string,IWebRequestCreate> registred_prefixes = new Dictionary<string,IWebRequestCreate> ();

		public abstract string ContentType { get; set; }
		public abstract WebHeaderCollection Headers { get; set; }
		public abstract string Method { get; set; }
		public abstract Uri RequestUri { get; }

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

		public static WebRequest Create (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			if (!uri.IsAbsoluteUri)
				throw new InvalidOperationException ("Uri is not absolute.");

			switch (uri.Scheme) {
			case "http":
			case "https":
				// we don't use whatever has been registred but our own
				return CreateBrowserWebRequest (uri);
			default:
				IWebRequestCreate creator;
				if (registred_prefixes.TryGetValue (uri.Scheme, out creator)) {
					return creator.Create (uri);
				} else {
					throw new NotSupportedException (string.Format ("Scheme {0} not supported", uri.Scheme));
				}
			}
		}

		static WebRequest CreateBrowserWebRequest (Uri uri)
		{
			if (browser_http_request == null) {
				var assembly = Assembly.Load ("System.Windows.Browser, Version=2.0.5.0, Culture=Neutral, PublicKeyToken=7cec85d7bea7798e");
				if (assembly == null)
					throw new InvalidOperationException ("Can not load System.Windows.Browser");

				browser_http_request = assembly.GetType ("System.Windows.Browser.Net.BrowserHttpWebRequest");
				if (browser_http_request == null)
					throw new InvalidOperationException ("Can not get BrowserHttpWebRequest");
			}

			return (WebRequest) Activator.CreateInstance (browser_http_request, new object [] { uri });
		}

		public static bool RegisterPrefix (string prefix, IWebRequestCreate creator)
		{
			if (prefix == null)
				throw new ArgumentNullException ("prefix");
			if (creator == null)
				throw new ArgumentNullException ("creator");

			// LAMESPEC: according to doc registering http or https will fail. Actually this is not true
			// the registration works but the class being registred won't be used for http[s]
			prefix = prefix.ToLowerInvariant ();
			if (registred_prefixes.ContainsKey (prefix))
				return false;

			registred_prefixes.Add (prefix, creator);
			return true;
		}

		internal void SetupProgressDelegate (Delegate progress_delegate)
		{
			FieldInfo fi = GetType ().GetField ("progress_delegate", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fi != null)
				fi.SetValue (this, progress_delegate);
		}
	}
}

#endif
