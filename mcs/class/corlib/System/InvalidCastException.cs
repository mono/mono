//
// System.InvalidCastException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class InvalidCastException : SystemException
	{
		const int Result = unchecked ((int)0x80004002);

		// Constructors
		public InvalidCastException ()
			: base (Locale.GetText ("Cannot cast from source type to destination type."))
		{
			HResult = Result;
		}

		public InvalidCastException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public InvalidCastException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		protected InvalidCastException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
