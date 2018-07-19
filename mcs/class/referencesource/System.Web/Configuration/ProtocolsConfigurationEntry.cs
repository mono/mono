//------------------------------------------------------------------------------
// <copyright file="ProtocolsConfigurationEntry.cs" company="Microsoft">
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
    using System.Globalization;
    using System.Web.Hosting;
    using System.Web.Security;
    using System.Web.Util;
    using System.Xml;

    internal class ProtocolsConfigurationEntry {

        private String _id;
        private String _processHandlerTypeName;
        private Type   _processHandlerType;
        private String _appDomainHandlerTypeName;
        private Type   _appDomainHandlerType;
        private bool   _typesValidated;
        private String _configFileName;
        private int    _configFileLine;

        internal ProtocolsConfigurationEntry(
            String id,
            String processHandlerType,
            String appDomainHandlerType,
            bool validate,
            String configFileName,
            int configFileLine) {

            _id = id;
            _processHandlerTypeName = processHandlerType;
            _appDomainHandlerTypeName = appDomainHandlerType;
            _configFileName = configFileName;
            _configFileLine = configFileLine;

            if (validate) {
                ValidateTypes();
            }
        }

        private void ValidateTypes() {
            if (_typesValidated)
                return;

            // check process protocol handler

            Type processHandlerType;
            try {
                 processHandlerType = Type.GetType(_processHandlerTypeName, true /*throwOnError*/);
            }
            catch (Exception e) {
                throw new ConfigurationErrorsException(e.Message, e, _configFileName, _configFileLine);
            }
            HandlerBase.CheckAssignableType(_configFileName, _configFileLine, typeof(ProcessProtocolHandler), processHandlerType);

            // check app domain protocol handler

            Type appDomainHandlerType;
            try {
                 appDomainHandlerType = Type.GetType(_appDomainHandlerTypeName, true /*throwOnError*/);
            }
            catch (Exception e) {
                throw new ConfigurationErrorsException(e.Message, e, _configFileName, _configFileLine);
            }
            HandlerBase.CheckAssignableType(_configFileName, _configFileLine, typeof(AppDomainProtocolHandler), appDomainHandlerType);

            // remember types

            _processHandlerType = processHandlerType;
            _appDomainHandlerType = appDomainHandlerType;
            _typesValidated = true;
        }


    }
}


