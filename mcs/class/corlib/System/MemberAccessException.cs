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
		const int Result = unchecked ((int)0x8013151A);

		public MemberAccessException ()
			: base (Locale.GetText ("Cannot access a class member."))
		{
			HResult = Result;
		}

		public MemberAccessException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected MemberAccessException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		
		public MemberAccessException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}
	}
}
