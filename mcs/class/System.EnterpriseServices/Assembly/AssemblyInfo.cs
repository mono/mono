//
// AssemblyInfo.cs
//
// Author:
//   Marek Safar (marek.safar@seznam.cz)
//
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.EnterpriseServices;

[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

#if (ONLY_1_1)
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
[assembly: TypeLibVersion (1, 10)]
#elif (NET_2_0)
[assembly: ComVisible(true)]
[assembly: AssemblyTitle("System.EnterpriseServices.dll")]
[assembly: AssemblyDescription("System.EnterpriseServices.dll")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2004 Various Authors")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyInformationalVersion(Consts.FxFileVersion)]
[assembly: AssemblyFileVersion(Consts.FxFileVersion)]
#endif

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: ApplicationID("1e246775-2281-484f-8ad4-044c15b86eb7")]
[assembly: ApplicationName(".NET Utilities")]
[assembly: Guid("4fb2d46f-efc8-4643-bcd0-6e5bfa6a174c")]

[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("../msfinal.pub")]

