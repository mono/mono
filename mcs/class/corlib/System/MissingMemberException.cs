using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System {

	[Serializable]
	public class MissingMemberException : MemberAccessException {

	     // Fields
		protected string ClassName;
		protected string MemberName;
	     protected byte[] Signature;
		   

		// Constructors
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

		[MonoTODO]
		public MissingMemberException (string className, string memberName)
		{
		}

	     // Properties
		[MonoTODO]
		public override string Message
		{
			   get { return null; }
		}

		// Methods
		[MonoTODO]
		public override void GetObjectData (SerializationInfo info,
									 StreamingContext context)
		{
		}
		   
	}
}
