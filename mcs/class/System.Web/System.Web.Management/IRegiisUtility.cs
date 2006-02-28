//
// System.Web.Management.IRegiisUtility.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.Runtime.InteropServices;

#if NET_2_0
namespace System.Web.Management
{
	[InterfaceTypeAttribute (ComInterfaceType.InterfaceIsIUnknown)]
	[GuidAttribute ("C84F668A-CC3F-11D7-B79E-505054503030")]
	[ComImportAttribute]
        public interface IRegiisUtility
        {
                void ProtectedConfigAction (
                        long actionToPerform,
                        [In,MarshalAs(UnmanagedType.LPWStr)] string firstArgument,
                        [In,MarshalAs(UnmanagedType.LPWStr)] string secondArgument,
                        [In,MarshalAs(UnmanagedType.LPWStr)] string providerName,
                        [In,MarshalAs(UnmanagedType.LPWStr)] string appPath,
                        [In,MarshalAs(UnmanagedType.LPWStr)] string site,
                        [In,MarshalAs(UnmanagedType.LPWStr)] string cspOrLocation,
                        int keySize,
                        out IntPtr exception);

                void RegisterAsnetMmcAssembly (
                        int doReg,
                        [In,MarshalAs(UnmanagedType.LPWStr)] string assemblyName,
                        [In,MarshalAs(UnmanagedType.LPWStr)] string binaryDirectory,
                        out IntPtr exception);

                void RegisterSystemWebAssembly (int doReg,
						out IntPtr exception);

		void RemoveBrowserCaps (out IntPtr exception);
        }
}
#endif
