//
// Mono.Unix/FileNameMarshaler.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2005 Jonathan Pryor
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
using Mono.Unix;

namespace Mono.Unix.Native {

	class FileNameMarshaler : ICustomMarshaler {

		private static FileNameMarshaler Instance = new FileNameMarshaler ();

		public static ICustomMarshaler GetInstance (string s)
		{
			return Instance;
		}

		public void CleanUpManagedData (object o)
		{
		}

		public void CleanUpNativeData (IntPtr pNativeData)
		{
			// Console.WriteLine ("# FileNameMarshaler.CleanUpManagedData ({0:x})", pNativeData);
			UnixMarshal.FreeHeap (pNativeData);
		}

		public int GetNativeDataSize ()
		{
			return IntPtr.Size;
		}

		public IntPtr MarshalManagedToNative (object obj)
		{
			string s = obj as string;
			if (s == null)
				return IntPtr.Zero;
			IntPtr p = UnixMarshal.StringToHeap (s, UnixEncoding.Instance);
			// Console.WriteLine ("# FileNameMarshaler.MarshalNativeToManaged for `{0}'={1:x}", s, p);
			return p;
		}

		public object MarshalNativeToManaged (IntPtr pNativeData)
		{
			string s = UnixMarshal.PtrToString (pNativeData, UnixEncoding.Instance);
			// Console.WriteLine ("# FileNameMarshaler.MarshalNativeToManaged ({0:x})=`{1}'",
			// 		pNativeData, s);
			return s;
		}
	}
}

// vim: noexpandtab
