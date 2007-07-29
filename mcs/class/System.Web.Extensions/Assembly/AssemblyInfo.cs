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
using System.Web.UI;
using System.Security;

// General Information about the System.Web.Extensions assembly

[assembly: AssemblyVersion ("1.0.61025.0")]

[assembly: AssemblyTitle ("System.Web.Extensions.dll")]
[assembly: AssemblyDescription ("System.Web.Extensions.dll")]
[assembly: AssemblyConfiguration ("Development version")]
[assembly: AssemblyCompany ("MONO development team")]
[assembly: AssemblyProduct ("MONO CLI")]
[assembly: AssemblyCopyright ("(c) 2007 Various Authors")]
[assembly: AssemblyTrademark ("")]
#if TARGET_JVM
[assembly: CLSCompliant(false)]
#else
[assembly: CLSCompliant (true)]
#endif
[assembly: ComVisible (false)]
[assembly: AssemblyDefaultAlias ("System.Web.Extensions.dll")]
[assembly: AssemblyInformationalVersion ("0.0.0.1")]
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: AllowPartiallyTrustedCallers ()]
[assembly: TagPrefix ("System.Web.UI", "asp")]

[assembly: WebResource ("MicrosoftAjax.js", "application/x-javascript")]
[assembly: WebResource ("MicrosoftAjax.debug.js", "application/x-javascript")]
[assembly: WebResource ("MicrosoftAjaxWebForms.js", "application/x-javascript")]
[assembly: WebResource ("MicrosoftAjaxWebForms.debug.js", "application/x-javascript")]
[assembly: WebResource ("MicrosoftAjaxTimer.js", "application/x-javascript")]
[assembly: WebResource ("MicrosoftAjaxTimer.debug.js", "application/x-javascript")]

//
// This is needed only in the 3.5 profile, which we do not build in mcs yet
//
//[assembly: AssemblyDelaySign (true)]
//[assembly: AssemblyKeyFile ("../winfx.pub")]
