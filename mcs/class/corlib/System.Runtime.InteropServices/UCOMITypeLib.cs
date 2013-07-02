
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

// System.Runtime.InteropServices.UCOMITypeLib.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

#if !FULL_AOT_RUNTIME
namespace System.Runtime.InteropServices
{
	[Obsolete]
	[ComImport]
	[Guid("00020402-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMITypeLib { 
		[PreserveSig]
		int GetTypeInfoCount ();
		void GetTypeInfo (int index, out UCOMITypeInfo ppTI);
		void GetTypeInfoType (int index, out TYPEKIND pTKind);
		void GetTypeInfoOfGuid (ref Guid guid, out UCOMITypeInfo ppTInfo);
		void GetLibAttr (out IntPtr ppTLibAttr);
		void GetTypeComp (out UCOMITypeComp ppTComp); 
		void GetDocumentation (int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
		[return: MarshalAs (UnmanagedType.Bool)]
		bool IsName ([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal);
		void FindName ([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal, [Out, MarshalAs (UnmanagedType.LPArray)] UCOMITypeInfo[] ppTInfo, [Out, MarshalAs (UnmanagedType.LPArray)] int[] rgMemId, ref short pcFound);
		[PreserveSig]
		void ReleaseTLibAttr (IntPtr pTLibAttr);
	}
}

#endif
