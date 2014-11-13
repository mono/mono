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

    public abstract class MexBindingElement<TStandardBinding> : StandardBindingElement
        where TStandardBinding : Binding
    {

        protected MexBindingElement(string name)
            : base(name)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(TStandardBinding); }
        }

        protected override void OnApplyConfiguration(Binding binding) { }
    }
}
