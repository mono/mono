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

#if (NET_1_0)
	[assembly: AssemblyVersion ("1.0.3300.0")]
	[assembly: SatelliteContractVersion ("1.0.3300.0")]
#elif (NET_2_0)
        [assembly: AssemblyVersion("2.0.3600.0")]
	[assembly: SatelliteContractVersion("2.0.3600.0")]
#elif (NET_1_1)
	[assembly: AssemblyVersion ("1.0.5000.0")]
	[assembly: SatelliteContractVersion ("1.0.5000.0")]
	[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
	[assembly: TypeLibVersion (1, 10)]
#endif

[assembly: AssemblyTitle ("System.Data.OracleClient.dll")]
[assembly: AssemblyDescription ("System.Data.OracleClient.dll")]
[assembly: AssemblyConfiguration ("Development version")]
[assembly: AssemblyCompany ("MONO development team")]
[assembly: AssemblyProduct ("MONO CLI")]
[assembly: AssemblyCopyright ("(c) 2002-2005 Novell, Inc and Various Authors")]
[assembly: AssemblyTrademark ("")]

[assembly: CLSCompliant (true)]
[assembly: AssemblyDefaultAlias ("System.Data.OracleClient.dll")]
[assembly: AssemblyInformationalVersion ("0.0.0.1")]
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: ComVisible (false)]

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile ("../ecma.pub")]
