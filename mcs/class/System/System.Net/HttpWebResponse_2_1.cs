//
// System.Net.HttpWebResponse (for 2.1 profile)
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
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

namespace System.Net 
{
	public abstract class HttpWebResponse
	{
		public abstract void Close ();
		public abstract string GetResponseHeader (string headerName);
		public abstract Stream GetResponseStream ();
		public abstract string CharacterSet { get; }
		public abstract string ContentEncoding { get; }
		public abstract long ContentLength { get; }
		public abstract string ContentType { get; }
		public abstract WebHeaderCollection Headers { get; }
		public abstract DateTime LastModified { get; }
		public abstract string Method { get; }
		public abstract Version ProtocolVersion { get; }
		public abstract Uri ResponseUri { get; }
		public abstract string Server { get; }
		public abstract HttpStatusCode StatusCode { get; }
		public abstract string StatusDescription { get; }
	}

}
#endif
