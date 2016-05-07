//------------------------------------------------------------------------------
// <copyright file="ClientFormsAuthenticationMembershipProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.ClientServices.Providers
{
    using System;
    using System.Security.Principal;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;
    using System.Windows.Forms;
    using System.Web;
    using System.Web.Resources;
    using System.Web.Security;
    using System.Threading;
    using System.Security.Cryptography;
    using System.Globalization;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Collections.Specialized;
    using System.Net;
    using System.Web.ClientServices;
    using System.Configuration;
    using System.Collections;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.AccessControl;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Security.Cryptography;

    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    public class ClientFormsAuthenticationMembershipProvider : MembershipProvider
    {
        private string  _GetCredentialsTypeName     = null;
        private string  _ConnectionString           = null;
        private string  _ConnectionStringProvider   = null;
        private string  _ServiceUri                 = null;
        private Type    _GetCredentialsType         = null;
        private bool    _SavePasswordHash           = true;
        private bool    _UsingFileSystemStore       = false;
        private bool    _UsingIsolatedStore         = false;
        private bool    _UsingWFCService            = false;


        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////

        [ SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId="2#", Justification="Reviewed and approved by feature crew"),
          SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="username", Justification="consistent with Whidbey")
        ]
        public static bool ValidateUser(string username, string password, string serviceUri)
        {
            CookieContainer cookies = null;
            bool useWFCService = serviceUri.EndsWith(".svc", StringComparison.OrdinalIgnoreCase);
            bool validated = ValidateUserByCallingLogin(username, password,
                                                        false, serviceUri, useWFCService,
                                                        ref cookies, null, null);
            if (validated){
                Thread.CurrentPrincipal = new ClientRolePrincipal(new ClientFormsIdentity(username, password,
                                                                                          new ClientFormsAuthenticationMembershipProvider(),
                                                                                          "ClientForms", true, cookies));
            }
            return validated;
        }


        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        private static bool ValidateUserByCallingLogin(string username, string password, bool rememberMe, string serviceUri,
                                                bool useWFCService, ref CookieContainer cookies,
                                                string connectionString, string connectionStringProvider)
        {
            if (useWFCService) {
                throw new NotImplementedException();

//                 CustomBinding binding = ProxyHelper.GetBinding();
//                 ChannelFactory<LoginService> channelFactory = new ChannelFactory<LoginService>(binding, new EndpointAddress(serviceUri)); //(@"http://localhost/AuthSvc/service.svc"));
//                 LoginService clientService = channelFactory.CreateChannel();
//                 using (new OperationContextScope((IContextChannel)clientService)) {
//                     ProxyHelper.AddCookiesToWCF(cookies, serviceUri, username, connectionString, connectionStringProvider);
//                     bool validated = clientService.Login(username, password, string.Empty, rememberMe);
//                     ProxyHelper.GetCookiesFromWCF(cookies, serviceUri, username, connectionString, connectionStringProvider);
//                     return validated;
//                 }
            } else {
                serviceUri = serviceUri + "/Login";
                string [] paramNames = new string [] { "userName", "password", "createPersistentCookie"};
                object [] paramValues = new object [] {username, password, rememberMe};
                object o = ProxyHelper.CreateWebRequestAndGetResponse(serviceUri,
                                                                      ref cookies,
                                                                      username,
                                                                      connectionString,
                                                                      connectionStringProvider,
                                                                      paramNames,
                                                                      paramValues,
                                                                      typeof(bool));
                return ((o != null) && (o is bool) && ((bool)o) == true);
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            base.Initialize(name, config);
            _GetCredentialsTypeName = config["credentialsProvider"];
            _ConnectionString = config["connectionStringName"];
            ServiceUri = config["serviceUri"];

            if (string.IsNullOrEmpty(_ConnectionString)) {
                _ConnectionString = SqlHelper.GetDefaultConnectionString();
            } else {
                if (ConfigurationManager.ConnectionStrings[_ConnectionString] != null) {
                    _ConnectionStringProvider = ConfigurationManager.ConnectionStrings[_ConnectionString].ProviderName;
                    _ConnectionString = ConfigurationManager.ConnectionStrings[_ConnectionString].ConnectionString;
                }
            }

            switch(SqlHelper.IsSpecialConnectionString(_ConnectionString))
            {
            case 1:
                _UsingFileSystemStore = true;
                break;
            case 2:
                _UsingIsolatedStore = true;
                break;
            default:
                break;
            }

            string temp = config["savePasswordHashLocally"];
            if (!string.IsNullOrEmpty(temp))
                _SavePasswordHash = (string.Compare(temp, "true", StringComparison.OrdinalIgnoreCase) == 0);

            config.Remove("savePasswordHashLocally");
            config.Remove("name");
            config.Remove("description");
            config.Remove("credentialsProvider");
            config.Remove("connectionStringName");
            config.Remove("serviceUri");
            foreach (string attribUnrecognized in config.Keys)
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, AtlasWeb.AttributeNotRecognized, attribUnrecognized));
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="username", Justification="consistent with Whidbey")]
        public override bool ValidateUser(string username, string password)
        {
            return ValidateUserCore(username, password, 2);
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="username", Justification="consistent with Whidbey")]
        public bool ValidateUser(string username, string password, bool rememberMe)
        {
            return ValidateUserCore(username, password, rememberMe ? 1 : 0);
        }
        private bool ValidateUserCore(string username, string password, int rememberMeInt)
        {
            lock (this) {
                int promptCount = string.IsNullOrEmpty(username) ? 0 : 3;

                if (ValidateUserCore(username, password, rememberMeInt, ref promptCount, true)) {
                    if (UserValidated != null)
                        UserValidated(this, new UserValidatedEventArgs(Thread.CurrentPrincipal.Identity.Name));
                    return true;
                }

                if (!string.IsNullOrEmpty(_GetCredentialsTypeName)) {
                    while (promptCount < 3) {
                        if (ValidateUserCore(null, password, rememberMeInt, ref promptCount, false)) {
                            if (UserValidated != null)
                                UserValidated(this, new UserValidatedEventArgs(Thread.CurrentPrincipal.Identity.Name));
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        public void Logout()
        {
            IPrincipal p = Thread.CurrentPrincipal;
            if (p == null || !(p.Identity is ClientFormsIdentity))
                return;
            lock (this) {
                if (!ConnectivityStatus.IsOffline) {
                    CookieContainer cookies = ((ClientFormsIdentity)p.Identity).AuthenticationCookies;

                    if (_UsingWFCService) {
                        throw new NotImplementedException();

//                         CustomBinding binding = ProxyHelper.GetBinding();
//                         ChannelFactory<LoginService> channelFactory = new ChannelFactory<LoginService>(binding, new EndpointAddress(GetServiceUri()));
//                         LoginService clientService = channelFactory.CreateChannel();
//                         using (new OperationContextScope((IContextChannel)clientService)) {
//                             ProxyHelper.AddCookiesToWCF(cookies, GetServiceUri(), p.Identity.Name, _ConnectionString, _ConnectionStringProvider);
//                             clientService.Logout();
//                             ProxyHelper.GetCookiesFromWCF(cookies, GetServiceUri(), p.Identity.Name, _ConnectionString, _ConnectionStringProvider);
//                         }
                    } else {
                        ProxyHelper.CreateWebRequestAndGetResponse(GetServiceUri() + "/Logout",
                                                                   ref cookies,
                                                                   p.Identity.Name,
                                                                   _ConnectionString,
                                                                   _ConnectionStringProvider,
                                                                   null,
                                                                   null,
                                                                   null);

                    }
                }
                SqlHelper.DeleteAllCookies(p.Identity.Name, _ConnectionString, _ConnectionStringProvider);
                Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            }
            StoreLastUserNameInOffileStore(null);
            if (UserValidated != null)
                UserValidated(this, new UserValidatedEventArgs(""));
        }


        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private string GetServiceUri()
        {
            if (string.IsNullOrEmpty(_ServiceUri))
                throw new ArgumentException(AtlasWeb.ServiceUriNotFound);
            return _ServiceUri;
        }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification="Reviewed and approved by feature crew")]
        public string ServiceUri
        {
            [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId="2#", Justification="Reviewed and approved by feature crew")]
            get {
                return _ServiceUri;
            }
            [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId="2#", Justification="Reviewed and approved by feature crew")]
            set {
                _ServiceUri = value;
                if (string.IsNullOrEmpty(_ServiceUri)) {
                    _UsingWFCService = false;
                } else {
                    _UsingWFCService = _ServiceUri.EndsWith(".svc", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public event EventHandler<UserValidatedEventArgs> UserValidated ; /*{
            add { _UserValidated += value; }
            remove { _UserValidated -= value; }
        }
        private event EventHandler<UserValidatedEventArgs> _UserValidated; */
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        // Private methods
        private bool ValidateUserCore(string username, string password, int rememberMeInt, ref int promptCount, bool tryToUseLastLoggedInUser)
        {
            // (new PermissionSet(PermissionState.Unrestricted)).Assert(); // 

            string              currentUser         = null;
            bool                validated           = false;
            string              lastLoggedInUser    = (tryToUseLastLoggedInUser ? GetLastUserNameFromOffileStore() : null);
            bool                usernameNotSupplied = string.IsNullOrEmpty(username);
            CookieContainer     cookies             = null;
            bool                sameAsLastLoggedInUser = false;
            bool                rememberMe          = (rememberMeInt == 1);
            bool                rememberMeExplicitlySet = (rememberMeInt != 2);

            if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity is ClientFormsIdentity)
                currentUser = Thread.CurrentPrincipal.Identity.Name;
            if (string.IsNullOrEmpty(lastLoggedInUser) && currentUser != null)
                lastLoggedInUser = currentUser;

            ///////////////////////////////////////////////////////////////
            // Step 1: If username not supplied, use the last remembered user
            if (usernameNotSupplied)
                username = lastLoggedInUser;

            if ((Thread.CurrentPrincipal is ClientRolePrincipal) && (Thread.CurrentPrincipal.Identity is ClientFormsIdentity) && Thread.CurrentPrincipal.Identity.Name == username)
            {
                cookies = ((ClientFormsIdentity)Thread.CurrentPrincipal.Identity).AuthenticationCookies;
            }

            ///////////////////////////////////////////////////////////////
            // Step 2: If username is same as the last remembered user,
            //         try authenticating by looking in the cookie store and calling the
            //          WebService if we are on-line
            if (!string.IsNullOrEmpty(lastLoggedInUser) && string.Compare(lastLoggedInUser, username, StringComparison.OrdinalIgnoreCase) == 0) {
                if (!ConnectivityStatus.IsOffline) {
                    validated = ValidateByCallingIsLoggedIn(lastLoggedInUser, ref cookies);
                } else {
                    validated = ProxyHelper.DoAnyCookiesExist(GetServiceUri(), lastLoggedInUser, _ConnectionString, _ConnectionStringProvider);
                }
                sameAsLastLoggedInUser = true;
            }


            if (!validated) {
                ///////////////////////////////////////////////////////////////
                // Step 3: If username is not supplied, then prompt for it
                if(usernameNotSupplied) {
                    promptCount++;
                    if (!GetCredsFromUI(ref username, ref password, ref rememberMe)) {
                        promptCount += 100; // don't prompt again
                        return false;
                    }
                    rememberMeExplicitlySet = true;
                }

                if (!ConnectivityStatus.IsOffline) {
                    ///////////////////////////////////////////////////////////////
                    // Step 4: If app is online, connect to the server
                    if (!ValidateUserByCallingLogin(username, password, rememberMe, GetServiceUri(),
                                                    _UsingWFCService, ref cookies,
                                                    _ConnectionString, _ConnectionStringProvider))
                        return false;
                    // Store hash of password
                    StoreHashedPasswordInDB(username, password);
                } else {
                    ///////////////////////////////////////////////////////////////
                    // Step 5: If app is offline, validate with offline store
                    if (!ValidateUserWithOfflineStore(username, password))
                        return false;
                }
            }

            ///////////////////////////////////////////////////////////////
            // Step 6: Store last logged in user
            if (!sameAsLastLoggedInUser || rememberMeExplicitlySet)
                StoreLastUserNameInOffileStore(rememberMe ? username : null);

            ///////////////////////////////////////////////////////////////
            // Step 7: Save principal
            if ( !(Thread.CurrentPrincipal is ClientRolePrincipal) || !(Thread.CurrentPrincipal.Identity is ClientFormsIdentity) || Thread.CurrentPrincipal.Identity.Name != username)
            {
                if (cookies == null)
                    cookies = ProxyHelper.ConstructCookieContainer(GetServiceUri(), username, _ConnectionString, _ConnectionStringProvider);
                Thread.CurrentPrincipal = new ClientRolePrincipal(new ClientFormsIdentity(username, password, this, "ClientForms", true, cookies));
            }
            if (currentUser != null && string.Compare(username, currentUser, StringComparison.OrdinalIgnoreCase) != 0)
                SqlHelper.DeleteAllCookies(currentUser, _ConnectionString, _ConnectionStringProvider);

            return true;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private string GetLastUserNameFromOffileStore()
        {
            //if (MustAssertForSql)
            //  (new PermissionSet(PermissionState.Unrestricted)).Assert();
            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                return ClientDataManager.GetAppClientData(_UsingIsolatedStore).LastLoggedInUserName;
            }

            using (DbConnection conn = SqlHelper.GetConnection(null, _ConnectionString, _ConnectionStringProvider)) {
                DbTransaction trans = null;
                try
                {
                    trans = conn.BeginTransaction();
                    DbCommand cmd = conn.CreateCommand();
                    cmd.Transaction = trans;
                    cmd.CommandText = "SELECT PropertyValue FROM ApplicationProperties WHERE PropertyName = N'LastLoggedInUserName'";
                    object o = cmd.ExecuteScalar();
                    return ((o!=null) ?  o.ToString() : null);
                } catch {
                    if (trans != null) {
                        trans.Rollback();
                        trans = null;
                    }
                    throw;
                } finally {
                    if (trans != null)
                        trans.Commit();
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private void StoreLastUserNameInOffileStore(string username)
        {
            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                ClientData cd = ClientDataManager.GetAppClientData(_UsingIsolatedStore);
                cd.LastLoggedInUserName = username;
                cd.LastLoggedInDateUtc = DateTime.UtcNow;
                cd.Save();
                return;
            }

            //if (MustAssertForSql)
            //    (new PermissionSet(PermissionState.Unrestricted)).Assert();
            using (DbConnection conn = SqlHelper.GetConnection(null, _ConnectionString, _ConnectionStringProvider)) {
                DbTransaction trans = null;
                try
                {
                    trans = conn.BeginTransaction();
                    DbCommand cmd = conn.CreateCommand();
                    cmd.Transaction = trans;
                    cmd.CommandText = "DELETE FROM ApplicationProperties WHERE PropertyName = N'LastLoggedInUserName'";
                    cmd.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(username)) {
                        cmd = conn.CreateCommand();
                        cmd.Transaction = trans;
                        cmd.CommandText = "INSERT INTO ApplicationProperties(PropertyName, PropertyValue) VALUES (N'LastLoggedInUserName', @UserName)";
                        SqlHelper.AddParameter(conn, cmd, "@UserName", username);
                        cmd.ExecuteNonQuery();

                        cmd = conn.CreateCommand();
                        cmd.Transaction = trans;
                        cmd.CommandText = "INSERT INTO ApplicationProperties(PropertyName, PropertyValue) VALUES (N'LastLoggedInDate', @Date)";
                        SqlHelper.AddParameter(conn, cmd, "@Date", DateTime.Now.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture));
                        cmd.Transaction = trans;
                        cmd.ExecuteNonQuery();
                    }
                } catch {
                    if (trans != null) {
                        trans.Rollback();
                        trans = null;
                    }
                    throw;
                } finally {
                    if (trans != null)
                        trans.Commit();
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private bool GetCredsFromUI(ref string username, ref string password, ref bool rememberMe)
        {
            if (_GetCredentialsType == null) {
                if (string.IsNullOrEmpty(_GetCredentialsTypeName))
                    return false;
                _GetCredentialsType = Type.GetType(_GetCredentialsTypeName, true, true);
            }


            ClientFormsAuthenticationCredentials creds =  ((IClientFormsAuthenticationCredentialsProvider)Activator.CreateInstance(_GetCredentialsType)).GetCredentials();
            if (creds == null)
                return false;
            username = creds.UserName;
            password = creds.Password;
            rememberMe = creds.RememberMe;
            return true;
        }


        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private void StoreHashedPasswordInDB(string username, string password)
        {
            if (!_SavePasswordHash)
                return;

            // if (MustAssertForSql)
            //    (new PermissionSet(PermissionState.Unrestricted)).Assert();
            byte[] buf = new byte[16];
            (new RNGCryptoServiceProvider()).GetBytes(buf);

            string passwordSalt = Convert.ToBase64String(buf);
            string passwordHash = EncodePassword(password, buf);

            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                ClientData cd = ClientDataManager.GetUserClientData(username, _UsingIsolatedStore);
                cd.PasswordHash = passwordHash;
                cd.PasswordSalt = passwordSalt;
                cd.Save();
                return;
            }


            using (DbConnection conn = SqlHelper.GetConnection(username, _ConnectionString, _ConnectionStringProvider)) {
                DbTransaction trans = null;
                DbCommand cmd = null;
                try {

                    trans = conn.BeginTransaction();

                    cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM UserProperties WHERE PropertyName = @PasswordHashName";
                    SqlHelper.AddParameter(conn, cmd, "@PasswordHashName", "PasswordHash_" + username);
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();

                    cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM UserProperties WHERE PropertyName = @PasswordSaltName";
                    SqlHelper.AddParameter(conn, cmd, "@PasswordSaltName", "PasswordSalt_" + username);
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();


                    cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO UserProperties(PropertyName, PropertyValue) VALUES (@PasswordHashName, @PasswordHashValue)";
                    SqlHelper.AddParameter(conn, cmd, "@PasswordHashName", "PasswordHash_" + username);
                    SqlHelper.AddParameter(conn, cmd, "@PasswordHashValue", passwordHash);
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();

                    cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO UserProperties(PropertyName, PropertyValue) VALUES (@PasswordSaltName, @PasswordSaltValue)";
                    SqlHelper.AddParameter(conn, cmd, "@PasswordSaltName", "PasswordSalt_" + username);
                    SqlHelper.AddParameter(conn, cmd, "@PasswordSaltValue", passwordSalt);
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                } catch {
                    if (trans != null) {
                        trans.Rollback();
                        trans = null;
                    }
                    throw;
                } finally {
                    if (trans != null)
                        trans.Commit();
                }
            }
        }


        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private static string EncodePassword(string password, byte[] salt)
        {
            byte [] bufPass = Encoding.Unicode.GetBytes(password);
            byte [] bufAll = new byte[salt.Length + bufPass.Length];
            salt.CopyTo(bufAll, 0);
            bufPass.CopyTo(bufAll, salt.Length);

            byte[] buffer = null;
            // SHA1 is forbidden for *new* code, but this is an existing feature that we could
            // not change without locking users out of their existing membership databases.
            // We are tracking upgrading this to a stronger algorithm in DevDiv #286797.
#pragma warning disable 618 // [Obsolete] warning
            using(SHA1 s = CryptoAlgorithms.CreateSHA1())
                buffer = s.ComputeHash(bufAll);
#pragma warning restore 618
            return Convert.ToBase64String(buffer);
        }


        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        private bool ValidateByCallingIsLoggedIn(string username, ref CookieContainer cookies)
        {
            if (_UsingWFCService) {
                throw new NotImplementedException();

//                 CustomBinding binding = ProxyHelper.GetBinding();
//                 ChannelFactory<LoginService> channelFactory = new ChannelFactory<LoginService>(binding, new EndpointAddress(GetServiceUri()));
//                 LoginService clientService = channelFactory.CreateChannel();
//                 using (new OperationContextScope((IContextChannel)clientService)) {
//                     ProxyHelper.AddCookiesToWCF(cookies, GetServiceUri(), username, _ConnectionString, _ConnectionStringProvider);
//                     bool validated = clientService.IsLoggedIn();
//                     ProxyHelper.GetCookiesFromWCF(cookies, GetServiceUri(), username, _ConnectionString, _ConnectionStringProvider);
//                     return validated;
//                 }
            } else {
                object o = ProxyHelper.CreateWebRequestAndGetResponse(GetServiceUri() + "/IsLoggedIn",
                                                                      ref cookies,
                                                                      username,
                                                                      _ConnectionString,
                                                                      _ConnectionStringProvider,
                                                                      null, null,
                                                                      typeof(bool));
                return ((o != null) && (o is bool) && ((bool)o) == true);
            }

        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private bool ValidateUserWithOfflineStore(string username, string password)
        {
            if (!_SavePasswordHash)
                return false;

            string passwordHash = null;
            string passwordSalt = null;

            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                ClientData cd = ClientDataManager.GetUserClientData(username, _UsingIsolatedStore);
                passwordHash = cd.PasswordHash;
                passwordSalt = cd.PasswordSalt;
            } else {

                // if (MustAssertForSql)
                //     (new PermissionSet(PermissionState.Unrestricted)).Assert();
                DbTransaction trans = null;
                using (DbConnection conn = SqlHelper.GetConnection(username, _ConnectionString, _ConnectionStringProvider)) {
                    try {
                        DbCommand cmd = conn.CreateCommand();
                        cmd.Transaction = trans;
                        cmd.CommandText = "SELECT PropertyValue FROM UserProperties WHERE PropertyName = @PasswordHashName";
                        SqlHelper.AddParameter(conn, cmd, "@PasswordHashName", "PasswordHash_" + username);
                        passwordHash = cmd.ExecuteScalar() as string;

                        cmd = conn.CreateCommand();
                        cmd.Transaction = trans;
                        cmd.CommandText = "SELECT PropertyValue FROM UserProperties WHERE PropertyName = @PasswordSaltName";
                        SqlHelper.AddParameter(conn, cmd, "@PasswordSaltName", "PasswordSalt_" + username);
                        passwordSalt = cmd.ExecuteScalar() as string;
                    } catch {
                        if (trans != null) {
                            trans.Rollback();
                            trans = null;
                        }
                        throw;
                    } finally {
                        if (trans != null)
                            trans.Commit();
                    }
                }
            }

            if (string.IsNullOrEmpty(passwordHash) || string.IsNullOrEmpty(passwordSalt))
                return false;

            byte [] buf = Convert.FromBase64String(passwordSalt);
            return passwordHash == EncodePassword(password, buf);
        }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
//         private bool _MustAssertForSqlDecided = false;
//         private bool _MustAssertForSql        = false;
//         private bool MustAssertForSql {
//            get {
//                if (!_MustAssertForSqlDecided) {
//                    _MustAssertForSql = (_ConnectionString == "Data Source = |SQL\\CE|");
//                    _MustAssertForSqlDecided = true;
//                }
//                return _MustAssertForSql;
//            }
//         }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        // Non-implemented public properties
        public override bool EnablePasswordRetrieval { get { return false; } }
        public override bool EnablePasswordReset { get { return false; } }
        public override bool RequiresQuestionAndAnswer { get { return false; } }
        public override string ApplicationName { get { return ""; } set { } }
        public override int MaxInvalidPasswordAttempts { get { return int.MaxValue; } }
        public override int PasswordAttemptWindow { get { return int.MaxValue; } }
        public override bool RequiresUniqueEmail { get { return false; } }
        public override MembershipPasswordFormat PasswordFormat { get { return MembershipPasswordFormat.Hashed; } }
        public override int MinRequiredPasswordLength { get { return 1; } }
        public override int MinRequiredNonAlphanumericCharacters { get { return 0; } }
        public override string PasswordStrengthRegularExpression { get { return "*"; } }


        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
                                                   bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotSupportedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotSupportedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotSupportedException();
        }


        public override string ResetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotSupportedException();
        }


        public override bool UnlockUser(string username)
        {
            throw new NotSupportedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotSupportedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotSupportedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotSupportedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotSupportedException();
        }


        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }


        public override int GetNumberOfUsersOnline()
        {
            throw new NotSupportedException();
        }


        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }
    }
}
