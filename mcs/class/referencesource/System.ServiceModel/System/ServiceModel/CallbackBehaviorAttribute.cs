//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.ServiceModel.Administration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.ServiceModel.Configuration;
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Threading;
    using System.Transactions;
    using System.Runtime.CompilerServices;
    using System.Globalization;

    [AttributeUsage(ServiceModelAttributeTargets.CallbackBehavior)]
    public sealed class CallbackBehaviorAttribute : Attribute, IEndpointBehavior
    {
        ConcurrencyMode concurrencyMode = ConcurrencyMode.Single;
        bool includeExceptionDetailInFaults = false;
        bool validateMustUnderstand = true;
        bool ignoreExtensionDataObject = DataContractSerializerDefaults.IgnoreExtensionDataObject;
        int maxItemsInObjectGraph = DataContractSerializerDefaults.MaxItemsInObjectGraph;
        bool automaticSessionShutdown = true;
        bool useSynchronizationContext = true;
        internal static IsolationLevel DefaultIsolationLevel = IsolationLevel.Unspecified;
        IsolationLevel transactionIsolationLevel = DefaultIsolationLevel;
        bool isolationLevelSet = false;
        TimeSpan transactionTimeout = TimeSpan.Zero;
        string transactionTimeoutString;
        bool transactionTimeoutSet = false;

        public bool AutomaticSessionShutdown
        {
            get { return this.automaticSessionShutdown; }
            set { this.automaticSessionShutdown = value; }
        }

        public IsolationLevel TransactionIsolationLevel
        {
            get { return this.transactionIsolationLevel; }
            set
            {
                switch (value)
                {
                    case IsolationLevel.Serializable:
                    case IsolationLevel.RepeatableRead:
                    case IsolationLevel.ReadCommitted:
                    case IsolationLevel.ReadUncommitted:
                    case IsolationLevel.Unspecified:
                    case IsolationLevel.Chaos:
                    case IsolationLevel.Snapshot:
                        break;

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.transactionIsolationLevel = value;
                isolationLevelSet = true;
            }
        }

        internal bool IsolationLevelSet
        {
            get { return this.isolationLevelSet; }
        }

        public bool IncludeExceptionDetailInFaults
        {
            get { return this.includeExceptionDetailInFaults; }
            set { this.includeExceptionDetailInFaults = value; }
        }

        public ConcurrencyMode ConcurrencyMode
        {
            get { return this.concurrencyMode; }
            set
            {
                if (!ConcurrencyModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.concurrencyMode = value;
            }
        }

        public string TransactionTimeout
        {
            get { return transactionTimeoutString; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }

                try
                {
                    TimeSpan timeout = TimeSpan.Parse(value, CultureInfo.InvariantCulture);

                    if (timeout < TimeSpan.Zero)
                    {
                        string message = SR.GetString(SR.SFxTimeoutOutOfRange0);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, message));
                    }

                    this.transactionTimeout = timeout;
                    this.transactionTimeoutString = value;
                    this.transactionTimeoutSet = true;
                }
                catch (FormatException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxTimeoutInvalidStringFormat), "value", e));
                }
                catch (OverflowException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
            }
        }

        internal bool TransactionTimeoutSet
        {
            get { return this.transactionTimeoutSet; }
        }

        public bool UseSynchronizationContext
        {
            get { return this.useSynchronizationContext; }
            set { this.useSynchronizationContext = value; }
        }

        public bool ValidateMustUnderstand
        {
            get { return validateMustUnderstand; }
            set { validateMustUnderstand = value; }
        }

        public bool IgnoreExtensionDataObject
        {
            get { return ignoreExtensionDataObject; }
            set { ignoreExtensionDataObject = value; }
        }

        public int MaxItemsInObjectGraph
        {
            get { return maxItemsInObjectGraph; }
            set { maxItemsInObjectGraph = value; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void SetIsolationLevel(ChannelDispatcher channelDispatcher)
        {
            if (channelDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelDispatcher");
            }

            channelDispatcher.TransactionIsolationLevel = this.transactionIsolationLevel;
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection parameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
        {
            if (!serviceEndpoint.Contract.IsDuplex())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.SFxCallbackBehaviorAttributeOnlyOnDuplex, serviceEndpoint.Contract.Name)));
            }
            DispatchRuntime dispatchRuntime = clientRuntime.DispatchRuntime;
            dispatchRuntime.ValidateMustUnderstand = validateMustUnderstand;
            dispatchRuntime.ConcurrencyMode = this.concurrencyMode;
            dispatchRuntime.ChannelDispatcher.IncludeExceptionDetailInFaults = this.includeExceptionDetailInFaults;
            dispatchRuntime.AutomaticInputSessionShutdown = this.automaticSessionShutdown;
            if (!this.useSynchronizationContext)
            {
                dispatchRuntime.SynchronizationContext = null;
            }

            dispatchRuntime.ChannelDispatcher.TransactionTimeout = transactionTimeout;

            if (isolationLevelSet)
            {
                SetIsolationLevel(dispatchRuntime.ChannelDispatcher);
            }

            DataContractSerializerServiceBehavior.ApplySerializationSettings(serviceEndpoint, this.ignoreExtensionDataObject, this.maxItemsInObjectGraph);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                SR.GetString(SR.SFXEndpointBehaviorUsedOnWrongSide, typeof(CallbackBehaviorAttribute).Name)));
        }
    }
}
