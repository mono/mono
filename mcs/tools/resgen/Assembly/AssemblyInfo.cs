//
// AssemblyInfo.cs
//
// Author:
//   Gert Driesen (drieseng@user.sourceforge.net)
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the system assembly

#if (NET_1_0)
	[assembly: AssemblyVersion("1.0.3300.0")]
	[assembly: SatelliteContractVersion("1.0.3300.0")]
	[assembly: AssemblyInformationalVersion("1.0.3705.0")]
#elif (NET_2_0)
        [assembly: AssemblyVersion("2.0.3600.0")]
	[assembly: SatelliteContractVersion("2.0.3600.0")]
	[assembly: AssemblyInformationalVersion("2.0.40301.9")]
	[assembly: AssemblyFileVersion("2.0.40301.9")]
#elif (NET_1_1)
	[assembly: AssemblyVersion("1.0.5000.0")]
	[assembly: SatelliteContractVersion("1.0.5000.0")]
	[assembly: AssemblyInformationalVersion("1.1.4322.573")]
#endif

[assembly: AssemblyTitle("ResGen.exe")]
[assembly: AssemblyDescription("ResGen.exe")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]
[assembly: AssemblyTrademark("")]

[assembly: CLSCompliant(true)]
[assembly: AssemblyDefaultAlias("ResGen.exe")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: ComVisible(false)]
