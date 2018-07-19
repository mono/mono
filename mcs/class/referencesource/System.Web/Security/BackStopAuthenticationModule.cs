//------------------------------------------------------------------------------
// <copyright file="BackStopAuthenticationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class DefaultAuthenticationModule : IHttpModule {
        private DefaultAuthenticationEventHandler _eventHandler;


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Security.DefaultAuthenticationModule'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public DefaultAuthenticationModule() {
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        internal static DefaultAuthenticationModule CreateDefaultAuthenticationModuleWithAssert() {
            return new DefaultAuthenticationModule();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public event DefaultAuthenticationEventHandler Authenticate {
            add {
                // WOS 1709222: DefaultAuthentication_Authenticate is not supported in integrated mode.
                if (HttpRuntime.UseIntegratedPipeline) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Method_Not_Supported_By_Iis_Integrated_Mode, "DefaultAuthentication.Authenticate"));
                }
                _eventHandler += value;
            }
            remove {
                _eventHandler -= value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Dispose() {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Init(HttpApplication app) {
            // adding this module back to IIS7
            // it needs to run between Windows auth in PostAuthn
            // and RoleManager (or anyone else who needs the principal)
            // so ordering is important
            // If the subscribed event changes, WindowsAuthenticationModule
            // needs work, too.
            if (HttpRuntime.UseIntegratedPipeline) {
                app.PostAuthenticateRequest += new EventHandler(this.OnEnter);
            }
            else {
                app.DefaultAuthentication += new EventHandler(this.OnEnter);
            }
        }

        ////////////////////////////////////////////////////////////
        // OnAuthenticate: Custom Authentication modules can override
        //             this method to create a custom IPrincipal object from
        //             a DefaultIdentity
        void OnAuthenticate(DefaultAuthenticationEventArgs e) {
            ////////////////////////////////////////////////////////////
            // If there are event handlers, invoke the handlers
            if (_eventHandler != null) {
                _eventHandler(this, e);
            }
        }

        ////////////////////////////////////////////////////////////
        // AddOnAuthenticate and RemoveOnAuthenticate: Use these
        //   methods to hook up event handlers to handle the
        //   OnAuthenticate Event
        [SecurityPermission(SecurityAction.Assert, ControlPrincipal = true)]
        void OnEnter(Object source, EventArgs eventArgs) {
            HttpApplication app;
            HttpContext context;

            app = (HttpApplication)source;
            context = app.Context;

            ////////////////////////////////////////////////////////////
            // Step 1: Check if authentication failed
            if (context.Response.StatusCode > 200) { // Invalid credentials
                if (context.Response.StatusCode == 401)
                    WriteErrorMessage(context);

                app.CompleteRequest();
                return;
            }

            ////////////////////////////////////////////////////////////
            // Step 2: If no auth module has created an IPrincipal, then fire
            //         OnAuthentication event
            if (context.User == null) {
                OnAuthenticate (new DefaultAuthenticationEventArgs(context) );
                if (context.Response.StatusCode > 200) { // Invalid credentials
                    if (context.Response.StatusCode == 401)
                        WriteErrorMessage(context);

                    app.CompleteRequest();
                    return;
                }
            }

            ////////////////////////////////////////////////////////////
            // Step 3: Attach an anonymous user to this request, if none
            //         of the authentication modules created a user
            if (context.User == null) {
                context.SetPrincipalNoDemand(new GenericPrincipal(new GenericIdentity(String.Empty, String.Empty), new String[0]), false /*needToSetNativePrincipal*/);
            }

            Thread.CurrentPrincipal = context.User;
        }

        /////////////////////////////////////////////////////////////////////////////
        void WriteErrorMessage(HttpContext context) {
            context.Response.Write(AuthFailedErrorFormatter.GetErrorText());
            // In Integrated pipeline, ask for handler headers to be generated.  This would be unnecessary
            // if we just threw an access denied exception, and used the standard error mechanism
            context.Response.GenerateResponseHeadersForHandler();
        }
    }

    //////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////
    internal class AuthFailedErrorFormatter : ErrorFormatter {
        private static string _strErrorText;
        private static object _syncObject   = new object();

        internal AuthFailedErrorFormatter() {
        }

        internal /*public*/ static string GetErrorText() {
            if (_strErrorText != null)
                return _strErrorText;

            lock(_syncObject) {
                if (_strErrorText == null)
                    _strErrorText = (new AuthFailedErrorFormatter()).GetErrorMessage();
            }

            return _strErrorText;                
        }

        protected override string ErrorTitle {
            get { return SR.GetString(SR.Assess_Denied_Title);}
        }

        protected override string Description {
            get {
                return SR.GetString(SR.Assess_Denied_Description1);
                //"An error occurred while accessing the resources required to serve this request. &nbsp; This typically happens when you provide the wrong user-name and/or password.";
            }
        }

        protected override string MiscSectionTitle {            
            get { return SR.GetString(SR.Assess_Denied_MiscTitle1);} 
            //"Error message 401.1";}
        }

        protected override string MiscSectionContent {
            get {
                string miscContent = SR.GetString(SR.Assess_Denied_MiscContent1);
                AdaptiveMiscContent.Add(miscContent);
                return miscContent;
                //return "Logon credentials provided were not recognized. Make sure you are providing the correct user-name and password. Otherwise, ask the web server's administrator for help.";
            }
        }

        protected override string ColoredSquareTitle {
            get { return null;}
        }

        protected override string ColoredSquareContent {
            get { return null;}
        }

        protected override bool ShowSourceFileInfo {
            get { return false;}
        }
    }
}



