//
// System.ApplicationException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Miguel de Icaza (miguel@ximian.com) 
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Reflection;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class ApplicationException : Exception
	{
		// Constructors
		public ApplicationException ()
			: base (Locale.GetText ("An application exception has occurred."))
		{
		}

		public ApplicationException (string message)
			: base (message)
		{
		}

		public ApplicationException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		protected ApplicationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
