//	
// System.FieldAccessException.cs
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
	public class FieldAccessException : MemberAccessException
	{
		const int Result = unchecked ((int)0x80131507);

		// Constructors
		public FieldAccessException ()
			: base (Locale.GetText ("Attempt to access a private/protected field failed."))
		{
			HResult = Result;
		}

		public FieldAccessException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected FieldAccessException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public FieldAccessException (string message, Exception innerException)
			:base (message, innerException)
		{
			HResult = Result;
		}
	}
}
