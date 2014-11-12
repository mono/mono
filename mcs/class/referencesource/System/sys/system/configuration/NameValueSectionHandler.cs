//------------------------------------------------------------------------------
// <copyright file="NameValueSectionHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Globalization;

    /// <devdoc>
    ///    Simple dictionary config factory
    /// </devdoc>
    public class NameValueSectionHandler : IConfigurationSectionHandler {
        const string defaultKeyAttribute = "key";
        const string defaultValueAttribute = "value";

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object Create(object parent, object context, XmlNode section) {
            return CreateStatic(parent, section, KeyAttributeName, ValueAttributeName);
        }

        internal static object CreateStatic(object parent, XmlNode section) {
            return CreateStatic(parent, section, defaultKeyAttribute, defaultValueAttribute);
        }

        internal static object CreateStatic(object parent, XmlNode section, string keyAttriuteName, string valueAttributeName) {
            ReadOnlyNameValueCollection result;

            // start result off as a shallow clone of the parent

            if (parent == null)
                result = new ReadOnlyNameValueCollection(StringComparer.OrdinalIgnoreCase);
            else {
                ReadOnlyNameValueCollection parentCollection = (ReadOnlyNameValueCollection)parent;
                result = new ReadOnlyNameValueCollection(parentCollection);
            }

            // process XML

            HandlerBase.CheckForUnrecognizedAttributes(section);

            foreach (XmlNode child in section.ChildNodes) {

                // skip whitespace and comments
                if (HandlerBase.IsIgnorableAlsoCheckForNonElement(child))
                    continue;

                // handle <set>, <remove>, <clear> tags
                if (child.Name == "add") {
                    String key = HandlerBase.RemoveRequiredAttribute(child, keyAttriuteName);
                    String value = HandlerBase.RemoveRequiredAttribute(child, valueAttributeName, true/*allowEmptyString*/);
                    HandlerBase.CheckForUnrecognizedAttributes(child);

                    result[key] = value;
                }
                else if (child.Name == "remove") {
                    String key = HandlerBase.RemoveRequiredAttribute(child, keyAttriuteName);
                    HandlerBase.CheckForUnrecognizedAttributes(child);

                    result.Remove(key);
                }
                else if (child.Name.Equals("clear")) {
                    HandlerBase.CheckForUnrecognizedAttributes(child);
                    result.Clear();
                }
                else {
                    HandlerBase.ThrowUnrecognizedElement(child);
                }
            }

            result.SetReadOnly();

            return result;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual string KeyAttributeName {
            get { return defaultKeyAttribute;}
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual string ValueAttributeName {
            get { return defaultValueAttribute;}
        }
    }
}
