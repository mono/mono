//
// System.DuplicateWaitObjectException.cs
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
	public class DuplicateWaitObjectException : ArgumentException
	{
		const int Result = unchecked ((int)0x80131529);

		// Constructors
		public DuplicateWaitObjectException ()
			: base (Locale.GetText ("Duplicate objects in argument."))
		{
			HResult = Result;
		}

		public DuplicateWaitObjectException (string parameterName)
			: base (Locale.GetText ("Duplicate objects in argument."), parameterName)
		{
			HResult = Result;
		}

		public DuplicateWaitObjectException (string parameterName, string message)
			: base (message, parameterName)
		{
			HResult = Result;
		}

		protected DuplicateWaitObjectException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
