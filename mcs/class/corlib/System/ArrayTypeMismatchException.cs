//
// System.ArrayTypeMismatchException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class ArrayTypeMismatchException : SystemException
	{
		// Constructors
		public ArrayTypeMismatchException ()
			: base (Locale.GetText ("Source array type cannot be assigned to destination array type."))
		{
		}

		public ArrayTypeMismatchException (string message)
			: base (message)
		{
		}

		public ArrayTypeMismatchException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		protected ArrayTypeMismatchException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
