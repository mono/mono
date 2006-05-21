//
// System.Runtime.InteropServices.SafeArrayTypeMismatchException.cs
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
	public class SafeArrayTypeMismatchException : SystemException
	{
		private const int ErrorCode = -2146233037; // = 0x80131533

		public SafeArrayTypeMismatchException ()
			: base (Locale.GetText ("The incoming SAVEARRAY does not match the expected managed signature"))
		{
			this.HResult = ErrorCode;
		}

		public SafeArrayTypeMismatchException (string message)
			: base (message)
		{
			this.HResult = ErrorCode;
		}

		public SafeArrayTypeMismatchException (string message, Exception inner)
			: base (message, inner)
		{
			this.HResult = ErrorCode;
		}

		protected SafeArrayTypeMismatchException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
