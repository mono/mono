//------------------------------------------------------------------------------
// <copyright file="ConfigUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Threading;
    using System.Configuration;
    using System.Xml;
    using System.Web.Compilation;
    using System.Web.Util;

    internal class ConfigUtil {
        private ConfigUtil() {
        }

        internal static void CheckBaseType(Type expectedBaseType, Type userBaseType, string propertyName, ConfigurationElement configElement) {
            // Make sure the base type is valid
            if (!expectedBaseType.IsAssignableFrom(userBaseType)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_type_to_inherit_from,
                        userBaseType.FullName,
                        expectedBaseType.FullName), configElement.ElementInformation.Properties[propertyName].Source,
                        configElement.ElementInformation.Properties[propertyName].LineNumber);
            }
        }

        internal static Type GetType(string typeName, string propertyName, ConfigurationElement configElement,
            XmlNode node, bool checkAptcaBit, bool ignoreCase) {

            // We should get either a propertyName/configElement or node, but not both.
            // They are used only for error reporting.
            Debug.Assert((propertyName != null) != (node != null));

            Type val;
            try {
                val = BuildManager.GetType(typeName, true /*throwOnError*/, ignoreCase);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }

                if (node != null) {
                    throw new ConfigurationErrorsException(e.Message, e, node);
                }
                else {
                    if (configElement != null) {
                        throw new ConfigurationErrorsException(e.Message, e,
                                                               configElement.ElementInformation.Properties[propertyName].Source,
                                                               configElement.ElementInformation.Properties[propertyName].LineNumber);
                    }
                    else {
                        throw new ConfigurationErrorsException(e.Message, e);
                    }
                }
            }

            // If we're not in full trust, only allow types that have the APTCA bit (ASURT 139687),
            // unless the checkAptcaBit flag is false
            if (checkAptcaBit) {
                if (node != null) {
                    HttpRuntime.FailIfNoAPTCABit(val, node);
                }
                else {
                    HttpRuntime.FailIfNoAPTCABit(val,
                        configElement != null ? configElement.ElementInformation : null,
                        propertyName);
                }
            }

            return val;
        }

        internal static Type GetType(string typeName, string propertyName, ConfigurationElement configElement) {
            return GetType(typeName, propertyName, configElement, true /*checkAptcaBit*/);
        }

        internal static Type GetType(string typeName, string propertyName, ConfigurationElement configElement, bool checkAptcaBit) {
            return GetType(typeName, propertyName, configElement, checkAptcaBit, false);
        }

        internal static Type GetType(string typeName, string propertyName, ConfigurationElement configElement, bool checkAptcaBit, bool ignoreCase) {
            return GetType(typeName, propertyName, configElement, null /*node*/, checkAptcaBit, ignoreCase);
        }

        internal static Type GetType(string typeName, XmlNode node) {
            return GetType(typeName, node, false /*ignoreCase*/);
        }

        internal static Type GetType(string typeName, XmlNode node, bool ignoreCase) {
            return GetType(typeName, null, null, node, true /*checkAptcaBit*/, ignoreCase);
        }

        internal static void CheckAssignableType(Type baseType, Type type, ConfigurationElement configElement, string propertyName) {
            if (!baseType.IsAssignableFrom(type)) {
                throw new ConfigurationErrorsException(
                                SR.GetString(SR.Type_doesnt_inherit_from_type, type.FullName, baseType.FullName),
                                configElement.ElementInformation.Properties[propertyName].Source, configElement.ElementInformation.Properties[propertyName].LineNumber);
            }
        }

        internal static void CheckAssignableType(Type baseType, Type baseType2, Type type, ConfigurationElement configElement, string propertyName) {
            if (!baseType.IsAssignableFrom(type) && !baseType2.IsAssignableFrom(type)) {
                throw new ConfigurationErrorsException(
                                SR.GetString(SR.Type_doesnt_inherit_from_type, type.FullName, baseType.FullName),
                                configElement.ElementInformation.Properties[propertyName].Source,
                                configElement.ElementInformation.Properties[propertyName].LineNumber);
            }
        }

        internal static bool IsTypeHandlerOrFactory(Type t) {
            return typeof(IHttpHandler).IsAssignableFrom(t)
                || typeof(IHttpHandlerFactory).IsAssignableFrom(t);
        }

        internal static ConfigurationErrorsException MakeConfigurationErrorsException(string message, Exception innerException = null, PropertyInformation configProperty = null) {
            return new ConfigurationErrorsException(message, innerException, (configProperty != null) ? configProperty.Source : null, (configProperty != null) ? configProperty.LineNumber : 0);
        }

        // If a configuration section has a default value coming from 4.0 config and the AppDomain is opted in to auto-config upgrade,
        // set the configuration section element explicitly to its new 4.5 value. If the property has been explicitly set, no change
        // will be made.
        internal static void SetFX45DefaultValue(ConfigurationSection configSection, ConfigurationProperty property, object newDefaultValue) {
            if (BinaryCompatibility.Current.TargetsAtLeastFramework45 && !configSection.IsReadOnly()) {
                PropertyInformation propInfo = configSection.ElementInformation.Properties[property.Name];
                Debug.Assert(propInfo != null);
                Debug.Assert(propInfo.Type.IsInstanceOfType(newDefaultValue));

                if (propInfo.ValueOrigin == PropertyValueOrigin.Default) {
                    try {
                        propInfo.Value = newDefaultValue;
                    }
                    catch (ConfigurationErrorsException) {
                        // Calling the Value setter might throw if the configuration element is locked.
                        // We can technically override the "is locked?" check by calling the appropriate
                        // method, but for now let's just honor the locks and ignore these errors. The
                        // config sections we're touching shouldn't really ever be locked anyway, so
                        // nobody should ever run into this in practice.
                    }
                }
            }
        }

    }
}
