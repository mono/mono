//
// System.Net.WebResponse
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
	public abstract class WebResponse : MarshalByRefObject, ISerializable, IDisposable
	{
		// Constructors
		
		protected WebResponse () { }
		
		protected WebResponse (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}
		
		// Properties
		
		public virtual long ContentLength {		
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual string ContentType {		
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual WebHeaderCollection Headers {		
			get { throw new NotSupportedException (); }
		}
		
		public virtual Uri ResponseUri {		
			get { throw new NotSupportedException (); }
		}		

		// Methods
		
		public virtual void Close()
		{
			throw new NotSupportedException ();
		}
		
		public virtual Stream GetResponseStream()
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
