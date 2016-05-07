//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    // By default, when the TypeLoader sees an IContractBehavior attribute on a service implementation class, 
    // it will add that behavior to each contract (endpoint) the service implements.  But if the attribute
    // implements the interface below, then the TypeLoader will only add the behavior to the applicable contracts.
    public interface IContractBehaviorAttribute {
        Type TargetContract { get; }
    }
}
