//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    public abstract class ServiceHostFactoryBase
    {
        public abstract ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses);
    }
}
