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
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web.UI;

// General Information about the System.Web assembly

#if (NET_1_0)
	[assembly: AssemblyVersion("1.0.3300.0")]
	[assembly: SatelliteContractVersion("1.0.3300.0")]
#elif (NET_2_0)
	[assembly: AssemblyVersion ("2.0.3600.0")]
	[assembly: SatelliteContractVersion ("2.0.3600.0")]
#elif (NET_1_1)
	[assembly: AssemblyVersion("1.0.5000.0")]
	[assembly: SatelliteContractVersion("1.0.5000.0")]
#endif

[assembly: AssemblyTitle("System.Web.dll")]
[assembly: AssemblyDescription("System.Web.dll")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]
[assembly: AssemblyTrademark("")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: AssemblyDefaultAlias("System.Web.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AllowPartiallyTrustedCallers()]
[assembly: TagPrefix("System.Web.UI.WebControls", "asp")]

[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("../msfinal.pub")]

// Resources

#if NET_2_0

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

[assembly: WebResource ("TreeView.js", "text/javascript")]

#endif
