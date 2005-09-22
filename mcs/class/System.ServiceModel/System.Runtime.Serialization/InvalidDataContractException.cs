using System;
using System.Xml;

namespace System.Runtime.Serialization
{
	[Serializable]
	public class InvalidDataContractException : SerializationException
	{
		public InvalidDataContractException ()
			: base ()
		{
		}

		public InvalidDataContractException (string message)
			: base (message)
		{
		}

		public InvalidDataContractException (SerializationInfo info,
			StreamingContext context)
			: base (info, context)
		{
		}

		public InvalidDataContractException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
