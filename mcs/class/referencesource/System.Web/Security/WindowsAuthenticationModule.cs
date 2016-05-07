//------------------------------------------------------------------------------
// <copyright file="WindowsAuthenticationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * WindowsAuthenticationModule class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>
    ///       Allows ASP.NET applications to use Windows/IIS authentication.
    ///    </para>
    /// </devdoc>
    public sealed class WindowsAuthenticationModule : IHttpModule {

        private WindowsAuthenticationEventHandler _eventHandler;

        private static bool             _fAuthChecked;
        private static bool             _fAuthRequired;

        // anonymous identity + principal are static for easy referencing + reuse
        private static readonly WindowsIdentity AnonymousIdentity = WindowsIdentity.GetAnonymous();
        internal static readonly WindowsPrincipal AnonymousPrincipal = new WindowsPrincipal(AnonymousIdentity);


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Security.WindowsAuthenticationModule'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public WindowsAuthenticationModule() {
        }


        /// <devdoc>
        ///    This is a global.asax event that must be
        ///    named WindowsAuthenticate_OnAuthenticate event. It's used primarily to attach a
        ///    custom IPrincipal object to the context.
        /// </devdoc>
        public event WindowsAuthenticationEventHandler Authenticate {
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
        }

        ////////////////////////////////////////////////////////////
        // OnAuthenticate: Custom Authentication modules can override
        //             this method to create a custom IPrincipal object from
        //             a WindowsIdentity

        /// <devdoc>
        ///    Calls the
        ///    WindowsAuthentication_OnAuthenticate handler if one exists.
        /// </devdoc>
        void OnAuthenticate(WindowsAuthenticationEventArgs e) {
            ////////////////////////////////////////////////////////////
            // If there are event handlers, invoke the handlers
            if (_eventHandler != null)
                 _eventHandler(this, e);

            if (e.Context.User == null)
            {
                if (e.User != null)
                    e.Context.User = e.User;
                else  if (e.Identity == AnonymousIdentity)
                    e.Context.SetPrincipalNoDemand(AnonymousPrincipal, false /*needToSetNativePrincipal*/);
                else
                    e.Context.SetPrincipalNoDemand(new WindowsPrincipal(e.Identity), false /*needToSetNativePrincipal*/);
            }
        }



        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // Methods for internal implementation

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true, ControlPrincipal = true)]
        void OnEnter(Object source, EventArgs eventArgs) {
            if (!IsEnabled)
                return;

            HttpApplication         app = (HttpApplication)source;
            HttpContext             context = app.Context;;
            WindowsIdentity         identity = null;
            
            //////////////////////////////////////////////////////////////////
            // Step 2: Create a Windows Identity from the credentials from IIS
            if (HttpRuntime.UseIntegratedPipeline) {

                // The native WindowsAuthenticationModule sets the user principal in IIS7WorkerRequest.SynchronizeVariables.
                // The managed WindowsAuthenticationModule provides backward compatibility by rasing the OnAuthenticate event.
                WindowsPrincipal user = context.User as WindowsPrincipal;
                if (user != null) {
                    // identity will be null if this is not a WindowsIdentity
                    identity = user.Identity as WindowsIdentity;
                    // clear Context.User for backward compatibility (it will be set in OnAuthenticate)
                    context.SetPrincipalNoDemand(null, false /*needToSetNativePrincipal*/);
                }
            }
            else {
                String  strLogonUser  = context.WorkerRequest.GetServerVariable("LOGON_USER");
                String  strAuthType   = context.WorkerRequest.GetServerVariable("AUTH_TYPE");
                if (strLogonUser == null) {
                    strLogonUser = String.Empty;
                }
                if (strAuthType == null) {
                    strAuthType = String.Empty;
                }

                if (strLogonUser.Length == 0 && (strAuthType.Length == 0 || 
                                                 StringUtil.EqualsIgnoreCase(strAuthType, "basic"))) 
                {
                    ////////////////////////////////////////////////////////
                    // Step 2a: Use the anonymous identity
                    identity = AnonymousIdentity;
                }
                else
                {
                    identity = new WindowsIdentity(
                        context.WorkerRequest.GetUserToken(), 
                        strAuthType,
                        WindowsAccountType.Normal,
                        true);
                }
            }

            ///////////////////////////////////////////////////////////////////////////////////
            // Step 3: Call OnAuthenticate to create IPrincipal for this request.
            if (identity != null) {
                OnAuthenticate( new WindowsAuthenticationEventArgs(identity, context) );
            }
        }

        internal static bool IsEnabled {
            get {
                if (!_fAuthChecked) {
                    _fAuthRequired = (AuthenticationConfig.Mode == AuthenticationMode.Windows);
                    _fAuthChecked = true;
                }
                return _fAuthRequired;
            }
        }
    }
}
