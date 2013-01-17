//
// System.MonoAsyncCall.cs
//
// Author:
//    Zoltan Varga (vargaz@gmail.com)
//
//

//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

//
// This is the managed counterpart of the ASyncCall structure used by the threadpools.
//
using System.Runtime.InteropServices;

namespace System {

#pragma warning disable 169

	[StructLayout (LayoutKind.Sequential)]
	internal class MonoAsyncCall {
		#region Sync with the unmanaged ASyncCall structure
		object     msg;
		IntPtr     cb_method;
		object     cb_target;
		object     state;
		object     res;
		object     out_args;
		#endregion
	}

#pragma warning restore 169	

}



