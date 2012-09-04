// System.EnterpriseServices.Internal.IServerWebConfig.cs
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

	[Guid("6261e4b5-572a-4142-a2f9-1fe1a0c97097")]
	public interface IServerWebConfig {
		[DispId(1)]
		void AddElement (
			[MarshalAs(UnmanagedType.BStr)] string FilePath,
			[MarshalAs(UnmanagedType.BStr)] string AssemblyName,
			[MarshalAs(UnmanagedType.BStr)] string TypeName,
			[MarshalAs(UnmanagedType.BStr)] string ProgId,
			[MarshalAs(UnmanagedType.BStr)] string Mode,
			[MarshalAs(UnmanagedType.BStr)] out string Error);

		[DispId(2)]
		void Create (
			[MarshalAs(UnmanagedType.BStr)] string FilePath,
			[MarshalAs(UnmanagedType.BStr)] string FileRootName,
			[MarshalAs(UnmanagedType.BStr)] out string Error);
	}
}
