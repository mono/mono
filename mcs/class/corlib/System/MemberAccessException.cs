//
// System.MemberAccessException.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class MemberAccessException : SystemException
	{
		public MemberAccessException ()
			: base (Locale.GetText ("Cannot access a class member."))
		{
		}

		public MemberAccessException (string message)
			: base (message)
		{
		}

		protected MemberAccessException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		
		public MemberAccessException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
