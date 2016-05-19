//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ServiceModel.Security;

    public abstract class StreamUpgradeBindingElement : BindingElement
    {
        protected StreamUpgradeBindingElement()
        {
        }

        protected StreamUpgradeBindingElement(StreamUpgradeBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
        }

        public abstract StreamUpgradeProvider BuildClientStreamUpgradeProvider(BindingContext context);
        public abstract StreamUpgradeProvider BuildServerStreamUpgradeProvider(BindingContext context);
    }
}
