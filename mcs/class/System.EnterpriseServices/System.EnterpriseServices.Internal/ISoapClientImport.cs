// System.EnterpriseServices.Internal.ISoapClientImport.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

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

namespace System.EnterpriseServices.Internal {

	[Guid("E7F0F021-9201-47e4-94DA-1D1416DEC27A")]
	public interface ISoapClientImport {
		[DispId(1)]
		void ProcessClientTlbEx (
			[MarshalAs(UnmanagedType.BStr)] string progId,
			[MarshalAs(UnmanagedType.BStr)] string virtualRoot,
			[MarshalAs(UnmanagedType.BStr)] string baseUrl,
			[MarshalAs(UnmanagedType.BStr)] string authentication,
			[MarshalAs(UnmanagedType.BStr)] string assemblyName,
			[MarshalAs(UnmanagedType.BStr)] string typeName);
	}
}
