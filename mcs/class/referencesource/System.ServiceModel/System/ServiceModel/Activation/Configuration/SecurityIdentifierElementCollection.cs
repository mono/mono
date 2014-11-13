//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Channels;
    using System.Security.Principal;

    [ConfigurationCollection(typeof(SecurityIdentifierElement))]
    public sealed class SecurityIdentifierElementCollection : ServiceModelConfigurationElementCollection<SecurityIdentifierElement>
    {
        public SecurityIdentifierElementCollection() : base() { }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            SecurityIdentifierElement configElementKey = (SecurityIdentifierElement)element;
            return configElementKey.SecurityIdentifier.Value;
        }

        internal void SetDefaultIdentifiers()
        {
            if (Iis7Helper.IisVersion >= 7)
            {
                this.Add(new SecurityIdentifierElement(new SecurityIdentifier(ConfigurationStrings.IIS_IUSRSSid)));
            }

            this.Add(new SecurityIdentifierElement(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null)));
            this.Add(new SecurityIdentifierElement(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null)));
            this.Add(new SecurityIdentifierElement(new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null)));
            this.Add(new SecurityIdentifierElement(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null)));
        }
    }
}


