// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;

    /// <summary>
    /// DelegatingHandlerElement for DelegatingHandler
    /// </summary>
    public sealed partial class DelegatingHandlerElement : ConfigurationElement
    {
        Guid id = Guid.NewGuid();

        public DelegatingHandlerElement()
        {
        }

        internal DelegatingHandlerElement(Type handlerType)
        {
            Fx.Assert(handlerType != null, "handlerType should not be null.");
            this.Type = handlerType.AssemblyQualifiedName;
        }

        [ConfigurationProperty(ConfigurationStrings.Type, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 1)]
        public string Type
        {
            get
            {
                return (string)base[ConfigurationStrings.Type];
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = string.Empty;
                }

                base[ConfigurationStrings.Type] = value;
            }
        }

        internal Guid Id 
        {
            get
            {
                return this.id;
            }
        }
    }
}
