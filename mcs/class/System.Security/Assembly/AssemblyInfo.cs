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

// General Information about the system assembly

#if (NET_1_0)
	[assembly: AssemblyVersion ("1.0.3300.0")]
	[assembly: SatelliteContractVersion ("1.0.3300.0")]
#endif
#if (NET_1_1)
	[assembly: AssemblyVersion ("1.0.5000.0")]
	[assembly: SatelliteContractVersion ("1.0.5000.0")]
#endif
#if (NET_1_2)
	[assembly: AssemblyVersion ("1.2.3400.0")]
	[assembly: SatelliteContractVersion ("1.2.3400.0")]
	[assembly: AssemblyCompany ("MONO development team")]
	[assembly: AssemblyCopyright ("(c) 2003-2004 Various Authors")]
	[assembly: AssemblyDescription ("System.Security.dll")]
	[assembly: AssemblyProduct ("MONO CLI")]
	[assembly: AssemblyTitle ("System.Security.dll")]
#endif

[assembly: CLSCompliant (true)]
[assembly: ComVisible (false)]
[assembly: NeutralResourcesLanguage ("en-US")]

//[assembly: AssemblyDelaySign (true)]
//[assembly: AssemblyKeyFile ("")]
