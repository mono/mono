//
// System.Runtime.InteropServices.UCOMIMoniker.cs
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

using System;

namespace System.Runtime.InteropServices
{
	[Guid ("0000000f-0000-0000-c000-000000000046")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIMoniker
	{
		void BindToObject (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, [In] ref Guid riidResult, out object ppvResult);
		void BindToStorage (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, [In] ref Guid riid, out object ppvObj);
		void CommonPrefixWith (UCOMIMoniker pmkOther, out UCOMIMoniker ppmkPrefix);
		void ComposeWith (UCOMIMoniker pmkRight, bool fOnlyIfNotGeneric, out UCOMIMoniker ppmkComposite);
		void Enum (bool fForward, out UCOMIEnumMoniker ppenumMoniker);
		void GetClassID (out Guid pClassID);
		void GetDisplayName (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, out string ppszDisplayName);
		void GetSizeMax (out long pcbSize);
		void GetTimeOfLastChange (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, out FILETIME pFileTime);
		void Hash (out int pdwHash);
		void Inverse (out UCOMIMoniker ppmk);
		int IsDirty ();
		void IsEqual (UCOMIMoniker pmkOtherMoniker);
		void IsRunning (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, UCOMIMoniker pmkNewlyRunning);
		void IsSystemMoniker (out int pdwMksys);
		void Load (UCOMIStream pStm);
		void ParseDisplayName (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, string pszDisplayName, out int pchEaten, out UCOMIMoniker ppmkOut);
		void Reduce (UCOMIBindCtx pbc, int dwReduceHowFar, ref UCOMIMoniker ppmkToLeft, out UCOMIMoniker ppmkReduced);
		void RelativePathTo (UCOMIMoniker pmkOther, out UCOMIMoniker ppmkRelPath);
		void Save (UCOMIStream pStm, bool fClearDirty);
	}
}
