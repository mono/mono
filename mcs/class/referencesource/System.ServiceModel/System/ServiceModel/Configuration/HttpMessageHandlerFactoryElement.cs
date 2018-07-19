// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Runtime;

    /// <summary>
    /// HttpMessageHandlerFactoryElement for HttpMessageHandlerFactory
    /// </summary>
    public sealed partial class HttpMessageHandlerFactoryElement : ConfigurationElement
    {   
        public HttpMessageHandlerFactoryElement()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.Configuration, "Configuration104",
                        Justification = "Don't need a validator for this strong typed element.")]
        [ConfigurationProperty(ConfigurationStrings.Handlers)]
        public DelegatingHandlerElementCollection Handlers
        {
            get { return (DelegatingHandlerElementCollection)this[ConfigurationStrings.Handlers]; }
            internal set { base[ConfigurationStrings.Handlers] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Type)]
        [StringValidator(MinLength = 0)]
        public string Type
        {
            get { return (string)base[ConfigurationStrings.Type]; }
            set { base[ConfigurationStrings.Type] = value; }
        }
    }
}
