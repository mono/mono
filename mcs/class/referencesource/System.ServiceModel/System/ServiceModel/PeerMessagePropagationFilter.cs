//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ServiceModel.Channels;

    public abstract class PeerMessagePropagationFilter
    {
        public abstract PeerMessagePropagation ShouldMessagePropagate(Message message, PeerMessageOrigination origination);
    }
}
