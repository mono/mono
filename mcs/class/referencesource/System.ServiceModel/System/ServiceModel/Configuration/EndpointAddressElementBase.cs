//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.Xml;

    public partial class EndpointAddressElementBase : ServiceModelConfigurationElement
    {
        protected EndpointAddressElementBase()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.Address, DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
        public Uri Address
        {
            get { return (Uri)base[ConfigurationStrings.Address]; }
            set { base[ConfigurationStrings.Address] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Headers)]
        public AddressHeaderCollectionElement Headers
        {
            get { return (AddressHeaderCollectionElement)base[ConfigurationStrings.Headers]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Identity)]
        public IdentityElement Identity
        {
            get { return (IdentityElement)base[ConfigurationStrings.Identity]; }
        }


        internal protected void Copy(EndpointAddressElementBase source)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == source)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }

            this.Address = source.Address;
            this.Headers.Headers = source.Headers.Headers;
            PropertyInformationCollection properties = source.ElementInformation.Properties;
            if (properties[ConfigurationStrings.Identity].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Identity.Copy(source.Identity);
            }
        }

        public void InitializeFrom(EndpointAddress endpointAddress)
        {
            if (null == endpointAddress)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointAddress");
            }

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Address, endpointAddress.Uri);
            this.Headers.InitializeFrom(endpointAddress.Headers);

            if (null != endpointAddress.Identity)
            {
                this.Identity.InitializeFrom(endpointAddress.Identity);
            }
        }
    }
}



