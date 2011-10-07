//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// (C) 2010 Novell, Inc (http://novell.com/)
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
using System.Security.Permissions;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile ("../ecma.pub")]

#if NET_4_0
[assembly: System.Security.SecurityCritical]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: AllowPartiallyTrustedCallers]
[assembly: AssemblyTitle("System.Data.Services.Client.dll")]
[assembly: AssemblyDescription("System.Data.Services.Client.dll")]
[assembly: AssemblyDefaultAlias("System.Data.Services.Client.dll")]
[assembly: AssemblyCompany(Consts.MonoCompany)]
[assembly: AssemblyProduct(Consts.MonoProduct)]
[assembly: AssemblyCopyright(Consts.MonoCopyright)]
[assembly: AssemblyVersion(Consts.FxVersion)]
[assembly: SatelliteContractVersion(Consts.FxVersion)]
[assembly: AssemblyInformationalVersion(Consts.FxFileVersion)]
[assembly: AssemblyFileVersion(Consts.FxFileVersion)]

[assembly: Debuggable (DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: NeutralResourcesLanguage ("en-US")]
[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
[assembly: SecurityRules (SecurityRuleSet.Level1, SkipVerificationInFullTrust=true)]

internal static class FX35Assembly
{
    internal const string Version = "4.0.0.0";
}

internal static class AssemblyRef
{
    internal const string MicrosoftPublicKeyToken = "b03f5f7f11d50a3a";

    internal const string EcmaPublicKeyToken = "b77a5c561934e089";
}
#endif
