//
// System.Runtime.InteropServices.InvalidOleVariantTypeException.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
#if NET_2_0
	[ComVisible(true)]
#endif
	[Serializable]
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
