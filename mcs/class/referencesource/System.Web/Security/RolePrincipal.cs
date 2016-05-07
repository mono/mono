//------------------------------------------------------------------------------
// <copyright file="RolePrincipal.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * RolePrincipal
 *
 * Copyright (c) 2002 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Claims;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Security.Cryptography;
    using System.Web.Util;

    [Serializable]
    public class RolePrincipal : ClaimsPrincipal, ISerializable    
    {
        [NonSerialized]
        static Type s_type;

        public RolePrincipal(IIdentity identity, string encryptedTicket)
        {
            if (identity == null)
                throw new ArgumentNullException( "identity" );

            if (encryptedTicket == null)
                throw new ArgumentNullException( "encryptedTicket" );
            _Identity = identity;
            _ProviderName = Roles.Provider.Name;
            if (identity.IsAuthenticated)
                InitFromEncryptedTicket(encryptedTicket);
            else
                Init();
        }

        public RolePrincipal(IIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException( "identity" );
            _Identity = identity;
            Init();
        }

        public RolePrincipal(string providerName, IIdentity identity )
        {
            if (identity == null)
                throw new ArgumentNullException( "identity" );

            if( providerName == null)
                throw new ArgumentException( SR.GetString(SR.Role_provider_name_invalid) , "providerName" );

            _ProviderName = providerName;
            if (Roles.Providers[providerName] == null)
                throw new ArgumentException(SR.GetString(SR.Role_provider_name_invalid), "providerName");

            _Identity = identity;
            Init();
        }

        public RolePrincipal(string providerName, IIdentity identity, string encryptedTicket )
        {
            if (identity == null)
                throw new ArgumentNullException( "identity" );

            if (encryptedTicket == null)
                throw new ArgumentNullException( "encryptedTicket" );

            if( providerName == null)
                throw new ArgumentException( SR.GetString(SR.Role_provider_name_invalid) , "providerName" );

            _ProviderName = providerName;
            if (Roles.Providers[_ProviderName] == null)
                throw new ArgumentException(SR.GetString(SR.Role_provider_name_invalid), "providerName");
            _Identity = identity;
            if (identity.IsAuthenticated)
                InitFromEncryptedTicket(encryptedTicket);
            else
                Init();
        }

        private void InitFromEncryptedTicket( string encryptedTicket )
        {
            if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc) && HttpContext.Current != null)
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_BEGIN, HttpContext.Current.WorkerRequest);

            if (string.IsNullOrEmpty(encryptedTicket))
                goto Exit;

            byte[] bTicket = CookieProtectionHelper.Decode(Roles.CookieProtectionValue, encryptedTicket, Purpose.RolePrincipal_Ticket);
            if (bTicket == null)
                goto Exit;

            RolePrincipal   rp = null;
            MemoryStream    ms = null;
            try{
                ms = new System.IO.MemoryStream(bTicket);
                rp = (new BinaryFormatter()).Deserialize(ms) as RolePrincipal;
            } catch {
            } finally {
                ms.Close();
            }
            if (rp == null)
                goto Exit;
            if (!StringUtil.EqualsIgnoreCase(rp._Username, _Identity.Name))
                goto Exit;
            if (!StringUtil.EqualsIgnoreCase(rp._ProviderName, _ProviderName))
                goto Exit;
            if (DateTime.UtcNow > rp._ExpireDate)
                goto Exit;

            _Version = rp._Version;
            _ExpireDate = rp._ExpireDate;
            _IssueDate = rp._IssueDate;
            _IsRoleListCached = rp._IsRoleListCached;
            _CachedListChanged = false;
            _Username = rp._Username;
            _Roles = rp._Roles;



            // will it be the case that _Identity.Name != _Username?

            RenewIfOld();

            if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc) && HttpContext.Current != null)
                EtwTrace.Trace( EtwTraceType.ETW_TYPE_ROLE_END, HttpContext.Current.WorkerRequest, "RolePrincipal", _Identity.Name);

            return;
        Exit:
            Init();
            _CachedListChanged = true;
            if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc) && HttpContext.Current != null)
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_END, HttpContext.Current.WorkerRequest, "RolePrincipal", _Identity.Name);
            return;
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        private void Init() {
            _Version = 1;
            _IssueDate = DateTime.UtcNow;
            _ExpireDate = DateTime.UtcNow.AddMinutes(Roles.CookieTimeout);
            //_CookiePath = Roles.CookiePath;
            _IsRoleListCached = false;
            _CachedListChanged = false;
            if (_ProviderName == null)
                _ProviderName = Roles.Provider.Name;
            if (_Roles == null)
                _Roles = new HybridDictionary(true);
            if (_Identity != null)
                _Username = _Identity.Name;

            AddIdentityAttachingRoles(_Identity);
        }

        /// <summary>
        /// helper method to bind roles with an identity.
        /// </summary>
        [SecuritySafeCritical]
        void AddIdentityAttachingRoles(IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = null;

            if (identity is ClaimsIdentity)
            {
                claimsIdentity = (identity as ClaimsIdentity).Clone();
            }
            else
            {
                claimsIdentity = new ClaimsIdentity(identity);
            }

            AttachRoleClaims(claimsIdentity);
            base.AddIdentity(claimsIdentity);
        }

        [SecuritySafeCritical]
        void AttachRoleClaims(ClaimsIdentity claimsIdentity)
        {
            RoleClaimProvider claimProvider = new RoleClaimProvider(this, claimsIdentity);

            if (s_type == null)
            {
                s_type = typeof(DynamicRoleClaimProvider);
            }

            s_type.InvokeMember("AddDynamicRoleClaims", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { claimsIdentity, claimProvider.Claims }, CultureInfo.InvariantCulture);
        }


        ////////////////////////////////////////////////////////////
        // Public properties

        public int       Version           { get { return _Version;}}
        public DateTime  ExpireDate        { get { return _ExpireDate.ToLocalTime();}}
        public DateTime  IssueDate         { get { return _IssueDate.ToLocalTime();}}
        // DevDiv Bugs: 9446
        // Expired should check against DateTime.UtcNow instead of DateTime.Now because
        // _ExpireData is a Utc DateTime.
        public bool      Expired           { get { return _ExpireDate < DateTime.UtcNow;}}
        public String    CookiePath        { get { return Roles.CookiePath;}} //
        public override  IIdentity Identity          { get { return _Identity; }}
        public bool      IsRoleListCached  { get { return _IsRoleListCached; }}
        public bool      CachedListChanged { get { return _CachedListChanged; }}
        public string    ProviderName      { get { return _ProviderName; } }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // Public functions

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public string ToEncryptedTicket()
        {
            if (!Roles.Enabled)
                return null;
            if (_Identity != null && !_Identity.IsAuthenticated)
                return null;
            if (_Identity == null && string.IsNullOrEmpty(_Username))
                return null;
            if (_Roles.Count > Roles.MaxCachedResults)
                return null;

            MemoryStream ms = new System.IO.MemoryStream();
            byte[] buf = null;
            IIdentity id = _Identity;
            try {
                _Identity = null;
                BinaryFormatter bf = new BinaryFormatter();
                bool originalSerializingForCookieValue = _serializingForCookie;
                try {
                    // DevDiv 481327: ClaimsPrincipal is an expensive type to serialize and deserialize. If the developer is using
                    // role management, then he is going to be querying regular ASP.NET membership roles rather than claims, so
                    // we can cut back on the number of bytes sent across the wire by ignoring any claims in the underlying
                    // identity. Otherwise we risk sending a cookie too large for the browser to handle.
                    _serializingForCookie = true;
                    bf.Serialize(ms, this);
                }
                finally {
                    _serializingForCookie = originalSerializingForCookieValue;
                }
                buf = ms.ToArray();
            } finally {
                ms.Close();
                _Identity = id;
            }

            return CookieProtectionHelper.Encode(Roles.CookieProtectionValue, buf, Purpose.RolePrincipal_Ticket);
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        private void RenewIfOld() {
            if (!Roles.CookieSlidingExpiration)
                return;
            DateTime dtN = DateTime.UtcNow;
            TimeSpan t1  = dtN - _IssueDate;
            TimeSpan t2  = _ExpireDate - dtN;

            if (t2 > t1)
                return;
            _ExpireDate = dtN + (_ExpireDate - _IssueDate);
            _IssueDate = dtN;
            _CachedListChanged = true;
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        public string[] GetRoles()
        {
            if (_Identity == null)
                throw new ProviderException(SR.GetString(SR.Role_Principal_not_fully_constructed));

            if (!_Identity.IsAuthenticated)
                return new string[0];
            string[] roles;

            if (!_IsRoleListCached || !_GetRolesCalled) {
                _Roles.Clear();
                roles = Roles.Providers[_ProviderName].GetRolesForUser(Identity.Name);
                foreach (string role in roles)
                    if (_Roles[role] == null)
                        _Roles.Add(role, String.Empty);
                _IsRoleListCached = true;
                _CachedListChanged = true;
                _GetRolesCalled = true;
                return roles;
            } else {
                roles = new string[_Roles.Count];
                int index = 0;
                foreach (string role in _Roles.Keys)
                    roles[index++] = role;
                return roles;
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        public override bool IsInRole(string role)
        {
            if (_Identity == null)
                throw new ProviderException(SR.GetString(SR.Role_Principal_not_fully_constructed));

            if (!_Identity.IsAuthenticated || role == null)
                return false;
            role = role.Trim();
            if (!IsRoleListCached) {
                _Roles.Clear();
                string[] roles = Roles.Providers[_ProviderName].GetRolesForUser(Identity.Name);
                foreach(string roleTemp in roles)
                    if (_Roles[roleTemp] == null)
                        _Roles.Add(roleTemp, String.Empty);

                _IsRoleListCached = true;
                _CachedListChanged = true;
            }
            if (_Roles[role] != null)
                return true;

            // 
            return base.IsInRole(role);
        }

        public void SetDirty()
        {
            _IsRoleListCached = false;
            _CachedListChanged = true;
        }


        protected RolePrincipal(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
            _Version = info.GetInt32("_Version");
            _ExpireDate = info.GetDateTime("_ExpireDate");
            _IssueDate = info.GetDateTime("_IssueDate");
            try {
                _Identity = info.GetValue("_Identity", typeof(IIdentity)) as IIdentity;
            } catch { } // Ignore Exceptions
            _ProviderName = info.GetString("_ProviderName");
            _Username = info.GetString("_Username");
            _IsRoleListCached = info.GetBoolean("_IsRoleListCached");
            _Roles = new HybridDictionary(true);
            string allRoles = info.GetString("_AllRoles");
            if (allRoles != null) {
                foreach(string role in allRoles.Split(new char[] {','}))
                    if (_Roles[role] == null)
                        _Roles.Add(role, String.Empty);
            }

            // attach ourselves to the first valid claimsIdentity.  
            bool found = false;
            foreach (var claimsIdentity in base.Identities)
            {
                if (claimsIdentity != null)
                {
                    AttachRoleClaims(claimsIdentity);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                AddIdentityAttachingRoles(new ClaimsIdentity(_Identity));
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (!_serializingForCookie) {
                base.GetObjectData(info, context);
            }

            info.AddValue("_Version", _Version);

            info.AddValue("_ExpireDate", _ExpireDate);
            info.AddValue("_IssueDate", _IssueDate);
            try {
                info.AddValue("_Identity", _Identity);
            } catch { } // Ignore Exceptions
            info.AddValue("_ProviderName", _ProviderName);
            info.AddValue("_Username", _Identity == null ? _Username : _Identity.Name);
            info.AddValue("_IsRoleListCached", _IsRoleListCached);
            if (_Roles.Count > 0) {
                StringBuilder sb = new StringBuilder(_Roles.Count * 10);
                foreach(object role in _Roles.Keys)
                    sb.Append(((string)role) + ",");
                string allRoles = sb.ToString();
                info.AddValue("_AllRoles", allRoles.Substring(0, allRoles.Length - 1));
            } else {
                info.AddValue("_AllRoles", String.Empty);
            }

        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        private int         _Version;
        private DateTime    _ExpireDate;
        private DateTime    _IssueDate;
        private IIdentity   _Identity;
        private string      _ProviderName;
        private string      _Username;
        private bool        _IsRoleListCached;
        private bool        _CachedListChanged;

        [ThreadStatic]
        private static bool _serializingForCookie;

        [NonSerialized]
        private HybridDictionary _Roles = null;
        [NonSerialized]
        private bool _GetRolesCalled;
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
     }
}
