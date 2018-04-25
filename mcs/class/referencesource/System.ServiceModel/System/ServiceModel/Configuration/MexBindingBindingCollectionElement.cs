//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;
    using System.ServiceModel.Security;
    using System.Text;
    using System.ComponentModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public abstract class MexBindingBindingCollectionElement<TStandardBinding, TBindingConfiguration> : StandardBindingCollectionElement<TStandardBinding, TBindingConfiguration>
        where TStandardBinding : Binding
        where TBindingConfiguration : StandardBindingElement, new()
        {
            protected internal override bool TryAdd(string name, Binding binding, Configuration config)
            {
                // We will never match a binding to the mex*Bindings.  The logic to do so would be:
                //     1) The binding was created using the mex*Binding config, or MetadataExchangeBindings.CreateMex*Binding()
                //     2) The binding has not been modified.
                // (2) is complicated and we don't have the time at this point.
                return false;
            }
        }

}
