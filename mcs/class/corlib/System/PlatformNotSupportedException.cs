//
// System.PlatformNotSupportedException.cs
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
	public class PlatformNotSupportedException : NotSupportedException
	{
		// Constructors
		public PlatformNotSupportedException ()
			: base (Locale.GetText ("This platform is not supported."))
		{
		}

		public PlatformNotSupportedException (string message)
			: base (message)
		{
		}

		protected PlatformNotSupportedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public PlatformNotSupportedException (string message, Exception innerException)
			:base (message, innerException)
		{
		}
	}
}
