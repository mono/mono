//	
// System.IO.EndOfStreamException.cs
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
	   public class EndOfStreamException : SystemException
	   {
			 // Constructors
			 public EndOfStreamException ()
				    : base (Locale.GetText ("Failed to read past end of stream."))
			 {
			 }

			 public EndOfStreamException (string message)
				    : base (message)
			 {
			 }
			 
			 protected EndOfStreamException (SerializationInfo info,
				    StreamingContext context)
				    : base (info, context)
			 {
			 }

			 public EndOfStreamException (string message, Exception innerException)
				    :base (message, innerException)
			 {
			 }
				    
	   }
}
