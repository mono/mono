//	
// System.EntryPointNotFoundException.cs
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
	public class EntryPointNotFoundException : TypeLoadException
	{
		const int Result = unchecked ((int)0x80131523);

		// Constructors
		public EntryPointNotFoundException ()
			: base (Locale.GetText ("Cannot load class because of missing entry method."))
		{
			HResult = Result;
		}

		public EntryPointNotFoundException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected EntryPointNotFoundException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public EntryPointNotFoundException (string message, Exception innerException)
			:base (message, innerException)
		{
			HResult = Result;
		}
	}
}
