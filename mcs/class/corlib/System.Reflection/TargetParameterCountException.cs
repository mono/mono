//
// System.Reflection.TargetParameterCountException.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;
using System.Globalization;

namespace System.Reflection
{
	[Serializable]
	public sealed class TargetParameterCountException : ApplicationException
	{
		public TargetParameterCountException ()
			: base (Locale.GetText ("Number of parameter does not match expected count."))
		{
		}

		public TargetParameterCountException (string message)
			: base (message)
		{
		}

		public TargetParameterCountException (string message, Exception inner)
			: base (message, inner)
		{
		}

		internal TargetParameterCountException (SerializationInfo info,
						       StreamingContext context)
			: base (info, context)
		{
		}
	}
}
