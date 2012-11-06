//
// System.Runtime.InteropServices.UCOMIPersistFile.cs
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

#if !FULL_AOT_RUNTIME
namespace System.Runtime.InteropServices
{
	[Obsolete]
	[ComImport]
	[Guid ("0000010b-0000-0000-c000-000000000046")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIPersistFile
	{
		void GetClassID (out Guid pClassID);
		[PreserveSig]
		int IsDirty ();
		void Load ([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, int dwMode);
		void Save ([MarshalAs (UnmanagedType.LPWStr)] string pszFileName, [MarshalAs (UnmanagedType.Bool)] bool fRemember);
		void SaveCompleted ([MarshalAs (UnmanagedType.LPWStr)]string pszFileName);
		void GetCurFile ([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
	}
}
#endif
