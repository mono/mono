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

// General Information about the system assembly

#if (NET_1_0)
	[assembly: AssemblyVersion("1.0.3300.0")]
	[assembly: SatelliteContractVersion("1.0.3300.0")]
#endif
#if (NET_1_1)
	[assembly: AssemblyVersion("1.0.5000.0")]
	[assembly: SatelliteContractVersion("1.0.5000.0")]
#endif

[assembly: AssemblyTitle("System.dll")]
[assembly: AssemblyDescription("System.dll")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]
[assembly: AssemblyTrademark("")]

[assembly: CLSCompliant(true)]
[assembly: AssemblyDefaultAlias("System.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AllowPartiallyTrustedCallers]

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]