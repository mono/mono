// System.Configuration.Installer.IManagedInstaller.cs
//
// Author:
// 	Alejandro Sánchez Acosta
//
// (C) Alejandro Sánchez Acosta
// 

using System.Runtime.InteropServices;

namespace System.Configuration.Installer
{
	//[Guid("")]
	//[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IManagedInstaller
	{
		int ManagedInstall (string commandLine, int hInstall);
	}
}
