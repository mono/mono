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
using System.Web.UI;

// General Information about the system assembly

#if (NET_1_0)
	[assembly: AssemblyVersion("1.0.3300.0")]
	[assembly: SatelliteContractVersion("1.0.3300.0")]
#endif
#if (NET_1_1)
	[assembly: AssemblyVersion("1.0.5000.0")]
	[assembly: SatelliteContractVersion("1.0.5000.0")]
#endif

[assembly: AssemblyTitle("System.Web.dll")]
[assembly: AssemblyDescription("System.Web.dll")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]
[assembly: AssemblyTrademark("")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: AssemblyDefaultAlias("System.Web.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AllowPartiallyTrustedCallers]

[assembly: TagPrefix("System.Web.UI.WebControls", "asp")]

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]