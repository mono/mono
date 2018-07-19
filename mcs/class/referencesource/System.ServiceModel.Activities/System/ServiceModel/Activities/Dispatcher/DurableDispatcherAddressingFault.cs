//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;
    using SR2 = System.ServiceModel.Activities.SR;

    class DurableDispatcherAddressingFault : MessageFault
    {
        FaultCode faultCode;
        FaultReason faultReason;

        public DurableDispatcherAddressingFault()
        {
            this.faultCode = FaultCode.CreateSenderFaultCode(XD2.ContextHeader.MissingContextHeader, XD2.ContextHeader.Namespace);
            this.faultReason = new FaultReason(new FaultReasonText(SR2.CurrentOperationCannotCreateInstance, CultureInfo.CurrentCulture));
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
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
