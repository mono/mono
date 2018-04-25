//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    public sealed class StandardBindingElementCollection<TBindingConfiguration> : ServiceModelEnhancedConfigurationElementCollection<TBindingConfiguration>
        where TBindingConfiguration : StandardBindingElement, new()
    {
        public StandardBindingElementCollection()
            : base(ConfigurationStrings.Binding)
        { }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            TBindingConfiguration configElementKey = (TBindingConfiguration)element;
            return configElementKey.Name;
        }
    }

}


