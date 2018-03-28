//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(AllowedAudienceUriElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed partial class AllowedAudienceUriElementCollection : ServiceModelConfigurationElementCollection<AllowedAudienceUriElement>
    {
        public AllowedAudienceUriElementCollection()
            : base(ConfigurationElementCollectionType.BasicMap, ConfigurationStrings.Add)
        { }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AllowedAudienceUriElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            AllowedAudienceUriElement configElementKey = (AllowedAudienceUriElement)element;
            return configElementKey.AllowedAudienceUri;
        }

        protected override bool ThrowOnDuplicate
        {
            get { return true; }
        }
    }
}
