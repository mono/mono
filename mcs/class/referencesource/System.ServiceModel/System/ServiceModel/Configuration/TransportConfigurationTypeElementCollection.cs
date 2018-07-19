//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(TransportConfigurationTypeElement))]
    public sealed class TransportConfigurationTypeElementCollection : ServiceModelConfigurationElementCollection<TransportConfigurationTypeElement>
    {
        public TransportConfigurationTypeElementCollection()
            : base(ConfigurationElementCollectionType.AddRemoveClearMap, null)
        { }

        protected override Object GetElementKey(ConfigurationElement element) 
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            TransportConfigurationTypeElement configElementKey = (TransportConfigurationTypeElement)element;
            return configElementKey.Name;
        }
    }
}


