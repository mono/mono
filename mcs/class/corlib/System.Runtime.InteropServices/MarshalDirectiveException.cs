//
// System.Runtime.InteropServices.MarshalDirectiveException.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	public class MarshalDirectiveException : SystemException
	{
		private const int ErrorCode = -2146233035; // = 0x80131535

		public MarshalDirectiveException ()
			: base (Locale.GetText ("Unsupported MarshalAsAttribute found"))
		{
			this.HResult = ErrorCode;
		}

		public MarshalDirectiveException (string message)
			: base (message)
		{
			this.HResult = ErrorCode;
		}

		public MarshalDirectiveException (string message, Exception inner)
			: base (message, inner)
		{
			this.HResult = ErrorCode;
		}

		protected MarshalDirectiveException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
