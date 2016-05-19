//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(ClaimTypeElement))]
    public sealed class ClaimTypeElementCollection : ServiceModelConfigurationElementCollection<ClaimTypeElement>
    {
        public ClaimTypeElementCollection()
            : base()
        { }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ClaimTypeElement claimElement = (ClaimTypeElement)element;
            return claimElement.ClaimType;
        }
    }
}


