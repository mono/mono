//
// System.Net.HttpWebRequest (for 2.1 profile)
//
// Authors:
//	Atsushi Enomoto  <atsushi@ximian.com>
//  Jb Evain  <jbevain@novell.com>
//
// Copyright (C) 2007, 2009 Novell, Inc (http://www.novell.com)
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

	public abstract class HttpWebRequest : WebRequest {

		private string accept;
		private string content_type;
		private WebHeaderCollection headers;

		protected HttpWebRequest ()
		{
		}

		public string Accept {
			get { return accept; }
			set {
				if (String.IsNullOrEmpty (value))
					accept = null;
				else
					accept = value;
			}
		}

		public virtual bool AllowReadStreamBuffering {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public override string ContentType {
			get { return content_type; }
			set {
				if (String.IsNullOrEmpty (value))
					content_type = null;
				else
					content_type = value;
			}
		}

		public virtual bool HaveResponse {
			get { throw new NotImplementedException (); }
		}

		public override WebHeaderCollection Headers {
			get { return headers; }
			set {
				if (value == null)
					throw new NullReferenceException ();
				headers = value;
			}
		}

		public override string Method {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public override Uri RequestUri {
			get { throw new NotImplementedException (); }
		}


		public override void Abort ()
		{
			throw new NotImplementedException ();
		}

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif

