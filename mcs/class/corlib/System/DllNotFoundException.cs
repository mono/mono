//
// System.DllNotFoundException.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class DllNotFoundException : TypeLoadException
	{
		const int Result = unchecked ((int)0x80131524);

		// Constructors
		public DllNotFoundException ()
			: base (Locale.GetText ("DLL not found."))
		{
			HResult = Result;
		}

		public DllNotFoundException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected DllNotFoundException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public DllNotFoundException (string message, Exception innerException)
			:base (message, innerException)
		{
			HResult = Result;
		}
	}
}
