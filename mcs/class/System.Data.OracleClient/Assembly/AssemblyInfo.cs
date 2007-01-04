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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the System.Data.OracleClient assembly

[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

#if (ONLY_1_1)
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
[assembly: TypeLibVersion (1, 10)]
#endif

[assembly: AssemblyTitle ("System.Data.OracleClient.dll")]
[assembly: AssemblyDescription ("System.Data.OracleClient.dll")]
#if !NET_2_0
[assembly: AssemblyConfiguration ("Development version")]
#endif
#if NET_2_0
[assembly: AssemblyFileVersion("2.0.50727.42")]
#endif
[assembly: AssemblyCompany ("MONO development team")]
[assembly: AssemblyProduct ("MONO CLI")]
[assembly: AssemblyCopyright ("(c) 2002-2005 Novell, Inc and Various Authors")]
#if !TARGET_JVM
[assembly: CLSCompliant (true)]
#endif
[assembly: AssemblyDefaultAlias ("System.Data.OracleClient.dll")]
#if !NET_2_0
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyInformationalVersion ("1.1.4322.2032")]
#endif
#if NET_2_0
[assembly: AssemblyInformationalVersion ("2.0.50727.42")]
[assembly: Dependency ("System.Data", LoadHint.Always)]
[assembly: ComCompatibleVersion(1,0,3300,0)]
[assembly: AllowPartiallyTrustedCallers]
#endif
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: ComVisible (false)]

[assembly: AssemblyDelaySign (true)]
#if !TARGET_JVM
[assembly: AssemblyKeyFile ("../ecma.pub")]
#endif
