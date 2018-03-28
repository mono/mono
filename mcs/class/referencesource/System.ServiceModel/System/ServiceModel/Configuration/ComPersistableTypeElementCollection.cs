//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(ComPersistableTypeElement), AddItemName = ConfigurationStrings.Type)]
    public sealed class ComPersistableTypeElementCollection : ServiceModelEnhancedConfigurationElementCollection<ComPersistableTypeElement>
    {
        public ComPersistableTypeElementCollection()
            : base(ConfigurationStrings.Type)
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

            ComPersistableTypeElement configElementKey = (ComPersistableTypeElement)element;
            return configElementKey.ID;
        }
    }
}


