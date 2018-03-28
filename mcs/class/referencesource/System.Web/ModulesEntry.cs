//------------------------------------------------------------------------------
// <copyright file="ModulesEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Config related classes for HttpApplication
 * 
 */

namespace System.Web.Configuration.Common {

    using System.Runtime.Serialization.Formatters;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.Collections;
    using System.Reflection;
    using System.Globalization;
    using System.Configuration;
    using System.Web;
    using System.Web.SessionState;
    using System.Web.Security;
    using System.Web.Util;
    using System.Web.Compilation;
    using System.Security;
    using System.Security.Permissions;

    /*
     * Single Entry of request to class
     */
    internal class ModulesEntry {
        private String _name;
        private Type _type;

        internal ModulesEntry(String name, String typeName, string propertyName, ConfigurationElement configElement) {
            _name = (name != null) ? name : String.Empty;

            // Don't check the APTCA bit for modules (VSWhidbey 467768, 550122)
            _type = SecureGetType(typeName, propertyName, configElement);
            if (!typeof(IHttpModule).IsAssignableFrom(_type)) {
                if (configElement == null) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Type_not_module, typeName)); 
                }
                else {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Type_not_module, typeName),
                              configElement.ElementInformation.Properties["type"].Source, configElement.ElementInformation.Properties["type"].LineNumber);
                }
            }
        }

        internal static bool IsTypeMatch(Type type, String typeName) {
            return(type.Name.Equals(typeName) || type.FullName.Equals(typeName));
        }

        internal String ModuleName {
            get { return _name; }
        }

        internal /*public*/ IHttpModule Create() {
            return (IHttpModule)HttpRuntime.CreateNonPublicInstance(_type);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private Type SecureGetType(string typeName, string propertyName, ConfigurationElement configElement) {
            return ConfigUtil.GetType(typeName, propertyName, configElement, false /*checkAptcaBit*/);
        }
    }
}
