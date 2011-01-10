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
