//
// System.Net.HttpWebRequest (for 2.1 profile)
//
// Authors:
//	Atsushi Enomoto  <atsushi@ximian.com>
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

using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace System.Net 
{

	public abstract class HttpWebRequest
	{
		public HttpWebRequest (Uri uri)
		{
		}

		public abstract void Abort();
		public abstract void AddRange (int range);
		public abstract void AddRange (int from, int to);
		public abstract void AddRange (string rangeSpecifier, int range);
		public abstract void AddRange (string rangeSpecifier, int from, int to);
		public abstract IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state);
		public abstract IAsyncResult BeginGetResponse (AsyncCallback callback, object state);
		public abstract Stream EndGetRequestStream (IAsyncResult asyncResult);
		public abstract HttpWebResponse EndGetResponse (IAsyncResult asyncResult);
		public abstract Stream GetRequestStream ();
		public abstract HttpWebResponse GetResponse ();

		public abstract string Accept { get; set; }
		public abstract Uri Address { get; }
		public abstract bool AllowAutoRedirect { get; set; }
		public abstract bool AllowWriteStreamBuffering { get; set; }
		public abstract DecompressionMethods AutomaticDecompression { get; set; }
		public abstract string Connection { get; set; }
		public abstract long ContentLength { get; set; }
		public abstract string ContentType { get; set; }
		public abstract string Expect { get; set; }
		public abstract bool HaveResponse { get; }
		public abstract WebHeaderCollection Headers { get; set; }
		public abstract bool KeepAlive { get; set; }
		public abstract string MediaType { get; set; }
		public abstract string Method { get; set; }
		public abstract bool Pipelined { get; set; }
		public abstract bool PreAuthenticate { get; set; }

		public abstract int ReadWriteTimeout { get; set; }
		public abstract string Referer { get; set; }
		public abstract Uri RequestUri { get; }
		public abstract bool SendChunked { get; set; }
		public abstract int Timeout { get; set; }
		public abstract string TransferEncoding { get; set; }
		public abstract string UserAgent { get; set; }

		Version version;
		public virtual Version ProtocolVersion {
			get { return version; }
			set { 
				if (value != HttpVersion.Version10 && value != HttpVersion.Version11)
					throw new ArgumentException ("value");

				version = value; 
			}
		}
	}
}

#endif
