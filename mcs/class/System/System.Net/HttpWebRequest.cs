//
// System.Net.HttpWebRequest
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public class HttpWebRequest : WebRequest, ISerializable
	{
		private Uri uri;
		
		// Constructors
		
		internal HttpWebRequest (Uri uri) 
		{ 
			this.uri = uri;
		}		
		
		protected HttpWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			throw new NotSupportedException ();
		}
		
		// Properties
		


		// Methods
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}
	}
}