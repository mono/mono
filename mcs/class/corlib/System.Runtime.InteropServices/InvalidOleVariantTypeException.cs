//
// System.Runtime.InteropServices.InvalidOleVariantTypeException.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	public class InvalidOleVariantTypeException : SystemException
	{
		private const int ErrorCode = -2146233039; // = 0x80131531

		public InvalidOleVariantTypeException ()
			: base (Locale.GetText ("Found native variant type cannot be marshalled to managed code"))
		{
			this.HResult = ErrorCode;
		}

		public InvalidOleVariantTypeException (string message)
			: base (message)
		{
			this.HResult = ErrorCode;
		}

		public InvalidOleVariantTypeException (string message, Exception inner)
			: base (message, inner)
		{
			this.HResult = ErrorCode;
		}

		protected InvalidOleVariantTypeException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
