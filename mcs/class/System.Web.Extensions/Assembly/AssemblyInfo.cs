//
// AssemblyInfo.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;
using System;
using System.Diagnostics;
using System.Web.UI;
using System.Security;
using System.Security.Permissions;

// General Information about the System.Web.Extensions assembly
// v3.5 Assembly

[assembly: AssemblyTitle ("System.Web.Extensions.dll")]
[assembly: AssemblyDescription ("System.Web.Extensions.dll")]
[assembly: AssemblyDefaultAlias ("System.Web.Extensions.dll")]

[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
#if NET_3_5
	[assembly: AssemblyVersion (Consts.FxVersion)]
	[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]
#else
	[assembly: AssemblyVersion ("1.0.61025.0")]
	[assembly: AssemblyInformationalVersion ("1.0.61025.0")]
#endif
[assembly: SatelliteContractVersion (Consts.FxVersion)]
[assembly: AssemblyFileVersion (Consts.FxFileVersion)]

[assembly: NeutralResourcesLanguage ("en-US")]

#if !(TARGET_JVM || TARGET_DOTNET)
	[assembly: CLSCompliant (true)]
	[assembly: AssemblyDelaySign (true)]
	[assembly: AssemblyKeyFile ("../winfx.pub")]
#endif

[assembly: ComVisible (false)]
[assembly: AllowPartiallyTrustedCallers]

[assembly: TagPrefix ("System.Web.UI", "asp")]
[assembly: TagPrefix ("System.Web.UI.WebControls", "asp")]
[assembly: Dependency ("System,", LoadHint.Always)]

[assembly: SecurityPermission (SecurityAction.RequestMinimum, Execution = true)]
[assembly: SecurityPermission (SecurityAction.RequestMinimum, SkipVerification = true)]

[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
[assembly: Debuggable (DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: RuntimeCompatibility (WrapNonExceptionThrows = true)]
// Extension attribute should be added by compiler

[assembly: InternalsVisibleTo ("System.Web.Extensions.Test, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293")]
[assembly: InternalsVisibleTo ("System.Web.Extensions.Design, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]

[assembly: WebResource ("MicrosoftAjax.js", "application/x-javascript")]
[assembly: WebResource ("MicrosoftAjax.debug.js", "application/x-javascript")]
[assembly: WebResource ("MicrosoftAjaxWebForms.js", "application/x-javascript")]
[assembly: WebResource ("MicrosoftAjaxWebForms.debug.js", "application/x-javascript")]
[assembly: WebResource ("MicrosoftAjaxTimer.js", "application/x-javascript")]
[assembly: WebResource ("MicrosoftAjaxTimer.debug.js", "application/x-javascript")]
#if TARGET_J2EE
	[assembly: WebResource ("MicrosoftAjaxExtension.js", "application/x-javascript")]
	[assembly: WebResource ("MicrosoftAjaxWebFormsExtension.js", "application/x-javascript")]
#endif

// Those entries must not be enabled until the appropriate .resx files with translations for the client scripts are
// created.
// The default (English) strings are embedded in the scripts - maybe we should extract them on the build time and create
// the .resx files on the fly.
//
// DO NOT ENABLE UNTIL THE .resx FILES ARE PRESENT - ENABLING CAUSES BUG #384144
#if DO_NOT_ENABLE_UNLESS_RESX_FILES_ARE_PRESENT
[assembly: ScriptResource ("MicrosoftAjax.js", "System.Web.Resources.ScriptLibrary.Res", "Sys.Res")]
[assembly: ScriptResource ("MicrosoftAjax.debug.js", "System.Web.Resources.ScriptLibrary.Res.debug", "Sys.Res")]
[assembly: ScriptResource ("MicrosoftAjaxWebForms.js", "System.Web.Resources.ScriptLibrary.WebForms.Res", "Sys.WebForms.Res")]
[assembly: ScriptResource ("MicrosoftAjaxWebForms.debug.js", "System.Web.Resources.ScriptLibrary.WebForms.Res.debug", "Sys.WebForms.Res")]
#endif
