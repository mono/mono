//------------------------------------------------------------------------------
// <copyright file="AuthorizationSection.cs" company="Microsoft">
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

    /*
        <authorization>

            <!--
            allow/deny Attributes:
              users="[*|?|name]"
                * - All users
                ? - Anonymous users
                [name] - Named user
              roles="[name]"
            -->
            <allow users="*" />
                <!--  <allow     users="[comma separated list of users]"
                                 roles="[comma separated list of roles]"
                                 verbs="[comma separated list of verbs]" />
                      <deny      users="[comma separated list of users]"
                                 roles="[comma separated list of roles]"
                                 verbs="[comma separated list of verbs]" />
                -->

        </authorization>

        <authorization>
            <allow users="*" />
        </authorization>

        */

    /// <devdoc>
    ///     <para> Adds Authorization specific information to this section.        
    ///     </para>
    /// </devdoc>
    public sealed class AuthorizationSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propRules =
            new ConfigurationProperty(null, typeof(AuthorizationRuleCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        private bool _EveryoneAllowed = false;
        internal bool EveryoneAllowed { get { return _EveryoneAllowed; } }

        static AuthorizationSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propRules);
        }

        public AuthorizationSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public AuthorizationRuleCollection Rules {
            get {
                return (AuthorizationRuleCollection)base[_propRules];
            }
        }

        protected override void PostDeserialize() {
            if (Rules.Count > 0) {
                _EveryoneAllowed = (Rules[0].Action == AuthorizationRuleAction.Allow && Rules[0].Everyone);
            }
        }

        internal bool IsUserAllowed(IPrincipal user, String verb) {
            return Rules.IsUserAllowed(user, verb);
        }
    } // class AuthorizationSection
}
