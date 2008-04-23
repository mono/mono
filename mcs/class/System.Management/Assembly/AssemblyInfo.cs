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
using System.Security;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the System.Management assembly
// This one is very different for version < 2.0

#if NET_2_0
	[assembly: AssemblyTitle ("System.Management.dll")]
	[assembly: AssemblyDescription ("System.Management.dll")]
	[assembly: AssemblyDefaultAlias ("System.Management.dll")]
#endif
[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: AssemblyVersion (Consts.FxVersion)]
#if NET_2_0
	[assembly: SatelliteContractVersion (Consts.FxVersion)]
	[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]

	[assembly: NeutralResourcesLanguage ("en-US")]
#endif
[assembly: ComVisible (false)]

[assembly: CLSCompliant (true)]
[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile("../msfinal.pub")]

#if NET_2_0
	[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
	[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
	[assembly: Debuggable (DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
	[assembly: RuntimeCompatibility (WrapNonExceptionThrows = true)]
#elif NET_1_1 || NET_1_0
	[assembly: AssemblyTitle ("System.Management")]
	[assembly: AssemblyDescription ("This assembly contains the classes necessary to access management information from managed code")]
	[assembly: AssemblyTrademark ("")]
	[assembly: AssemblyConfiguration ("")]
	[assembly: AssemblyKeyName ("")]
#endif
