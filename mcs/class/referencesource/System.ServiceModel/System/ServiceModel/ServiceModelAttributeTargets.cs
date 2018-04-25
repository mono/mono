//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.Reflection;

    static class ServiceModelAttributeTargets
    {
        public const AttributeTargets ServiceContract = AttributeTargets.Interface | AttributeTargets.Class;
        public const AttributeTargets OperationContract = AttributeTargets.Method;
        public const AttributeTargets MessageContract = AttributeTargets.Class | AttributeTargets.Struct;
        public const AttributeTargets MessageMember = AttributeTargets.Property | AttributeTargets.Field;
        public const AttributeTargets Parameter = AttributeTargets.ReturnValue | AttributeTargets.Parameter;

        public const AttributeTargets ServiceBehavior = AttributeTargets.Class;
        public const AttributeTargets CallbackBehavior = AttributeTargets.Class;
        public const AttributeTargets ClientBehavior = AttributeTargets.Interface;
        public const AttributeTargets ContractBehavior = ServiceBehavior | ClientBehavior;
        public const AttributeTargets OperationBehavior = AttributeTargets.Method;
    }
}