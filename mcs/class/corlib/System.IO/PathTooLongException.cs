//	
// System.PathTooLongException.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.IO
{
	   [Serializable]
	   public class PathTooLongException : SystemException
	   {
			 // Constructors
			 public PathTooLongException ()
				    : base (Locale.GetText ("Pathname is longer than the maximum length"))
			 {
			 }

			 public PathTooLongException (string message)
				    : base (message)
			 {
			 }
			 
			 protected PathTooLongException (SerializationInfo info,
				    StreamingContext context)
				    : base (info, context)
			 {
			 }

			 public PathTooLongException (string message, Exception innerException)
				    :base (message, innerException)
			 {
			 }
				    
	   }
}
