//
// System.ArgumentNullException.cs
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
	public class ArgumentNullException : ArgumentException
	{
		// Constructors
		public ArgumentNullException ()
			: base (Locale.GetText ("Argument cannot be null."))
		{
		}

		public ArgumentNullException (string paramName)
			: base (Locale.GetText ("Argument cannot be null."), paramName)
		{
		}

		public ArgumentNullException (string paramName, string message)
			: base (message, paramName)
		{
		}

		protected ArgumentNullException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
