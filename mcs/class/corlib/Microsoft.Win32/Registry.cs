//
// Microsoft.Win32.Registry.cs
//
// Author:
//   stubbed out by Alexandre Pigolkine (pigolkine@gmx.de)
//

using System;

namespace Microsoft.Win32
{
	public sealed class Registry
	{
		private Registry () { }
		public static readonly RegistryKey ClassesRoot = new RegistryKey (
				RegistryHive.ClassesRoot, "HKEY_CLASSES_ROOT");
		public static readonly RegistryKey CurrentConfig = new RegistryKey (
				RegistryHive.CurrentConfig, "HKEY_CURRENT_CONFIG");
		public static readonly RegistryKey CurrentUser = new RegistryKey (
				RegistryHive.CurrentUser, "HKEY_CURRENT_USER");
		public static readonly RegistryKey DynData = new RegistryKey (
				RegistryHive.DynData, "HKEY_DYN_DATA");
		public static readonly RegistryKey LocalMachine = new RegistryKey (
				RegistryHive.LocalMachine, "HKEY_DYN_DATA");
		public static readonly RegistryKey PerformanceData = new RegistryKey (
				RegistryHive.PerformanceData, "HKEY_PERFORMANCE_DATA");
		public static readonly RegistryKey Users = new RegistryKey (
				RegistryHive.Users, "HKEY_USERS");
	}
}
