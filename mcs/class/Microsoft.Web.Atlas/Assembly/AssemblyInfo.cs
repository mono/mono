//
// AssemblyInfo.cs
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc.  http://www.novell.com
//

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web.UI;

// General Information about the System.Web assembly

[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

[assembly: AssemblyTitle("Microsoft.Web.Atlas.dll")]
[assembly: AssemblyDescription("Microsoft.Web.Atlas.dll")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2005 Various Authors")]
[assembly: AssemblyTrademark("")]
#if TARGET_JVM
[assembly: CLSCompliant(false)]
#else
[assembly: CLSCompliant(true)]
#endif

[assembly: ComVisible(false)]
[assembly: AssemblyDefaultAlias("Microsoft.Web.Atlas.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

#if !TARGET_JVM
//[assembly: AssemblyDelaySign(true)]
//[assembly: AssemblyKeyFile("../msfinal.pub")]
#endif
