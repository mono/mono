
//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Compat", Justification = "Compat is an accepted abbreviation")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ClientOperationCompatBase
    {
        internal ClientOperationCompatBase() { }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        public IList<IParameterInspector> ParameterInspectors
        {
            get
            {
                return this.parameterInspectors;
            }
        }
        internal SynchronizedCollection<IParameterInspector> parameterInspectors;
    }

    public sealed class ClientOperation : ClientOperationCompatBase
    {
        string action;
        SynchronizedCollection<FaultContractInfo> faultContractInfos;
        bool serializeRequest;
        bool deserializeReply;
        IClientMessageFormatter formatter;
        IClientFaultFormatter faultFormatter;
        bool isInitiating = true;
        bool isOneWay;
        bool isTerminating;
        bool isSessionOpenNotificationEnabled;
        string name;

        ClientRuntime parent;
        string replyAction;
        MethodInfo beginMethod;
        MethodInfo endMethod;
        MethodInfo syncMethod;
        MethodInfo taskMethod;
        Type taskTResult;
        bool isFaultFormatterSetExplicit = false;

        public ClientOperation(ClientRuntime parent, string name, string action)
            : this(parent, name, action, null)
        {
        }

        public ClientOperation(ClientRuntime parent, string name, string action, string replyAction)
        {
            if (parent == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");

            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");

            this.parent = parent;
            this.name = name;
            this.action = action;
            this.replyAction = replyAction;

            this.faultContractInfos = parent.NewBehaviorCollection<FaultContractInfo>();
            this.parameterInspectors = parent.NewBehaviorCollection<IParameterInspector>();
        }

        public string Action
        {
            get { return this.action; }
        }

        public SynchronizedCollection<FaultContractInfo> FaultContractInfos
        {
            get { return this.faultContractInfos; }
        }

        public MethodInfo BeginMethod
        {
            get { return this.beginMethod; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.beginMethod = value;
                }
            }
        }

        public MethodInfo EndMethod
        {
            get { return this.endMethod; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.endMethod = value;
                }
            }
        }

        public MethodInfo SyncMethod
        {
            get { return this.syncMethod; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.syncMethod = value;
                }
            }
        }
        
        public IClientMessageFormatter Formatter
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
        
        internal IClientFaultFormatter FaultFormatter
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

        internal bool IsFaultFormatterSetExplicit
        {
            get 
            {
                return this.isFaultFormatterSetExplicit; 
            }
        }

        internal IClientMessageFormatter InternalFormatter
        {
            get { return this.formatter; }
            set { this.formatter = value; }
        }

        public bool IsInitiating
        {
            get { return this.isInitiating; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.isInitiating = value;
                }
            }
        }

        public bool IsOneWay
        {
            get { return this.isOneWay; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.isOneWay = value;
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

        public string Name
        {
            get { return this.name; }
        }

        public ICollection<IParameterInspector> ClientParameterInspectors
        {
            get { return this.ParameterInspectors; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new SynchronizedCollection<IParameterInspector> ParameterInspectors
        {
            get { return this.parameterInspectors; }
        }

        public ClientRuntime Parent
        {
            get { return this.parent; }
        }

        public string ReplyAction
        {
            get { return this.replyAction; }
        }

        public bool SerializeRequest
        {
            get { return this.serializeRequest; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.serializeRequest = value;
                }
            }
        }

        public bool DeserializeReply
        {
            get { return this.deserializeReply; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.deserializeReply = value;
                }
            }
        }

        public MethodInfo TaskMethod 
        {
            get { return this.taskMethod; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.taskMethod = value;
                }
            } 
        }

        public Type TaskTResult 
        {
            get { return this.taskTResult; }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.taskTResult = value;
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

    }
}
