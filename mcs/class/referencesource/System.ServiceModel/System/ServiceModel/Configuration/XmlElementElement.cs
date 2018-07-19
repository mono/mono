//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.Security;
    using System.Xml;

    public sealed partial class XmlElementElement : ConfigurationElement
    {
        public XmlElementElement()
        {
        }

        public XmlElementElement(XmlElement element) : this()
        {
            this.XmlElement = element;
        }

        public void Copy(XmlElementElement source)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == source)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }

            if (null != source.XmlElement)
            {
                this.XmlElement = (XmlElement)source.XmlElement.Clone();
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

        void DeserializeElementCore(XmlReader reader)
        {
            XmlDocument doc = new XmlDocument();
            this.XmlElement = (XmlElement)doc.ReadNode(reader);
        }

        internal void ResetInternal(XmlElementElement element)
        {
            this.Reset(element);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls ConfigurationHelpers.SetIsPresent which elevates in order to set a property.",
            Safe = "Only passes 'this', does not let caller influence parameter.")]
        [SecurityCritical]
        void SetIsPresent()
        {
            ConfigurationHelpers.SetIsPresent(this);
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, String elementName)
        {
            bool dataToWrite = this.XmlElement != null;
            if (dataToWrite && writer != null)
            {
                if (!String.Equals(elementName, ConfigurationStrings.XmlElement, StringComparison.Ordinal))
                {
                    writer.WriteStartElement(elementName);
                }

                using (XmlNodeReader reader = new XmlNodeReader(this.XmlElement))
                {
                    writer.WriteNode(reader, false);
                }

                if (!String.Equals(elementName, ConfigurationStrings.XmlElement, StringComparison.Ordinal))
                {
                    writer.WriteEndElement();
                }
            }
            return dataToWrite;
        }

        protected override void PostDeserialize()
        {
            this.Validate();
            base.PostDeserialize();
        }

        void Validate()
        {
            if (this.XmlElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigXmlElementMustBeSet),
                    this.ElementInformation.Source,
                    this.ElementInformation.LineNumber));
            }
        }

        [ConfigurationProperty(ConfigurationStrings.XmlElement, DefaultValue = null, Options = ConfigurationPropertyOptions.IsKey)]
        public XmlElement XmlElement
        {
            get { return (XmlElement)base[ConfigurationStrings.XmlElement]; }
            set { base[ConfigurationStrings.XmlElement] = value; }
        }
    }
}



