//
// System.SystemException.cs
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
	public class SystemException : Exception
	{
		const int Result = unchecked ((int)0x80131501);

		// Constructors
		public SystemException ()
			: base (Locale.GetText ("A system exception has occurred."))
		{
			HResult = Result;
		}

		public SystemException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected SystemException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public SystemException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}
	}
}
