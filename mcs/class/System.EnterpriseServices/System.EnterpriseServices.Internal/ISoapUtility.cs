// System.EnterpriseServices.Internal.ISoapUtility.cs
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

	[Guid("5AC4CB7E-F89F-429b-926B-C7F940936BF4")]
	public interface ISoapUtility {
		[DispId(2)]
		void GetServerBinPath (
			[MarshalAs(UnmanagedType.BStr)] string rootWebServer,
			[MarshalAs(UnmanagedType.BStr)] string inBaseUrl,
			[MarshalAs(UnmanagedType.BStr)] string inVirtualRoot,
			[MarshalAs(UnmanagedType.BStr)] out string binPath);

		[DispId(1)]
		void GetServerPhysicalPath (
			[MarshalAs(UnmanagedType.BStr)] string rootWebServer,
			[MarshalAs(UnmanagedType.BStr)] string inBaseUrl,
			[MarshalAs(UnmanagedType.BStr)] string inVirtualRoot,
			[MarshalAs(UnmanagedType.BStr)] out string physicalPath);

		[DispId(3)]
		void Present ();
	}
}
