//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;
    using System.Globalization;
    using System.Messaging;
    using System.Xml;

    class WorkflowOperationFault : MessageFault
    {
        const string operationNotAvailable = "OperationNotAvaialable";
        const string operationNotImplemented = "OperationNotImplemented";

        FaultCode faultCode;
        FaultReason faultReason;

        public WorkflowOperationFault(MessageQueueErrorCode errorCode)
        {
            if (errorCode == MessageQueueErrorCode.QueueNotAvailable)
            {
                faultCode = FaultCode.CreateSenderFaultCode(operationNotAvailable, ContextMessageHeader.ContextHeaderNamespace);
                faultReason = new FaultReason(new FaultReasonText(SR2.GetString(SR2.OperationNotAvailable), CultureInfo.CurrentCulture));
            }
            else
            {
                faultCode = FaultCode.CreateSenderFaultCode(operationNotImplemented, ContextMessageHeader.ContextHeaderNamespace);
                faultReason = new FaultReason(new FaultReasonText(SR2.GetString(SR2.OperationNotImplemented), CultureInfo.CurrentCulture));
            }
        }

        public override FaultCode Code
        {
            get
            {
                return this.faultCode;
            }
        }

        public override bool HasDetail
        {
            get
            {
                return false;
            }
        }

        public override FaultReason Reason
        {
            get
            {
                return this.faultReason;
            }
        }

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }
    }
}
