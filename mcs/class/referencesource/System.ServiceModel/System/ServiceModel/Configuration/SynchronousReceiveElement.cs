//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    public sealed partial class SynchronousReceiveElement : BehaviorExtensionElement
    {
        public SynchronousReceiveElement()
        {
        }

        protected internal override object CreateBehavior()
        {
            return new System.ServiceModel.Description.SynchronousReceiveBehavior();
        }

        public override Type BehaviorType
        {
            get { return typeof(System.ServiceModel.Description.SynchronousReceiveBehavior); }
        }
    }
}

