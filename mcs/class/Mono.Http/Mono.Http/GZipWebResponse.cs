//
// Mono.Http.GZipHttpWebResponse
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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
			if (stream != null)
				return;

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
		}		

		public override void Close ()
		{
			response.Close ();
		}
		
		void IDisposable.Dispose ()
		{
			response.Close ();
		}
	}	
}

