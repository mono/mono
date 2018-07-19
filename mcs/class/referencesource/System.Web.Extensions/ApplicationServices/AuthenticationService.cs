//------------------------------------------------------------------------------
// <copyright file="AuthenticationService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.ApplicationServices {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Web;
    using System.Web.Management;
    using System.Web.Resources;
    using System.Web.Security;

    /// <devdoc>
    ///     Implements login service contract to be exposed as a WCF service. Uses Membership provider
    ///     or custom authentication login in the Authenticating event. Also uses Forms.SetAuthCookie() or
    ///     custom cookie generation via the CreatingCookie event.
    /// </devdoc>

    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required),
    ServiceContract(Namespace="http://asp.net/ApplicationServices/v200"),
    ServiceBehavior(Namespace="http://asp.net/ApplicationServices/v200", InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)
    ]
    public class AuthenticationService {

        /// <devdoc>
        ///    Raised to authenticate the user . The event handler sets the e.AuthenticationIsComplete flag to true
        ///    and e.Authenticated to the result.
        /// </devdoc>
        private static object _authenticatingEventHandlerLock = new object();
        private static EventHandler<AuthenticatingEventArgs> _authenticating;
        public static event EventHandler<AuthenticatingEventArgs> Authenticating {
            add {
                lock (_authenticatingEventHandlerLock) {
                    _authenticating += value;
                }
            }
            remove {
                lock (_authenticatingEventHandlerLock) {
                    _authenticating -= value;
                }
            }
        }

        /// <devdoc>
        ///    Raised to create and set the cookie. The event handler shouldset the e.CookieIsSet flag to true, if it is
        ///    setting the cookie.
        /// </devdoc>
        private static object _creatingCookieEventHandlerLock = new object();
        private static EventHandler<CreatingCookieEventArgs> _creatingCookie;
        public static event EventHandler<CreatingCookieEventArgs> CreatingCookie {
            add {
                lock (_creatingCookieEventHandlerLock) {
                    _creatingCookie += value;
                }
            }
            remove {
                lock (_creatingCookieEventHandlerLock) {
                    _creatingCookie -= value;
                }
            }
        }

        public AuthenticationService() {
        }
        
        /// <devdoc>
        ///    Raises the AuthentincatingEvent if atleast one handler is assigned.
        /// </devdoc>
        private void OnAuthenticating(AuthenticatingEventArgs e) {
            EventHandler<AuthenticatingEventArgs> handler = _authenticating;
            if (null != handler) {
                handler(this, e);
            }
        }

        /// <devdoc>
        ///     Raises the CreatingCookieEvent if atleast one handler is assigned.
        /// </devdoc>
        private void OnCreatingCookie(CreatingCookieEventArgs e) {
            EventHandler<CreatingCookieEventArgs> handler = _creatingCookie;
            if (null != handler) {
                handler(this, e);
            }
        }

        /// <devdoc>
        ///     Validates user credentials,without actually setting the FormAuth cookie
        /// </devdoc>
        /// <param name="username">Username of the account</param>
        /// <param name="password">Password of the account</param>
        /// <param name="customCredential">Any misc. string to be used by custom authentication logic</param>
        /// <returns>True, if credentials are valid, otherwise false</returns>
        [OperationContract]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="username", Justification="consistent with Whidbey")]
        public bool ValidateUser(string username, string password, string customCredential) {
            ApplicationServiceHelper.EnsureAuthenticationServiceEnabled(HttpContext.Current, true);
            return LoginInternal(username, password, customCredential, false, false);
        }

        /// <devdoc>
        ///     Validates user credentials,and sets the FormAuth cookie if the credentials are valid.
        /// </devdoc>
        /// <param name="username">Username of the account</param>
        /// <param name="password">Password of the account</param>
        /// <param name="customCredential">Any misc. string to be used by custom authentication logic</param>
        /// <param name="isPersistent">If true the persistant cookie is generated. </param>
        /// <returns>True, if credentials are valid, otherwise false</returns>
        [OperationContract]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="username", Justification="consistent with Whidbey")]
        public bool Login(string username, string password, string customCredential, bool isPersistent) {
            ApplicationServiceHelper.EnsureAuthenticationServiceEnabled(HttpContext.Current, true);
            return LoginInternal(username, password, customCredential, isPersistent, true);
        }

        /// <devdoc>
        ///    Checks whether the Forms Authentication cookie attached to the request is valid.
        /// </devdoc>
        [OperationContract]
        public bool IsLoggedIn() {
            ApplicationServiceHelper.EnsureAuthenticationServiceEnabled(HttpContext.Current, true);
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        /// <devdoc>
        ///   Clears the Forms Authentication cookie
        /// </devdoc>
        [OperationContract]
        public void Logout() {
            ApplicationServiceHelper.EnsureAuthenticationServiceEnabled(HttpContext.Current, false);
            FormsAuthentication.SignOut();
        }

        /// <devdoc>
        ///     Validates the user credentials.
        /// </devdoc>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="customCredential"></param>
        /// <param name="isPersistent"></param>
        /// <param name="setCookie">If this is true, CreatingCookie event is raised, and cookie is set in HttpResponse</param>
        /// <returns></returns>
        private bool LoginInternal(string username, string password, string customCredential, bool isPersistent, bool setCookie) {
            if (null == username) {
                throw new ArgumentNullException("username");
            }

            if (null == password) {
                throw new ArgumentNullException("password");
            }
            AuthenticatingEventArgs authEventArgs = new AuthenticatingEventArgs(username, password, customCredential);
            try {
                OnAuthenticating(authEventArgs);

                if (!authEventArgs.AuthenticationIsComplete) {
                    MembershipValidate(authEventArgs);
                }
                if (!authEventArgs.Authenticated) {
                    Logout();
                }
                if (authEventArgs.Authenticated && setCookie) {
                    CreatingCookieEventArgs cookieEventArgs = new CreatingCookieEventArgs(username, password, isPersistent, customCredential);
                    OnCreatingCookie(cookieEventArgs);
                    if (!cookieEventArgs.CookieIsSet) {
                        SetCookie(username, isPersistent);
                    }
                }
            }
            catch (Exception e) {
                LogException(e);
                throw;
            }
            return authEventArgs.Authenticated;
        }


        private static void MembershipValidate(AuthenticatingEventArgs e) {
            e.Authenticated = Membership.ValidateUser(e.UserName, e.Password);
        }

        private static void SetCookie(string username, bool isPersistent) {
            FormsAuthentication.SetAuthCookie(username, isPersistent);
        }

        private void LogException(Exception e) {
            WebServiceErrorEvent errorevent = new WebServiceErrorEvent(AtlasWeb.UnhandledExceptionEventLogMessage, this, e);
            errorevent.Raise();
        }

    }
}
