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

// General Information about the Microsoft.VisualBasic assembly

#if (NET_1_0)
	[assembly: AssemblyVersion("7.0.3300.0")]
	[assembly: SatelliteContractVersion("7.0.3300.0")]
#endif
#if (NET_1_1)
	[assembly: AssemblyVersion("7.0.5000.0")]
	[assembly: SatelliteContractVersion("7.0.5000.0")]
	[assembly: ComCompatibleVersion(7, 0, 3300, 0)]
	[assembly: TypeLibVersion(7, 1)]
#endif

[assembly: AssemblyTitle("Microsoft.VisualBasic.dll")]
[assembly: AssemblyDescription("Microsoft.VisualBasic.dll")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AllowPartiallyTrustedCallers()]

[assembly: AssemblyDelaySign(true)]
//[assembly: AssemblyKeyFile("")]