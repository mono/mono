//
// System.Net.HttpWebResponse
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.IO;
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public class HttpWebResponse : WebResponse, ISerializable, IDisposable
	{
		private Uri uri;
		private WebHeaderCollection webHeaders;
		
		// Constructors
		
		protected HttpWebResponse (Uri uri) 
		{ 
			this.uri = uri;
		}
		
		protected HttpWebResponse (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}
		
		// Properties
		
		public override long ContentLength {		
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public override string ContentType {		
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public override WebHeaderCollection Headers {		
			get { return webHeaders; }
		}
		
		public override Uri ResponseUri {		
			get { return uri; }
		}		

		// Methods
		
		public override void Close()
		{
			throw new NotSupportedException ();
		}
		
		public override Stream GetResponseStream()
		{
			throw new NotSupportedException ();
		}
		
		void IDisposable.Dispose()
		{
			Close ();
		}
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}		
	}
}