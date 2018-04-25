//------------------------------------------------------------------------------
// <copyright file="ScriptingWebServicesSectionGroup.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Web;

    public sealed class ScriptingWebServicesSectionGroup : ConfigurationSectionGroup {

        [ConfigurationProperty("jsonSerialization")]
#pragma warning disable 0436
        public ScriptingJsonSerializationSection JsonSerialization {
            get {
                return (ScriptingJsonSerializationSection)Sections["jsonSerialization"];
            }
        }
#pragma warning restore 0436

        [ConfigurationProperty("profileService")]
#pragma warning disable 0436
        public ScriptingProfileServiceSection ProfileService {
            get {
                return (ScriptingProfileServiceSection)Sections["profileService"];
            }
        }
#pragma warning restore 0436


        [ConfigurationProperty("authenticationService")]
        public ScriptingAuthenticationServiceSection AuthenticationService {
            get {
                return (ScriptingAuthenticationServiceSection)Sections["authenticationService"];
            }
        }


        [ConfigurationProperty("roleService")]
        public ScriptingRoleServiceSection RoleService {
            get {
                return (ScriptingRoleServiceSection)Sections["roleService"];
            }
        }

    }
}
