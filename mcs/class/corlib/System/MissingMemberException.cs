
using System;
using System.Runtime.Serialization;
using System.Globalization;

namespace System {
	public class MissingMemberException : MemberAccessException {
		
		public MissingMemberException ()
			: base (Locale.GetText ("A missing member exception has occurred."))
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
