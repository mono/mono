//
// System.Security.Principal.WindowsBuiltInRole.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Security.Principal {

	[Serializable]
	public enum WindowsBuiltInRole {
		Administrator = 544,
		User = 545,
		Guest = 546,
		PowerUser = 547,
		AccountOperator = 548,
		SystemOperator = 549,
		PrintOperator = 550,
		BackupOperator = 551,
		Replicator = 552,
	}
}
