//
// System.NotSupportedException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class NotSupportedException : SystemException
	{
		const int Result = unchecked ((int)0x80131515);

		// Constructors
		public NotSupportedException ()
			: base (Locale.GetText ("Operation is not supported."))
		{
			HResult = Result;
		}

		public NotSupportedException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public NotSupportedException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		protected NotSupportedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
