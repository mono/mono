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

	public sealed class ProtectedData {

		[MonoTODO ("interop with MS implementation ?")]
		public static byte[] Protect (byte[] userData, byte[] optionalEntropy, DataProtectionScope scope) 
		{
			return userData;
		}

		[MonoTODO ("interop with MS implementation ?")]
		public static byte[] Unprotect (byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope) 
		{
			return encryptedData;
		}
 	} 
}

#endif