//
// System.NotImplementedException.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class NotImplementedException : SystemException
	{
		const int Result = unchecked ((int)0x80004001);

		// Constructors
		public NotImplementedException ()
			: base (Locale.GetText ("The requested feature is not implemented."))
		{
			HResult = Result;
		}

		public NotImplementedException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public NotImplementedException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		protected NotImplementedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
