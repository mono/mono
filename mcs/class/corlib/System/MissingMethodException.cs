using System;
using System.Runtime.Serialization;

namespace System {
	public class MissingMethodException : MissingMemberException {
		public MissingMethodException ()
			: base ("A missing method exception has occurred.")
		{
		}

		public MissingMethodException (string message)
			: base (message)
		{
		}

		protected MissingMethodException (SerializationInfo info, StreamingContext context)
			: base (info, context) {
		}
		
		public MissingMethodException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}


