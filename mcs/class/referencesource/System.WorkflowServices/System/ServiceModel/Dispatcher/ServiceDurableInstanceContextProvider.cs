//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Persistence;
    using System.Threading;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;

    class ServiceDurableInstanceContextProvider : DurableInstanceContextProvider
    {
        TimeSpan operationTimeout;
        PersistenceProviderFactory providerFactory;
        DurableRuntimeValidator runtimeValidator;
        bool saveStateInOperationTransaction;
        Type serviceType;
        UnknownExceptionAction unknownExceptionAction;

        public ServiceDurableInstanceContextProvider(
            ServiceHostBase serviceHostBase,
            bool isPercall,
            Type serviceType,
            PersistenceProviderFactory providerFactory,
            bool saveStateInOperationTransaction,
            UnknownExceptionAction unknownExceptionAction,
            DurableRuntimeValidator runtimeValidator,
            TimeSpan operationTimeout)
            : base(serviceHostBase, isPercall)
        {
            if (serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceType");
            }

            if (providerFactory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("providerFactory");
            }

            if (runtimeValidator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("runtimeValidator");
            }

            this.serviceType = serviceType;
            this.providerFactory = providerFactory;
            this.saveStateInOperationTransaction = saveStateInOperationTransaction;
            this.unknownExceptionAction = unknownExceptionAction;
            this.runtimeValidator = runtimeValidator;
            this.operationTimeout = operationTimeout;
        }

        protected override DurableInstance OnCreateNewInstance(Guid instanceId)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string traceText = SR2.GetString(SR2.InstanceContextProviderCreatedNewInstance, "Service", instanceId);
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.ActivatingMessageReceived, SR.GetString(SR.TraceCodeActivatingMessageReceived),
                    new StringTraceRecord("NewInstanceDetail", traceText),
                    this, null);
            }

            return new ServiceDurableInstance(
                this.providerFactory.CreateProvider(instanceId),
                this,
                this.saveStateInOperationTransaction,
                this.unknownExceptionAction,
                this.runtimeValidator,
                this.operationTimeout,
                this.serviceType);
        }

        protected override DurableInstance OnGetExistingInstance(Guid instanceId)
        {
            return new ServiceDurableInstance(
                this.providerFactory.CreateProvider(instanceId),
                this,
                this.saveStateInOperationTransaction,
                this.unknownExceptionAction,
                this.runtimeValidator,
                this.operationTimeout);
        }
    }
}
