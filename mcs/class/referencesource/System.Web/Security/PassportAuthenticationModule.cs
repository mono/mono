//------------------------------------------------------------------------------
// <copyright file="PassportAuthenticationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * PassportAuthenticationModule class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Web;
    using  System.Security.Principal;
    using System.Web.Configuration;
    using System.Web.Handlers;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.Management;



    /// <devdoc>
    ///    This 
    ///       module provides a wrapper around passport authentication services. 
    /// </devdoc>
    [Obsolete("This type is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
    public sealed class PassportAuthenticationModule : IHttpModule {
        private PassportAuthenticationEventHandler _eventHandler;

        private static bool _fAuthChecked  = false;
        private static bool _fAuthRequired = false;
        private static String _LoginUrl    = null;


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Security.PassportAuthenticationModule'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public PassportAuthenticationModule() {
        }

        ////////////////////////////////////////////////////////////
        // AddOnAuthenticate and RemoveOnAuthenticate: Use these
        //   methods to hook up event handlers to handle the
        //   OnAuthenticate Event

        /// <devdoc>
        ///    This is a global.asax event that must be
        ///    named PassportAuthenticate_OnAuthenticate event.
        /// </devdoc>
        public event PassportAuthenticationEventHandler Authenticate {
            add {
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
            app.AuthenticateRequest += new EventHandler(this.OnEnter);
            app.EndRequest += new EventHandler(this.OnLeave);
        }

        ////////////////////////////////////////////////////////////
        // OnAuthenticate: Custom Authentication modules can override
        //             this method to create a custom IPrincipal object from
        //             a PassportIdentity

        /// <devdoc>
        ///    Calls the
        ///    PassportAuthentication_OnAuthenticate handler, if one exists.
        /// </devdoc>
        void OnAuthenticate(PassportAuthenticationEventArgs e) {
            ////////////////////////////////////////////////////////////
            // If there are event handlers, invoke the handlers
            if (_eventHandler != null) {
                _eventHandler(this, e);
                if (e.Context.User == null && e.User != null)
                {
                    InternalSecurityPermissions.ControlPrincipal.Demand();
                    e.Context.User = e.User;
                }
            }

            ////////////////////////////////////////////////////////////
            // Default Implementation: If IPrincipal has not been created,
            //                         create a PassportUser
            if (e.Context.User == null)
            {
                InternalSecurityPermissions.ControlPrincipal.Demand();
                e.Context.User = new PassportPrincipal(e.Identity, new String[0]);
            }
        }


        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // Methods for internal implementation

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        void OnEnter(Object source, EventArgs eventArgs) {
            if (_fAuthChecked && !_fAuthRequired)
                return;

            HttpApplication app;
            HttpContext context;

            app = (HttpApplication)source;
            context = app.Context;

            if (!_fAuthChecked) {
                AuthenticationSection settings = RuntimeConfig.GetAppConfig().Authentication;
                _fAuthRequired = (AuthenticationConfig.Mode == AuthenticationMode.Passport);
                _LoginUrl = settings.Passport.RedirectUrl;
                _fAuthChecked = true;
            }                    

            if (!_fAuthRequired)
                return;

            ////////////////////////////////////////////////////////
            // Step 1: See if this request is valid or not
            // VSWhidbey 442515: We no longer need to do this check, always proceed

            ////////////////////////////////////////////////////////
            // Step 2: Create a Passport Identity from the credentials
            //     from IIS
            PassportIdentity identity = new PassportIdentity();

            ////////////////////////////////////////////////////////
            // Step 4: Call OnAuthenticate virtual method to create
            //    an IPrincipal for this request
            OnAuthenticate( new PassportAuthenticationEventArgs(identity, context) );

            ////////////////////////////////////////////////////////
            // Skip AuthZ if accessing the login page
            context.SetSkipAuthorizationNoDemand(AuthenticationConfig.AccessingLoginPage(context, _LoginUrl), false /*managedOnly*/);

            if (!context.SkipAuthorization) {
                context.SkipAuthorization = AssemblyResourceLoader.IsValidWebResourceRequest(context);
            }
        }

        void OnLeave(Object source, EventArgs eventArgs) {
            HttpApplication app;
            HttpContext context;
            app = (HttpApplication)source;
            context = app.Context;
            if (!_fAuthChecked || !_fAuthRequired || context.User == null || context.User.Identity == null || !(context.User.Identity is PassportIdentity))
                return;



            PassportIdentity id = (PassportIdentity) context.User.Identity;
            if (context.Response.StatusCode != 401 || id.WWWAuthHeaderSet)
                return;

            if ( _LoginUrl==null || _LoginUrl.Length < 1 || String.Compare(_LoginUrl, "internal", StringComparison.Ordinal) == 0) {
                context.Response.Clear();
                context.Response.StatusCode = 200;

                if (!ErrorFormatter.RequiresAdaptiveErrorReporting(context)) {
                    String strUrl = context.Request.Url.ToString();
                    int iPos = strUrl.IndexOf('?');
                    if (iPos >= 0) {
                        strUrl = strUrl.Substring(0, iPos);
                    }
                    String strLogoTag = id.LogoTag2(HttpUtility.UrlEncode(strUrl, context.Request.ContentEncoding));

                    String strMsg = SR.GetString(SR.PassportAuthFailed, strLogoTag);
                    context.Response.Write(strMsg);
                }
                else {
                    ErrorFormatter errorFormatter = new PassportAuthFailedErrorFormatter();
                    context.Response.Write(errorFormatter.GetAdaptiveErrorMessage(context, true));
                }
            }
            else {
                ////////////////////////////////////////////////////////////
                // Step 1: Get the redirect url
                String redirectUrl = AuthenticationConfig.GetCompleteLoginUrl(context, _LoginUrl);
                
                ////////////////////////////////////////////////////////////
                // Step 2: Check if we have a valid url to the redirect-page
                if (redirectUrl == null || redirectUrl.Length <= 0) 
                    throw new HttpException(SR.GetString(SR.Invalid_Passport_Redirect_URL));

                
                ////////////////////////////////////////////////////////////
                // Step 3: Construct the redirect-to url
                String             strUrl       = context.Request.Url.ToString();
                String             strRedirect;
                int                iIndex;
                String             strSep;
            
                if (redirectUrl.IndexOf('?') >= 0)
                    strSep = "&";
                else
                    strSep = "?";
                
                strRedirect = redirectUrl  + strSep + "ReturnUrl=" + HttpUtility.UrlEncode(strUrl, context.Request.ContentEncoding);
                

                ////////////////////////////////////////////////////////////
                // Step 4: Add the query-string from the current url
                iIndex = strUrl.IndexOf('?');
                if (iIndex >= 0 && iIndex < strUrl.Length-1)
                    strRedirect += "&" + strUrl.Substring(iIndex+1);
                

                ////////////////////////////////////////////////////////////
                // Step 5: Do the redirect
                context.Response.Redirect(strRedirect, false);
            }
        }

    }

    //////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////
    // ErrorFormatter for generating adaptive error for mobile devices
    internal class PassportAuthFailedErrorFormatter : ErrorFormatter {

        protected override string ErrorTitle {
            get { return SR.GetString(SR.PassportAuthFailed_Title);}
        }

        protected override string Description {
            get { return SR.GetString(SR.PassportAuthFailed_Description);}
        }

        protected override string MiscSectionTitle {
            get { return SR.GetString(SR.Assess_Denied_Title);}
        }

        protected override string MiscSectionContent {
            get { return null;}
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
