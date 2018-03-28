//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(ComUdtElement), AddItemName = ConfigurationStrings.ComUdt)]
    public sealed class ComUdtElementCollection : ServiceModelEnhancedConfigurationElementCollection<ComUdtElement>
    {
        public ComUdtElementCollection()
            : base(ConfigurationStrings.ComUdt)
        { }

        protected override bool ThrowOnDuplicate
        {
            get { return false; }
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            ComUdtElement configElementKey = (ComUdtElement)element;
            return configElementKey.TypeDefID;
        }
    }
}


