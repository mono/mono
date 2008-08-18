//
// System.Net.WebRequest (for 2.1 profile)
//
// Authors:
//	Jb Evain  <jbevain@novell.com>
//
// (c) 2008 Novell, Inc. (http://www.novell.com)
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
using System.IO;
using System.Reflection;

namespace System.Net {

	public abstract class WebRequest {

		static Type browser_http_request;

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

		public static WebRequest Create (Uri uri)
		{
			if (uri.IsAbsoluteUri && !uri.Scheme.StartsWith ("http"))
				throw new NotSupportedException (string.Format ("Scheme {0} not supported", uri.Scheme));

			return CreateBrowserHttpRequest (uri);
		}

		static WebRequest CreateBrowserHttpRequest (Uri uri)
		{
			if (browser_http_request == null)
				browser_http_request = GetBrowserHttpFromMoonlight ();

			return (WebRequest) Activator.CreateInstance (browser_http_request, new object [] { uri });
		}

		static Type GetBrowserHttpFromMoonlight ()
		{
			var assembly = Assembly.Load ("System.Windows.Browser, Version=2.0.5.0, Culture=Neutral, PublicKeyToken=7cec85d7bea7798e");
			if (assembly == null)
				throw new InvalidOperationException ("Can not load System.Windows.Browser");

			var type = assembly.GetType ("System.Windows.Browser.Net.BrowserHttpWebRequest");
			if (type == null)
				throw new InvalidOperationException ("Can not get BrowserHttpWebRequest");

			return type;
		}

		public static bool RegisterPrefix (string prefix, IWebRequestCreate creator)
		{
			throw new NotSupportedException ();
		}

		internal void SetupProgressDelegate (Delegate progress_delegate)
		{
			if (browser_http_request == null)
				browser_http_request = GetBrowserHttpFromMoonlight ();

			this.GetType ().GetField ("progress_delegate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (this, progress_delegate);
		}
		
	}
}

#endif
