//
// System.ArrayTypeMismatchException.cs
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
	public class ArrayTypeMismatchException : SystemException
	{
		const int Result = unchecked ((int)0x80131503);

		// Constructors
		public ArrayTypeMismatchException ()
			: base (Locale.GetText ("Source array type cannot be assigned to destination array type."))
		{
			HResult = Result;
		}

		public ArrayTypeMismatchException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public ArrayTypeMismatchException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		protected ArrayTypeMismatchException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
