//------------------------------------------------------------------------------
// <copyright file="RuntimeConfigurationRecord.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Xml;
    using System.Net;
    using System.Configuration.Internal;
    using Assembly = System.Reflection.Assembly;
    using System.Diagnostics.CodeAnalysis;

    internal sealed class RuntimeConfigurationRecord : BaseConfigurationRecord {

        static internal IInternalConfigRecord Create(
                InternalConfigRoot          configRoot,
                IInternalConfigRecord       parent,
                string                      configPath) {

            RuntimeConfigurationRecord configRecord = new RuntimeConfigurationRecord();
            configRecord.Init(configRoot, (BaseConfigurationRecord) parent, configPath, null);
            return configRecord;
        }

        private RuntimeConfigurationRecord() {
        }

        static readonly SimpleBitVector32 RuntimeClassFlags = new SimpleBitVector32(
                  ClassSupportsChangeNotifications
                | ClassSupportsRefresh
                | ClassSupportsImpersonation
                | ClassSupportsRestrictedPermissions
                | ClassSupportsDelayedInit);

        override protected SimpleBitVector32 ClassFlags {
            get {
                return RuntimeClassFlags;
            }
        }

        // Create the factory that will evaluate configuration 
        override protected object CreateSectionFactory(FactoryRecord factoryRecord) {
            return new RuntimeConfigurationFactory(this, factoryRecord);
        }

        // parentConfig contains the config that we'd merge with.
        override protected object CreateSection(bool inputIsTrusted, FactoryRecord factoryRecord, SectionRecord sectionRecord, SectionInput sectionInput, object parentConfig, ConfigXmlReader reader) {
            // Get the factory used to create a section.
            RuntimeConfigurationFactory factory = (RuntimeConfigurationFactory) factoryRecord.Factory;

            // Use the factory to create a section.
            object config = factory.CreateSection(inputIsTrusted, this, factoryRecord, sectionRecord, sectionInput, parentConfig, reader);

            return config;
        }

        override protected object UseParentResult(string configKey, object parentResult, SectionRecord sectionRecord) {
            return parentResult;
        }

        // Ignore user code on the stack
        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private object GetRuntimeObjectWithFullTrust(ConfigurationSection section) {
            return section.GetRuntimeObject();
        }

        [SuppressMessage("Microsoft.Security", "CA2107:ReviewDenyAndPermitOnlyUsage", Justification = "This PermitOnly is meant to protect unassuming handlers from malicious callers by undoing any asserts we have put on the stack.")]
        private object GetRuntimeObjectWithRestrictedPermissions(ConfigurationSection section) {
            // Run configuration section handlers as if user code was on the stack
            bool revertPermitOnly = false;

            try {
                PermissionSet permissionSet = GetRestrictedPermissions();
                if (permissionSet != null) {
                    permissionSet.PermitOnly();
                    revertPermitOnly = true;
                }

                return section.GetRuntimeObject();
            }
            finally {
                if (revertPermitOnly) {
                    CodeAccessPermission.RevertPermitOnly();
                }
            }
        }

        override protected object GetRuntimeObject(object result) {
            object runtimeObject;
            ConfigurationSection section = result as ConfigurationSection;
            if (section == null) {
                runtimeObject = result;
            }
            else {
                // Call into config section while impersonating process or UNC identity
                // so that the section could read files from disk if needed
                try {
                    using (Impersonate()) {
                        // If this configRecord is trusted, ignore user code on stack
                        if (_flags[IsTrusted]) {
                            runtimeObject = GetRuntimeObjectWithFullTrust(section);
                        }
                        else {
                            // Run configuration section handlers as if user code was on the stack
                            runtimeObject = GetRuntimeObjectWithRestrictedPermissions(section);
                        }
                    }
                }
                catch (Exception e) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_exception_in_config_section_handler, section.SectionInformation.SectionName), e);
                }
            }

            return runtimeObject;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        protected override string CallHostDecryptSection(string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfig) {
            // Decrypt should always succeed in runtime.  (VSWhidbey 429996)
            // Need to override in order to Assert before calling the base class method.
            return base.CallHostDecryptSection(encryptedXml, protectionProvider, protectedConfig);
        }

        private class RuntimeConfigurationFactory {
            ConstructorInfo                 _sectionCtor;
            IConfigurationSectionHandler    _sectionHandler;

            internal RuntimeConfigurationFactory(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord) {
                // If the factory record was defined in a trusted config record, ignore user code on stack
                if (factoryRecord.IsFromTrustedConfigRecord) {
                    InitWithFullTrust(configRecord, factoryRecord);
                }
                else {
                    // Run configuration section handlers as if user code was on the stack
                    InitWithRestrictedPermissions(configRecord, factoryRecord);
                }
            }

            private void Init(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord) {
                // Get the type of the factory
                Type type = TypeUtil.GetTypeWithReflectionPermission(configRecord.Host, factoryRecord.FactoryTypeName, true);

                // If the type is a ConfigurationSection, that's the type.
                if (typeof(ConfigurationSection).IsAssignableFrom(type)) {
                    _sectionCtor = TypeUtil.GetConstructorWithReflectionPermission(type, typeof(ConfigurationSection), true);
                }
                else {
                    // Note: in v1, IConfigurationSectionHandler is in effect a factory that has a Create method
                    // that creates the real section object.

                    // throws if type does not implement IConfigurationSectionHandler
                    TypeUtil.VerifyAssignableType(typeof(IConfigurationSectionHandler), type, true);

                    // Create an instance of the handler
                    _sectionHandler = (IConfigurationSectionHandler) TypeUtil.CreateInstanceWithReflectionPermission(type);
                }
            }

            // Ignore user code on the stack
            [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
            private void InitWithFullTrust(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord) {
                Init(configRecord, factoryRecord);
            }

            [SuppressMessage("Microsoft.Security", "CA2107:ReviewDenyAndPermitOnlyUsage", Justification = "This PermitOnly is meant to protect unassuming handlers from malicious callers by undoing any asserts we have put on the stack.")]
            private void InitWithRestrictedPermissions(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord) {
                // Run configuration section handlers as if user code was on the stack
                bool revertPermitOnly = false;
                try {
                    PermissionSet permissionSet = configRecord.GetRestrictedPermissions();
                    if (permissionSet != null) {
                        permissionSet.PermitOnly();
                        revertPermitOnly = true;
                    }

                    Init(configRecord, factoryRecord);
                }
                finally {
                    if (revertPermitOnly) {
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }
            }

            //
            // Throw an exception if an attribute within a legacy section is one of our
            // reserved locking attributes. We do not want admins to think they can lock
            // an attribute or element within a legacy section.
            //
            private static void CheckForLockAttributes(string sectionName, XmlNode xmlNode) {
                XmlAttributeCollection attributes = xmlNode.Attributes;
                if (attributes != null) {
                    foreach (XmlAttribute attribute in attributes) {
                        if (ConfigurationElement.IsLockAttributeName(attribute.Name)) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_element_locking_not_supported, sectionName), attribute);
                        }
                    }
                }

                foreach (XmlNode child in xmlNode.ChildNodes) {
                    if (xmlNode.NodeType == XmlNodeType.Element) {
                        CheckForLockAttributes(sectionName, child);
                    }
                }
            }

            private object CreateSectionImpl(
                    RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord, 
                    SectionInput sectionInput, object parentConfig, ConfigXmlReader reader) {

                object config;

                if (_sectionCtor != null) {
                    ConfigurationSection configSection = (ConfigurationSection) TypeUtil.InvokeCtorWithReflectionPermission(_sectionCtor);

                    configSection.SectionInformation.SetRuntimeConfigurationInformation(configRecord, factoryRecord, sectionRecord);

                    configSection.CallInit();

                    ConfigurationSection parentSection = (ConfigurationSection) parentConfig;
                    configSection.Reset(parentSection);

                    if (reader != null) {
                        configSection.DeserializeSection(reader);
                    }

                    if (configRecord != null && sectionInput != null && sectionInput.ConfigBuilder != null) {
                        configSection = configRecord.CallHostProcessConfigurationSection(configSection, sectionInput.ConfigBuilder);
                    }

                    // throw if there are any cached errors
                    ConfigurationErrorsException errors = configSection.GetErrors();
                    if (errors != null) {
                        throw errors;
                    }

                    // don't allow changes to sections at runtime
                    configSection.SetReadOnly();

                    // reset the modified bit
                    configSection.ResetModified();

                    config = configSection;
                }
                else {
                    if (reader != null) {
                        XmlNode xmlNode = ErrorInfoXmlDocument.CreateSectionXmlNode(reader);

                        CheckForLockAttributes(factoryRecord.ConfigKey, xmlNode);

                        // In v1, our old section handler expects a context that contains the virtualPath from the configPath
                        object configContext = configRecord.Host.CreateDeprecatedConfigContext(configRecord.ConfigPath);

                        config = _sectionHandler.Create(parentConfig, configContext, xmlNode);
                    }
                    else {
                        config = null;
                    }
                }

                return config;
            }

            // Ignore user code on the stack
            [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
            private object CreateSectionWithFullTrust(
                    RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord, 
                    SectionInput sectionInput, object parentConfig, ConfigXmlReader reader) {

                        return CreateSectionImpl(configRecord, factoryRecord, sectionRecord, sectionInput, parentConfig, reader);
            }

            [SuppressMessage("Microsoft.Security", "CA2107:ReviewDenyAndPermitOnlyUsage", Justification = "This PermitOnly is meant to protect unassuming handlers from malicious callers by undoing any asserts we have put on the stack.")]
            private object CreateSectionWithRestrictedPermissions(
                    RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord,
                    SectionInput sectionInput, object parentConfig, ConfigXmlReader reader) {

                // run configuration section handlers as if user code was on the stack
                bool revertPermitOnly = false;
                try {
                    PermissionSet permissionSet = configRecord.GetRestrictedPermissions();
                    if (permissionSet != null) {
                        permissionSet.PermitOnly();
                        revertPermitOnly = true;
                    }

                    return CreateSectionImpl(configRecord, factoryRecord, sectionRecord, sectionInput, parentConfig, reader);
                }
                finally {
                    if (revertPermitOnly) {
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }
            }

            internal object CreateSection(bool inputIsTrusted, RuntimeConfigurationRecord configRecord, 
                    FactoryRecord factoryRecord, SectionRecord sectionRecord, SectionInput sectionInput, object parentConfig, ConfigXmlReader reader) {

                if (inputIsTrusted) {
                    return CreateSectionWithFullTrust(configRecord, factoryRecord, sectionRecord, sectionInput, parentConfig, reader);
                }
                else {
                    return CreateSectionWithRestrictedPermissions(configRecord, factoryRecord, sectionRecord, sectionInput, parentConfig, reader);
                }
            }
        }
    }
}
