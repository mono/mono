//
// System.NullReferenceException.cs
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
	public class NullReferenceException : SystemException
	{
		const int Result = unchecked ((int)0x80004003);

		// Constructors
		public NullReferenceException ()
			: base (Locale.GetText ("A null value was found where an object instance was required."))
		{
			HResult = Result;
		}

		public NullReferenceException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public NullReferenceException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		protected NullReferenceException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
