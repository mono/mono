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

	public sealed class ProtectedMemory {

		[MonoTODO]
		public static void Protect (byte[] userData, MemoryProtectionScope scope) 
		{
		}

		[MonoTODO]
		public static void Unprotect (byte[] encryptedData, MemoryProtectionScope scope) 
		{
		}
	} 
}

#endif