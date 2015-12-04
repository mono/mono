//------------------------------------------------------------------------------
// <copyright file="DbProviderConfigurationHandler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;

    // this calss can be used by any provider to support a provider specific configuration section. The configutation
    // Object is a NameValueCollection
   
    // <configSections>
    //     <section name="system.data.<provider>" type="System.data.common.DbProviderConfigurationHandler, System.Data, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%" />
    // </configSections>
    // <system.data.<provider>
    //     <settings>
    //         <add name="<provider setting" value = "<value of setting>"  />
    //     </settings>
    // </system.data.<provider>
    // this class is delayed created, use ConfigurationManager.GetSection("system.data.<provider>") to obtain
    
    public class DbProviderConfigurationHandler : IConfigurationSectionHandler { // V1.2.3300
        internal const string settings = "settings";
      

        public DbProviderConfigurationHandler() { // V1.2.3300
        }
/*
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
*/
        static internal NameValueCollection CloneParent(NameValueCollection parentConfig) {
            if (null == parentConfig) {
                parentConfig = new NameValueCollection();
            }
            else {
                parentConfig = new NameValueCollection(parentConfig);
            }
            return parentConfig;
        }
        
        virtual public object Create(object parent, object configContext, XmlNode section) { // V1.2.3300
#if DEBUG
            try {
#endif
                return CreateStatic(parent, configContext, section);
#if DEBUG
            }
            catch(Exception e) {
                // 
                if (ADP.IsCatchableExceptionType(e)) {
                    ADP.TraceExceptionWithoutRethrow(e); // it will be rethrown
                }
                throw;
            }
#endif
        }

        static internal object CreateStatic(object parent, object configContext, XmlNode section) {            
            object config = parent;
            if (null != section) {
                config = CloneParent(parent as NameValueCollection);
                bool foundSettings = false;

                HandlerBase.CheckForUnrecognizedAttributes(section);
                foreach (XmlNode child in section.ChildNodes) {
                    if (HandlerBase.IsIgnorableAlsoCheckForNonElement(child)) {
                        continue;
                    }
                    string sectionGroup = child.Name;
                    switch(sectionGroup) {
                    case DbProviderConfigurationHandler.settings:
                        if (foundSettings) {
                            throw ADP.ConfigSectionsUnique(DbProviderConfigurationHandler.settings);
                        }
                        foundSettings= true;
                        DbProviderDictionarySectionHandler.CreateStatic(config as NameValueCollection, configContext, child);
                        break;
                    default:
                        throw ADP.ConfigUnrecognizedElement(child);
                    }
                }
            }
            return config;
        }
/*
        // skip whitespace and comments, throws if non-element
        static internal bool IsIgnorableAlsoCheckForNonElement(XmlNode node) {
            if ((XmlNodeType.Comment == node.NodeType) || (XmlNodeType.Whitespace == node.NodeType)) {
                return true;
            }
            HandlerBase.CheckForNonElement(node);
            return false;
        }
  */      
        static internal string RemoveAttribute(XmlNode node, string name) {
            XmlNode attribute = node.Attributes.RemoveNamedItem(name);
            if (null == attribute) {
                throw ADP.ConfigRequiredAttributeMissing(name, node);
            }
            string value = attribute.Value;
            if (0 == value.Length) {
                throw ADP.ConfigRequiredAttributeEmpty(name, node);
            }
            return value;
        }
        
        // based off of DictionarySectionHandler
        sealed private class DbProviderDictionarySectionHandler/* : IConfigurationSectionHandler*/ {
            
            static internal NameValueCollection CreateStatic(NameValueCollection config, Object context, XmlNode section) {
                if (null != section) {
                    HandlerBase.CheckForUnrecognizedAttributes(section);
                    }
                    
                    foreach (XmlNode child in section.ChildNodes) {
                        if (HandlerBase.IsIgnorableAlsoCheckForNonElement(child)) {
                            continue;
                        }
                        switch(child.Name) {
                        case "add":
                            HandleAdd(child, config);
                            break;
                        case "remove":
                            HandleRemove(child, config);
                            break;
                        case "clear":
                            HandleClear(child, config);
                            break;
                        default:
                            throw ADP.ConfigUnrecognizedElement(child);
                        }
                    }
                return config;
                
            }
            static private void HandleAdd(XmlNode child, NameValueCollection config) {

                // should add vaildate that setting is a known supported setting 
                // (i.e. that the value of the name attribute is is good)
                HandlerBase.CheckForChildNodes(child);
                string name = RemoveAttribute(child, "name");
                string value = RemoveAttribute(child, "value");
                HandlerBase.CheckForUnrecognizedAttributes(child);
                config.Add(name,value);
                
            }
            static private void HandleRemove(XmlNode child, NameValueCollection config) {
                HandlerBase.CheckForChildNodes(child);                
                String name = RemoveAttribute(child, "name");                
                HandlerBase.CheckForUnrecognizedAttributes(child);
                config.Remove(name);
            }
            static private void HandleClear(XmlNode child, NameValueCollection config) {
                HandlerBase.CheckForChildNodes(child);
                HandlerBase.CheckForUnrecognizedAttributes(child);
                config.Clear();
            }
        }
        
    }
}

