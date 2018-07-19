//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Xaml.Hosting.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.Web;

    public sealed class XamlHostingSection : ConfigurationSection
    {
        [ConfigurationProperty(XamlHostingConfiguration.CollectionName, IsDefaultCollection = true)]
        public HandlerElementCollection Handlers
        {
            get
            {
                return (HandlerElementCollection)base[XamlHostingConfiguration.CollectionName];
            }
        }
    }
}

