//	
// System.AppDomainUnloadedException.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class AppDomainUnloadedException : SystemException
	{
		 // Constructors
		 public AppDomainUnloadedException ()
			    : base (Locale.GetText ("Can't access an unloaded application domain."))
		 {
		 }

		 public AppDomainUnloadedException (string message)
			    : base (message)
		 {
		 }
			 
		 protected AppDomainUnloadedException (SerializationInfo info,
			    StreamingContext context)
			    : base (info, context)
		 {
		 }

		 public AppDomainUnloadedException (string message, Exception innerException)
			    :base (message, innerException)
		 {
		 }
			    
	}
}
