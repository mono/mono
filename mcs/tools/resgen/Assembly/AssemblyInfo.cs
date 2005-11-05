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

[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

[assembly: AssemblyInformationalVersion(Consts.FxFileVersion)]
#if (NET_2_0)
	[assembly: AssemblyFileVersion(Consts.FxFileVersion)]
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
