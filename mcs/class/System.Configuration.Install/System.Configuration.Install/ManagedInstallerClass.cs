// System.Configuration.Install.ManagedInstallerClass.cs
//
// Author:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Novell
//

using System.Runtime.InteropServices;

namespace System.Configuration.Install
{
	[GuidAttribute ("42EB0342-0393-448f-84AA-D4BEB0283595")]
	[ComVisible (true)]
	public class ManagedInstallerClass : IManagedInstaller
	{
		public ManagedInstallerClass ()
		{
		}

		[MonoTODO]
		public static void InstallHelper (string[] args)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IManagedInstaller.ManagedInstall (string argString, int hInstall)
		{
			throw new NotImplementedException ();
		}
	}
}
