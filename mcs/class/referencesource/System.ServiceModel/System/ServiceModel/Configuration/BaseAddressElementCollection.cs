//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(BaseAddressElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed partial class BaseAddressElementCollection : ServiceModelConfigurationElementCollection<BaseAddressElement>
    {
        public BaseAddressElementCollection()
            : base(ConfigurationElementCollectionType.BasicMap, ConfigurationStrings.Add)
        { }

        protected override ConfigurationElement CreateNewElement()
        {
            return new BaseAddressElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            BaseAddressElement configElementKey = (BaseAddressElement)element;
            return configElementKey.BaseAddress;
        }

        protected override bool ThrowOnDuplicate
        {
            get { return true; }
        }
    }
}
