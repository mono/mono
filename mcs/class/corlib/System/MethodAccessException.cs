//	
// System.MethodAccessException.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class MethodAccessException : MemberAccessException
	{
		const int Result = unchecked ((int)0x80131510);

		// Constructors
		public MethodAccessException ()
			: base (Locale.GetText ("Attempt to access a private/protected method failed."))
		{
			HResult = Result;
		}

		public MethodAccessException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected MethodAccessException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public MethodAccessException (string message, Exception innerException)
			:base (message, innerException)
		{
			HResult = Result;
		}
	}
}
