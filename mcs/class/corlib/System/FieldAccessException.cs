//	
// System.FieldAccessException.cs
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
	   public class FieldAccessException : SystemException
	   {
			 // Constructors
			 public FieldAccessException ()
				    : base (Locale.GetText ("Attempt to access a private/protected field failed."))
			 {
			 }

			 public FieldAccessException (string message)
				    : base (message)
			 {
			 }
			 
			 public FieldAccessException (SerializationInfo info,
				    StreamingContext context)
				    : base (info, context)
			 {
			 }

			 public FieldAccessException (string message, Exception innerException)
				    :base (message, innerException)
			 {
			 }
				    
	   }
}
