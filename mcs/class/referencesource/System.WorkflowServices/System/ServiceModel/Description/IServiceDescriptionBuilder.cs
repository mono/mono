//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;

    interface IServiceDescriptionBuilder
    {
        void BuildServiceDescription(ServiceDescriptionContext context);
    }
}
