//
// System.FormatException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class FormatException : SystemException
	{
		const int Result = unchecked ((int)0x80131537);

		// Constructors
		public FormatException ()
			: base (Locale.GetText ("Invalid format."))
		{
			HResult = Result;
		}

		public FormatException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public FormatException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		protected FormatException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
