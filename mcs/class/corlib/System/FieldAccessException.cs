//	
// System.FieldAccessException.cs
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
	public class FieldAccessException : MemberAccessException
	{
		// Constructors
		public FieldAccessException ()
			: base (Locale.GetText ("Attempt to access a private/protected field failed."))
		{
		}

		public FieldAccessException (string message)
			: base (message)
		{
		}

		protected FieldAccessException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public FieldAccessException (string message, Exception innerException)
			:base (message, innerException)
		{
		}
	}
}
