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

[assembly: AssemblyTitle ("System.Windows.Forms.dll")]
[assembly: AssemblyDescription ("System.Windows.Forms.dll")]
[assembly: AssemblyDefaultAlias ("System.Windows.Forms.dll")]

[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]
[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]

[assembly: CLSCompliant (true)]
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: ComVisible (false)]
[assembly: AllowPartiallyTrustedCallers]

[assembly: AssemblyDelaySign (true)]

[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
[assembly: Dependency ("System.Drawing,", LoadHint.Always)]
[assembly: Dependency ("System,", LoadHint.Always)]
[assembly: StringFreezing]
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]

[assembly: InternalsVisibleTo("UIAutomationWinforms, PublicKey=00240000048000009400000006020000002400005253413100040000110000004bb98b1af6c1df0df8c02c380e116b7a7f0c8c827aecfccddc6e29b7c754cd608b49dfcef4df9699ad182e50f66afa4e68dabc7b6aeeec0aa4719a5f8e0aae8c193080a706adc3443a8356b1f254142034995532ac176398e12a30f6a74a119a89ac47672c9ae24d7e90de686557166e3b873cd707884431a0451d9d6f7fe795")]
[assembly: InternalsVisibleTo("Mono.WinformsSupport, PublicKey=00240000048000009400000006020000002400005253413100040000110000004bb98b1af6c1df0df8c02c380e116b7a7f0c8c827aecfccddc6e29b7c754cd608b49dfcef4df9699ad182e50f66afa4e68dabc7b6aeeec0aa4719a5f8e0aae8c193080a706adc3443a8356b1f254142034995532ac176398e12a30f6a74a119a89ac47672c9ae24d7e90de686557166e3b873cd707884431a0451d9d6f7fe795")]
[assembly: InternalsVisibleTo("CocoaDriver, PublicKey=0024000004800000940000000602000000240000525341310004000001000100dfb6f531e52a405fce7bb127fdff8b462a29426618ae319093a6479dbc037c76ce025581c272d47806d3c4c9a65304b7ddacff806e6c7e6483f985a5ac39498190c87b7ddb13d3e9c7107f0ceef392ce3fd01391fd9f61199449fd8702ab0d9c2d32dee637bc557ecc7f75c85b350d0d80d8efdb5bdaa6ecaddae0a23a1eb8db")]

