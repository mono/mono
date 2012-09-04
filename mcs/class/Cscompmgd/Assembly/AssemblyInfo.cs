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
using System.Runtime.CompilerServices;
#if NET_2_0
	using System.Runtime.ConstrainedExecution;
#endif
using System.Runtime.InteropServices;

// General Information about the cscompmgd assembly

[assembly: AssemblyVersion (Consts.VsVersion)]
[assembly: AssemblyFileVersion (Consts.VsFileVersion)]

[assembly: CLSCompliant (true)]
[assembly: ComVisible (false)]

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile ("../msfinal.pub")]

[assembly: AssemblyTitle ("cscompmgd.dll")]
[assembly: AssemblyDefaultAlias ("cscompmgd.dll")]
[assembly: AssemblyDescription ("cscompmgd.dll")]
[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: SatelliteContractVersion (Consts.VsVersion)]
[assembly: AssemblyInformationalVersion (Consts.VsFileVersion)]
[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
[assembly: NeutralResourcesLanguage ("en-US")]
[assembly: ReliabilityContract (Consistency.MayCorruptProcess, Cer.None)]
