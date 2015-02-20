//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Xaml.Hosting.Configuration
{
    using System;
    using System.Web;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    public sealed class HandlerElement : ConfigurationElement
    {
        static ConfigurationPropertyCollection properties = InitializeProperties(); 

        Type httpHandlerCLRType;

        Type xamlRootElementClrType;

        static ConfigurationPropertyCollection InitializeProperties()
        {
            ConfigurationProperty handler = new ConfigurationProperty(XamlHostingConfiguration.HttpHandlerType, typeof(string), " ", null, new StringValidator(1), ConfigurationPropertyOptions.IsRequired);
            ConfigurationProperty xamlRoot = new ConfigurationProperty(XamlHostingConfiguration.XamlRootElementType, typeof(string), " ", null, new StringValidator(1), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
            ConfigurationPropertyCollection tempProperties = new ConfigurationPropertyCollection(); 
            tempProperties.Add(xamlRoot);
            tempProperties.Add(handler);
            return tempProperties;
        }

        public HandlerElement()
        {
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DoNotCallOverridableMethodsInConstructors,
            Justification = "This is enforced by configuration classes in framework library")]
        public HandlerElement(string xamlType, string handlerType)
        {
            XamlRootElementType = xamlType;
            HttpHandlerType = handlerType;
        }

        [ConfigurationProperty(XamlHostingConfiguration.HttpHandlerType, DefaultValue = " ",
            Options = ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string HttpHandlerType
        {
            get
            {
                return (string)base[XamlHostingConfiguration.HttpHandlerType];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base[XamlHostingConfiguration.HttpHandlerType] = value;
            }
        }

        [ConfigurationProperty(XamlHostingConfiguration.XamlRootElementType, DefaultValue = " ",
            Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        [StringValidator(MinLength = 1)]
        public string XamlRootElementType
        {
            get
            {
                return (string)base[XamlHostingConfiguration.XamlRootElementType];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base[XamlHostingConfiguration.XamlRootElementType] = value;
            }
        }

        internal string Key
        {
            get
            {
                return XamlRootElementType;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        internal Type LoadHttpHandlerType()
        {
            if (this.httpHandlerCLRType == null)
            {
                this.httpHandlerCLRType = Type.GetType(HttpHandlerType, true);
            }
            return this.httpHandlerCLRType;
        }


        internal Type LoadXamlRootElementType()
        {
            if (this.xamlRootElementClrType == null)
            {
                this.xamlRootElementClrType = Type.GetType(XamlRootElementType, true);
            }
            return this.xamlRootElementClrType;
        }
    }
}

