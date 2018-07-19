//------------------------------------------------------------------------------
// <copyright file="UrlAuthorizationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * UrlAuthorizationModule class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Runtime.Serialization;
    using System.Web;
    using System.Web.Util;
    using System.Collections;
    using System.Web.Configuration;
    using System.IO;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Web.Management;
    using System.Web.Hosting;
    using System.Collections.Generic;



    /// <devdoc>
    ///    This module provides URL based
    ///    authorization services for allowing or denying access to specified resources
    /// </devdoc>
    public sealed class UrlAuthorizationModule : IHttpModule {


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Security.UrlAuthorizationModule'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public UrlAuthorizationModule() {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Init(HttpApplication app) {
            app.AuthorizeRequest += new EventHandler(this.OnEnter);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Dispose() {
        }

        private static bool s_EnabledDetermined;
        private static bool s_Enabled;

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static bool CheckUrlAccessForPrincipal(String virtualPath, IPrincipal user, string verb) {
            if (virtualPath == null)
                throw new ArgumentNullException("virtualPath");
            if (user == null)
                throw new ArgumentNullException("user");
            if (verb == null)
                throw new ArgumentNullException("verb");
            verb = verb.Trim();

            VirtualPath vPath = VirtualPath.Create(virtualPath);

            if (!vPath.IsWithinAppRoot)
                throw new ArgumentException(SR.GetString(SR.Virtual_path_outside_application_not_supported), "virtualPath");

            if (!s_EnabledDetermined) {
                if( !HttpRuntime.UseIntegratedPipeline) {
                    HttpModulesSection modulesSection = RuntimeConfig.GetConfig().HttpModules;
                    int len = modulesSection.Modules.Count;
                    for (int iter = 0; iter < len; iter++) {
                        HttpModuleAction module = modulesSection.Modules[iter];
                        if (Type.GetType(module.Type, false) == typeof(UrlAuthorizationModule)) {
                            s_Enabled = true;
                            break;
                        }
                    }
                }
                else {
                    List<ModuleConfigurationInfo> modules = HttpApplication.IntegratedModuleList;
                    foreach (ModuleConfigurationInfo mod in modules) {
                        if (Type.GetType(mod.Type, false) == typeof(UrlAuthorizationModule)) {
                            s_Enabled = true;
                            break;
                        }
                    }
                }
                s_EnabledDetermined = true;
            }
            if (!s_Enabled)
                return true;
            AuthorizationSection settings = RuntimeConfig.GetConfig(vPath).Authorization;

            // Check if the user is allowed, or the request is for the login page
            return settings.EveryoneAllowed || settings.IsUserAllowed(user, verb);
        }

        internal static void ReportUrlAuthorizationFailure(HttpContext context, object webEventSource) {
            // Deny access
            context.Response.StatusCode = 401;
            WriteErrorMessage(context);

            if (context.User != null && context.User.Identity.IsAuthenticated) {
                // We don't raise failure audit event for anonymous user
                WebBaseEvent.RaiseSystemEvent(webEventSource, WebEventCodes.AuditUrlAuthorizationFailure);
            }
            context.ApplicationInstance.CompleteRequest();
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // Module Enter: Get the authorization configuration section
        //    and see if this user is allowed or not
        void OnEnter(Object source, EventArgs eventArgs) {
            HttpApplication    app;
            HttpContext        context;

            app = (HttpApplication)source;
            context = app.Context;
            if (context.SkipAuthorization)
            {
                if (context.User == null || !context.User.Identity.IsAuthenticated)
                    PerfCounters.IncrementCounter(AppPerfCounter.ANONYMOUS_REQUESTS);
                return;
            }

            // Get the authorization config object
            AuthorizationSection settings = RuntimeConfig.GetConfig(context).Authorization;

            // Check if the user is allowed, or the request is for the login page
            if (!settings.EveryoneAllowed && !settings.IsUserAllowed(context.User, context.Request.RequestType))
            {
                ReportUrlAuthorizationFailure(context, this);

            }
            else
            {
                if (context.User == null || !context.User.Identity.IsAuthenticated)
                    PerfCounters.IncrementCounter(AppPerfCounter.ANONYMOUS_REQUESTS);

                WebBaseEvent.RaiseSystemEvent(this, WebEventCodes.AuditUrlAuthorizationSuccess);
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        static void WriteErrorMessage(HttpContext context) {
            context.Response.Write(UrlAuthFailedErrorFormatter.GetErrorText());
            // In Integrated pipeline, ask for handler headers to be generated.  This would be unnecessary
            // if we just threw an access denied exception, and used the standard error mechanism
            context.Response.GenerateResponseHeadersForHandler();
        }

        static internal bool RequestRequiresAuthorization(HttpContext context) {
            if (context.SkipAuthorization)
                return false;

            AuthorizationSection settings = RuntimeConfig.GetConfig(context).Authorization;

            // Check if the anonymous user is allowed
            if (_AnonUser == null)
                _AnonUser = new GenericPrincipal(new GenericIdentity(String.Empty, String.Empty), new String[0]);

            return !settings.IsUserAllowed(_AnonUser, context.Request.RequestType);
        }

        internal static bool IsUserAllowedToPath(HttpContext context, VirtualPath virtualPath)
        {
            AuthorizationSection settings = RuntimeConfig.GetConfig(context, virtualPath).Authorization;

            return settings.EveryoneAllowed || settings.IsUserAllowed(context.User, context.Request.RequestType);
        }

        static GenericPrincipal _AnonUser;
    }
}




