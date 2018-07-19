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
using System.Security.Permissions;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Web;

// General Information about the System.Web.Abstractions assembly

[assembly: AssemblyTitle ("System.Web.Abstractions.dll")]
[assembly: AssemblyDescription ("System.Web.Abstractions.dll")]
[assembly: AssemblyDefaultAlias ("System.Web.Abstractions.dll")]

[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]
[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]
[assembly: AssemblyFileVersion (Consts.FxFileVersion)]

[assembly: NeutralResourcesLanguage ("en-US")]
[assembly: CLSCompliant (true)]
[assembly: AssemblyDelaySign (true)]

[assembly: ComVisible (false)]
[assembly: AllowPartiallyTrustedCallers]

// FIXME: We get collisions with this internalsVisibleTo because of Consts.cs and MonoTodo
//[assembly: InternalsVisibleTo ("System.ServiceModel.Web, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]

[assembly: SecurityRules (SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
[assembly: TypeForwardedTo (typeof (System.Web.HttpStaticObjectsCollectionBase))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpStaticObjectsCollectionWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpContextWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpContextBase))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpFileCollectionBase))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpPostedFileBase))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpFileCollectionWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpCachePolicyWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpApplicationStateWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpApplicationStateBase))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpBrowserCapabilitiesBase))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpCachePolicyBase))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpBrowserCapabilitiesWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpPostedFileWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpSessionStateWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpServerUtilityBase))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpSessionStateBase))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpServerUtilityWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpRequestBase))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpRequestWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpResponseWrapper))]
[assembly: TypeForwardedTo (typeof (System.Web.HttpResponseBase))]
