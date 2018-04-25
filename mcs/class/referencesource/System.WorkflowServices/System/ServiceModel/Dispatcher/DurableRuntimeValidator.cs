//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Description;
    using System.Workflow.Runtime;
    using System.Runtime;

    class DurableRuntimeValidator
    {
        ConcurrencyMode concurrencyMode;
        UnknownExceptionAction exceptionAction;
        bool saveStateInOperationTransaction;
        bool validated;

        public DurableRuntimeValidator(bool saveStateInOperationTransaction, UnknownExceptionAction exceptionAction)
        {
            this.saveStateInOperationTransaction = saveStateInOperationTransaction;
            this.exceptionAction = exceptionAction;
            this.validated = false;
        }

        public ConcurrencyMode ConcurrencyMode
        {
            get
            {
                if (!this.validated)
                {
                    ValidateRuntime();
                }

                return concurrencyMode;
            }
        }

        public void ValidateRuntime()
        {
            if (!this.validated)
            {
                Fx.Assert(
                    OperationContext.Current != null &&
                    OperationContext.Current.EndpointDispatcher != null &&
                    OperationContext.Current.EndpointDispatcher.DispatchRuntime != null,
                    "There shouldn't have been a null value in " +
                    "OperationContext.Current.EndpointDispatcher.DispatchRuntime.");

                this.concurrencyMode = OperationContext.Current.EndpointDispatcher.DispatchRuntime.ConcurrencyMode;

                if (this.concurrencyMode == ConcurrencyMode.Multiple)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(
                        SR2.GetString(SR2.ConcurrencyMultipleNotSupported)));
                }

                if (this.saveStateInOperationTransaction && this.concurrencyMode != ConcurrencyMode.Single)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(
                        SR2.GetString(SR2.SaveStateInTransactionRequiresSingle)));
                }

                if (this.concurrencyMode == ConcurrencyMode.Reentrant
                    && this.exceptionAction == UnknownExceptionAction.AbortInstance)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(
                        SR2.GetString(SR2.ConcurrencyReentrantAndAbortNotSupported)));
                }

                this.validated = true;
            }
        }
    }
}
