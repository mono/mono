// 
// System.EnterpriseServices.IRegistrationHelper.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

namespace System.EnterpriseServices {
	[Guid("55e3ea25-55cb-4650-8887-18e8d30bb4bc")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IRegistrationHelper {

		#region Methods

		void InstallAssembly ([In, MarshalAs(UnmanagedType.BStr)] string assembly, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string application, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string tlb, [In] InstallationFlags installFlags);
		void UninstallAssembly ([In, MarshalAs (UnmanagedType.BStr)] string assembly, [In, MarshalAs (UnmanagedType.BStr)] string application);

		#endregion

	}
}
