
using System;
using System.Runtime.Serialization;

namespace System {
	public class MissingMemberException : MemberAccessException {
		
		public MissingMemberException ()
			: base ("A missing member exception has occurred.")
		{
		}

		public MissingMemberException (string message)
			: base (message)
		{
		}

		protected MissingMemberException (SerializationInfo info, StreamingContext context)
			: base (info, context) {
		}
		
		public MissingMemberException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
