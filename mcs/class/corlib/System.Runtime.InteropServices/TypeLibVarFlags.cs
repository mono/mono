// System.Runtime.InteropServices.TypeLibVarFlags.cs
// 
// Name: Duncan Mak  (duncan@ximian.com)
// 
// (C) Ximian, Inc.
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
namespace System.Runtime.InteropServices {
	
	[ComVisible(true)]
	[Flags] [Serializable]
	public enum TypeLibVarFlags {
		FReadOnly = 1,
		FSource = 2,
		FBindable = 4,
		FRequestEdit = 8,
		FDisplayBind = 16,
		FDefaultBind = 32,
		FHidden = 64,
		FRestricted = 128,
		FDefaultCollelem = 256,
		FUiDefault = 512,
		FNonBrowsable = 1024,
		FReplaceable = 2048,
		FImmediateBind = 4096,
	}
}
#endif
