//------------------------------------------------------------------------------
// <copyright file="AuthorizationRule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Security.Principal;
    using System.Web.Util;
    using System.ComponentModel;
    using System.Security.Permissions;

    public sealed class AuthorizationRule : ConfigurationElement {
        private static readonly TypeConverter s_PropConverter = new CommaDelimitedStringCollectionConverter();

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propVerbs =
            new ConfigurationProperty("verbs",
                                        typeof(CommaDelimitedStringCollection),
                                        null,
                                        s_PropConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUsers =
            new ConfigurationProperty("users",
                                        typeof(CommaDelimitedStringCollection),
                                        null,
                                        s_PropConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRoles =
            new ConfigurationProperty("roles",
                                        typeof(CommaDelimitedStringCollection),
                                        null,
                                        s_PropConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private AuthorizationRuleAction _Action = AuthorizationRuleAction.Allow;
        internal string _ActionString = AuthorizationRuleAction.Allow.ToString();


        private string _ElementName = "allow";

        private CommaDelimitedStringCollection _Roles = null;
        private CommaDelimitedStringCollection _Verbs = null;
        private CommaDelimitedStringCollection _Users = null;

        private StringCollection _RolesExpanded;
        private StringCollection _UsersExpanded;

        private char[] _delimiters = { ',' };

        private string machineName = null;
        private const String _strAnonUserTag = "?";
        private const String _strAllUsersTag = "*";
        private bool _AllUsersSpecified = false;
        private bool _AnonUserSpecified = false;
        private bool DataReady = false;
        private bool _Everyone = false;
        internal bool Everyone { get { return _Everyone; } }
        private bool _ActionModified = false;

        static AuthorizationRule() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();

            // If updating, check out CloneMutableExposedObjects
            _properties.Add(_propVerbs);
            _properties.Add(_propUsers);
            _properties.Add(_propRoles);
        }

        internal AuthorizationRule() {
        }

        public AuthorizationRule(AuthorizationRuleAction action) : this() {
            Action = action;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        protected override void Unmerge(ConfigurationElement sourceElement,
                                                ConfigurationElement parentElement,
                                                ConfigurationSaveMode saveMode) {
            AuthorizationRule parentProviders = parentElement as AuthorizationRule;
            AuthorizationRule sourceProviders = sourceElement as AuthorizationRule;

            if (parentProviders != null) {
                parentProviders.UpdateUsersRolesVerbs();
            }
            if (sourceProviders != null) {
                sourceProviders.UpdateUsersRolesVerbs();
            }
            base.Unmerge(sourceElement, parentElement, saveMode);
        }

        protected override void Reset(ConfigurationElement parentElement) {
            AuthorizationRule parentProviders = parentElement as AuthorizationRule;

            if (parentProviders != null) {
                parentProviders.UpdateUsersRolesVerbs();
            }
            base.Reset(parentElement);
            EvaluateData();
        }

        internal void AddRole(string role) {
            if (!String.IsNullOrEmpty(role)) {
                role = role.ToLower(CultureInfo.InvariantCulture);
            }
            Roles.Add(role);
            RolesExpanded.Add(ExpandName(role));
        }

        internal void AddUser(string user) {
            if (!String.IsNullOrEmpty(user)) {
                user = user.ToLower(CultureInfo.InvariantCulture);
            }
            Users.Add(user);
            UsersExpanded.Add(ExpandName(user));
        }

        private void UpdateUsersRolesVerbs() {
            CommaDelimitedStringCollection roles;
            CommaDelimitedStringCollection users;
            CommaDelimitedStringCollection verbs;

            roles = (CommaDelimitedStringCollection)Roles;
            users = (CommaDelimitedStringCollection)Users;
            verbs = (CommaDelimitedStringCollection)Verbs;

            if (roles.IsModified) {
                _RolesExpanded = null;   // throw away old collection and force a new one to be created
                base[_propRoles] = roles;  // Update property bag
            }

            if (users.IsModified) {
                _UsersExpanded = null;   // throw away old collection and force a new one to be created
                base[_propUsers] = users;  // Update property bag
            }

            if (verbs.IsModified) {
                base[_propVerbs] = verbs;  // Update property bag
            }
        }
        protected override bool IsModified() {
            UpdateUsersRolesVerbs();
            return _ActionModified || base.IsModified() || (((CommaDelimitedStringCollection)Users).IsModified) ||
                (((CommaDelimitedStringCollection)Roles).IsModified) ||
                (((CommaDelimitedStringCollection)Verbs).IsModified);
        }

        protected override void ResetModified() {
            _ActionModified = false;
            base.ResetModified();
        }

        public override bool Equals(object obj) {
            AuthorizationRule o = obj as AuthorizationRule;
            bool bRet = false;
            if (o != null) {
                UpdateUsersRolesVerbs();

                bRet = (o.Verbs.ToString() == Verbs.ToString() &&
                        o.Roles.ToString() == Roles.ToString() &&
                        o.Users.ToString() == Users.ToString() &&
                        o.Action == Action);
            }
            return bRet;
        }

        public override int GetHashCode() {
            string __verbs = Verbs.ToString();
            string __roles = Roles.ToString();
            string __users = Users.ToString();

            if (__verbs == null) {
                __verbs = String.Empty;
            }
            if (__roles == null) {
                __roles = String.Empty;
            }
            if (__users == null) {
                __users = String.Empty;
            }

            int hHashCode = HashCodeCombiner.CombineHashCodes(__verbs.GetHashCode(), __roles.GetHashCode(),
                                                              __users.GetHashCode(), (int)Action);

            return hHashCode;
        }
        
        protected override void SetReadOnly() {
            ((CommaDelimitedStringCollection)Users).SetReadOnly();
            ((CommaDelimitedStringCollection)Roles).SetReadOnly();
            ((CommaDelimitedStringCollection)Verbs).SetReadOnly();
            base.SetReadOnly();
        }

        /// <devdoc>
        ///     <para>  Defines the action that needs to be taken if the rule is satisfied.
        ///     </para>
        /// </devdoc>

        //
        // No Configuration properties are set on this property because its supposed to be hidden to tools.
        //
        public AuthorizationRuleAction Action {
            get {
                return _Action;
            }
            set {
                _ElementName = value.ToString().ToLower(CultureInfo.InvariantCulture);
                _Action = value;
                _ActionString = _Action.ToString();
                _ActionModified = true;
            }
        }

        /// <devdoc>
        ///     <para> Defines a list of verbs needed to satisfy the rule         
        ///     </para>
        /// </devdoc>

        [ConfigurationProperty("verbs")]
        [TypeConverter(typeof(CommaDelimitedStringCollectionConverter))]
        public StringCollection Verbs {
            get {
                if (_Verbs == null) {
                    CommaDelimitedStringCollection propertyBagValue;

                    propertyBagValue = (CommaDelimitedStringCollection)base[_propVerbs];

                    if (propertyBagValue == null) {
                        _Verbs = new CommaDelimitedStringCollection();
                    }
                    else {
                        // Clone it so we don't give back same mutable 
                        // object as possibly parent
                        _Verbs = propertyBagValue.Clone();
                    }
                }

                return (StringCollection)_Verbs;
            }
        }

        /// <devdoc>
        ///     <para> Defines a list of users authorized for this rule.
        ///     </para>
        /// </devdoc>

        [ConfigurationProperty("users")]
        [TypeConverter(typeof(CommaDelimitedStringCollectionConverter))]
        public StringCollection Users {
            get {
                if (_Users == null) {
                    CommaDelimitedStringCollection propertyBagValue;

                    propertyBagValue = (CommaDelimitedStringCollection)base[_propUsers];

                    if (propertyBagValue == null) {
                        _Users = new CommaDelimitedStringCollection();
                    }
                    else {
                        // Clone it so we don't give back same mutable 
                        // object as possibly parent
                        _Users = propertyBagValue.Clone();
                    }

                    _UsersExpanded = null; // throw away old collection and force a new one to be created
                }

                return (StringCollection)_Users;
            }
        }

        [ConfigurationProperty("roles")]
        [TypeConverter(typeof(CommaDelimitedStringCollectionConverter))]
        public StringCollection Roles {
            get {
                if (_Roles == null) {
                    CommaDelimitedStringCollection propertyBagValue;

                    propertyBagValue = (CommaDelimitedStringCollection)base[_propRoles];

                    if (propertyBagValue == null) {
                        _Roles = new CommaDelimitedStringCollection();
                    }
                    else {
                        // Clone it so we don't give back same mutable 
                        // object as possibly parent
                        _Roles = propertyBagValue.Clone();
                    }

                    _RolesExpanded = null; // throw away old collection and force a new one to be created
                }

                return (StringCollection)_Roles;
            }
        }

        internal StringCollection UsersExpanded {
            get {
                if (_UsersExpanded == null) {
                    _UsersExpanded = CreateExpandedCollection(Users);
                }

                return _UsersExpanded;
            }
        }

        internal StringCollection RolesExpanded {
            get {
                if (_RolesExpanded == null) {
                    _RolesExpanded = CreateExpandedCollection(Roles);
                }

                return _RolesExpanded;
            }
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey) {
            bool DataToWrite = false;
            UpdateUsersRolesVerbs();            // the ismodifed can be short circuited

            if (base.SerializeElement(null, false) == true) {
                if (writer != null) {
                    writer.WriteStartElement(_ElementName);
                    DataToWrite |= base.SerializeElement(writer, false);
                    writer.WriteEndElement();
                }
                else
                    DataToWrite |= base.SerializeElement(writer, false);
            }
            return DataToWrite;
        }

        private string ExpandName(string name) {
            string ExpandedName = name;

            if (StringUtil.StringStartsWith(name, @".\")) {
                if (machineName == null) {
                    machineName = HttpServerUtility.GetMachineNameInternal().ToLower(CultureInfo.InvariantCulture);
                }

                ExpandedName = machineName + name.Substring(1);
            }

            return ExpandedName;
        }

        private StringCollection CreateExpandedCollection(StringCollection collection) {
            StringCollection ExpandedCollection = new StringCollection();

            foreach (string name in collection) {
                string ExpandedName = ExpandName(name);
                ExpandedCollection.Add(ExpandedName);
            }

            return ExpandedCollection;
        }

        private void EvaluateData() {
            if (DataReady == false) {
                if (Users.Count > 0) {
                    foreach (string User in Users) {
                        if (User.Length > 1) {
                            int foundIndex = User.IndexOfAny(new char[] { '*', '?' });

                            if (foundIndex >= 0) {
                                throw new ConfigurationErrorsException(SR.GetString(SR.Auth_rule_names_cant_contain_char, User[foundIndex].ToString(CultureInfo.InvariantCulture)));
                            }
                        }

                        if (User.Equals(_strAllUsersTag)) {
                            _AllUsersSpecified = true;
                        }

                        if (User.Equals(_strAnonUserTag)) {
                            _AnonUserSpecified = true;
                        }
                    }
                }

                if (Roles.Count > 0) {
                    foreach (string Role in Roles) {
                        if (Role.Length > 0) {
                            int foundIndex = Role.IndexOfAny(new char[] { '*', '?' });

                            if (foundIndex >= 0) {
                                throw new ConfigurationErrorsException(SR.GetString(SR.Auth_rule_names_cant_contain_char, Role[foundIndex].ToString(CultureInfo.InvariantCulture)));
                            }
                        }
                    }
                }

                _Everyone = (_AllUsersSpecified && (Verbs.Count == 0));
                _RolesExpanded = CreateExpandedCollection(Roles);
                _UsersExpanded = CreateExpandedCollection(Users);
                if (Roles.Count == 0 && Users.Count == 0) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Auth_rule_must_specify_users_andor_roles));
                }
                DataReady = true;
            }
        }

        internal bool IncludesAnonymous {
            get {
                EvaluateData();
                return (_AnonUserSpecified && Verbs.Count == 0);
            }
        }

        protected override void PreSerialize(XmlWriter writer) {
            EvaluateData();
        }

        protected override void PostDeserialize() {
            EvaluateData();
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // 0 => Don't know, 1 => Yes, -1 => No
        internal int IsUserAllowed(IPrincipal user, String verb) {
            EvaluateData();

            int answer = ((Action == AuthorizationRuleAction.Allow) ? 1 : -1); // return value if this rule applies

            if (Everyone) {
                return answer;
            }

            ///////////////////////////////////////////////////
            // Step 1: Make sure the verbs match
            if (!FindVerb(verb)) {
                return 0;
            }

            //////////////////////////////////////////////////
            // Step 2a: See if the rule applies to all users
            if (_AllUsersSpecified) { // All users specified
                return answer;
            }

            //////////////////////////////////////////////////
            // Step 2b: See if the rule applies to anonymous user and this user is anonymous
            if (_AnonUserSpecified && !user.Identity.IsAuthenticated) {
                return answer;
            }

            //////////////////////////////////////////////////
            // Step 3: Is user is WindowsIdentity, use the expanded
            //         set of users and roles
            StringCollection users;
            StringCollection roles;

            if (user.Identity is WindowsIdentity) {
                users = UsersExpanded;
                roles = RolesExpanded;
            }
            else {
                users = Users;
                roles = Roles;
            }

            ////////////////////////////////////////////////////
            // Step 4: See if the user is specified
            if (users.Count > 0 && FindUser(users, user.Identity.Name)) {
                return answer;
            }

            ////////////////////////////////////////////////////
            // Step 5: See if the user is in any specified role
            if (roles.Count > 0 && IsTheUserInAnyRole(roles, user)) {
                return answer;
            }

            // Rule doesn't apply
            return 0;
        }

        /////////////////////////////////////////////////////////////////////////
        private bool FindVerb(String verb) {
            if (Verbs.Count < 1) {
                return true; // No verbs specified => all verbs are allowed
            }

            foreach (string sVerb in Verbs) {
                if (String.Equals(sVerb, verb, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        private bool FindUser(StringCollection users, String principal) {
            foreach (string user in users) {
                if (String.Equals(user, principal, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        private bool IsTheUserInAnyRole(StringCollection roles, IPrincipal principal) {
            if (!HttpRuntime.DisableProcessRequestInApplicationTrust) {
                if (HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust) {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
            }
            
            foreach (string role in roles) {
                if (principal.IsInRole(role)) {
                    return true; // Found it!
                }
            }
            // Not in any specified role
            return false;
        }
    } // class AuthorizationRule
}
