//
// DataProtectionScope.cs: Scope for ProtectData
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_2_0

using System;

namespace System.Security.Cryptography {

	public enum DataProtectionScope {
		CurrentUser,
		LocalMachine
 	}
}

#endif