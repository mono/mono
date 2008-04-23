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

// General Information about the System.DirectoryServices assembly

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the system assembly

[assembly: AssemblyTitle ("System.DirectoryServices.dll")]
[assembly: AssemblyDescription ("System.DirectoryServices.dll")]
[assembly: AssemblyDefaultAlias ("System.DirectoryServices.dll")]

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

#if NET_2_0
	[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
	[assembly: AllowPartiallyTrustedCallers]
	[assembly: RuntimeCompatibility (WrapNonExceptionThrows = true)]
	[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
	[assembly: Debuggable (DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
#elif NET_1_1
	[assembly: AssemblyTrademark ("")]
	[assembly: AssemblyConfiguration ("")]
#elif NET_1_0
	[assembly: AssemblyTrademark ("")]
	[assembly: AssemblyConfiguration ("")]
#endif
