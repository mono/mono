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
        public interface IRegiisUtility
        {
                void ProtectedConfigAction (
                        long actionToPerform,
                        [In] string first_argument,
                        [In] string second_argument,
                        [In] string provider_name,
                        [In] string app_path,
                        [In] string csp_or_location,
                        int key_size,
                        out string exception);

                void RegisterAsnetMmcAssembly (
                        int do_reg,
                        [In] string assembly_name,
                        [In] string binary_directory,
                        out string exception);

                void RegisterSystemWebAssembly (int do_reg, out string exception);

                void ToggleWebAdminToolConfigs (
                        [In] string site,
                        [In] int [] settings,
                        int size,
                        out string exception);
        }
}
#endif
