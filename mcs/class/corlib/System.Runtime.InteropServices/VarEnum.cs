
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

namespace System.Runtime.InteropServices {
#if NET_2_0
	[ComVisible(true)]
#endif
	[Serializable]
	public enum VarEnum {
		VT_EMPTY = 0,
		VT_NULL = 1,
		VT_I2 = 2,
		VT_I4 = 3,
		VT_R4 = 4,
		VT_R8 = 5,
		VT_CY = 6,
		VT_DATE = 7,
		VT_BSTR = 8,
		VT_DISPATCH = 9,
		VT_ERROR = 10,
		VT_BOOL = 11,
		VT_VARIANT = 12,
		VT_UNKNOWN = 13,
		VT_DECIMAL = 14,
		VT_I1 = 16,
		VT_UI1 = 17,
		VT_UI2 = 18,
		VT_UI4 = 19,
		VT_I8 = 20,
		VT_UI8 = 21,
		VT_INT = 22,
		VT_UINT = 23,
		VT_VOID = 24,
		VT_HRESULT = 25,
		VT_PTR = 26,
		VT_SAFEARRAY = 27,
		VT_CARRAY = 28,
		VT_USERDEFINED = 29,
		VT_LPSTR = 30,
		VT_LPWSTR = 31,
		VT_RECORD = 36,
		VT_FILETIME = 64,
		VT_BLOB = 65,
		VT_STREAM = 66,
		VT_STORAGE = 67,
		VT_STREAMED_OBJECT = 68,
		VT_STORED_OBJECT = 69,
		VT_BLOB_OBJECT = 70,
		VT_CF = 71,
		VT_CLSID = 72,
		VT_VECTOR = 4096,
		VT_ARRAY = 8192,
		VT_BYREF = 16384
	}
}
