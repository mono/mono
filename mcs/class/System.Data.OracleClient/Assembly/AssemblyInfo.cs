//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the System.Data.OracleClient assembly

[assembly: AssemblyTitle ("System.Data.OracleClient.dll")]
[assembly: AssemblyDescription ("System.Data.OracleClient.dll")]
[assembly: AssemblyDefaultAlias ("System.Data.OracleClient.dll")]

[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]
[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]

[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: ComVisible (false)]

#if !TARGET_JVM
	[assembly: CLSCompliant (true)]
	[assembly: AssemblyDelaySign (true)]
	[assembly: AssemblyKeyFile ("../ecma.pub")]
#endif

[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
[assembly: AllowPartiallyTrustedCallers]
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
[assembly: Dependency ("System.Data,", LoadHint.Always)]
