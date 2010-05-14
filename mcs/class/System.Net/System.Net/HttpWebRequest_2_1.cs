//
// System.Net.HttpWebRequest (for 2.1 profile)
//
// Authors:
//	Atsushi Enomoto  <atsushi@ximian.com>
//  Jb Evain  <jbevain@novell.com>
//
// Copyright (C) 2007, 2009-2010 Novell, Inc (http://www.novell.com)
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

using System.IO;

namespace System.Net { 

	// note: the NotImplementedException are needed to match MS implementation

	// note: MS documents a lot of thing for this type but, in truth, all happens
	// in a type that derive from HttpWebRequest. In Moonlight case this is either
	// * BrowserHttpWebRequest (browser stack) located in System.Windows.Browser.dll; or
	// * System.Net.Browser.ClientHttpWebRequest (client stack) located in System.Windows.dll

	public abstract class HttpWebRequest : WebRequest {

		private WebHeaderCollection headers;

		protected HttpWebRequest ()
		{
		}

		public string Accept {
			get { return Headers [HttpRequestHeader.Accept]; }
			// this header cannot be set directly inside the collection (hence the helper)
			set { Headers.SetHeader ("accept", value); }
		}

		public virtual bool AllowReadStreamBuffering {
			get { throw NotImplemented (); }
			set { throw NotImplemented (); }
		}

		// new in SL4 RC
		public virtual bool AllowWriteStreamBuffering {
			get { throw NotImplemented (); }
			set { throw NotImplemented (); }
		}

		public override string ContentType {
			get { return Headers [HttpRequestHeader.ContentType]; }
			// this header cannot be set directly inside the collection (hence the helper)
			set { Headers.SetHeader ("content-type", value); }
		}

		public virtual bool HaveResponse {
			get { throw NotImplemented (); }
		}

		public override WebHeaderCollection Headers {
			get {
				if (headers == null)
					headers = new WebHeaderCollection (true);
				return headers;
			}
			set {
				// note: this is not a field assignment but a copy (see unit tests)
				// make sure everything we're supplied is valid...
				string[] keys = value.AllKeys;
				foreach (string header in keys) {
					// anything bad will throw
					WebHeaderCollection.ValidateHeader (header);
				}
				// ... before making those values our own
				Headers.Clear ();
				foreach (string header in keys) {
					headers [header] = value [header];
				}
			}
		}

		public virtual CookieContainer CookieContainer {
			get { throw NotImplemented (); }
			set { throw NotImplemented (); }
		}

		public override string Method {
			get { throw NotImplemented (); }
			set { throw NotImplemented (); }
		}

		public override Uri RequestUri {
			get { throw NotImplemented (); }
		}

		// new in SL4 RC
		public virtual bool SupportsCookieContainer {
			get { return false; }
		}

		public override void Abort ()
		{
			throw NotImplemented ();
		}

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state)
		{
			throw NotImplemented ();
		}

		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			throw NotImplemented ();
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			throw NotImplemented ();
		}

		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			throw NotImplemented ();
		}

		static Exception NotImplemented ()
		{
			// a bit less IL and hide the "normal" NotImplementedException from corcompare-like tools
			return new NotImplementedException ();
		}
	}
}

#endif

