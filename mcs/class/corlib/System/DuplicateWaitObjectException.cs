//
// System.DuplicateWaitObjectException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class DuplicateWaitObjectException : ArgumentException
	{
		// Constructors
		public DuplicateWaitObjectException ()
			: base (Locale.GetText ("Duplicate objects in argument."))
		{
		}

		public DuplicateWaitObjectException (string parameterName)
			: base (Locale.GetText ("Duplicate objects in argument."), parameterName)
		{
		}

		public DuplicateWaitObjectException (string parameterName, string message)
			: base (message, parameterName)
		{
		}

		protected DuplicateWaitObjectException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
