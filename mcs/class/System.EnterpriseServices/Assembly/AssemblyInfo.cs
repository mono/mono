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
using System.EnterpriseServices;

#if (NET_1_0)
	[assembly: AssemblyVersion("1.0.3300.0")]
	[assembly: SatelliteContractVersion("1.0.3300.0")]
#elif (NET_2_0)
	[assembly: AssemblyVersion("2.0.3600.0")]
	[assembly: SatelliteContractVersion("2.0.3600.0")]
	[assembly: ComVisible(true)]
	[assembly: AssemblyTitle("System.EnterpriseServices.dll")]
	[assembly: AssemblyDescription("System.EnterpriseServices.dll")]
	[assembly: AssemblyConfiguration("Development version")]
	[assembly: AssemblyCompany("MONO development team")]
	[assembly: AssemblyProduct("MONO CLI")]
	[assembly: AssemblyCopyright("(c) 2004 Various Authors")]
	[assembly: AssemblyTrademark("")]
	[assembly: AssemblyInformationalVersion("2.0.40301.9")]
	[assembly: AssemblyFileVersion("2.0.40301.9")]
#elif (NET_1_1)
	[assembly: AssemblyVersion("1.0.5000.0")]
	[assembly: SatelliteContractVersion("1.0.5000.0")]
	[assembly: ComCompatibleVersion(1, 0, 3300, 0)]
	[assembly: TypeLibVersion(1, 10)]
#endif

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: ApplicationID("1e246775-2281-484f-8ad4-044c15b86eb7")]
[assembly: ApplicationName(".NET Utilities")]
[assembly: Guid("4fb2d46f-efc8-4643-bcd0-6e5bfa6a174c")]

[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("../msfinal.pub")]

