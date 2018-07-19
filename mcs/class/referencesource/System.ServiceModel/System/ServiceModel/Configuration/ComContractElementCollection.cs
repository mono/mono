//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(ComContractElement), AddItemName = ConfigurationStrings.ComContract)]
    public sealed class ComContractElementCollection : ServiceModelEnhancedConfigurationElementCollection<ComContractElement>
    {
        public ComContractElementCollection()
            : base(ConfigurationStrings.ComContract)
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

            ComContractElement configElementKey = (ComContractElement)element;
            return configElementKey.Contract;
        }
    }
}


