//------------------------------------------------------------------------------
// <copyright file="ProtocolsConfigurationHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System.IO;
    using System.Runtime.Serialization.Formatters;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.Collections;
    using System.Configuration;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Web.Hosting;
    using System.Web.Security;
    using System.Web.Util;
    using System.Xml;

    //
    // Protocols config (machine.config only):
    //
    /*
        <protocols>
            <add
                id="<protocolID>" 
                processHandlerType="<typeName>"
                appDomainHandlerType="<typeName>"
                [validate="false"]
            />
            ...
        </protocols>
    */
    //
    public sealed class ProtocolsConfigurationHandler : IConfigurationSectionHandler {

        public ProtocolsConfigurationHandler() {
        }

        public object Create(Object parent, Object configContextObj, XmlNode section) {
            // can be called from client config ( default app domain)
            Debug.Assert(parent == null, "<protocols> config is only allowed in machine.config");
            return new ProtocolsConfiguration(section);
        }
    }
}


