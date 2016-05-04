//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(IssuedTokenClientBehaviorsElement))]
    public sealed class IssuedTokenClientBehaviorsElementCollection : ServiceModelConfigurationElementCollection<IssuedTokenClientBehaviorsElement>
    {
        public IssuedTokenClientBehaviorsElementCollection()
            : base()
        { }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            IssuedTokenClientBehaviorsElement behaviorElement = (IssuedTokenClientBehaviorsElement)element;
            return behaviorElement.IssuerAddress;
        }
    }
}


