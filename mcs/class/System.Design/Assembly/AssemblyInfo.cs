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

// General Information about the System.Design assembly

#if (NET_1_0)
	[assembly: AssemblyVersion ("1.0.3300.0")]
	[assembly: SatelliteContractVersion ("1.0.3300.0")]
#endif
#if (NET_1_1)
	[assembly: AssemblyVersion ("1.0.5000.0")]
	[assembly: SatelliteContractVersion ("1.0.5000.0")]
	[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
	[assembly: TypeLibVersion (1, 10)]
#endif

[assembly: AssemblyTitle ("System.Design.dll")]
[assembly: AssemblyDescription ("System.Design.dll")]
[assembly: AssemblyConfiguration ("Development version")]
[assembly: AssemblyCompany ("MONO development team")]
[assembly: AssemblyProduct ("MONO CLI")]
[assembly: AssemblyCopyright ("(c) 2003 Various Authors")]
[assembly: AssemblyTrademark ("")]

// FIXME: add once CLS compliance is reached (CLS compliance requests that Accessibility needs to be referenced - doesn't on mcs, probably a mcs bug)
//[assembly: CLSCompliant (true)]
[assembly: AssemblyDefaultAlias ("System.Design.dll")]
[assembly: AssemblyInformationalVersion ("0.0.0.1")]
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: ComVisible (false)]

//[assembly: AssemblyDelaySign (true)]
//[assembly: AssemblyKeyFile ("")]