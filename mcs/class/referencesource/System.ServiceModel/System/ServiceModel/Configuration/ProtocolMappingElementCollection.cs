//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(ProtocolMappingElement), AddItemName = ConfigurationStrings.Add)]
    public sealed class ProtocolMappingElementCollection : ServiceModelEnhancedConfigurationElementCollection<ProtocolMappingElement>
    {
        public ProtocolMappingElementCollection()
            : base(ConfigurationStrings.Add)
        { }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            ProtocolMappingElement configElementKey = (ProtocolMappingElement)element;
            return configElementKey.Scheme;
        }
    }
}


