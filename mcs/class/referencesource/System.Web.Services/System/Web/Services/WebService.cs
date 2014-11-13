//------------------------------------------------------------------------------
// <copyright file="WebService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Web.Services {

    using System.Diagnostics;
    using System.Web;
    using System.ComponentModel;
    using System.Web.SessionState;
    using System.Web.Services.Protocols;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;

    /// <include file='doc\WebService.uex' path='docs/doc[@for="WebService"]/*' />
    /// <devdoc>
    ///    <para>Defines the
    ///       optional base class for Web Services, which provides direct access to common
    ///       ASP.NET objects, like those for application and session state.</para>
    /// </devdoc>
    public class WebService : MarshalByValueComponent {

        private HttpContext context;

        /// <include file='doc\WebService.uex' path='docs/doc[@for="WebService.Application"]/*' />
        /// <devdoc>
        ///    <para>Gets a
        ///       reference to the application object for the current HTTP request.</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Description("The ASP.NET application object for the current request.")]
        public HttpApplicationState Application {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
            get {
                return Context.Application;
            }
        }

        /// <include file='doc\WebService.uex' path='docs/doc[@for="WebService.Context"]/*' />
        /// <devdoc>
        ///    <para>Gets the ASP.NET Context object for the current request,
        ///       which encapsulates all HTTP-specific context
        ///       used by the HTTP server to process Web requests.</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription(Res.WebServiceContext)]
        public HttpContext Context {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
            get {
                PartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
                if (context == null)
                    context = HttpContext.Current;
                if (context == null)
                    throw new InvalidOperationException(Res.GetString(Res.WebMissingHelpContext));
                return context;
            }
        }

        /// <include file='doc\WebService.uex' path='docs/doc[@for="WebService.Session"]/*' />
        /// <devdoc>
        /// <para>Gets a reference to the <see cref='T:System.Web.HttpSessionState'/>
        /// instance for the current request.</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription(Res.WebServiceSession)]
        public HttpSessionState Session {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
            get {
                return Context.Session;
            }
        }

        /// <include file='doc\WebService.uex' path='docs/doc[@for="WebService.Server"]/*' />
        /// <devdoc>
        /// <para>Gets a reference to the <see cref='T:System.Web.HttpServerUtility'/>
        /// for the current request.</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription(Res.WebServiceServer)]
        public HttpServerUtility Server {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
            get {
                return Context.Server;
            }
        }       

        /// <include file='doc\WebService.uex' path='docs/doc[@for="WebService.User"]/*' />
        /// <devdoc>
        ///    <para>Gets the ASP.NET server User object, used for authorizing the request.</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription(Res.WebServiceUser)]
        public IPrincipal User {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
            get {
                return Context.User;
            }
        }

        /// <include file='doc\WebService.uex' path='docs/doc[@for="WebService.SoapVersion"]/*' />
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription(Res.WebServiceSoapVersion), ComVisible(false)]
        public SoapProtocolVersion SoapVersion {
            [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
            get {
                object o = Context.Items[SoapVersionContextSlot];
                if (o != null && o is SoapProtocolVersion)
                    return (SoapProtocolVersion)o;
                else
                    return SoapProtocolVersion.Default;
            }
        }

        internal static readonly string SoapVersionContextSlot = "WebServiceSoapVersion";

        internal void SetContext(HttpContext context) {
            this.context = context;
        }

    }
}
