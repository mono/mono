//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Text;

    public sealed partial class XPathMessageFilterElement : ConfigurationElement
    {
        const int DefaultNodeQuota = 1000;

        [ConfigurationProperty(ConfigurationStrings.Filter, DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        public XPathMessageFilter Filter
        {
            get { return (XPathMessageFilter)base[ConfigurationStrings.Filter]; }
            set { base[ConfigurationStrings.Filter] = value; }
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            StringBuilder filterStringBuilder = new StringBuilder();
            string nodeQuotaStringValue = String.Empty;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.OmitXmlDeclaration = false;

            using (XmlWriter tempWriter = XmlWriter.Create(filterStringBuilder, settings))
            {
                tempWriter.WriteStartElement(reader.Name);

                if (0 < reader.AttributeCount)
                {
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToAttribute(i);
                        if (reader.Name.Equals(ConfigurationStrings.NodeQuota, StringComparison.Ordinal))
                        {
                            nodeQuotaStringValue = reader.Value;
                        }
                        else
                        {
                            if (reader.Name.Contains(":"))
                            {
                                string[] attributeName = reader.Name.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                tempWriter.WriteAttributeString(attributeName[0], attributeName[1], null, reader.Value);
                            }
                            else
                            {
                                tempWriter.WriteAttributeString(reader.Name, reader.Value);
                            }
                        }
                    }

                    reader.MoveToElement();
                }

                string filterString = reader.ReadString();
                filterString = filterString.Trim();
                if (String.IsNullOrEmpty(filterString))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.ConfigXPathFilterMustNotBeEmpty)));
                }
                tempWriter.WriteString(filterString);
                tempWriter.WriteEndElement();
            }

            XPathMessageFilter filter = null;

            using (StringReader stringReader = new StringReader(filterStringBuilder.ToString()))
            {
                using (XmlReader tempReader = XmlReader.Create(stringReader))
                {
                    filter = new XPathMessageFilter(tempReader);
                }
            }

            if (null != filter)
            {
                if (!String.IsNullOrEmpty(nodeQuotaStringValue))
                {
                    filter.NodeQuota = int.Parse(nodeQuotaStringValue, CultureInfo.CurrentCulture);
                }
                else
                {
                    filter.NodeQuota = XPathMessageFilterElement.DefaultNodeQuota;
                }
            }

            this.Filter = filter;
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, String elementName)
        {
            bool dataToWrite = this.Filter != null;
            if (dataToWrite && writer != null)
            {
                //this.Filter.WriteXPathTo(writer, null, elementName, null, true);
                writer.WriteStartElement(elementName);
                writer.WriteAttributeString(ConfigurationStrings.NodeQuota, Filter.NodeQuota.ToString(NumberFormatInfo.CurrentInfo));

                StringBuilder filterStringBuilder = new StringBuilder();

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                settings.OmitXmlDeclaration = false;

                using (XmlWriter tempWriter = XmlWriter.Create(filterStringBuilder, settings))
                {
                    this.Filter.WriteXPathTo(tempWriter, null, elementName, null, true);
                }

                using (StringReader stringReader = new StringReader(filterStringBuilder.ToString()))
                {
                    using (XmlReader tempReader = XmlReader.Create(stringReader))
                    {
                        if (tempReader.Read())
                        {
                            if (0 < tempReader.AttributeCount)
                            {
                                for (int i = 0; i < tempReader.AttributeCount; i++)
                                {
                                    tempReader.MoveToAttribute(i);
                                    writer.WriteAttributeString(tempReader.Name, tempReader.Value);
                                }

                                tempReader.MoveToElement();
                            }

                            writer.WriteString(tempReader.ReadString());
                        }
                    }
                }

                writer.WriteEndElement();
            }
            return dataToWrite;
        }
    }
}



