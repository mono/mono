//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    class DiscoveryMessageProperty
    {
        public const string Name = "System.ServiceModel.Discovery.DiscoveryMessageProperty";

        public DiscoveryMessageProperty()
        {
        }

        public DiscoveryMessageProperty(object correlationState)
        {
            this.CorrelationState = correlationState;
        }

        public object CorrelationState { get; set; }
    }
}
