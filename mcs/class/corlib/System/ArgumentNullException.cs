//
// System.ArgumentNullException.cs
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
	public class ArgumentNullException : ArgumentException
	{
		const int Result = unchecked ((int)0x80004003);

		// Constructors
		public ArgumentNullException ()
			: base (Locale.GetText ("Argument cannot be null."))
		{
			HResult = Result;
		}

		public ArgumentNullException (string paramName)
			: base (Locale.GetText ("Argument cannot be null."), paramName)
		{
			HResult = Result;
		}

		public ArgumentNullException (string paramName, string message)
			: base (message, paramName)
		{
			HResult = Result;
		}

		protected ArgumentNullException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
