//
// System.AppDomainUnloadedException.cs
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
	public class AppDomainUnloadedException : SystemException
	{
		const int Result = unchecked ((int)0x80131014);

		// Constructors
		public AppDomainUnloadedException ()
			: base (Locale.GetText ("Can't access an unloaded application domain."))
		{
			HResult = Result;
		}

		public AppDomainUnloadedException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public AppDomainUnloadedException (string message, Exception innerException)
			:base (message, innerException)
		{
			HResult = Result;
		}

		protected AppDomainUnloadedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
