//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// (C) 2003-2009 Novell, Inc (http://novell.com)
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
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web.UI;

// General Information about the System.Web assembly

[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

[assembly: AssemblyTitle("System.Web.dll")]
[assembly: AssemblyDescription("System.Web.dll")]
#if !NET_4_0
[assembly: AssemblyConfiguration("Development version")]
#endif
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]
#if !NET_4_0
[assembly: AssemblyTrademark("")]
#endif
#if TARGET_JVM
[assembly: CLSCompliant(false)]
#else
[assembly: CLSCompliant(true)]
#endif
[assembly: ComVisible(false)]
[assembly: AssemblyDefaultAlias("System.Web.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AllowPartiallyTrustedCallers()]
[assembly: TagPrefix("System.Web.UI.WebControls", "asp")]
#if !(TARGET_JVM || TARGET_DOTNET)
[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("../msfinal.pub")]

#if NET_4_0
[assembly: Debuggable (true, false)]
[assembly: AssemblyFileVersion (Consts.FxVersion)]
[assembly: AssemblyTargetedPatchBand ("1.0.21-0")]
[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
[assembly: Dependency ("System", LoadHint.Always)]
[assembly: TypeLibVersion (4, 2)]
[assembly: SecurityRules (SecurityRuleSet.Level2, SkipVerificationInFullTrust=true)]

[assembly: TypeForwardedTo (typeof (System.Web.Security.MembershipPasswordException))]
[assembly: TypeForwardedTo (typeof (System.Web.Security.RoleProvider))]
[assembly: TypeForwardedTo (typeof (System.Web.Security.MembershipCreateStatus))]
[assembly: TypeForwardedTo (typeof (System.Web.Security.MembershipCreateUserException))]
[assembly: TypeForwardedTo (typeof (System.Web.Security.MembershipPasswordFormat))]
[assembly: TypeForwardedTo (typeof (System.Web.Security.ValidatePasswordEventArgs))]
[assembly: TypeForwardedTo (typeof (System.Web.Security.MembershipValidatePasswordEventHandler))]
[assembly: TypeForwardedTo (typeof (System.Web.Security.MembershipUser))]
[assembly: TypeForwardedTo (typeof (System.Web.Security.MembershipUserCollection))]
[assembly: TypeForwardedTo (typeof (System.Web.Security.MembershipProviderCollection))]
[assembly: TypeForwardedTo (typeof (System.Web.Security.MembershipProvider))]
#endif

[assembly: InternalsVisibleTo ("System.Web.Extensions, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]

#endif

[assembly: InternalsVisibleTo ("SystemWebTestShim, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo ("System.Web_test_net_2_0, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo ("System.Web_test_net_4_0, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]

// Resources

[assembly: WebResource ("TreeView_noexpand.gif", "image/gif")]
[assembly: WebResource ("TreeView_dash.gif", "image/gif")]
[assembly: WebResource ("TreeView_dashminus.gif", "image/gif")]
[assembly: WebResource ("TreeView_dashplus.gif", "image/gif")]
[assembly: WebResource ("TreeView_i.gif", "image/gif")]
[assembly: WebResource ("TreeView_l.gif", "image/gif")]
[assembly: WebResource ("TreeView_lminus.gif", "image/gif")]
[assembly: WebResource ("TreeView_lplus.gif", "image/gif")]
[assembly: WebResource ("TreeView_minus.gif", "image/gif")]
[assembly: WebResource ("TreeView_plus.gif", "image/gif")]
[assembly: WebResource ("TreeView_r.gif", "image/gif")]
[assembly: WebResource ("TreeView_rminus.gif", "image/gif")]
[assembly: WebResource ("TreeView_rplus.gif", "image/gif")]
[assembly: WebResource ("TreeView_t.gif", "image/gif")]
[assembly: WebResource ("TreeView_tminus.gif", "image/gif")]
[assembly: WebResource ("TreeView_tplus.gif", "image/gif")]
[assembly: WebResource ("arrow_minus.gif", "image/gif")]
[assembly: WebResource ("arrow_noexpand.gif", "image/gif")]
[assembly: WebResource ("arrow_plus.gif", "image/gif")]
[assembly: WebResource ("box_full.gif", "image/gif")]
[assembly: WebResource ("box_empty.gif", "image/gif")]
[assembly: WebResource ("box_minus.gif", "image/gif")]
[assembly: WebResource ("box_noexpand.gif", "image/gif")]
[assembly: WebResource ("box_plus.gif", "image/gif")]
[assembly: WebResource ("contact.gif", "image/gif")]
[assembly: WebResource ("dot_empty.gif", "image/gif")]
[assembly: WebResource ("dot_full.gif", "image/gif")]
[assembly: WebResource ("dots.gif", "image/gif")]
[assembly: WebResource ("inbox.gif", "image/gif")]
[assembly: WebResource ("star_empty.gif", "image/gif")]
[assembly: WebResource ("star_full.gif", "image/gif")]
[assembly: WebResource ("warning.gif", "image/gif")]
[assembly: WebResource ("arrow_up.gif", "image/gif")]
[assembly: WebResource ("arrow_down.gif", "image/gif")]
[assembly: WebResource ("transparent.gif", "image/gif")]
[assembly: WebResource ("file.gif", "image/gif")]
[assembly: WebResource ("folder.gif", "image/gif")]
[assembly: WebResource ("computer.gif", "image/gif")]
[assembly: WebResource ("TreeView.js", "text/javascript")]
[assembly: WebResource ("Menu.js", "text/javascript")]
#if NET_4_0
[assembly: WebResource ("MenuModern.js", "text/javascript")]
#endif
[assembly: WebResource ("GridView.js", "text/javascript")]
[assembly: WebResource ("webform.js", "text/javascript")]
[assembly: WebResource ("WebUIValidation_2.0.js", "text/javascript")]
