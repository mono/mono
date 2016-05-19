//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;

    interface IServiceModelActivationHandler
    {
        ServiceHostFactoryBase GetFactory();
    }
}