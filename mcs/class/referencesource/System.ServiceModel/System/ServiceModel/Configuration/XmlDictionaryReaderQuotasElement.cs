//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Xml;
    using System.ServiceModel;
    using System.ComponentModel;
    using System.ServiceModel.Channels;

    public sealed partial class XmlDictionaryReaderQuotasElement : ServiceModelConfigurationElement
    {
        // for all properties, a value of 0 means "just use the default"
        [ConfigurationProperty(ConfigurationStrings.MaxDepth, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int MaxDepth
        {
            get { return (int)base[ConfigurationStrings.MaxDepth]; }
            set { base[ConfigurationStrings.MaxDepth] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxStringContentLength, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int MaxStringContentLength
        {
            get { return (int)base[ConfigurationStrings.MaxStringContentLength]; }
            set { base[ConfigurationStrings.MaxStringContentLength] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxArrayLength, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int MaxArrayLength
        {
            get { return (int)base[ConfigurationStrings.MaxArrayLength]; }
            set { base[ConfigurationStrings.MaxArrayLength] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxBytesPerRead, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int MaxBytesPerRead
        {
            get { return (int)base[ConfigurationStrings.MaxBytesPerRead]; }
            set { base[ConfigurationStrings.MaxBytesPerRead] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxNameTableCharCount, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int MaxNameTableCharCount
        {
            get { return (int)base[ConfigurationStrings.MaxNameTableCharCount]; }
            set { base[ConfigurationStrings.MaxNameTableCharCount] = value; }
        }

        internal void ApplyConfiguration(XmlDictionaryReaderQuotas readerQuotas)
        {
            if (readerQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("readerQuotas");
            }
            if (this.MaxDepth != 0)
            {
                readerQuotas.MaxDepth = this.MaxDepth;
            }
            if (this.MaxStringContentLength != 0)
            {
                readerQuotas.MaxStringContentLength = this.MaxStringContentLength;
            }
            if (this.MaxArrayLength != 0)
            {
                readerQuotas.MaxArrayLength = this.MaxArrayLength;
            }
            if (this.MaxBytesPerRead != 0)
            {
                readerQuotas.MaxBytesPerRead = this.MaxBytesPerRead;
            }
            if (this.MaxNameTableCharCount != 0)
            {
                readerQuotas.MaxNameTableCharCount = this.MaxNameTableCharCount;
            }
        }

        internal void InitializeFrom(XmlDictionaryReaderQuotas readerQuotas)
        {
            if (readerQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("readerQuotas");
            }
            if (readerQuotas.MaxDepth != EncoderDefaults.MaxDepth)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxDepth, readerQuotas.MaxDepth);                
            }
            if (readerQuotas.MaxStringContentLength != EncoderDefaults.MaxStringContentLength)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxStringContentLength, readerQuotas.MaxStringContentLength);
            }
            if (readerQuotas.MaxArrayLength != EncoderDefaults.MaxArrayLength)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxArrayLength, readerQuotas.MaxArrayLength);
            }
            if (readerQuotas.MaxBytesPerRead != EncoderDefaults.MaxBytesPerRead)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBytesPerRead, readerQuotas.MaxBytesPerRead);
            }
            if (readerQuotas.MaxNameTableCharCount != EncoderDefaults.MaxNameTableCharCount)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxNameTableCharCount, readerQuotas.MaxNameTableCharCount);
            } 
        }
    }
}
