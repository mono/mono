//------------------------------------------------------------------------------
// <copyright file="SystemWebExtensionsSectionGroup.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Web;

    public sealed class SystemWebExtensionsSectionGroup : ConfigurationSectionGroup {

        [ConfigurationProperty("scripting")]
        public ScriptingSectionGroup Scripting {
            get {
                return (ScriptingSectionGroup)SectionGroups["scripting"];
            }
        }
    }
}
