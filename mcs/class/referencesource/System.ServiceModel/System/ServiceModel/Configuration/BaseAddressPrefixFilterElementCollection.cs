//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Configuration;

    [ConfigurationCollection(typeof(BaseAddressPrefixFilterElement))]
    public sealed class BaseAddressPrefixFilterElementCollection : ServiceModelConfigurationElementCollection<BaseAddressPrefixFilterElement>
    {
        public BaseAddressPrefixFilterElementCollection()
            : base(ConfigurationElementCollectionType.AddRemoveClearMap, ConfigurationStrings.Add)
        { }

        protected override ConfigurationElement CreateNewElement()
        {
            return new BaseAddressPrefixFilterElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            BaseAddressPrefixFilterElement configElementKey = (BaseAddressPrefixFilterElement)element;
            return configElementKey.Prefix;
        }

        protected override bool ThrowOnDuplicate
        {
            get { return true; }
        }

    }
}
