//
// System.Reflection.TargetException.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	public class TargetException : ApplicationException
	{
		public TargetException ()
			: base (Locale.GetText ("Unable to invoke an invalid target."))
		{
		}

		public TargetException (string message)
			: base (message)
		{
		}

		public TargetException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		protected TargetException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
