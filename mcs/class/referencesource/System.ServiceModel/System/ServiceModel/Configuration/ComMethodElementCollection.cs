//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(ComMethodElement))]
    public sealed class ComMethodElementCollection : ServiceModelEnhancedConfigurationElementCollection<ComMethodElement>
    {
        public ComMethodElementCollection()
            : base(ConfigurationStrings.Add)
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

            ComMethodElement configElementKey = (ComMethodElement)element;
            return configElementKey.ExposedMethod;
        }
    }
}


