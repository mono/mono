//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel;
    using System.Collections.Generic;

    public sealed class DispatchOperation
    {
        string action;
        SynchronizedCollection<ICallContextInitializer> callContextInitializers;
        SynchronizedCollection<FaultContractInfo> faultContractInfos;
        IDispatchMessageFormatter formatter;
        IDispatchFaultFormatter faultFormatter;
        bool includeExceptionDetailInFaults;
        ImpersonationOption impersonation;
        IOperationInvoker invoker;
        bool isTerminating;
        bool isSessionOpenNotificationEnabled;
        string name;
        SynchronizedCollection<IParameterInspector> parameterInspectors;
        DispatchRuntime parent;
        bool releaseInstanceAfterCall;
        bool releaseInstanceBeforeCall;
        string replyAction;
        bool transactionAutoComplete;
        bool transactionRequired;
        bool deserializeRequest = true;
        bool serializeReply = true;
        bool isOneWay;
        bool autoDisposeParameters = true;
        bool hasNoDisposableParameters;
        bool isFaultFormatterSetExplicit = false;
        bool isInsideTransactedReceiveScope = false;

        public DispatchOperation(DispatchRuntime parent, string name, string action)
        {
            if (parent == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");

            this.parent = parent;
            this.name = name;
            this.action = action;
            this.impersonation = OperationBehaviorAttribute.DefaultImpersonationOption;

            this.callContextInitializers = parent.NewBehaviorCollection<ICallContextInitializer>();
            this.faultContractInfos = parent.NewBehaviorCollection<FaultContractInfo>();
            this.parameterInspectors = parent.NewBehaviorCollection<IParameterInspector>();
            this.isOneWay = true;
        }

        public DispatchOperation(DispatchRuntime parent, string name, string action, string replyAction)
            : this(parent, name, action)
        {
            this.replyAction = replyAction;
            this.isOneWay = false;
        }

        public bool IsOneWay
        {
            get { return this.isOneWay; }
        }

        public string Action
        {
            get { return this.action; }
        }

        public SynchronizedCollection<ICallContextInitializer> CallContextInitializers
        {
            get { return this.callContextInitializers; }
        }

        public SynchronizedCollection<FaultContractInfo> FaultContractInfos
        {
            get { return this.faultContractInfos; }
        }

        public bool AutoDisposeParameters
        {
            get { return this.autoDisposeParameters; }

            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.autoDisposeParameters = value;
                }
            }
        }

        public IDispatchMessageFormatter Formatter
        {
            get { return this.formatter; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.formatter = value;
                }
            }
        }

        internal IDispatchFaultFormatter FaultFormatter
        {
            get
            {
                if (this.faultFormatter == null)
                {
                    this.faultFormatter = new DataContractSerializerFaultFormatter(this.faultContractInfos);
                }
                return this.faultFormatter;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.faultFormatter = value;
                    this.isFaultFormatterSetExplicit = true;
                }
            }
        }

        internal bool IncludeExceptionDetailInFaults
        {
            get
            {
                return this.includeExceptionDetailInFaults;
            }
            set
            {
                this.includeExceptionDetailInFaults = value;
            }
        }

        internal bool IsFaultFormatterSetExplicit
        {
            get
            {
                return this.isFaultFormatterSetExplicit;
            }
        }

        public ImpersonationOption Impersonation
        {
            get { return this.impersonation; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.impersonation = value;
                }
            }
        }

        internal bool HasNoDisposableParameters
        {
            get { return this.hasNoDisposableParameters; }
            set { this.hasNoDisposableParameters = value; }
        }

        internal IDispatchMessageFormatter InternalFormatter
        {
            get { return this.formatter; }
            set { this.formatter = value; }
        }

        internal IOperationInvoker InternalInvoker
        {
            get { return this.invoker; }
            set { this.invoker = value; }
        }

        public IOperationInvoker Invoker
        {
            get { return this.invoker; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.invoker = value;
                }
            }
        }

        public bool IsTerminating
        {
            get { return this.isTerminating; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.isTerminating = value;
                }
            }
        }

        internal bool IsSessionOpenNotificationEnabled
        {
            get { return this.isSessionOpenNotificationEnabled; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.isSessionOpenNotificationEnabled = value;
                }
            }
        }

        public string Name
        {
            get { return this.name; }
        }

        public SynchronizedCollection<IParameterInspector> ParameterInspectors
        {
            get { return this.parameterInspectors; }
        }

        public DispatchRuntime Parent
        {
            get { return this.parent; }
        }

        internal ReceiveContextAcknowledgementMode ReceiveContextAcknowledgementMode
        {
            get;
            set;
        }

        internal bool BufferedReceiveEnabled
        {
            get { return this.parent.ChannelDispatcher.BufferedReceiveEnabled; }
            set { this.parent.ChannelDispatcher.BufferedReceiveEnabled = value; }
        }

        public bool ReleaseInstanceAfterCall
        {
            get { return this.releaseInstanceAfterCall; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.releaseInstanceAfterCall = value;
                }
            }
        }

        public bool ReleaseInstanceBeforeCall
        {
            get { return this.releaseInstanceBeforeCall; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.releaseInstanceBeforeCall = value;
                }
            }
        }

        public string ReplyAction
        {
            get { return this.replyAction; }
        }

        public bool DeserializeRequest
        {
            get { return this.deserializeRequest; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.deserializeRequest = value;
                }
            }
        }

        public bool SerializeReply
        {
            get { return this.serializeReply; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.serializeReply = value;
                }
            }
        }

        public bool TransactionAutoComplete
        {
            get { return this.transactionAutoComplete; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.transactionAutoComplete = value;
                }
            }
        }

        public bool TransactionRequired
        {
            get { return this.transactionRequired; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.transactionRequired = value;
                }
            }
        }

        public bool IsInsideTransactedReceiveScope
        {
            get { return this.isInsideTransactedReceiveScope; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.isInsideTransactedReceiveScope = value;
                }
            }
        }
    }
}
