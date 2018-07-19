//------------------------------------------------------------------------------
// <copyright file="ConfigurationSectionHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Mobile
{
    using System.Xml;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ConfigurationSectionHelper
    {
        private XmlNode _currentNode = null;

        internal ConfigurationSectionHelper() {
        }

        internal /*public*/ XmlNode Node
        {
            get
            {
                return _currentNode;
            }
            
            set
            {
                _currentNode = value;
            }
        }

        private XmlNode GetAndRemoveAttribute(String attributeName, bool required)
        {
            XmlNode attibuteNode = _currentNode.Attributes.RemoveNamedItem(attributeName);

            if (required && attibuteNode == null)
            {
                String msg = SR.GetString(SR.ConfigSect_MissingAttr,
                                          attributeName);
                throw new ConfigurationErrorsException(msg, _currentNode);
            }

            return attibuteNode;
        }


        internal /*public*/ String RemoveStringAttribute(String attributeName,
            bool required)
        {
            Debug.Assert(null != _currentNode);

            XmlNode attributeNode = GetAndRemoveAttribute(attributeName, required);
            if(attributeNode != null)
            {
                if(required && (attributeNode.Value != null && attributeNode.Value.Length == 0))
                {
                    String msg = SR.GetString(SR.ConfigSect_MissingValue,
                                              attributeName);
                    throw new ConfigurationErrorsException(msg, _currentNode);
                }
                return attributeNode.Value;
            }
            else
            {
                return null;
            }
        }

#if UNUSED_CODE
        internal /*public*/ bool RemoveBoolAttribute(String attributeName,
                                        bool required,
                                        bool defaultValue)
        {
            Debug.Assert(null != _currentNode);

            XmlNode attributeNode = GetAndRemoveAttribute(attributeName, required);
            if(attributeNode != null)
            {
                try
                {
                    return bool.Parse(attributeNode.Value);
                }
                catch
                {
                    String msg =
                        SR.GetString(SR.ConfigSect_InvalidBooleanAttr,
                                     attributeName);
                    throw new ConfigurationErrorsException(msg, _currentNode);
                }
            }
            else
            {
                return defaultValue;
            }
            
        }

        internal /*public*/ int RemoveIntAttribute(String attributeName,
                                      bool required,
                                      int defaultValue)
        {
            Debug.Assert(null != _currentNode);

            XmlNode attributeNode = GetAndRemoveAttribute(attributeName, required);
            if(attributeNode != null)
            {
                try
                {
                    return int.Parse(attributeNode.Value, CultureInfo.InvariantCulture);
                }
                catch
                {
                    String msg =
                        SR.GetString(SR.ConfigSect_InvalidIntegerAttr,
                                     attributeName);
                    throw new ConfigurationErrorsException(msg, _currentNode);
                }
            }
            else
            {
                return defaultValue;
            }
            
        }
#endif

        internal /*public*/ void CheckForUnrecognizedAttributes()
        {
            Debug.Assert(null != _currentNode);

            if(_currentNode.Attributes.Count != 0)
            {
                String msg = SR.GetString(SR.ConfigSect_UnknownAttr,
                                          _currentNode.Attributes[0].Name);
                throw new ConfigurationErrorsException(msg, _currentNode);
            }
        }


        internal /*public*/ bool IsWhitespaceOrComment()
        {
            Debug.Assert(null != _currentNode);

            return _currentNode.NodeType == XmlNodeType.Comment ||
                _currentNode.NodeType == XmlNodeType.Whitespace;
        }


        internal /*public*/ void RejectNonElement()
        {
            Debug.Assert(null != _currentNode);

            if(_currentNode.NodeType != XmlNodeType.Element)
            {
                throw new ConfigurationErrorsException(SR.GetString(SR.ConfigSect_UnrecognizedXML),
                                                 _currentNode);
            }
        }
    }
}
