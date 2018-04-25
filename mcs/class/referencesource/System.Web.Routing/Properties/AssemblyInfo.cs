//------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security;
using System.Web.Routing;

[assembly: DependencyAttribute("System.Web,", LoadHint.Always)]

// We can't make it SecurityTransparent due to performance implications
//[assembly: SecurityTransparent]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]
#pragma warning restore 618

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames",
    Justification = "Assembly is delay-signed.")]

[assembly: TypeForwardedTo(typeof(HttpMethodConstraint))]
[assembly: TypeForwardedTo(typeof(IRouteConstraint))]
[assembly: TypeForwardedTo(typeof(IRouteHandler))]
[assembly: TypeForwardedTo(typeof(RequestContext))]
[assembly: TypeForwardedTo(typeof(Route))]
[assembly: TypeForwardedTo(typeof(RouteBase))]
[assembly: TypeForwardedTo(typeof(RouteCollection))]
[assembly: TypeForwardedTo(typeof(RouteData))]
[assembly: TypeForwardedTo(typeof(RouteDirection))]
[assembly: TypeForwardedTo(typeof(RouteTable))]
[assembly: TypeForwardedTo(typeof(RouteValueDictionary))]
[assembly: TypeForwardedTo(typeof(StopRoutingHandler))]
[assembly: TypeForwardedTo(typeof(UrlRoutingHandler))]
[assembly: TypeForwardedTo(typeof(UrlRoutingModule))]
[assembly: TypeForwardedTo(typeof(VirtualPathData))]

#if ATLAS_DEV
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: AssemblyVersion("99.0.0.0")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AllowPartiallyTrustedCallers(PartialTrustVisibilityLevel = PartialTrustVisibilityLevel.NotVisibleByDefault)]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
[assembly: SecurityTransparent]
#endif
