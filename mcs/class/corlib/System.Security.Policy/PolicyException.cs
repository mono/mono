//	
// System.PolicyException.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Security.Policy
{
	   [Serializable]
	   public class PolicyException : SystemException
	   {
			 // Constructors
			 public PolicyException ()
				    : base (Locale.GetText ("Cannot run because of policy."))
			 {
			 }

			 public PolicyException (string message)
				    : base (message)
			 {
			 }
			 
			 protected PolicyException (SerializationInfo info,
				    StreamingContext context)
				    : base (info, context)
			 {
			 }

			 public PolicyException (string message, Exception innerException)
				    :base (message, innerException)
			 {
			 }
				    
	   }
}
