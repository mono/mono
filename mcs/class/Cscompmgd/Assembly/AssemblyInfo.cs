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
#if NET_2_0
using System.Diagnostics;
#endif
using System.Reflection;
#if NET_2_0
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
#endif
using System.Runtime.InteropServices;

[assembly: AssemblyVersion (Consts.VsVersion)]
#if (NET_2_0)
[assembly: SatelliteContractVersion (Consts.VsVersion)]
#endif

#if (NET_1_0)
	[assembly: AssemblyDescription ("Managed interface for C# compiler")]
	[assembly: AssemblyFileVersion ("7.0.9466.0")]
	[assembly: AssemblyTitle ("Managed C# Compiler")]
#elif (NET_2_0)
	[assembly: AssemblyDefaultAlias ("cscompmgd.dll")]
	[assembly: AssemblyDescription ("cscompmgd.dll")]
	[assembly: AssemblyFileVersion ("8.0.50215.44")]
	[assembly: AssemblyInformationalVersion ("8.0.50215.44")]
	[assembly: AssemblyTitle ("cscompmgd.dll")]
	[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
	[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
	[assembly: NeutralResourcesLanguage ("en-US")]
	[assembly: ReliabilityContract(Consistency.MayCorruptProcess, Cer.None)]
#elif (NET_1_1)
	[assembly: AssemblyDescription ("Managed interface for C# compiler")]
	[assembly: AssemblyFileVersion ("7.10.3052.4")]
	[assembly: AssemblyTitle ("Managed C# Compiler")]
#endif

[assembly: CLSCompliant (true)]

[assembly: ComVisible (false)]

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile("../msfinal.pub")]

