//
// Microsoft.Win32.RegistryHive.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)

using System;

namespace Microsoft.Win32
{
	[Serializable]
	public enum RegistryHive
	{
		ClassesRoot = 0x80000000,
		CurrentConfig = 0x80000005,
		CurrentUser = 0x80000001,
		DynData = 0x80000006,
		LocalMachine = 0x80000002,
		PerformanceData = 0x80000004,
		Users = 0x80000003
	}

}