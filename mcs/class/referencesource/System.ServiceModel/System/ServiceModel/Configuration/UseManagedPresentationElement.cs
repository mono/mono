//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public partial class UseManagedPresentationElement : BindingElementExtensionElement
    {
        public override Type BindingElementType
        {
            get
            {
                return typeof( UseManagedPresentationBindingElement );
            }
        }

        protected internal override BindingElement CreateBindingElement()
        {
            UseManagedPresentationBindingElement binding = new UseManagedPresentationBindingElement();
            this.ApplyConfiguration(binding);
            return binding;
        }
    }
}
