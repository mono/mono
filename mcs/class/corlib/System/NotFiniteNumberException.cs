//
// System.NotFiniteNumberException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
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

using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public class NotFiniteNumberException : ArithmeticException
	{
		const int Result = unchecked ((int)0x80131528);

		double offending_number;

		// Constructors
		public NotFiniteNumberException ()
			: base (Locale.GetText ("The number encountered was not a finite quantity."))
		{
			HResult = Result;
		}

		public NotFiniteNumberException (double offendingNumber)
		{
			this.offending_number = offendingNumber;
			HResult = Result;
		}

		public NotFiniteNumberException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public NotFiniteNumberException (string message, double offendingNumber)
			: base (message)
		{
			this.offending_number = offendingNumber;
			HResult = Result;
		}

		public NotFiniteNumberException (string message, double offendingNumber, Exception innerException)
			: base (message, innerException)
		{
			this.offending_number = offendingNumber;
			HResult = Result;
		}

		protected NotFiniteNumberException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			offending_number = info.GetDouble ("OffendingNumber");
		}

#if NET_2_0
		public NotFiniteNumberException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}
#endif

		// Properties
		public double OffendingNumber {
			get {
				return offending_number;
			}
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("OffendingNumber", offending_number);
		}
	}
}
