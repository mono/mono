//
// Mono.Http.GZipHttpWebResponse
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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

using System;
using System.IO;
using System.Runtime.Serialization;
using ICSharpCode.SharpZipLib.GZip;

namespace System.Net 
{
	[Serializable]
	public class GZipWebResponse : WebResponse, ISerializable, IDisposable
	{
		WebResponse response;
		bool compressed;
		[NonSerialized] Stream stream;
		[NonSerialized] long gzipLength;

		internal GZipWebResponse (WebResponse response, bool compressed)
		{
			this.response = response;
			this.compressed = compressed;
		}

		protected GZipWebResponse (SerializationInfo info, StreamingContext context)
		{
			response = (WebResponse) info.GetValue ("response", typeof (WebResponse));
			compressed = info.GetBoolean ("compressed");
		}
		
		public override long ContentLength {		
			get {
				SetStream ();
				if (compressed)
					return gzipLength;

				return response.ContentLength;
			}
		}
		
		public override string ContentType {
			get { return response.ContentType; }
		}
		
		public override WebHeaderCollection Headers {
			get { return response.Headers; }
		}
		
		public override Uri ResponseUri {		
			get { return response.ResponseUri; }
		}		
		
		public WebResponse RealResponse {
			get { return response; }
		}

		public bool IsCompressed {
			get { return compressed; }
		}

		public override Stream GetResponseStream ()
		{
			SetStream ();
			return stream;
		}
		
		void SetStream ()
		{
			lock (this) {
				if (stream != null)
					return;

				Stream st = response.GetResponseStream ();
				if (!compressed) {
					stream = st;
				} else {
					stream = new GZipInputStream (st);
					gzipLength = stream.Length;
				}
			}
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("response", response);
			info.AddValue ("compressed", compressed);
		}		

		public override void Close ()
		{
			((IDisposable) this).Dispose ();
		}
		
		void IDisposable.Dispose ()
		{
			if (stream != null) {
				stream.Close ();
				stream = null;
			}

			if (response != null) {
				response.Close ();
				response = null;
			}
		}
	}	
}

