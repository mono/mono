//
// System.Net.WebException.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public class WebException : InvalidOperationException, ISerializable
	{
		private WebResponse response;
		private WebExceptionStatus status = WebExceptionStatus.RequestCanceled;
		

		// Constructors
		
		public WebException () : base ()
		{
		}
		
		public WebException (string message) : base (message)
		{
		}

		protected WebException (SerializationInfo serializationInfo,
		   			StreamingContext streamingContext)
			: base (serializationInfo, streamingContext)
		{
		}

		public WebException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		public WebException (string message, WebExceptionStatus status)
			: base (message)
		{
			this.status = status;
		}

		public WebException(string message, 
				    Exception innerException,
		   		    WebExceptionStatus status, 
		   		    WebResponse response)
			: base (message, innerException)		   		    
		{
			this.status = status;
			this.response = response;
		}
		
		// Properties
		
		public WebResponse Response {
			get { return this.response; }
		}
		
		public WebExceptionStatus Status {
			get { return this.status; }
		}
		
		// Methods
		
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}
	}
}
	
