//
// System.ApplicationException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class ApplicationException : Exception
	{
		const int Result = unchecked ((int)0x80131600);

		// Constructors
		public ApplicationException ()
			: base (Locale.GetText ("An application exception has occurred."))
		{
			HResult = Result;
		}

		public ApplicationException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public ApplicationException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		protected ApplicationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
