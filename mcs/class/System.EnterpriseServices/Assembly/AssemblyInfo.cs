//
// AssemblyInfo.cs
//
// Author:
//   Marek Safar (marek.safar@seznam.cz)
//
//

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if (NET_1_0)
	[assembly: AssemblyVersion("1.0.3300.0")]
	[assembly: SatelliteContractVersion("1.0.3300.0")]
#endif
#if (NET_1_1)
	[assembly: AssemblyVersion("1.0.5000.0")]
	[assembly: SatelliteContractVersion("1.0.5000.0")]
	[assembly: ComCompatibleVersion(1, 0, 3300, 0)]
	[assembly: TypeLibVersion(1, 10)]
#endif

[assembly: AssemblyTitle("System.EnterpriseServices.dll")]
[assembly: AssemblyDescription("System.EnterpriseServices.dll")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2004 Various Authors")]
[assembly: AssemblyTrademark("")]

[assembly: CLSCompliant(true)]
[assembly: AssemblyDefaultAlias("System.EnterpriseServices.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: ComVisible(false)]
[assembly: AllowPartiallyTrustedCallers]

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]