//	
// System.EntryPointNotFoundException.cs
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
	public class EntryPointNotFoundException : TypeLoadException
	{
		// Constructors
		public EntryPointNotFoundException ()
			: base (Locale.GetText ("Cannot load class because of missing entry method."))
		{
		}

		public EntryPointNotFoundException (string message)
			: base (message)
		{
		}

		protected EntryPointNotFoundException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public EntryPointNotFoundException (string message, Exception innerException)
			:base (message, innerException)
		{
		}
	}
}
