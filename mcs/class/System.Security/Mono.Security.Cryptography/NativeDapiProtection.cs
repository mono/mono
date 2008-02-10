//
// NativeDapiProtection.cs - 
//	Protect (encrypt) data without (user involved) key management
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;

namespace Mono.Security.Cryptography {

	// DAPI is only available in Windows 2000 and later operating systems
	// see ManagedProtection for other platforms

	// notes:
	// * no need to assert KeyContainerPermission here as unmanaged code can
	//   do what it wants;
	// * which is why we also need the [SuppressUnmanagedCodeSecurity] 
	//   attribute on each native function (so we don't require UnmanagedCode)

	internal class NativeDapiProtection {

		private const uint CRYPTPROTECT_UI_FORBIDDEN = 0x1;
		private const uint CRYPTPROTECT_LOCAL_MACHINE = 0x4;

		[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct DATA_BLOB {

			private int cbData;
			private IntPtr pbData;

			public void Alloc (int size)
			{
				if (size > 0) {
					pbData = Marshal.AllocHGlobal (size);
					cbData = size;
				}
			}

			public void Alloc (byte[] managedMemory)
			{
				if (managedMemory != null) {
					int size = managedMemory.Length;
					pbData = Marshal.AllocHGlobal (size);
					cbData = size;
					Marshal.Copy (managedMemory, 0, pbData, cbData);
				}
			}

			public void Free ()
			{
				if (pbData != IntPtr.Zero) {
					// clear copied memory!
					ZeroMemory (pbData, cbData);
					Marshal.FreeHGlobal (pbData);
					pbData = IntPtr.Zero;
					cbData = 0;
				}
			}

			public byte[] ToBytes ()
			{
				if (cbData <= 0)
					return new byte [0];

				byte[] managedMemory = new byte[cbData];
				Marshal.Copy (pbData, managedMemory, 0, cbData);
				return managedMemory;
			}
		}

		[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct CRYPTPROTECT_PROMPTSTRUCT {

			private int cbSize;
			private uint dwPromptFlags;
			private IntPtr hwndApp;
			private string szPrompt;

			public CRYPTPROTECT_PROMPTSTRUCT (uint flags)
			{
				cbSize = Marshal.SizeOf (typeof (CRYPTPROTECT_PROMPTSTRUCT));
				dwPromptFlags = flags;
				hwndApp = IntPtr.Zero;
				szPrompt = null;
			}
		}

		// http://msdn.microsoft.com/library/en-us/seccrypto/security/cryptprotectdata.asp
		[SuppressUnmanagedCodeSecurity]
		[DllImport ("crypt32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		private static extern bool CryptProtectData (ref DATA_BLOB pDataIn, string szDataDescr, ref DATA_BLOB pOptionalEntropy,
			IntPtr pvReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct, uint dwFlags, ref DATA_BLOB pDataOut);

		// http://msdn.microsoft.com/library/en-us/seccrypto/security/cryptunprotectdata.asp
		[SuppressUnmanagedCodeSecurity]
		[DllImport ("crypt32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		private static extern bool CryptUnprotectData (ref DATA_BLOB pDataIn, string szDataDescr, ref DATA_BLOB pOptionalEntropy,
			IntPtr pvReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct, uint dwFlags, ref DATA_BLOB pDataOut);

		// http://msdn.microsoft.com/library/en-us/memory/base/zeromemory.asp
		// note: SecureZeroMemory is an inline function (and can't be used here)
		// anyway I don't think the CLR will optimize this call away (like a C/C++ compiler could do)
		[SuppressUnmanagedCodeSecurity]
		[DllImport ("kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		private static extern void ZeroMemory (IntPtr dest, int size);


		// managed helpers

		public static byte[] Protect (byte[] userData, byte[] optionalEntropy, DataProtectionScope scope)
		{
			byte[] encdata = null;
			int hr = 0;

			DATA_BLOB data = new DATA_BLOB ();
			DATA_BLOB entropy = new DATA_BLOB ();
			DATA_BLOB cipher = new DATA_BLOB ();
			try {
				CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT (0);
				data.Alloc (userData);
				entropy.Alloc (optionalEntropy);

				// note: the scope/flags has already been check by the public caller
				uint flags = CRYPTPROTECT_UI_FORBIDDEN;
				if (scope == DataProtectionScope.LocalMachine)
					flags |= CRYPTPROTECT_LOCAL_MACHINE;

				// note: on Windows 2000 the string parameter *cannot* be null
				if (CryptProtectData (ref data, String.Empty, ref entropy, IntPtr.Zero,
					ref prompt, flags, ref cipher)) {
					// copy encrypted data back to managed codde
					encdata = cipher.ToBytes ();
				} else {
					hr = Marshal.GetLastWin32Error ();
				}
			}
			catch (Exception ex) {
				string msg = Locale.GetText ("Error protecting data.");
				throw new CryptographicException (msg, ex);
			}
			finally {
				cipher.Free ();
				data.Free ();
				entropy.Free ();
			}

			if ((encdata == null) || (hr != 0)) {
				throw new CryptographicException (hr);
			}
			return encdata;
		}

		public static byte[] Unprotect (byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope)
		{
			byte[] decdata = null;
			int hr = 0;

			DATA_BLOB cipher = new DATA_BLOB ();
			DATA_BLOB entropy = new DATA_BLOB ();
			DATA_BLOB data = new DATA_BLOB ();
			try {
				CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT (0);
				cipher.Alloc (encryptedData);
				entropy.Alloc (optionalEntropy);

				// note: the scope/flags has already been check by the public caller
				uint flags = CRYPTPROTECT_UI_FORBIDDEN;
				if (scope == DataProtectionScope.LocalMachine)
					flags |= CRYPTPROTECT_LOCAL_MACHINE;

				if (CryptUnprotectData (ref cipher, null, ref entropy, IntPtr.Zero,
					ref prompt, flags, ref data)) {
					// copy decrypted data back to managed codde
					decdata = data.ToBytes ();
				} else {
					hr = Marshal.GetLastWin32Error ();
				}
			}
			catch (Exception ex) {
				string msg = Locale.GetText ("Error protecting data.");
				throw new CryptographicException (msg, ex);
			}
			finally {
				cipher.Free ();
				data.Free ();
				entropy.Free ();
			}

			if ((decdata == null) || (hr != 0)) {
				throw new CryptographicException (hr);
			}
			return decdata;
		}
	}
}

#endif
