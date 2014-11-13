//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Security;
    using System.Runtime;

    public sealed partial class AddressHeaderCollectionElement : ServiceModelConfigurationElement
    {
        public AddressHeaderCollectionElement()
        {
        }

        internal void Copy(AddressHeaderCollectionElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }

            PropertyInformationCollection properties = source.ElementInformation.Properties;
            if (properties[ConfigurationStrings.Headers].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Headers = source.Headers;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Headers, DefaultValue = null)]
        public AddressHeaderCollection Headers
        {
            get
            {
                AddressHeaderCollection retVal = (AddressHeaderCollection)base[ConfigurationStrings.Headers];
                if (null == retVal)
                {
                    retVal = AddressHeaderCollection.EmptyHeaderCollection;
                }
                return retVal;
            }
            set
            {
                if (value == null)
                {
                    value = AddressHeaderCollection.EmptyHeaderCollection;
                }
                base[ConfigurationStrings.Headers] = value;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses the critical helper SetIsPresent.",
            Safe = "Controls how/when SetIsPresent is used, not arbitrarily callable from PT (method is protected and class is sealed).")]
        [SecuritySafeCritical]
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            SetIsPresent();
            DeserializeElementCore(reader);
        }

        private void DeserializeElementCore(XmlReader reader)
        {
            this.Headers = AddressHeaderCollection.ReadServiceParameters(XmlDictionaryReader.CreateDictionaryReader(reader));
        }

        [Fx.Tag.SecurityNote(Critical = "Uses the critical helper SetIsPresent which elevates in order to set a property.",
            Safe = "Only passes 'this', does not let caller influence parameter.")]
        [SecurityCritical]
        void SetIsPresent()
        {
            ConfigurationHelpers.SetIsPresent(this);
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, String elementName)
        {
            bool dataToWrite = this.Headers.Count != 0;
            if (dataToWrite && writer != null)
            {
                writer.WriteStartElement(elementName);
                this.Headers.WriteContentsTo(XmlDictionaryWriter.CreateDictionaryWriter(writer));
                writer.WriteEndElement();
            }
            return dataToWrite;
        }

        internal void InitializeFrom(AddressHeaderCollection headers)
        {
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Headers, headers);
        }
    }
}



