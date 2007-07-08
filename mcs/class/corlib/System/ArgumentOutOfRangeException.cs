//
// System.ArgumentOutOfRangeException.cs
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
	public class ArgumentOutOfRangeException : ArgumentException
	{
		const int Result = unchecked ((int)0x80131502);

		private object actual_value;

		// Constructors
		public ArgumentOutOfRangeException ()
			: base (Locale.GetText ("Argument is out of range."))
		{
			HResult = Result;
		}

		public ArgumentOutOfRangeException (string paramName)
			: base (Locale.GetText ("Argument is out of range."), paramName)
		{
			HResult = Result;
		}

		public ArgumentOutOfRangeException (string paramName, string message)
			: base (message, paramName)
		{
			HResult = Result;
		}

		public ArgumentOutOfRangeException (string paramName, object actualValue, string message)
			: base (message, paramName)
		{
			this.actual_value = actualValue;
			HResult = Result;
		}

		protected ArgumentOutOfRangeException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			actual_value = info.GetString ("ActualValue");
		}

#if NET_2_0
		public ArgumentOutOfRangeException (string message, Exception innerException)
		: base (message, innerException)
		{
			HResult = Result;
		}
#endif

		// Properties
		public virtual object ActualValue {
			get {
				return actual_value;
			}
		}

		public override string Message {
			get {
				string basemsg = base.Message;
				if (actual_value == null)
					return basemsg;
				return basemsg + Environment.NewLine + actual_value;
			}
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("ActualValue", actual_value);
		}
	}
}
