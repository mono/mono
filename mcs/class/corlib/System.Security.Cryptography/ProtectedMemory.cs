//
// ProtectedMemory.cs: Protect (encrypt) memory without (user involved) key management
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography {

	// References:
	// a.	Windows Data Protection
	//	http://msdn.microsoft.com/library/en-us/dnsecure/html/windataprotection-dpapi.asp?frame=true

	public sealed class ProtectedMemory {

		[MonoTODO]
		public static void Protect (byte[] userData, MemoryProtectionScope scope) 
		{
			if (userData == null)
				throw new ArgumentNullException ("userData");
			if (userData.Length % 16 != 0)
				throw new CryptographicException ("not a multiple of 16 bytes");

			// on Windows this is supported only under XP and later OS
			throw new PlatformNotSupportedException ();
		}

		[MonoTODO]
		public static void Unprotect (byte[] encryptedData, MemoryProtectionScope scope) 
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");
			if (encryptedData.Length % 16 != 0)
				throw new CryptographicException ("not a multiple of 16 bytes");

			// on Windows this is supported only under XP and later OS
			throw new PlatformNotSupportedException ();
		}
	} 
}

#endif