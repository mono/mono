// System.Configuration.Install.InstallEventHandler.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Runtime.Serialization;

namespace System.Configuration.Install
{
	[Serializable]
	public delegate void InstallEventHandler (object sender, InstallEventArgs e);
}
