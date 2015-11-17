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
using System.Runtime.CompilerServices;
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

		[SecuritySafeCritical]
		public static string GetSystemVersion ()
		{
			return "v" + Environment.Version.Major + "." + Environment.Version.Minor + "." + Environment.Version.Build;
		}

		[DllImport("mscoree")]
		private extern static int CLRCreateInstance (
		    [MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
		    [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
		    out IntPtr punk);

		[SecurityCritical]
		public static IntPtr GetRuntimeInterfaceAsIntPtr (Guid clsid, Guid riid)
		{
			IntPtr result;

			if (clsid == Guid.Empty)
			{
			    Guid CLSID_CLRMetaHost = new Guid ("9280188d-0e8e-4867-b30c-7fa83884e8de");
				ICLRMetaHost metahost = (ICLRMetaHost)GetRuntimeInterfaceAsObject (CLSID_CLRMetaHost, typeof(ICLRMetaHost).GUID);

				result = metahost.GetRuntime (GetSystemVersion (), riid);
			}
			else
				Marshal.ThrowExceptionForHR (CLRCreateInstance (clsid, riid, out result));

			return result;
		}

		[SecurityCritical]
		public static object GetRuntimeInterfaceAsObject (Guid clsid, Guid riid)
		{
			IntPtr punk = GetRuntimeInterfaceAsIntPtr (clsid, riid);
			object result;

			try
			{
				result = Marshal.GetObjectForIUnknown (punk);
			}
			finally
			{
				Marshal.Release (punk);
			}

			return result;
		}
	}

    [Guid ("d332db9e-b9b3-4125-8207-a14884f53216")]
    [InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport ()]
    internal interface ICLRMetaHost
    {
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr GetRuntime (
		    string pwzVersion,
		    [MarshalAs(UnmanagedType.LPStruct)] Guid iid);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetVersionFromFile (
		    string pwzFilePath,
		    IntPtr pwzBuffer,
		    ref uint pcchBuffer);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr EnumerateInstalledRuntimes (); // IEnumUnknown

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr EnumerateLoadedRuntimes (IntPtr hndProcess); // IEnumUnknown

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void RequestRuntimeLoadedNotification (IntPtr pCallbackFunction);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr QueryLegacyV2RuntimeBinding (Guid riid);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ExitProcess (int iExitCode);
    }
}
