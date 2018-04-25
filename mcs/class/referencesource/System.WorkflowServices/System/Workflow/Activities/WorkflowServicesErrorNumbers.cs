//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.ComponentModel
{
    internal static class WorkflowServicesErrorNumbers
    {
        // operation info validation errors
        public const int Error_OperationInfoNotSpecified = 0x700;
        public const int Error_OperationNameNotSpecified = 0x701;
        public const int Error_OperationNameInvalid = 0x702;
        public const int Error_ContractNameNotSpecified = 0x703;
        public const int Error_ContractNameInvalid = 0x704;
        public const int Error_ContractNameDuplicate = 0x705;
        public const int Error_ContractTypeNotSpecified = 0x706;
        public const int Error_ContractTypeNotFound = 0x707;
        public const int Error_ContractTypeNotInterface = 0x708;
        public const int Error_ServiceContractAttributeMissing = 0x709;
        public const int Error_OperationContractAttributeMissing = 0x70A;
        public const int Error_OperationNotInContract = 0x70B;
        public const int Error_OperationNotInitiating = 0x70C;
        public const int Error_OperationIsOneWay = 0x70D;
        public const int Error_OperationParameterPosition = 0x70E;
        public const int Error_OperationParameterPositionDuplicate = 0x70F;
        public const int Error_OperationParameterNameInvalid = 0x710;
        public const int Error_OperationParameterNameDuplicate = 0x711;
        public const int Error_OperationParameterDirectionInOneWayOperation = 0x712;
        public const int Error_ReturnTypeInOneWayOperation = 0x713;
        // contract implementation validation errors
        public const int Error_OperationNotImplemented = 0x714;
        // context token validation errors
        public const int Error_ContextTokenNameNotSpecified = 0x715;
        public const int Error_RootContextScope = 0x716;
        // channel token validation errors
        public const int Error_ChannelTokenNotSpecified = 0x717;
        public const int Error_ChannelTokenNameNotSpecified = 0x718;
        public const int Error_ChannelTokenConfigurationNameNotSpecified = 0x719;
        // service attribute validation errors
        public const int Error_InvalidMaxItemsInObjectGraph = 0x71A;
        // Send and Receive activity validation errors
        public const int Warning_SendActivityParameterBindingMissing = 0x71B;
        public const int Warning_ReceiveActivityParameterBindingMissing = 0x71C;
        public const int Warning_ReceiveActivityReturnValueBindingMissing = 0x71D;
        public const int Error_DuplicatedOperationName = 0x71E;
        public const int Error_AsyncPatternOperationNotSupported = 0x71F;
        public const int Error_OperationParameterType = 0x720;
        public const int Error_OwnerActivityNameNotFound = 0x721;
    }
}
