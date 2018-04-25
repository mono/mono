//------------------------------------------------------------------------------
// <copyright file="ProtocolsConfiguration.cs" company="Microsoft">
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

    internal class ProtocolsConfiguration {

        private Hashtable _protocolEntries = new Hashtable();

        internal ProtocolsConfiguration(XmlNode section) {

            // process XML section in order and apply the directives

            HandlerBase.CheckForUnrecognizedAttributes(section);

            foreach (XmlNode child in section.ChildNodes) {

                // skip whitespace and comments
                if (IsIgnorableAlsoCheckForNonElement(child))
                    continue;

                // process <add> elements

                if (child.Name == "add") {
                    String id = HandlerBase.RemoveRequiredAttribute(child, "id");
                    String phType = HandlerBase.RemoveRequiredAttribute(child, "processHandlerType");
                    String ahType = HandlerBase.RemoveRequiredAttribute(child, "appDomainHandlerType");

                    bool validate = true;
                    HandlerBase.GetAndRemoveBooleanAttribute(child, "validate", ref validate);

                    HandlerBase.CheckForUnrecognizedAttributes(child);
                    HandlerBase.CheckForNonCommentChildNodes(child);

                    // check duplicate Id
                    /* TEMPORARY allow duplicates for easy Indigo machine.config update
                    if (_protocolEntries[id] != null) {
                        throw new ConfigurationErrorsException(
                                        SR.GetString(SR.Dup_protocol_id, id), 
                                        child);
                    }
                    */

                    // add entry
                    /* TEMPORARY hide errors and ignore bad <add> tags
                       to let breaking changes through */
                    try {
                        _protocolEntries[id] = new ProtocolsConfigurationEntry(
                            id, phType, ahType, validate, 
                            ConfigurationErrorsException.GetFilename(child),
                            ConfigurationErrorsException.GetLineNumber(child));
                    }
                    catch {
                    }
                }
                else {
                    HandlerBase.ThrowUnrecognizedElement(child);
                }
            }
        }

        private bool IsIgnorableAlsoCheckForNonElement(XmlNode node) {
            if (node.NodeType == XmlNodeType.Comment || node.NodeType == XmlNodeType.Whitespace) {
                return true;
            }

            if (node.NodeType != XmlNodeType.Element)
            {
                throw new ConfigurationErrorsException(
                                SR.GetString(SR.Config_base_elements_only),
                                node);
            }

            return false;
        }

    }
}
