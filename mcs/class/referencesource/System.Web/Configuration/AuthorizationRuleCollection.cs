//------------------------------------------------------------------------------
// <copyright file="AuthorizationRuleCollection.cs" company="Microsoft">
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

    [ConfigurationCollection(typeof(AuthorizationRule), AddItemName = "allow,deny", 
     CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
    public sealed class AuthorizationRuleCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static AuthorizationRuleCollection() {
            _properties = new ConfigurationPropertyCollection();
        }

        public AuthorizationRuleCollection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        // public properties
        public AuthorizationRule this[int index] {
            get {
                return (AuthorizationRule)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
        // Protected Overrides
        protected override ConfigurationElement CreateNewElement() {
            return new AuthorizationRule();
        }
        protected override ConfigurationElement CreateNewElement(string elementName) {
            AuthorizationRule newElement = new AuthorizationRule();
            switch (elementName.ToLower(CultureInfo.InvariantCulture)) {
                case "allow":
                    newElement.Action = AuthorizationRuleAction.Allow;
                    break;
                case "deny":
                    newElement.Action = AuthorizationRuleAction.Deny;
                    break;
            }
            return newElement;
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            AuthorizationRule rule = (AuthorizationRule)element;
            return rule._ActionString;
        }

        protected override string ElementName {
            get {
                return String.Empty; //_LookUpInElement_
            }
        }
        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        // IsElement name allows collection with multiple element names to
        // exist with the base class architecture.  Given an element name
        // it simply returns true if the name is legal for the default collection
        // or false otherwise.
        protected override bool IsElementName(string elementname) {
            bool IsElement = false;
            switch (elementname.ToLower(CultureInfo.InvariantCulture)) {
                case "allow":
                case "deny":
                    IsElement = true;
                    break;
            }
            return IsElement;
        }

        internal bool IsUserAllowed(IPrincipal user, String verb) {
            if (user == null) {
                return false;
            }

            if (!_fCheckForCommonCasesDone) {
                DoCheckForCommonCases();
                _fCheckForCommonCasesDone = true;
            }
            if (!user.Identity.IsAuthenticated && _iAnonymousAllowed != 0)
                return (_iAnonymousAllowed > 0);

            if (_iAllUsersAllowed != 0)
                return (_iAllUsersAllowed > 0);

            // Go down the list permissions and check each one
            foreach (AuthorizationRule rule in this) {
                int result = rule.IsUserAllowed(user, verb);
                if (result != 0)
                    return (result > 0);
            }
            return false;
        }

        private void DoCheckForCommonCases()
        {
            bool fStillLookingForAnonymous  = true;
            bool fAnyAllowRulesFound        = false;
            bool fAnyDenyRulesFound         = false;

            foreach (AuthorizationRule rule in this)
            {
                if (rule.Everyone) // Found a rule for Every-user
                {
                    if (!fAnyAllowRulesFound && rule.Action == AuthorizationRuleAction.Deny)
                        _iAllUsersAllowed = -1;
                    if (!fAnyDenyRulesFound && rule.Action == AuthorizationRuleAction.Allow)
                        _iAllUsersAllowed = 1;                        
                    return; // done!
                }
                if (fStillLookingForAnonymous && rule.IncludesAnonymous) // Found a rule for anonymous-user
                {
                    if (!fAnyAllowRulesFound && rule.Action == AuthorizationRuleAction.Deny)
                        _iAnonymousAllowed = -1;
                    if (!fAnyDenyRulesFound && rule.Action == AuthorizationRuleAction.Allow)
                        _iAnonymousAllowed = 1;
                    fStillLookingForAnonymous = false;
                }

                if (!fAnyAllowRulesFound && rule.Action == AuthorizationRuleAction.Allow)
                    fAnyAllowRulesFound = true;
                if (!fAnyDenyRulesFound && rule.Action == AuthorizationRuleAction.Deny)
                    fAnyDenyRulesFound = true;                        

                if (!fStillLookingForAnonymous && fAnyAllowRulesFound && fAnyDenyRulesFound)
                    return;
            }
        }

        // public methods
        public void Add(AuthorizationRule rule) {
            BaseAdd(-1, rule); // add to the end of the list and dont overwrite dups!
        }

        public void Clear() {
            BaseClear();
        }

        public AuthorizationRule Get(int index) {
            return (AuthorizationRule)BaseGet(index);
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        public void Set(int index, AuthorizationRule rule) {
            BaseAdd(index, rule);
        }

        public int IndexOf(AuthorizationRule rule) {
            for (int x = 0; x < Count; x++) {
                if (Object.Equals(Get(x), rule)) {
                    return x;
                }
            }
            return -1;
        }

        public void Remove(AuthorizationRule rule) {
            int index = IndexOf(rule);
            if (index >= 0) {
                BaseRemoveAt(index);
            }
        }
        private int _iAllUsersAllowed = 0;
        private int _iAnonymousAllowed = 0;
        private bool _fCheckForCommonCasesDone = false;
    }
}
