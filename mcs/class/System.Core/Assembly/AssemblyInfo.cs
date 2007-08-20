//
// AssemblyInfo.cs
//
// Authors:
//	Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Security;
using System.Security.Permissions;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("System.Core.dll")]
[assembly: AssemblyDescription("System.Core.dll")]
[assembly: AssemblyCompany("Mono Development Team")]
[assembly: AssemblyProduct("System.Core")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
[assembly: AssemblyDefaultAlias ("System.Core.dll")]
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible (false)]

[assembly: AssemblyVersion(Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]
[assembly: AssemblyInformationalVersionAttribute (Consts.FxVersion)]

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile ("../ecma.pub")]
[assembly: SecurityPermission (SecurityAction.RequestMinimum, SkipVerification=true)]
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityCritical]

