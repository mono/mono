// System.Runtime.CompilerServices.RuntimeHelpers
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
//
// (C) Ximian, Inc. 2001

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

namespace System.Runtime.CompilerServices
{
	[Serializable]
	public sealed class RuntimeHelpers
	{
		private static int offset_to_string_data;

		static RuntimeHelpers () {
			offset_to_string_data = GetOffsetToStringData();
		}

		private RuntimeHelpers () {}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void InitializeArray (Array array, IntPtr fldHandle);

		public static void InitializeArray (Array array, RuntimeFieldHandle fldHandle)
		{
			InitializeArray (array, fldHandle.Value);
		}

		public static int OffsetToStringData {
			get {
				return offset_to_string_data;
			}
		}

#if NET_1_1
		public static int GetHashCode (object o) {
			return Object.InternalGetHashCode (o);
		}

		public static new bool Equals (object o1, object o2) {
			// LAMESPEC: According to MSDN, this is equivalent to 
			// Object::Equals (). But the MS version of Object::Equals()
			// includes the functionality of ValueType::Equals(), while
			// our version does not.
			if (o1 == o2)
				return true;
			if ((o1 == null) || (o2 == null))
				return false;
			if (o1 is ValueType)
				return ValueType.DefaultEquals (o1, o2);
			else
				return Object.Equals (o1, o2);
		}
#endif

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern object GetObjectValue (object obj);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void RunClassConstructor (IntPtr type);

		public static void RunClassConstructor (RuntimeTypeHandle type)
		{
			RunClassConstructor (type.Value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int GetOffsetToStringData();
	}
}
