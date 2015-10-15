//------------------------------------------------------------------------------
// <copyright file="HandlerBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;

    internal static class HandlerBase {

        static internal void CheckForChildNodes(XmlNode node) {
            if (node.HasChildNodes) {
                throw ADP.ConfigBaseNoChildNodes(node.FirstChild);
            }
        }
        static private void CheckForNonElement(XmlNode node) {
            if (XmlNodeType.Element != node.NodeType) {
                throw ADP.ConfigBaseElementsOnly(node);
            }
        }
        static internal void CheckForUnrecognizedAttributes(XmlNode node) {
            if (0 != node.Attributes.Count) {
                throw ADP.ConfigUnrecognizedAttributes(node);
            }
        }
        
        // skip whitespace and comments, throws if non-element
        static internal bool IsIgnorableAlsoCheckForNonElement(XmlNode node) {
            if ((XmlNodeType.Comment == node.NodeType) || (XmlNodeType.Whitespace == node.NodeType)) {
                return true;
            }
            CheckForNonElement(node);
            return false;
        }

        static internal string RemoveAttribute(XmlNode node, string name, bool required, bool allowEmpty) {
            XmlNode attribute = node.Attributes.RemoveNamedItem(name);
            if (null == attribute) {
                if (required) {
                    throw ADP.ConfigRequiredAttributeMissing(name, node);
                }
                return null;
            }
            string value = attribute.Value;
            if (!allowEmpty && (0 == value.Length)) {
                throw ADP.ConfigRequiredAttributeEmpty(name, node);
            }
            return value;
        }

        static internal DataSet CloneParent(DataSet parentConfig, bool insenstive) {
            if (null == parentConfig) {
                parentConfig = new DataSet(DbProviderFactoriesConfigurationHandler.sectionName);
                parentConfig.CaseSensitive = !insenstive;
                parentConfig.Locale = CultureInfo.InvariantCulture;
            }
            else {
                parentConfig = parentConfig.Copy();
            }
            return parentConfig;
        }
    }
}
