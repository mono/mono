//
// MemoryProtectionScope.cs: Scope for ProtectMemory
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography {

	public enum MemoryProtectionScope {
		SameProcess,
		CrossProcess,
		SameLogon
	} 
}

#endif