//
// System.Runtime.Serialization/SerializationException.cd
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Runtime.Serialization {

	[Serializable]
	public class SerializationException : SystemException {
		// Constructors
		public SerializationException ()
			: base ("An error occurred during (de)serialization")
		{
		}

		public SerializationException (string message)
			: base (message)
		{
		}

		public SerializationException (string message, Exception inner)
			: base (message, inner)
		{
		}


	}
}
