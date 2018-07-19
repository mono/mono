//------------------------------------------------------------------------------
// <copyright file="HandlerBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HandlerBase contains static helper functions for consistent XML parsing 
 * behavior and error messages.
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.Configuration {

    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Xml;
    using System.Web.Compilation;

    static internal class HandlerBase {


        //
        // XML Attribute Helpers
        //

        private static XmlNode GetAndRemoveAttribute(XmlNode node, string attrib, bool fRequired) {
            XmlNode a = node.Attributes.RemoveNamedItem(attrib);

            // If the attribute is required and was not present, throw
            if (fRequired && a == null) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Missing_required_attribute, attrib, node.Name),
                    node);
            }

            return a;
        }

        private static XmlNode GetAndRemoveStringAttributeInternal(XmlNode node, string attrib, bool fRequired, ref string val) {
            XmlNode a = GetAndRemoveAttribute(node, attrib, fRequired);
            if (a != null) {
                val = a.Value;
            }

            return a;
        }

        internal static XmlNode GetAndRemoveStringAttribute(XmlNode node, string attrib, ref string val) {
            return GetAndRemoveStringAttributeInternal(node, attrib, false /*fRequired*/, ref val);
        }

        internal static XmlNode GetAndRemoveRequiredStringAttribute(XmlNode node, string attrib, ref string val) {
            return GetAndRemoveStringAttributeInternal(node, attrib, true /*fRequired*/, ref val);
        }

        internal static XmlNode GetAndRemoveNonEmptyStringAttribute(XmlNode node, string attrib, ref string val) {
            return GetAndRemoveNonEmptyStringAttributeInternal(node, attrib, false /*fRequired*/, ref val);
        }

        internal static XmlNode GetAndRemoveRequiredNonEmptyStringAttribute(XmlNode node, string attrib, ref string val) {
            return GetAndRemoveNonEmptyStringAttributeInternal(node, attrib, true /*fRequired*/, ref val);
        }

        private static XmlNode GetAndRemoveNonEmptyStringAttributeInternal(XmlNode node, string attrib, bool fRequired, ref string val) {
            XmlNode a = GetAndRemoveStringAttributeInternal(node, attrib, fRequired, ref val);
            if (a != null && val.Length == 0) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Empty_attribute, attrib),
                    a);
            }

            return a;
        }

        // input.Xml cursor must be at a true/false XML attribute
        private static XmlNode GetAndRemoveBooleanAttributeInternal(XmlNode node, string attrib, bool fRequired, ref bool val) {
            XmlNode a = GetAndRemoveAttribute(node, attrib, fRequired);
            if (a != null) {
                if (a.Value == "true") {
                    val = true;
                }
                else if (a.Value == "false") {
                    val = false;
                }
                else {
                    throw new ConfigurationErrorsException(
                                    SR.GetString(SR.Invalid_boolean_attribute, a.Name),
                                    a);
                }
            }

            return a;
        }

        internal static XmlNode GetAndRemoveBooleanAttribute(XmlNode node, string attrib, ref bool val) {
            return GetAndRemoveBooleanAttributeInternal(node, attrib, false /*fRequired*/, ref val);
        }

        private static XmlNode GetAndRemoveIntegerAttributeInternal(XmlNode node, string attrib, bool fRequired, ref int val) {
            XmlNode a = GetAndRemoveAttribute(node, attrib, fRequired);
            if (a != null) {
                if (a.Value.Trim() != a.Value) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_integer_attribute, a.Name),
                        a);
                }

                try {
                    val = int.Parse(a.Value, CultureInfo.InvariantCulture);
                }
                catch (Exception e) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_integer_attribute, a.Name),
                        e, a);
                }
            }

            return a;
        }

        private static XmlNode GetAndRemovePositiveAttributeInternal(XmlNode node, string attrib, bool fRequired, ref int val) {
            XmlNode a = GetAndRemoveIntegerAttributeInternal(node, attrib, fRequired, ref val);

            if (a != null && val <= 0) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_positive_integer_attribute, attrib),
                    a);
            }

            return a;
        }


        internal static XmlNode GetAndRemovePositiveIntegerAttribute(XmlNode node, string attrib, ref int val) {
            return GetAndRemovePositiveAttributeInternal(node, attrib, false /*fRequired*/, ref val);
        }


        private static XmlNode GetAndRemoveTypeAttributeInternal(XmlNode node, string attrib, bool fRequired, ref Type val) {
            XmlNode a = GetAndRemoveAttribute(node, attrib, fRequired);

            if (a != null) {
                val = ConfigUtil.GetType(a.Value, a);
            }

            return a;
        }

        internal static XmlNode GetAndRemoveTypeAttribute(XmlNode node, string attrib, ref Type val) {
            return GetAndRemoveTypeAttributeInternal(node, attrib, false /*fRequired*/, ref val);
        }

        internal static void CheckForbiddenAttribute(XmlNode node, string attrib) {
            XmlAttribute attr = node.Attributes[attrib];
            if (attr != null) {
                throw new ConfigurationErrorsException(
                                SR.GetString(SR.Config_base_unrecognized_attribute, attrib),
                                attr);
            }
        }

        internal static void CheckForUnrecognizedAttributes(XmlNode node) {
            if (node.Attributes.Count != 0) {
                throw new ConfigurationErrorsException(
                                SR.GetString(SR.Config_base_unrecognized_attribute, node.Attributes[0].Name),
                                node.Attributes[0]);
            }
        }


        //
        // Obsolete XML Attribute Helpers
        //

        // if attribute not found return null
        internal static string RemoveAttribute(XmlNode node, string name) {

            XmlNode attribute = node.Attributes.RemoveNamedItem(name);

            if (attribute != null) {
                return attribute.Value;
            }

            return null;
        }

        // if attr not found throw standard message - "attribute x required"
        internal static string RemoveRequiredAttribute(XmlNode node, string name) {
            return RemoveRequiredAttribute(node, name, false);
        }

        internal static string RemoveRequiredAttribute(XmlNode node, string name, bool allowEmpty) {
            XmlNode attribute = node.Attributes.RemoveNamedItem(name);

            if (attribute == null) {
                throw new ConfigurationErrorsException(
                                SR.GetString(SR.Config_base_required_attribute_missing, name),
                                node);                
            }

            if (attribute.Value.Length == 0 && !allowEmpty) {
                throw new ConfigurationErrorsException(
                                SR.GetString(SR.Config_base_required_attribute_empty, name),
                                node);                
            }

            return attribute.Value;
        }



        //
        // XML Element Helpers
        //


        internal static void CheckForNonCommentChildNodes(XmlNode node) {
            foreach (XmlNode childNode in node.ChildNodes) {
                if (childNode.NodeType != XmlNodeType.Comment) {
                    throw new ConfigurationErrorsException(
                                    SR.GetString(SR.Config_base_no_child_nodes),
                                    childNode);
                }
            }
        }

        internal static void ThrowUnrecognizedElement(XmlNode node) {
            throw new ConfigurationErrorsException(
                            SR.GetString(SR.Config_base_unrecognized_element),
                            node);
        }


        internal static void CheckAssignableType(XmlNode node, Type baseType, Type type) {
            if (!baseType.IsAssignableFrom(type)) {
                throw new ConfigurationErrorsException(
                                SR.GetString(SR.Type_doesnt_inherit_from_type, type.FullName, baseType.FullName),
                                node);                
            }
        }

        internal static void CheckAssignableType(string filename, int lineNumber, Type baseType, Type type) {
            if (!baseType.IsAssignableFrom(type)) {
                throw new ConfigurationErrorsException(
                                SR.GetString(SR.Type_doesnt_inherit_from_type, type.FullName, baseType.FullName),
                                filename, lineNumber);                
            }
        }

        // Section handlers can run in client mode through:
        //      ConfigurationManager.GetSection("sectionName")
        // See ASURT 123738
        internal static bool IsServerConfiguration(object context) {
            return context is HttpConfigurationContext;
        }

        internal static bool CheckAndReadRegistryValue(ref string value, bool throwIfError) {
            if (value == null) {
                return true;
            }

            if (!StringUtil.StringStartsWithIgnoreCase(value, "registry:")) {
                // Not a registry value.  It's not an error.
                return true;
            }

            const int size = 1024;
            StringBuilder str = new StringBuilder(size);
            int iRet = UnsafeNativeMethods.GetCredentialFromRegistry(value, str, size);
            if (iRet == 0) {
                value = str.ToString();
                return true;
            }
            else {
                if (throwIfError) {
                    throw new ConfigurationErrorsException(
                            SR.GetString(SR.Invalid_registry_config));
                }
                else {
                    return false;
                }
            }
        }

        internal static bool CheckAndReadConnectionString(ref string connectionString, bool throwIfError) {
            ConnectionStringSettings connObj = RuntimeConfig.GetConfig().ConnectionStrings.ConnectionStrings[connectionString];
            if (connObj != null && connObj.ConnectionString != null && connObj.ConnectionString.Length > 0)
                connectionString = connObj.ConnectionString;
            
            return CheckAndReadRegistryValue(ref connectionString, throwIfError);
        }
    }
}
