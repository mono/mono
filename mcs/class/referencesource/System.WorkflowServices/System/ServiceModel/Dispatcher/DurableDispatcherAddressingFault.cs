//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;
    using System.Globalization;
    using System.Xml;

    class DurableDispatcherAddressingFault : MessageFault
    {
        const string missingContextHeaderFaultName = "MissingContext";

        FaultCode faultCode;
        FaultReason faultReason;

        public DurableDispatcherAddressingFault()
        {
            faultCode = FaultCode.CreateSenderFaultCode(missingContextHeaderFaultName, ContextMessageHeader.ContextHeaderNamespace);
            faultReason = new FaultReason(new FaultReasonText(SR2.GetString(SR2.CurrentOperationCannotCreateInstance), CultureInfo.CurrentCulture));
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
