//
// System.RankException.cs
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
	public class RankException : SystemException
	{
		const int Result = unchecked ((int)0x80131517);

		// Constructors
		public RankException ()
			: base (Locale.GetText ("Two arrays must have the same number of dimensions."))
		{
			HResult = Result;
		}

		public RankException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public RankException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		protected RankException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
