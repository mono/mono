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
//
// Authors:
//
//   	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


[assembly: AssemblyVersion ("3.0.0.0")]
[assembly: SatelliteContractVersion ("3.0.0.0")]
[assembly: AssemblyFileVersion("3.0.0.0")]
[assembly: CompilationRelaxations(8)]
[assembly: AssemblyTitle("System.Workflow.ComponentModel.dll")]
[assembly: AssemblyDescription("System.Workflow.ComponentModel.dll")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2006 Various Authors")]


#if !TARGET_JVM
[assembly: CLSCompliant(true)]
#endif
[assembly: AssemblyDefaultAlias("System.Workflow.ComponentModel.dll")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
[assembly: ComVisible(false)]
[assembly: AllowPartiallyTrustedCallers]

#if TARGET_JVM
[assembly: AssemblyDelaySign(false)]
#else
[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("../msfinal3.pub")]
#endif

