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
		
		ClassesRoot = -2147483648,
		CurrentConfig = -2147483643,
		CurrentUser = -2147483647,
		DynData = -2147483642,
		LocalMachine = -2147483646,
		PerformanceData = -2147483644,
		Users = -2147483645
	}

}
