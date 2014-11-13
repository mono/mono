//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.Xml;

    [ConfigurationCollection(typeof(XmlElementElement), AddItemName = ConfigurationStrings.XmlElement, CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed class XmlElementElementCollection : ServiceModelConfigurationElementCollection<XmlElementElement>
    {
        public XmlElementElementCollection()
            : base(ConfigurationElementCollectionType.BasicMap, ConfigurationStrings.XmlElement)
        { }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            return ((XmlElementElement)element).XmlElement.OuterXml;
        }

        protected override void Unmerge(ConfigurationElement sourceElement,
                                                 ConfigurationElement parentElement,
                                                 ConfigurationSaveMode saveMode)
        {
            if (sourceElement != null)
            {
                // Just copy from parent to here-- 
                XmlElementElementCollection source = (XmlElementElementCollection)sourceElement;
                XmlElementElementCollection parent = (XmlElementElementCollection)parentElement;
                for (int i = 0; i < source.Count; ++i)
                {
                    XmlElementElement element = source[i];
                    if ((parent == null) || !parent.ContainsKey(this.GetElementKey(element)))
                    {
                        XmlElementElement xmlElement = new XmlElementElement();
                        xmlElement.ResetInternal(element);
                        this.Add(xmlElement);
                    }
                }
            }
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            XmlDocument doc = new XmlDocument();
            this.Add(new XmlElementElement((XmlElement)doc.ReadNode(reader)));
            return true;
        }
    }
}


