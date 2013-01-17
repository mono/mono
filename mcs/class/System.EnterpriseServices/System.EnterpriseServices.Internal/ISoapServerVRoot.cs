// System.EnterpriseServices.Internal.ISoapServerVRoot.cs
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

	[Guid("A31B6577-71D2-4344-AEDF-ADC1B0DC5347")]
	public interface ISoapServerVRoot {
		[DispId(1)]
		void CreateVirtualRootEx (
			[MarshalAs(UnmanagedType.BStr)] string rootWebServer,
			[MarshalAs(UnmanagedType.BStr)] string inBaseUrl,
			[MarshalAs(UnmanagedType.BStr)] string inVirtualRoot,
			[MarshalAs(UnmanagedType.BStr)] string homePage,
			[MarshalAs(UnmanagedType.BStr)] string discoFile,
			[MarshalAs(UnmanagedType.BStr)] string secureSockets,
			[MarshalAs(UnmanagedType.BStr)] string authentication,
			[MarshalAs(UnmanagedType.BStr)] string operation,
			[MarshalAs(UnmanagedType.BStr)] out string baseUrl,
			[MarshalAs(UnmanagedType.BStr)] out string virtualRoot,
			[MarshalAs(UnmanagedType.BStr)] out string physicalPath);

		[DispId(2)]
		void DeleteVirtualRootEx (
			[MarshalAs(UnmanagedType.BStr)] string rootWebServer,
			[MarshalAs(UnmanagedType.BStr)] string baseUrl,
			[MarshalAs(UnmanagedType.BStr)] string virtualRoot);

		[DispId(3)]
		void GetVirtualRootStatus (
			[MarshalAs(UnmanagedType.BStr)] string rootWebServer,
			[MarshalAs(UnmanagedType.BStr)] string inBaseUrl,
			[MarshalAs(UnmanagedType.BStr)] string inVirtualRoot,
			[MarshalAs(UnmanagedType.BStr)] out string exists,
			[MarshalAs(UnmanagedType.BStr)] out string secureSockets,
			[MarshalAs(UnmanagedType.BStr)] out string windowsAuth,
			[MarshalAs(UnmanagedType.BStr)] out string anonymous,
			[MarshalAs(UnmanagedType.BStr)] out string homePage,
			[MarshalAs(UnmanagedType.BStr)] out string discoFile,
			[MarshalAs(UnmanagedType.BStr)] out string physicalPath,
			[MarshalAs(UnmanagedType.BStr)] out string baseUrl,
			[MarshalAs(UnmanagedType.BStr)] out string virtualRoot);
	}
}
