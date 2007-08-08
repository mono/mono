//
// System.Runtime.InteropServices.ComTypes.IMoniker.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Kazuki Oikawa (kazuki@panicode.com)
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
#if NET_2_0
using System;

namespace System.Runtime.InteropServices.ComTypes
{
	[ComImport]
	[Guid ("0000000f-0000-0000-c000-000000000046")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMoniker
	{
		void GetClassID (out Guid pClassID);
		[PreserveSig]
		int IsDirty ();
		void Load (IStream pStm);
		void Save (IStream pStm, [MarshalAs (UnmanagedType.Bool)] bool fClearDirty);
		void GetSizeMax (out long pcbSize);
		void BindToObject (IBindCtx pbc, IMoniker pmkToLeft, [In] ref Guid riidResult, [MarshalAs (UnmanagedType.Interface)] out object ppvResult);
		void BindToStorage (IBindCtx pbc, IMoniker pmkToLeft, [In] ref Guid riid, [MarshalAs (UnmanagedType.Interface)] out object ppvObj);
		void Reduce (IBindCtx pbc, int dwReduceHowFar, ref IMoniker ppmkToLeft, out IMoniker ppmkReduced);
		void ComposeWith (IMoniker pmkRight, [MarshalAs (UnmanagedType.Bool)] bool fOnlyIfNotGeneric, out IMoniker ppmkComposite);
		void Enum ([MarshalAs(UnmanagedType.Bool)] bool fForward, out IEnumMoniker ppenumMoniker);
		[PreserveSig]
		int IsEqual (IMoniker pmkOtherMoniker);
		void Hash (out int pdwHash);
		[PreserveSig]
		int IsRunning (IBindCtx pbc, IMoniker pmkToLeft, IMoniker pmkNewlyRunning);
		void GetTimeOfLastChange (IBindCtx pbc, IMoniker pmkToLeft, out FILETIME pFileTime);
		void Inverse (out IMoniker ppmk);
		void CommonPrefixWith (IMoniker pmkOther, out IMoniker ppmkPrefix);
		void RelativePathTo (IMoniker pmkOther, out IMoniker ppmkRelPath);
		void GetDisplayName (IBindCtx pbc, IMoniker pmkToLeft, [MarshalAs (UnmanagedType.LPWStr)] out string ppszDisplayName);
		void ParseDisplayName (IBindCtx pbc, IMoniker pmkToLeft, [MarshalAs (UnmanagedType.LPWStr)] string pszDisplayName, out int pchEaten, out IMoniker ppmkOut);
		[PreserveSig]
		int IsSystemMoniker (out int pdwMksys);
	}
}
#endif
