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
using System.Diagnostics;

// General Information about the System.Windows.Forms assembly

[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

#if (ONLY_1_1)
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
[assembly: TypeLibVersion (1, 10)]
#endif

#if ONLY_1_0 || ONLY_1_1
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyTrademark("")]
#endif

#if NET_2_0
[assembly: AssemblyFileVersion("2.0.50727.42")]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: CompilationRelaxations(CompilationRelaxations.NoStringInterning)]
[assembly: Dependency("System.Drawing,", LoadHint.Always)]
[assembly: Dependency("System,", LoadHint.Always)]
[assembly: StringFreezing]
[assembly: ComCompatibleVersion(1, 0, 3300, 0)]
#endif

[assembly: AssemblyTitle("System.Windows.Forms.dll")]
[assembly: AssemblyDescription("System.Windows.Forms.dll")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: AssemblyDefaultAlias("System.Windows.Forms.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AllowPartiallyTrustedCallers()]

[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("../ecma.pub")]
