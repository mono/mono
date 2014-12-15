//------------------------------------------------------------------------------
// <copyright file="HttpWebRequestElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class HttpWebRequestElement : ConfigurationElement
    {
        public HttpWebRequestElement()
        {
            this.properties.Add(this.maximumResponseHeadersLength);
            this.properties.Add(this.maximumErrorResponseLength);
            this.properties.Add(this.maximumUnauthorizedUploadLength);
            this.properties.Add(this.useUnsafeHeaderParsing);
        }

        protected override void PostDeserialize()
        {
            // Perf optimization. If the configuration is coming from machine.config
            // It is safe and we don't need to check for permissions.
            if (EvaluationContext.IsMachineLevel)
                return;

            PropertyInformation[] protectedProperties = {
                ElementInformation.Properties[ConfigurationStrings.MaximumResponseHeadersLength],
                ElementInformation.Properties[ConfigurationStrings.MaximumErrorResponseLength]
            };

            foreach (PropertyInformation property in protectedProperties)
                if (property.ValueOrigin == PropertyValueOrigin.SetHere)
                {
                    try {
                        ExceptionHelper.WebPermissionUnrestricted.Demand();
                    } catch (Exception exception) {
                        throw new ConfigurationErrorsException(
                                      SR.GetString(SR.net_config_property_permission, 
                                                   property.Name),
                                      exception);
                    }
                }
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get 
            {
                return this.properties;
            }
        }


        [ConfigurationProperty(ConfigurationStrings.maximumUnauthorizedUploadLength, DefaultValue=(int)(-1))] 
        public int MaximumUnauthorizedUploadLength
        {
            get { return (int)this[this.maximumUnauthorizedUploadLength]; }
            set { this[this.maximumUnauthorizedUploadLength] = value; }
        }
        
        [ConfigurationProperty(ConfigurationStrings.MaximumErrorResponseLength, DefaultValue=(int)(64))]
        public int MaximumErrorResponseLength
        {
            get { return (int)this[this.maximumErrorResponseLength]; }
            set { this[this.maximumErrorResponseLength] = value; }
        }
        

        [ConfigurationProperty(ConfigurationStrings.MaximumResponseHeadersLength, DefaultValue= 64)]
        public int MaximumResponseHeadersLength
        {
            get { return (int)this[this.maximumResponseHeadersLength]; }
            set { this[this.maximumResponseHeadersLength] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseUnsafeHeaderParsing, DefaultValue= false)]
        public bool UseUnsafeHeaderParsing
        {
            get { return (bool) this[this.useUnsafeHeaderParsing]; }
            set { this[this.useUnsafeHeaderParsing] = value; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty maximumResponseHeadersLength =
            new ConfigurationProperty(ConfigurationStrings.MaximumResponseHeadersLength, 
                                      typeof(int), 
                                      64, 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty maximumErrorResponseLength =
            new ConfigurationProperty(ConfigurationStrings.MaximumErrorResponseLength, 
                                      typeof(int), 
                                      64, 
                                      ConfigurationPropertyOptions.None);
        
        readonly ConfigurationProperty maximumUnauthorizedUploadLength =
            new ConfigurationProperty(ConfigurationStrings.maximumUnauthorizedUploadLength, 
                                      typeof(int), 
                                      -1, 
                                      ConfigurationPropertyOptions.None);
        
         readonly ConfigurationProperty useUnsafeHeaderParsing =
            new ConfigurationProperty(ConfigurationStrings.UseUnsafeHeaderParsing, 
                                      typeof(bool), 
                                      false, 
                                      ConfigurationPropertyOptions.None);
    }
}

