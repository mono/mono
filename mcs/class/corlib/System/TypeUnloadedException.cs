//	
// System.TypeUnloadedException.cs
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
	public class TypeUnloadedException : SystemException
	{
		// Constructors
		public TypeUnloadedException ()
			: base (Locale.GetText ("Cannot access an unloaded class."))
		{
		}

		public TypeUnloadedException (string message)
			: base (message)
		{
		}

		protected TypeUnloadedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public TypeUnloadedException (string message, Exception innerException)
			:base (message, innerException)
		{
		}
	}
}
