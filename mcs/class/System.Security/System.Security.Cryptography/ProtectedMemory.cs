//
// ProtectedMemory.cs: Protect (encrypt) memory without (user involved) key management
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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

#if NET_2_0

using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Security.Cryptography {

	// References:
	// a.	Windows Data Protection
	//	http://msdn.microsoft.com/library/en-us/dnsecure/html/windataprotection-dpapi.asp?frame=true

	public sealed class ProtectedMemory {

		private ProtectedMemory ()
		{
		}

		[MonoTODO ("only supported on Windows 2000 SP3 and later")]
// FIXME	[DataProtectionPermission (SecurityAction.Demand, ProtectMemory = true)]
		public static void Protect (byte[] userData, MemoryProtectionScope scope) 
		{
			if (userData == null)
				throw new ArgumentNullException ("userData");

			Check (userData.Length, scope);
			try {
				uint flags = (uint) scope;
				uint length = (uint) userData.Length;

				switch (impl) {
				case MemoryProtectionImplementation.Win32RtlEncryptMemory:
					int err = RtlEncryptMemory (userData, length, flags);
					if (err < 0) {
						string msg = Locale.GetText ("Error. NTSTATUS = {0}.", err);
						throw new CryptographicException (msg);
					}
					break;
				case MemoryProtectionImplementation.Win32CryptoProtect:
					bool result = CryptProtectMemory (userData, length, flags);
					if (!result)
						throw new CryptographicException (Marshal.GetLastWin32Error ());
					break;
				default:
					throw new PlatformNotSupportedException ();
				}
			}
			catch {
				// Windows 2000 before SP3 will throw
				impl = MemoryProtectionImplementation.Unsupported;
				throw new PlatformNotSupportedException ();
			}
		}

		[MonoTODO ("only supported on Windows 2000 SP3 and later")]
// FIXME	[DataProtectionPermission (SecurityAction.Demand, UnprotectMemory = true)]
		public static void Unprotect (byte[] encryptedData, MemoryProtectionScope scope) 
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			Check (encryptedData.Length, scope);
			try {
				uint flags = (uint) scope;
				uint length = (uint) encryptedData.Length;

				switch (impl) {
				case MemoryProtectionImplementation.Win32RtlEncryptMemory:
					int err = RtlDecryptMemory (encryptedData, length, flags);
					if (err < 0) {
						string msg = Locale.GetText ("Error. NTSTATUS = {0}.", err);
						throw new CryptographicException (msg);
					}
					break;
				case MemoryProtectionImplementation.Win32CryptoProtect:
					bool result = CryptUnprotectMemory (encryptedData, length, flags);
					if (!result)
						throw new CryptographicException (Marshal.GetLastWin32Error ());
					break;
				default:
					throw new PlatformNotSupportedException ();
				}
			}
			catch {
				// Windows 2000 before SP3 will throw
				impl = MemoryProtectionImplementation.Unsupported;
				throw new PlatformNotSupportedException ();
			}
		}

		// private stuff

		private const int BlockSize = 16;

		enum MemoryProtectionImplementation {
			Unknown,
			Win32RtlEncryptMemory,
			Win32CryptoProtect,
			Unsupported = Int32.MinValue
		}

		private static MemoryProtectionImplementation impl;

		private static void Detect ()
		{
			OperatingSystem os = Environment.OSVersion;
			switch (os.Platform) {
			case PlatformID.Win32NT:
				Version v = os.Version;
				if (v.Major < 5) {
					impl = MemoryProtectionImplementation.Unsupported;
				} else if (v.Major == 5) {
					if (v.Minor < 2) {
						// 2000 (5.0) Service Pack 3 and XP (5.1)
						impl = MemoryProtectionImplementation.Win32RtlEncryptMemory;
					} else {
						impl = MemoryProtectionImplementation.Win32CryptoProtect;
					}
				} else {
					// vista (6.0) and later
					impl = MemoryProtectionImplementation.Win32CryptoProtect;
				}
				break;
			default:
				impl = MemoryProtectionImplementation.Unsupported;
				break;
			}
		}

		private static void Check (int size, MemoryProtectionScope scope)
		{
			if (size % BlockSize != 0) {
				string msg = Locale.GetText ("Not a multiple of {0} bytes.", BlockSize);
				throw new CryptographicException (msg);
			}

			if ((scope < MemoryProtectionScope.SameProcess) || (scope > MemoryProtectionScope.SameLogon)) {
				string msg = Locale.GetText ("Invalid enum value for '{0}'.", "MemoryProtectionScope");
				throw new ArgumentException (msg, "scope");
			}

			switch (impl) {
			case MemoryProtectionImplementation.Unknown:
				Detect ();
				break;
			case MemoryProtectionImplementation.Unsupported:
				throw new PlatformNotSupportedException ();
			}
		}

		// http://msdn.microsoft.com/library/en-us/dncode/html/secure06122003.asp
		// Summary: CryptProtectMemory and CryptUnprotectMemory exists only in Windows 2003 +
		// but they are available in advapi32.dll as RtlEncryptMemory (SystemFunction040) and
		// RtlDecryptMemory (SystemFunction041) since Windows 2000 SP 3. Sadly both can disappear
		// anytime with newer OS so we include support for Crypt[Unp|P]rotectMemory too.

		[SuppressUnmanagedCodeSecurity]
		[DllImport ("advapi32.dll", EntryPoint="SystemFunction040", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		private static extern int RtlEncryptMemory (byte[] pData, uint cbData, uint dwFlags);

		[SuppressUnmanagedCodeSecurity]
		[DllImport ("advapi32.dll", EntryPoint = "SystemFunction041", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		private static extern int RtlDecryptMemory (byte[] pData, uint cbData, uint dwFlags);

		[SuppressUnmanagedCodeSecurity]
		[DllImport ("crypt32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		private static extern bool CryptProtectMemory (byte[] pData, uint cbData, uint dwFlags);

		[SuppressUnmanagedCodeSecurity]
		[DllImport ("crypt32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		private static extern bool CryptUnprotectMemory (byte[] pData, uint cbData, uint dwFlags);
	} 
}

#endif
