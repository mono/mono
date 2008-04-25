//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Resources;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the System.Net assembly
// v3.5 Assembly

[assembly: AssemblyTitle ("System.Net.dll")]
[assembly: AssemblyDescription ("System.Net.dll")]
[assembly: AssemblyDefaultAlias ("System.Net.dll")]

[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]
[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]
[assembly: AssemblyFileVersion (Consts.FxFileVersion)]

[assembly: NeutralResourcesLanguage ("en-US")]
[assembly: CLSCompliant (true)]
[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile ("../ecma.pub")]

[assembly: ComVisible (false)]
[assembly: AllowPartiallyTrustedCallers]

[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
[assembly: Debuggable (DebuggableAttribute.DebuggingModes.DisableOptimizations | 
                       DebuggableAttribute.DebuggingModes.EnableEditAndContinue | 
                       DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: RuntimeCompatibility (WrapNonExceptionThrows = true)]
[assembly: SecurityCritical]
[assembly: SecurityPermission (SecurityAction.RequestMinimum, Execution = true)]
[assembly: SecurityPermission (SecurityAction.RequestMinimum, SkipVerification = true)]





[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

#if (ONLY_1_1)
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
[assembly: TypeLibVersion (1, 10)]
#endif

[assembly: AssemblyTitle("System.Net.dll")]
[assembly: AssemblyDescription("System.Net.dll")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]
[assembly: AssemblyTrademark("")]

[assembly: CLSCompliant(true)]
[assembly: AssemblyDefaultAlias("System,Net.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AllowPartiallyTrustedCallers]
[assembly: ComVisible(false)]

#if ! BOOTSTRAP_WITH_OLDLIB
[assembly: SecurityPermission (SecurityAction.RequestMinimum, SkipVerification=true)]
[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("../ecma.pub")]
#endif
