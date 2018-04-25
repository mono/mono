//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;

    public interface IServiceContractGenerationExtension
    {
        void GenerateContract(ServiceContractGenerationContext context);
    }

}
