//	
// System.CannotUnloadAppDomainException.cs
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
	public class CannotUnloadAppDomainException : SystemException
	{
		// Constructors
		public CannotUnloadAppDomainException ()
			: base (Locale.GetText ("Attempt to unload application domain failed."))
		{
		}

		public CannotUnloadAppDomainException (string message)
			: base (message)
		{
		}

		protected CannotUnloadAppDomainException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public CannotUnloadAppDomainException (string message, Exception innerException)
			:base (message, innerException)
		{
		}
	}
}
