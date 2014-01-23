//
// System.Runtime.InteropServices/RuntimeEnvironment.cs
//
// Authors:
// 	Dominik Fretz (roboto@gmx.net)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Dominik Fretz
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Runtime.InteropServices
{
	[ComVisible (true)]
	public class RuntimeEnvironment
	{
		public RuntimeEnvironment ()
		{
		}

		public static string SystemConfigurationFile {
			get {
				// GetMachineConfigPath is internal and not protected by CAS
				string path = Environment.GetMachineConfigPath ();
				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, path).Demand ();
				}
				return path;
			}
		}

		public static bool FromGlobalAccessCache (Assembly a)
		{
			// yes, this will throw a NullReferenceException (just like MS, reported as ...)
			return a.GlobalAssemblyCache;
		}
	
		public static string GetRuntimeDirectory ()
		{
			return Path.GetDirectoryName (typeof (int).Assembly.Location);	
		}

#if NET_4_0
		[SecuritySafeCritical]
#else
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
#endif
		public static string GetSystemVersion ()
		{
			return "v" + Environment.Version.Major + "." + Environment.Version.Minor + "." + Environment.Version.Build;
		}
	}
}
