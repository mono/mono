using System;
using System.Runtime.Serialization;

namespace System {
	public class MemberAccessException : SystemException {
		
		public MemberAccessException ()
			: base ("A member access exception has occurred.")
		{
		}

		public MemberAccessException (string message)
			: base (message)
		{
		}

		protected MemberAccessException (SerializationInfo info, StreamingContext context)
			: base (info, context) {
		}
		
		public MemberAccessException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
