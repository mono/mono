//
// AssemblyInfo.cs
//
// Authors:
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  (sebastien@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the system assembly

#if (NET_1_0)
	[assembly: AssemblyVersion ("1.0.3300.0")]
	[assembly: SatelliteContractVersion ("1.0.3300.0")]
#elif (NET_1_1)
	[assembly: AssemblyVersion ("1.0.5000.0")]
	[assembly: SatelliteContractVersion ("1.0.5000.0")]
#elif (NET_1_2)
	[assembly: AssemblyVersion ("1.2.3400.0")]
	[assembly: SatelliteContractVersion ("1.2.3400.0")]
#endif

[assembly: AssemblyCompany ("MONO development team")]
[assembly: AssemblyCopyright ("(c) 2003-2004 Various Authors")]
[assembly: AssemblyDescription ("Mono.Security.Win32.dll")]
[assembly: AssemblyProduct ("MONO CLI")]
[assembly: AssemblyTitle ("Mono.Security.Win32.dll")]
[assembly: CLSCompliant (true)]
[assembly: ComVisible (false)]
[assembly: NeutralResourcesLanguage ("en-US")]


[assembly:SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode=true)]


//[assembly: AssemblyDelaySign (true)]
//[assembly: AssemblyKeyFile ("")]
