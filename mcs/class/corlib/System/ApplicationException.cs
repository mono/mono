//
// System.ApplicationException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Miguel de Icaza (miguel@ximian.com) 
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;
using System.Reflection;
using System.Globalization;

namespace System {

	public class ApplicationException : Exception {
		// Constructors
		public ApplicationException ()
			: base (Locale.GetText ("An application exception has occurred."))
		{
		}

		public ApplicationException (string message)
			: base (message)
		{
		}

		public ApplicationException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected ApplicationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
