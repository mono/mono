//------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Web.Script;
using System.Web.UI;
using WRA = System.Web.UI.WebResourceAttribute;

// Dependency Attribute for assemblies 
[assembly: DependencyAttribute("System.Web,", LoadHint.Always)]

#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]
#pragma warning restore 618

// System.Web.Extensions.Test is our managed code unit test assembly, which needs
// to have access to internal members in this assembly.
[assembly: InternalsVisibleTo("System.Web.Extensions.Test, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293")]

// System.Web.Extensions.Design needs to access internal APIs in System.Web.Extensions, to expose
// them to the designer.
[assembly: InternalsVisibleTo("System.Web.Extensions.Design, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]

// gives System.ServiceModel.Web access to the WebServiceData and ClientProxyGenerator by the WCFServiceClientProxyGenerator
[assembly: InternalsVisibleToAttribute("System.ServiceModel.Web, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]

[assembly: AjaxFrameworkAssembly]
// Web resources for Atlas script files (Debug and Release versions).
// The files are included in this project as linked references to the
// AtlasBuildOutput folder.
// To reliably check Microsoft.Ajax would involve going through multiple namespaces which would bloat the amount of rendered JS. We'll pick two arbitrary scripts and 
// assume the script is rendered correctly based on this.
[assembly: WebResource("MicrosoftAjax.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjax.js", LoadSuccessExpression = "window.Sys && Sys._Application && Sys.Observer")]
[assembly: WebResource("MicrosoftAjaxApplicationServices.js", "application/x-javascript", CdnSupportsSecureConnection = true, 
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxApplicationServices.js", LoadSuccessExpression = "window.Sys && Sys.Services")]
[assembly: WebResource("MicrosoftAjaxComponentModel.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxComponentModel.js", LoadSuccessExpression = "window.Sys && Sys.CommandEventArgs")]
[assembly: WebResource("MicrosoftAjaxCore.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxCore.js", LoadSuccessExpression = "window.Type && Sys.Observer")]
[assembly: WebResource("MicrosoftAjaxGlobalization.js", "application/x-javascript", CdnSupportsSecureConnection = true, 
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxGlobalization.js", LoadSuccessExpression = "window.Sys && Sys.CultureInfo")]
[assembly: WebResource("MicrosoftAjaxHistory.js", "application/x-javascript", CdnSupportsSecureConnection = true, 
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxHistory.js", LoadSuccessExpression="window.Sys && Sys.HistoryEventArgs")]
[assembly: WebResource("MicrosoftAjaxNetwork.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxNetwork.js", LoadSuccessExpression = "window.Sys && Sys.Net && Sys.Net.WebRequestExecutor")]
[assembly: WebResource("MicrosoftAjaxSerialization.js", "application/x-javascript", CdnSupportsSecureConnection = true, 
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxSerialization.js", LoadSuccessExpression="window.Sys && Sys.Serialization")]
[assembly: WebResource("MicrosoftAjaxTimer.js", "application/x-javascript", CdnSupportsSecureConnection = true, 
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxTimer.js", LoadSuccessExpression = "window.Sys && Sys.UI && Sys.UI._Timer")]
[assembly: WebResource("MicrosoftAjaxWebForms.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxWebForms.js", LoadSuccessExpression = "window.Sys && Sys.WebForms")]
[assembly: WebResource("MicrosoftAjaxWebServices.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxWebServices.js", LoadSuccessExpression = "window.Sys && Sys.Net && Sys.Net.WebServiceProxy")]
[assembly: WebResource("Date.HijriCalendar.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "Date.HijriCalendar.js", LoadSuccessExpression = "window.Type && Type._registerScript && Type._registerScript._scripts && Type._registerScript._scripts['Date.HijriCalendar.js']")]
[assembly: WebResource("Date.UmAlQuraCalendar.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "Date.UmAlQuraCalendar.js", LoadSuccessExpression = "window.Type && Type._registerScript && Type._registerScript._scripts && Type._registerScript._scripts['Date.UmAlQuraCalendar.js']")]
[assembly: WebResource("MicrosoftAjax.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjax.debug.js", LoadSuccessExpression = "window.Sys && Sys._Application && Sys.Observer")]
[assembly: WebResource("MicrosoftAjaxApplicationServices.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxApplicationServices.debug.js", LoadSuccessExpression = "window.Sys && Sys.Services")]
[assembly: WebResource("MicrosoftAjaxComponentModel.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true, 
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxComponentModel.debug.js", LoadSuccessExpression = "window.Sys && Sys.CommandEventArgs")]
[assembly: WebResource("MicrosoftAjaxCore.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxCore.debug.js", LoadSuccessExpression = "window.Type && Sys.Observer")]
[assembly: WebResource("MicrosoftAjaxGlobalization.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxGlobalization.debug.js", LoadSuccessExpression = "window.Sys && Sys.CultureInfo")]
[assembly: WebResource("MicrosoftAjaxHistory.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxHistory.debug.js", LoadSuccessExpression = "window.Sys && Sys.HistoryEventArgs")]
[assembly: WebResource("MicrosoftAjaxNetwork.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxNetwork.debug.js", LoadSuccessExpression = "window.Sys && Sys.Net && Sys.Net.WebRequestExecutor")]
[assembly: WebResource("MicrosoftAjaxSerialization.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxSerialization.debug.js", LoadSuccessExpression = "window.Sys && Sys.Serialization")]
[assembly: WebResource("MicrosoftAjaxTimer.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxTimer.debug.js", LoadSuccessExpression = "window.Sys && Sys.UI && Sys.UI._Timer")]
[assembly: WebResource("MicrosoftAjaxWebForms.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxWebForms.debug.js", LoadSuccessExpression = "window.Sys && Sys.WebForms")]
[assembly: WebResource("MicrosoftAjaxWebServices.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "MicrosoftAjaxWebServices.debug.js", LoadSuccessExpression = "window.Sys && Sys.Net && Sys.Net.WebServiceProxy")]
[assembly: WebResource("Date.HijriCalendar.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "Date.HijriCalendar.debug.js", LoadSuccessExpression = "window.Type && Type._registerScript && Type._registerScript._scripts && Type._registerScript._scripts['Date.HijriCalendar.js']")]
[assembly: WebResource("Date.UmAlQuraCalendar.debug.js", "application/x-javascript", CdnSupportsSecureConnection = true,
    CdnPath = WRA._microsoftCdnBasePath + "Date.UmAlQuraCalendar.debug.js", LoadSuccessExpression = "window.Type && Type._registerScript && Type._registerScript._scripts && Type._registerScript._scripts['Date.UmAlQuraCalendar.js']")]

// Script resources
[assembly: ScriptResource("MicrosoftAjax.js", "System.Web.Resources.ScriptLibrary.Res", "Sys.Res")]
[assembly: ScriptResource("MicrosoftAjax.debug.js", "System.Web.Resources.ScriptLibrary.Res.debug", "Sys.Res")]
[assembly: ScriptResource("MicrosoftAjaxCore.js", "System.Web.Resources.ScriptLibrary.Res", "Sys.Res")]
[assembly: ScriptResource("MicrosoftAjaxCore.debug.js", "System.Web.Resources.ScriptLibrary.Res.debug", "Sys.Res")]
[assembly: ScriptResource("MicrosoftAjaxWebForms.js", "System.Web.Resources.ScriptLibrary.WebForms.Res", "Sys.WebForms.Res")]
[assembly: ScriptResource("MicrosoftAjaxWebForms.debug.js", "System.Web.Resources.ScriptLibrary.WebForms.Res.debug", "Sys.WebForms.Res")]

// Default tag prefix for designer
[assembly: TagPrefix("System.Web.UI", "asp")]
[assembly: TagPrefix("System.Web.UI.WebControls", "asp")]

// Opts into the VS loading icons from the FrameworkIcon Satellite assemblies found under VSIP\Icons
[assembly:System.Drawing.BitmapSuffixInSatelliteAssemblyAttribute()]

#if ATLAS_DEV
[assembly: AllowPartiallyTrustedCallers(PartialTrustVisibilityLevel = PartialTrustVisibilityLevel.NotVisibleByDefault)]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
[assembly: ComVisible(false)]
[assembly: System.CLSCompliant(true)]
[assembly: AssemblyVersion("99.0.0.0")]
[assembly: NeutralResourcesLanguage("en-US")]
#endif

// Suppress Code Analysis violations for terms that appear frequently in resource strings
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly",
    MessageId = "postback", Scope = "resource", Target = "System.Web.Resources.AtlasWeb.resources",
    Justification = "Source code standardizes on casing 'PostBack' for consistency with legacy code; however, resource strings should use correct casing 'Postback'.")]
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly",
    MessageId = "postbacks", Scope = "resource", Target = "System.Web.Resources.AtlasWeb.resources",
    Justification = "Source code standardizes on casing 'PostBacks' for consistency with legacy code; however, resource strings should use correct casing 'Postbacks'.")]
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly",
    MessageId = "runat", Scope = "resource", Target = "System.Web.Resources.AtlasWeb.resources",
    Justification = "This term appears frequently in ASP.NET (as 'runat=server').")]
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly",
    MessageId = "onerror", Scope = "resource", Target = "System.Web.Resources.ScriptLibrary.Res.debug.resources",
    Justification = "This is a valid Javascript event handler name.")]
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly",
    MessageId = "svcmap", Scope = "resource", Target = "System.Web.Resources.WCFModelStrings.resources",
    Justification = "This term appears frequently in WCF.")]
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly",
    MessageId = "datasvcmap", Scope = "resource", Target = "System.Web.Resources.WCFModelStrings.resources",
    Justification = "This term appears frequently in WCF.")]
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly",
    MessageId = "dddddddd-dddd-dddd-dddd-dddddddddddd", Scope = "resource", Target = "System.Web.Resources.ScriptLibrary.AdoNet.Res.debug.resources",
    Justification = "This is a valid Javascript Guid form.")]
