//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the system assembly

[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

#if (ONLY_1_1)
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
[assembly: TypeLibVersion (1, 10)]
#endif

//[assembly: AssemblyTitle("mscorlib.dll")]
[assembly: AssemblyDescription("Common Language Runtime Library")]
//[assembly: AssemblyConfiguration("Development version")]
//[assembly: AssemblyCompany("MONO development team")]
//[assembly: AssemblyProduct("MONO CLI")]
//[assembly: AssemblyCopyright("(c) 2003 Various Authors")]

[assembly: CLSCompliant(true)]
//[assembly: AssemblyDefaultAlias("mscorlib.dll")]
//[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AllowPartiallyTrustedCallers]
[assembly: Guid("BED7F4EA-1A96-11D2-8F08-00A0C9A6186D")]

#if ! BOOTSTRAP_WITH_OLDLIB
[assembly: SecurityPermission (SecurityAction.RequestMinimum, SkipVerification=true)]
[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("../ecma.pub")]
#endif

#if NET_2_1
[assembly: InternalsVisibleTo ("System.SilverLight, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293")]
#endif
