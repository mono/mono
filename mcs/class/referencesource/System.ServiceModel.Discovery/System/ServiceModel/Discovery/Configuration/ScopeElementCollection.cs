//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Configuration;

    [ConfigurationCollection(typeof(ScopeElement))]
    public sealed class ScopeElementCollection : ServiceModelConfigurationElementCollection<ScopeElement>
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }

            return ((ScopeElement)element).Scope;
        }
    }
}
