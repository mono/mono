//
// System.ComponentModel.Design.CheckoutException.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.ComponentModel.Design
{
	[Serializable]
	public class CheckoutException : ExternalException
	{
		public static readonly CheckoutException Canceled = new CheckoutException (
			"The user canceled the checkout.",
			-2147467260);

		public CheckoutException()
			: base ()
		{
		}

		public CheckoutException (string message)
			: base (message)
		{
		}

		public CheckoutException (string message, int errorCode)
			: base (message, errorCode)
		{
		}

		public CheckoutException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		protected CheckoutException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
