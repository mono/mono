//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    class AcceleratedTokenProviderState : IssuanceTokenProviderState
    {
        byte[] entropy;

        public AcceleratedTokenProviderState(byte[] value)
        {
            this.entropy = value;
        }

        public byte[] GetRequestorEntropy()
        {       
            return this.entropy;
        }
    }
}
