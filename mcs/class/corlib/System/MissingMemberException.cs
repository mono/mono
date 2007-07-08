//
// System.MissingMemberException.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc. http://www.ximian.com
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
	public class MissingMemberException : MemberAccessException
	{
		const int Result = unchecked ((int)0x80131512);

		// Fields
		protected string ClassName;
		protected string MemberName;
		protected byte[] Signature;

		public MissingMemberException ()
			: base (Locale.GetText ("Cannot find the requested class member."))
		{
			HResult = Result;
		}

		public MissingMemberException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public MissingMemberException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		protected MissingMemberException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			ClassName = info.GetString ("MMClassName");
			MemberName = info.GetString ("MMMemberName");
			Signature = (byte[]) info.GetValue ("MMSignature", typeof(byte[]));
		}

		public MissingMemberException (string className, string memberName)
		{
			ClassName = className;
			MemberName = memberName;
			HResult = Result;
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("MMClassName", ClassName);
			info.AddValue ("MMMemberName", MemberName);
			info.AddValue ("MMSignature", Signature);
		}

		public override string Message {
			get {
				if (ClassName == null)
					return base.Message;

				String msg = Locale.GetText ("Member {0}.{1} not found.");
				return String.Format (msg, ClassName, MemberName);
			}
		}
	}
}
