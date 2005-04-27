//
// AssemblyInfo.cs
//
// Authors:
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about the system assembly

#if (NET_1_0)
	[assembly: AssemblyVersion ("1.0.3300.0")]
	[assembly: SatelliteContractVersion ("1.0.3300.0")]
#elif (NET_2_0)
        [assembly: AssemblyVersion ("2.0.0.0")]
	[assembly: SatelliteContractVersion ("2.0.0.0")]
#elif (NET_1_1)
	[assembly: AssemblyVersion ("1.0.5000.0")]
	[assembly: SatelliteContractVersion ("1.0.5000.0")]
#endif

[assembly: CLSCompliant (true)]
[assembly: ComVisible (false)]
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile ("../msfinal.pub")]

#if NET_2_0
[assembly: AssemblyCompany ("MONO development team")]
[assembly: AssemblyCopyright ("Copyright (C) 2002-2005 Various Authors")]
[assembly: AssemblyDefaultAlias ("System.Security.dll")]
[assembly: AssemblyDescription ("System.Security.dll")]
[assembly: AssemblyFileVersion ("2.0.50215.44")]
[assembly: AssemblyInformationalVersion ("")]
[assembly: AssemblyProduct ("MONO CLI")]
[assembly: AssemblyTitle ("System.Security.dll")]
[assembly: AllowPartiallyTrustedCallers ()]

[module: CompilationRelaxations (0)]
#endif
