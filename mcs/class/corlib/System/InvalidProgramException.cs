//
// System.InvalidProgramException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	[Serializable]
	public sealed class InvalidProgramException : SystemException
	{
		// Constructors
		public InvalidProgramException ()
			: base (Locale.GetText ("Metadata is invalid."))
		{
		}

		public InvalidProgramException (string message)
			: base (message)
		{
		}

		public InvalidProgramException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
