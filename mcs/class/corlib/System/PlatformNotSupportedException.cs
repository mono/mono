//
// System.PlatformNotSupportedException.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class PlatformNotSupportedException : NotSupportedException
	{
		const int Result = unchecked ((int)0x80131539);

		// Constructors
		public PlatformNotSupportedException ()
			: base (Locale.GetText ("This platform is not supported."))
		{
			HResult = Result;
		}

		public PlatformNotSupportedException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected PlatformNotSupportedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public PlatformNotSupportedException (string message, Exception innerException)
			:base (message, innerException)
		{
			HResult = Result;
		}
	}
}
