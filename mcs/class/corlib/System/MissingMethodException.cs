//
// System.MissingMethodException.cs
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

using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public class MissingMethodException : MissingMemberException
	{
		const int Result = unchecked ((int)0x80131513);

		public MissingMethodException ()
			: base (Locale.GetText ("Cannot find the requested method."))
		{
			HResult = Result;
		}

		public MissingMethodException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected MissingMethodException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public MissingMethodException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		public MissingMethodException (string className, string methodName)
			: base (className, methodName)
		{
			HResult = Result;
		}

		public override string Message {
			get {
				if (ClassName == null)
					return base.Message;

				String msg = Locale.GetText ("Method not found: '{0}.{1}'.");
				return String.Format (msg, ClassName, MemberName);
			}
		}
	}
}
