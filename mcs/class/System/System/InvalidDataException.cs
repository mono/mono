//
// System.InvalidDataException.cs
//
// Authors:
//   Christopher James Lahey <clahey@ximian.com>
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2004 Novell, Inc.  http://www.novell.com
//

#if NET_2_0

using System.Globalization;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class InvalidDataException : SystemException
	{
		const int Result = unchecked ((int)0x80131503);

		// Constructors
		public InvalidDataException ()
			: base (Locale.GetText ("Invalid data format."))
		{
			HResult = Result;
		}

		public InvalidDataException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public InvalidDataException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		protected InvalidDataException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}

#endif
