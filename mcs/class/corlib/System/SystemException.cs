//
// System.SystemException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;
using System;
using System.Globalization;

namespace System {

	[Serializable]
	public class SystemException : Exception {
		// Constructors
		public SystemException ()
			: base (Locale.GetText ("A system exception has occurred."))
		{
		}

		public SystemException (string message)
			: base (message)
		{
		}

		protected SystemException(SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		
		public SystemException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
