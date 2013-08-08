//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
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

[assembly: AssemblyTitle ("System.DirectoryServices.Protocols.dll")]
[assembly: AssemblyDescription ("System.DirectoryServices.Protocols.dll")]
[assembly: AssemblyDefaultAlias ("System.DirectoryServices.Protocols.dll")]

[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]
[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]

[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: ComVisible (false)]

#if !TARGET_JVM
	[assembly: CLSCompliant (true)]
	[assembly: AssemblyDelaySign (true)]
	[assembly: AssemblyKeyFile ("../msfinal.pub")]
#endif

[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
[assembly: AllowPartiallyTrustedCallers]
[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
