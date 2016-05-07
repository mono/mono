//------------------------------------------------------------------------------
// <copyright file="ClientRoleProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.ClientServices.Providers
{
    using System;
    using System.Web.Security;
    using System.Threading;
    using System.Security.Principal;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;
    using System.Windows.Forms;
    using System.Collections.Specialized;
    using System.Net;
    using System.Web.ClientServices;
    using System.Web.Resources;
    using System.Configuration;
    using System.Globalization;
    using System.Collections;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.AccessControl;
    using System.Diagnostics.CodeAnalysis;

    public class ClientRoleProvider : RoleProvider
    {
        private string      _ConnectionString           = null;
        private string      _ConnectionStringProvider   = null;
        private string      _ServiceUri                 = null;
        private string[]    _Roles                      = null;
        private string      _CurrentUser                = null;
        private int         _CacheTimeout               = 1440; // 1 day in minutes
        private DateTime    _CacheExpiryDate            = DateTime.UtcNow;
        private bool        _HonorCookieExpiry          = false;
        private bool        _UsingFileSystemStore       = false;
        private bool        _UsingIsolatedStore         = false;
        private bool        _UsingWFCService            = false;

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            base.Initialize(name, config);
            ServiceUri = config["serviceUri"];

            string temp = config["cacheTimeout"];
            if (!string.IsNullOrEmpty(temp))
                _CacheTimeout = int.Parse(temp, CultureInfo.InvariantCulture);

            _ConnectionString = config["connectionStringName"];
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

            temp = config["honorCookieExpiry"];
            if (!string.IsNullOrEmpty(temp))
                _HonorCookieExpiry = (string.Compare(temp, "true", StringComparison.OrdinalIgnoreCase) == 0);

            config.Remove("name");
            config.Remove("description");
            config.Remove("cacheTimeout");
            config.Remove("connectionStringName");
            config.Remove("serviceUri");
            config.Remove("honorCookieExpiry");
            foreach (string attribUnrecognized in config.Keys)
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, AtlasWeb.AttributeNotRecognized, attribUnrecognized));
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override bool IsUserInRole(string username, string roleName)
        {
            string[] roles = GetRolesForUser(username);
            foreach (string role in roles)
                if (string.Compare(role, roleName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override string[] GetRolesForUser(string username)
        {
            lock (this) {
                IPrincipal p = Thread.CurrentPrincipal;
                if (p == null || p.Identity == null || !p.Identity.IsAuthenticated)
                    return new string[0];
                if (!string.IsNullOrEmpty(username) && string.Compare(username, p.Identity.Name, StringComparison.OrdinalIgnoreCase) != 0)
                    throw new ArgumentException(AtlasWeb.ArgumentMustBeCurrentUser, "username");

                // Serve from memory cache if it is for the last-user, and the roles are fresh
                if (string.Compare(_CurrentUser, p.Identity.Name, StringComparison.OrdinalIgnoreCase) == 0 && DateTime.UtcNow < _CacheExpiryDate)
                    return _Roles;

                // Fetch from database, cache it, and serve it
                if (GetRolesFromDBForUser(p.Identity.Name)) // Return true, only if the roles are fresh. Also, sets _CurrentUser, _Roles and _CacheExpiryDate
                    return _Roles;
                if (ConnectivityStatus.IsOffline)
                    return new string[0];

                // Reset variables
                _Roles = null;
                _CacheExpiryDate = DateTime.UtcNow;
                _CurrentUser = p.Identity.Name;
                GetRolesForUserCore(p.Identity);
                if (!_HonorCookieExpiry && _Roles.Length < 1 && (p.Identity is ClientFormsIdentity))
                {
                    ((ClientFormsIdentity)p.Identity).RevalidateUser();
                    GetRolesForUserCore(p.Identity);
                }
                StoreRolesForCurrentUser();
                return _Roles;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public void ResetCache()
        {
            lock (this){
                _Roles = null;
                _CacheExpiryDate = DateTime.UtcNow;
                RemoveRolesFromDB();
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        // Private methods
        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        private void GetRolesForUserCore(IIdentity identity)
        {
            // (new PermissionSet(PermissionState.Unrestricted)).Assert(); // 

            CookieContainer cookies = null;
            if (identity is ClientFormsIdentity)
                cookies = ((ClientFormsIdentity)identity).AuthenticationCookies;

            if (_UsingWFCService) {
                throw new NotImplementedException();

//                 CustomBinding binding = ProxyHelper.GetBinding();
//                 ChannelFactory<RolesService> channelFactory = new ChannelFactory<RolesService>(binding, new EndpointAddress(GetServiceUri())); //(@"http://localhost/AuthSvc/service.svc"));
//                 RolesService clientService = channelFactory.CreateChannel();
//                 using (new OperationContextScope((IContextChannel)clientService)) {
//                     ProxyHelper.AddCookiesToWCF(cookies, GetServiceUri(), _CurrentUser, _ConnectionString, _ConnectionStringProvider);
//                     _Roles = clientService.GetRolesForCurrentUser();
//                     if (_Roles == null)
//                         _Roles = new string[0];
//                     ProxyHelper.GetCookiesFromWCF(cookies, GetServiceUri(), _CurrentUser, _ConnectionString, _ConnectionStringProvider);
//                 }
            } else {
                object o = ProxyHelper.CreateWebRequestAndGetResponse(GetServiceUri() + "/GetRolesForCurrentUser",
                                                                      ref cookies,
                                                                      identity.Name,
                                                                      _ConnectionString,
                                                                      _ConnectionStringProvider,
                                                                      null,
                                                                      null,
                                                                      typeof(string []));
                if (o != null)
                    _Roles = (string []) o;
                else
                    _Roles = new string[0];
            }
            _CacheExpiryDate = DateTime.UtcNow.AddMinutes(_CacheTimeout);
        }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private void RemoveRolesFromDB()
        {
            // if (MustAssertForSql)
            //     (new PermissionSet(PermissionState.Unrestricted)).Assert();
            if (string.IsNullOrEmpty(_CurrentUser))
                return;


            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                ClientData cd = ClientDataManager.GetUserClientData(_CurrentUser, _UsingIsolatedStore);
                cd.Roles = null;
                cd.Save();
                return;
            }


            using (DbConnection conn = SqlHelper.GetConnection(_CurrentUser, _ConnectionString, _ConnectionStringProvider)) {
                DbTransaction trans = null;
                try {
                    trans = conn.BeginTransaction();
                    DbCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM Roles WHERE UserName = @UserName";
                    SqlHelper.AddParameter(conn, cmd, "@UserName", _CurrentUser);
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();

                    cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM UserProperties WHERE PropertyName = @RolesCachedDate";
                    SqlHelper.AddParameter(conn, cmd, "@RolesCachedDate", "RolesCachedDate_" + _CurrentUser);
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
        private void StoreRolesForCurrentUser()
        {

            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                ClientData cd = ClientDataManager.GetUserClientData(_CurrentUser, _UsingIsolatedStore);
                cd.Roles = _Roles;
                cd.RolesCachedDateUtc = DateTime.UtcNow;
                cd.Save();
                return;
            }

            // if (MustAssertForSql)
            //     (new PermissionSet(PermissionState.Unrestricted)).Assert();
            RemoveRolesFromDB(); // Remove all old roles

            DbTransaction trans = null;
            using (DbConnection conn = SqlHelper.GetConnection(_CurrentUser, _ConnectionString, _ConnectionStringProvider)) {
                try {
                    trans = conn.BeginTransaction();

                    // Add the roles
                    DbCommand cmd = null;
                    foreach(string role in _Roles) {
                        cmd = conn.CreateCommand();
                        cmd.CommandText = "INSERT INTO Roles(UserName, RoleName) VALUES(@UserName, @RoleName)";
                        SqlHelper.AddParameter(conn, cmd, "@UserName", _CurrentUser);
                        SqlHelper.AddParameter(conn, cmd, "@RoleName", role);
                        cmd.Transaction = trans;
                        cmd.ExecuteNonQuery();
                    }
                    cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO UserProperties (PropertyName, PropertyValue) VALUES(@RolesCachedDate, @Date)";
                    SqlHelper.AddParameter(conn, cmd, "@RolesCachedDate", "RolesCachedDate_" + _CurrentUser);
                    SqlHelper.AddParameter(conn, cmd, "@Date", DateTime.UtcNow.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture));
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
        private bool GetRolesFromDBForUser(string username)
        {
            _Roles = null;
            _CacheExpiryDate = DateTime.UtcNow;
            _CurrentUser = username;

            // if (MustAssertForSql)
            //     (new PermissionSet(PermissionState.Unrestricted)).Assert();
            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                ClientData cd = ClientDataManager.GetUserClientData(username, _UsingIsolatedStore);
                if (cd.Roles == null)
                    return false;
                _Roles = cd.Roles;
                _CacheExpiryDate = cd.RolesCachedDateUtc.AddMinutes(_CacheTimeout);
                if (!ConnectivityStatus.IsOffline && _CacheExpiryDate < DateTime.UtcNow) // expired roles
                    return false;
                return true;
            }

            using (DbConnection conn = SqlHelper.GetConnection(_CurrentUser, _ConnectionString, _ConnectionStringProvider)) {
                DbTransaction trans = null;
                try {
                    trans = conn.BeginTransaction();
                    DbCommand cmd = conn.CreateCommand();
                    cmd.Transaction = trans;
                    cmd.CommandText = "SELECT PropertyValue FROM UserProperties WHERE PropertyName = @RolesCachedDate";
                    SqlHelper.AddParameter(conn, cmd, "@RolesCachedDate", "RolesCachedDate_" + _CurrentUser);
                    string date = cmd.ExecuteScalar() as string;
                    if (date == null) // not cached
                        return false;

                    long filetime = long.Parse(date, CultureInfo.InvariantCulture);
                    _CacheExpiryDate = DateTime.FromFileTimeUtc(filetime).AddMinutes(_CacheTimeout);
                    if (!ConnectivityStatus.IsOffline && _CacheExpiryDate < DateTime.UtcNow) // expired roles
                        return false;

                    cmd = conn.CreateCommand();
                    cmd.Transaction = trans;
                    cmd.CommandText = "SELECT RoleName FROM Roles WHERE UserName = @UserName ORDER BY RoleName";
                    SqlHelper.AddParameter(conn, cmd, "@UserName", _CurrentUser);
                    ArrayList al = new ArrayList();
                    using (DbDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read())
                            al.Add(reader.GetString(0));
                    }
                    _Roles = new string[al.Count];
                    for (int iter = 0; iter < al.Count; iter++)
                        _Roles[iter] = (string)al[iter];
                    return true;
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
        private string GetServiceUri()
        {
            if (string.IsNullOrEmpty(_ServiceUri))
                throw new ArgumentException(AtlasWeb.ServiceUriNotFound);
            return _ServiceUri;
        }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Reviewed and approved by feature crew")]
        public string ServiceUri
        {
            [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "Reviewed and approved by feature crew")]
            get {
                return _ServiceUri;
            }
            [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "Reviewed and approved by feature crew")]
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

        public override string ApplicationName { get { return ""; } set { } }
        public override void CreateRole(string roleName)
        {
            throw new NotSupportedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotSupportedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotSupportedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotSupportedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotSupportedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotSupportedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotSupportedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotSupportedException();
        }

    }
}
