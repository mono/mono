//
// System.Runtime.InteropServices.ComTypes.IEnumFORMATETC.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//

//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

namespace System.Runtime.InteropServices.ComTypes
{
	[Guid ("00000103-0000-0000-C000-000000000046")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IEnumSTATDATA
	{
		[PreserveSig]
		int Next (int celt, [MarshalAs (UnmanagedType.LPArray)] [Out] STATDATA [] rgelt, [MarshalAs (UnmanagedType.LPArray)] [Out] int [] pceltFetched);
		[PreserveSig]
		int Skip (int celt);
		[PreserveSig]
		int Reset ();
		void Clone (out IEnumSTATDATA newEnum);
	}
}
