//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(DefaultPortElement), AddItemName = ConfigurationStrings.Add)]
    public sealed partial class DefaultPortElementCollection : ServiceModelEnhancedConfigurationElementCollection<DefaultPortElement>
    {
        public DefaultPortElementCollection()
            : base(ConfigurationStrings.Add)
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            DefaultPortElement DefaultPortElement = (DefaultPortElement)element;
            return DefaultPortElement.Scheme;
        }
    }
}
