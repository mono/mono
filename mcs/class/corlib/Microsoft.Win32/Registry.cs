//
// Microsoft.Win32.Registry.cs
//
// Author:
//   stubbed out by Alexandre Pigolkine (pigolkine@gmx.de)

using System;

namespace Microsoft.Win32
{
	public sealed class Registry
	{
		private Registry () { }
		public static readonly RegistryKey ClassesRoot;
		public static readonly RegistryKey CurrentConfig;
		public static readonly RegistryKey CurrentUser;
		public static readonly RegistryKey DynData;
		public static readonly RegistryKey LocalMachine;
		public static readonly RegistryKey PerformanceData;
		public static readonly RegistryKey Users;
	}
}
