// 
// System.EnterpriseServices.InstallationFlags.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.EnterpriseServices {
	[Flags]
	[Serializable]
	public enum InstallationFlags {
		Configure,
		ConfigureComponentsOnly,
		CreateTargetApplication,
		Default,
		ExpectExistingTypeLib,
		FindOrCreateTargetApplication,
		Install,
		ReconfigureExistingApplication,
		Register,
		ReportWarningsToConsole
	}
}
