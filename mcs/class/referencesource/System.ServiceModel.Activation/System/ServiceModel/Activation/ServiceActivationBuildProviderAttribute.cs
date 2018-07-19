//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    // This attribute specifies which build providers generates SM services.
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ServiceActivationBuildProviderAttribute : Attribute
    {
    }
}
