//
// System.Net.WebRequest
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
	public abstract class WebRequest : MarshalByRefObject, ISerializable
	{
		// Constructors
		
		protected WebRequest () {}		
		
		protected WebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}
		
		// Properties
		
		public virtual string ConnectionGroupName { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual long ContentLength { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual string ContentType { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual ICredentials Credentials { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual WebHeaderCollection Headers { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual string Method { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual bool PreAuthenticate { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual IWebProxy Proxy { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual Uri RequestUri { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual int Timeout { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		// Methods
		
		public virtual void Abort()
		{
			throw new NotSupportedException ();
		}
		
		public virtual IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			throw new NotSupportedException ();
		}
		
		public virtual IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			throw new NotSupportedException ();
		}

		public static WebRequest Create (string requestUriString) 
		{
			if (requestUriString == null)
				throw new ArgumentNullException ("requestUriString");
			return Create (new Uri (requestUriString));
		}
				
		[MonoTODO]
		public static WebRequest Create (Uri requestUri) 
		{
			if (requestUri == null)
				throw new ArgumentNullException ("requestUri");
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static WebRequest CreateDefault (Uri requestUri)
		{
			if (requestUri == null)
				throw new ArgumentNullException ("requestUri");
			throw new NotImplementedException ();			
		}

		public virtual Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			throw new NotSupportedException ();
		}
		
		public virtual WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			throw new NotSupportedException ();
		}
		
		public virtual Stream GetRequestStream()
		{
			throw new NotSupportedException ();
		}
		
		public virtual WebResponse GetResponse()
		{
			throw new NotSupportedException ();
		}
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}

		[MonoTODO]		
		public static bool RegisterPrefix (string prefix, IWebRequestCreate creator)
		{
			throw new NotImplementedException ();			
		}
	}
}