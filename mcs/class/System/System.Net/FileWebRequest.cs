//
// System.Net.FileWebRequest
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
	public class FileWebRequest : WebRequest, ISerializable
	{
		private Uri uri;
		
		// Constructors
		
		internal FileWebRequest (Uri uri) 
		{ 
			this.uri = uri;
		}		
		
		protected FileWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
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