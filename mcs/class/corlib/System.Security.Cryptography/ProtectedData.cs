//
// ProtectedData.cs: Protect (encrypt) data without (user involved) key management
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

	public sealed class ProtectedData {

		// FIXME: interop could be important under windows - if one application protect some data using
		// mono and another one unprotects it using ms.net

		[MonoTODO ("interop with MS implementation ?")]
		public static byte[] Protect (byte[] userData, byte[] optionalEntropy, DataProtectionScope scope) 
		{
			if (userData == null)
				throw new ArgumentNullException ("userData");

			// on Windows this is supported only under 2000 and later OS
			throw new PlatformNotSupportedException ();
		}

		[MonoTODO ("interop with MS implementation ?")]
		public static byte[] Unprotect (byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope) 
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			// on Windows this is supported only under 2000 and later OS
			throw new PlatformNotSupportedException ();
		}
 	} 
}

#endif