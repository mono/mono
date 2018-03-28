//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Xaml.Hosting.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.Web;

    public sealed class XamlHostingSectionGroup : ConfigurationSectionGroup
    {
        public XamlHostingSectionGroup()
        {
        }

        public XamlHostingSection XamlHostingSection
        {
            get
            {
                return (XamlHostingSection)this.Sections[XamlHostingConfiguration.XamlHostingConfigGroup];
            }
        }
    }
}

