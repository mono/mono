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
using System.Runtime.InteropServices;
using System.Resources;

#if (NET_2_0)
	[assembly: AssemblyVersion ("2.0.3600.0")]
#elif (NET_1_1)
	[assembly: AssemblyVersion("7.0.5000.0")]
	[assembly: SatelliteContractVersion("7.0.5000.0")]
	[assembly: ComCompatibleVersion(7, 0, 3300, 0)]
	[assembly: TypeLibVersion(7, 1)]
#else
	[assembly: AssemblyVersion("7.0.3300.0")]
	[assembly: SatelliteContractVersion("7.0.3300.0")]
#endif

/* TODO COMPLETE INFORMATION

[assembly: AssemblyTitle ("")]
[assembly: AssemblyDescription ("")]

[assembly: CLSCompliant (true)]
[assembly: AssemblyFileVersion ("0.0.0.1")]

[assembly: ComVisible (false)]

*/

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile("../msfinal.pub")]

