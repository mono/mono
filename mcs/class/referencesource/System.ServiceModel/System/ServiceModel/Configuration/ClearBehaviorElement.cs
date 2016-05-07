//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    public sealed partial class ClearBehaviorElement : BehaviorExtensionElement
    {
        public ClearBehaviorElement() { }

        protected internal override object CreateBehavior()
        {
            return null;
        }

        public override Type BehaviorType
        {
            get { return null; }
        }
    }
}
