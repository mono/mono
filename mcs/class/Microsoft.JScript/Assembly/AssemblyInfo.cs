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
using System.Runtime.InteropServices;
using System.Security;
using System.Resources;

#if (NET_2_0)
	[assembly: AssemblyVersion ("8.0.3600.0")]
//	[assembly: CLSCompliant (true)]
#elif (NET_1_1)
	[assembly: AssemblyVersion("7.0.5000.0")]
	[assembly: SatelliteContractVersion("7.0.5000.0")]
	[assembly: TypeLibVersion(7, 1)]
#else
	[assembly: AssemblyVersion("7.0.3300.0")]
	[assembly: SatelliteContractVersion("7.0.3300.0")]
#endif

[assembly: AssemblyTitle ("Microsoft.JScript")]
[assembly: AssemblyDescription("Microsoft.JScript.dll")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AllowPartiallyTrustedCallers()]

[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("../msfinal.pub")]
