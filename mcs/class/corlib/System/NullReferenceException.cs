//
// System.NullReferenceException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System {

	public class NullReferenceException : SystemException {
		// Constructors
		public NullReferenceException ()
			: base (Locale.GetText ("A null value was found where an object instance was required"))
		{
		}

		public NullReferenceException (string message)
			: base (message)
		{
		}

		public NullReferenceException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected NullReferenceException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
