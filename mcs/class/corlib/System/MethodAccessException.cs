//	
// System.MethodAccessException.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class MethodAccessException : MemberAccessException
	{
		// Constructors
		public MethodAccessException ()
			: base (Locale.GetText ("Attempt to access a private/protected method failed."))
		{
		}

		public MethodAccessException (string message)
			: base (message)
		{
		}

		protected MethodAccessException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public MethodAccessException (string message, Exception innerException)
			:base (message, innerException)
		{
		}
	}
}
