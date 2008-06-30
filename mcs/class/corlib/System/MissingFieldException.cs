//	
// System.MissingFieldException.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
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
	public class MissingFieldException : MissingMemberException
	{
		const int Result = unchecked ((int)0x80131511);

		// Constructors
		public MissingFieldException ()
			: base (Locale.GetText ("Cannot find requested field."))
		{
			HResult = Result;
		}

		public MissingFieldException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected MissingFieldException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public MissingFieldException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		public MissingFieldException (string className, string fieldName)
			: base (className, fieldName)
		{
			HResult = Result;
		}

		public override string Message {
			get {
				if (ClassName == null)
					return base.Message;

				String msg = Locale.GetText ("Field '{0}.{1}' not found.");
				return String.Format (msg, ClassName, MemberName);
			}
		}
	}
}
