//	
// System.CannotUnloadAppDomainException.cs
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
	public class CannotUnloadAppDomainException : SystemException
	{
		const int Result = unchecked ((int)0x80131015);

		// Constructors
		public CannotUnloadAppDomainException ()
			: base (Locale.GetText ("Attempt to unload application domain failed."))
		{
			HResult = Result;
		}

		public CannotUnloadAppDomainException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected CannotUnloadAppDomainException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public CannotUnloadAppDomainException (string message, Exception innerException)
			:base (message, innerException)
		{
			HResult = Result;
		}
	}
}
