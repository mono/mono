//
// System.UriFormatException.cs
//
// Author:
//   Scott Sanders (scott@stonecobra.com)
//
// (C) 2001 Scott Sanders
//

using System.Runtime.Serialization;

namespace System {

	public class UriFormatException : Exception {

		// Constructors
		public UriFormatException ()
			: base ("Invalid URI format")
		{
		}

		public UriFormatException (string message)
			: base (message)
		{
		}

		protected UriFormatException( SerializationInfo info, StreamingContext context)
			: base ("UriFormatException: Please implement me")
		{
			//TODO - Implement me...  The Beta2 docs say nothing about what this method does
		}
	}
}
