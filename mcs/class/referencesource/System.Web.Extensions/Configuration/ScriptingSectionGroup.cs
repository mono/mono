//------------------------------------------------------------------------------
// <copyright file="ScriptingSectionGroup.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Web;

    public sealed class ScriptingSectionGroup : ConfigurationSectionGroup {

        [ConfigurationProperty("webServices")]
#pragma warning disable 0436
        public ScriptingWebServicesSectionGroup WebServices {
            get {
                return (ScriptingWebServicesSectionGroup)SectionGroups["webServices"];
            }
        }
#pragma warning restore 0436

        [ConfigurationProperty("scriptResourceHandler")]
        public ScriptingScriptResourceHandlerSection ScriptResourceHandler {
            get {
                return (ScriptingScriptResourceHandlerSection)Sections["scriptResourceHandler"];
            }
        }
    }
}
