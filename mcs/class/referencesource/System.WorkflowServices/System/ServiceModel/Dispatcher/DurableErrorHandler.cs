//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;
    using System.Diagnostics.CodeAnalysis;

    abstract class DurableErrorHandler : IErrorHandler
    {
        bool debug;

        public DurableErrorHandler(bool debug)
        {
            this.debug = debug;
        }

        public static void CleanUpInstanceContextAtOperationCompletion()
        {
            if (OperationContext.Current == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.NoOperationContext));
            }

            DurableMessageDispatchInspector.SuppressContextOnReply(OperationContext.Current);
            OperationContext.Current.InstanceContext.IncomingChannels.Clear();
        }

        public bool HandleError(Exception error)
        {
            return IsUserCodeException(error);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304")]
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (fault == null && IsUserCodeException(error))
            {
                FaultCode code = new FaultCode(FaultCodeConstants.Codes.InternalServiceFault, FaultCodeConstants.Namespaces.NetDispatch);
                code = FaultCode.CreateReceiverFaultCode(code);

                string action = FaultCodeConstants.Actions.NetDispatcher;
                MessageFault messageFault;

                if (this.debug)
                {
                    Exception toTrace = GetExceptionToTrace(error);
                    messageFault = MessageFault.CreateFault(code, new FaultReason(toTrace.Message), new ExceptionDetail(toTrace));
                }
                else
                {
                    string reason = SR.GetString(SR.SFxInternalServerError);
                    messageFault = MessageFault.CreateFault(code, new FaultReason(reason));
                }
                fault = Message.CreateMessage(version, messageFault, action);
            }
        }

        protected virtual Exception GetExceptionToTrace(Exception error)
        {
            return error;
        }

        protected abstract bool IsUserCodeException(Exception error);
    }
}
