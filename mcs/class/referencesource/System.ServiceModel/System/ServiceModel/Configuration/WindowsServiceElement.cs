//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;
    using System.Configuration;
    using System.ServiceModel.Security;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Security.Cryptography.X509Certificates;

    public sealed partial class WindowsServiceElement : ConfigurationElement
    {
        public WindowsServiceElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.IncludeWindowsGroups, DefaultValue = SspiSecurityTokenProvider.DefaultExtractWindowsGroupClaims)]
        public bool IncludeWindowsGroups
        {
            get { return (bool)base[ConfigurationStrings.IncludeWindowsGroups]; }
            set { base[ConfigurationStrings.IncludeWindowsGroups] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.AllowAnonymousLogons, DefaultValue = SspiSecurityTokenProvider.DefaultAllowUnauthenticatedCallers)]
        public bool AllowAnonymousLogons
        {
            get { return (bool)base[ConfigurationStrings.AllowAnonymousLogons]; }
            set { base[ConfigurationStrings.AllowAnonymousLogons] = value; }
        }

        public void Copy(WindowsServiceElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == from)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }

            this.AllowAnonymousLogons = from.AllowAnonymousLogons;
            this.IncludeWindowsGroups = from.IncludeWindowsGroups;
        }

        internal void ApplyConfiguration(WindowsServiceCredential windows)
        {
            if (windows == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windows");
            }
            windows.AllowAnonymousLogons = this.AllowAnonymousLogons;
            windows.IncludeWindowsGroups = this.IncludeWindowsGroups;
        }

    }
}



