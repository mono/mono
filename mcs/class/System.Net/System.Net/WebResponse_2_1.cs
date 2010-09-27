//
// System.Net.HttpWebResponse (for 2.1 profile)
//
// Authors:
//	Jb Evain  <jbevain@novell.com>
//
// (c) 2007 Novell, Inc. (http://www.novell.com)
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

	public abstract class WebResponse : IDisposable {

		private WebHeaderCollection headers;

		public abstract long ContentLength { get; }
		public abstract string ContentType { get; }
		public abstract Uri ResponseUri { get; }

		public virtual WebHeaderCollection Headers {
			get {
				if (!SupportsHeaders)
					throw NotImplemented ();
				return headers;
			}
		}

		internal WebHeaderCollection InternalHeaders {
			get { return headers; }
			set { headers = value; }
		}

		public virtual bool SupportsHeaders {
			get { return false; }
		}

		protected WebResponse ()
		{
		}

		public abstract void Close ();
		public abstract Stream GetResponseStream ();

		void IDisposable.Dispose ()
		{
			Close ();
		}

		static Exception NotImplemented ()
		{
			// hide the "normal" NotImplementedException from corcompare-like tools
			return new NotImplementedException ();
		}
	}
}

#endif
