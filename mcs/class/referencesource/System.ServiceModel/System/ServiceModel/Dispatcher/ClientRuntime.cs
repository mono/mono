//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Xml;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Compat", Justification = "Compat is an accepted abbreviation")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ClientRuntimeCompatBase
    {
        internal ClientRuntimeCompatBase() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        public IList<IClientMessageInspector> MessageInspectors
        {
            get
            {
                return this.messageInspectors;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        public KeyedCollection<string, ClientOperation> Operations
        {
            get
            {
                return this.compatOperations;
            }
        }
        internal SynchronizedCollection<IClientMessageInspector> messageInspectors;
        internal SynchronizedKeyedCollection<string, ClientOperation> operations;
        internal KeyedCollection<string, ClientOperation> compatOperations;
    }

    public sealed class ClientRuntime : ClientRuntimeCompatBase
    {
        bool addTransactionFlowProperties = true;
        Type callbackProxyType;
        ProxyBehaviorCollection<IChannelInitializer> channelInitializers;
        string contractName;
        string contractNamespace;
        Type contractProxyType;
        DispatchRuntime dispatchRuntime;
        IdentityVerifier identityVerifier;
        ProxyBehaviorCollection<IInteractiveChannelInitializer> interactiveChannelInitializers;

        IClientOperationSelector operationSelector;
        ImmutableClientRuntime runtime;
        ClientOperation unhandled;
        bool useSynchronizationContext = true;
        Uri via;
        SharedRuntimeState shared;
        int maxFaultSize;
        bool messageVersionNoneFaultsEnabled;

        internal ClientRuntime(DispatchRuntime dispatchRuntime, SharedRuntimeState shared)
            : this(dispatchRuntime.EndpointDispatcher.ContractName,
                   dispatchRuntime.EndpointDispatcher.ContractNamespace,
                   shared)
        {
            if (dispatchRuntime == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatchRuntime");

            this.dispatchRuntime = dispatchRuntime;
            this.shared = shared;

            Fx.Assert(shared.IsOnServer, "Server constructor called on client?");
        }

        internal ClientRuntime(string contractName, string contractNamespace)
            : this(contractName, contractNamespace, new SharedRuntimeState(false))
        {
            Fx.Assert(!shared.IsOnServer, "Client constructor called on server?");
        }

        ClientRuntime(string contractName, string contractNamespace, SharedRuntimeState shared)
        {
            this.contractName = contractName;
            this.contractNamespace = contractNamespace;
            this.shared = shared;

            OperationCollection operations = new OperationCollection(this);
            this.operations = operations;
            this.compatOperations = new OperationCollectionWrapper(operations);
            this.channelInitializers = new ProxyBehaviorCollection<IChannelInitializer>(this);
            this.messageInspectors = new ProxyBehaviorCollection<IClientMessageInspector>(this);
            this.interactiveChannelInitializers = new ProxyBehaviorCollection<IInteractiveChannelInitializer>(this);

            this.unhandled = new ClientOperation(this, "*", MessageHeaders.WildcardAction, MessageHeaders.WildcardAction);
            this.unhandled.InternalFormatter = new MessageOperationFormatter();
            this.maxFaultSize = TransportDefaults.MaxFaultSize;
        }

        internal bool AddTransactionFlowProperties
        {
            get { return this.addTransactionFlowProperties; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.addTransactionFlowProperties = value;
                }
            }
        }

        public Type CallbackClientType
        {
            get { return this.callbackProxyType; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.callbackProxyType = value;
                }
            }
        }

        public SynchronizedCollection<IChannelInitializer> ChannelInitializers
        {
            get { return this.channelInitializers; }
        }

        public string ContractName
        {
            get { return this.contractName; }
        }

        public string ContractNamespace
        {
            get { return this.contractNamespace; }
        }

        public Type ContractClientType
        {
            get { return this.contractProxyType; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.contractProxyType = value;
                }
            }
        }

        internal IdentityVerifier IdentityVerifier
        {
            get
            {
                if (this.identityVerifier == null)
                {
                    this.identityVerifier = IdentityVerifier.CreateDefault();
                }

                return this.identityVerifier;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.InvalidateRuntime();

                this.identityVerifier = value;
            }
        }

        public Uri Via
        {
            get { return this.via; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.via = value;
                }
            }
        }

        public bool ValidateMustUnderstand
        {
            get { return this.shared.ValidateMustUnderstand; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.shared.ValidateMustUnderstand = value;
                }
            }
        }

        public bool MessageVersionNoneFaultsEnabled
        {
            get
            {
                return this.messageVersionNoneFaultsEnabled;
            }
            set
            {
                this.InvalidateRuntime();
                this.messageVersionNoneFaultsEnabled = value;
            }
        }

        internal DispatchRuntime DispatchRuntime
        {
            get { return this.dispatchRuntime; }
        }

        public DispatchRuntime CallbackDispatchRuntime
        {
            get
            {
                if (this.dispatchRuntime == null)
                    this.dispatchRuntime = new DispatchRuntime(this, this.shared);

                return this.dispatchRuntime;
            }
        }

        internal bool EnableFaults
        {
            get
            {
                if (this.IsOnServer)
                {
                    return this.dispatchRuntime.EnableFaults;
                }
                else
                {
                    return this.shared.EnableFaults;
                }
            }
            set
            {
                lock (this.ThisLock)
                {
                    if (this.IsOnServer)
                    {
                        string text = SR.GetString(SR.SFxSetEnableFaultsOnChannelDispatcher0);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(text));
                    }
                    else
                    {
                        this.InvalidateRuntime();
                        this.shared.EnableFaults = value;
                    }
                }
            }
        }

        public SynchronizedCollection<IInteractiveChannelInitializer> InteractiveChannelInitializers
        {
            get { return this.interactiveChannelInitializers; }
        }

        public int MaxFaultSize
        {
            get
            {
                return this.maxFaultSize;
            }
            set
            {
                this.InvalidateRuntime();
                this.maxFaultSize = value;
            }
        }

        internal bool IsOnServer
        {
            get { return this.shared.IsOnServer; }
        }

        public bool ManualAddressing
        {
            get
            {
                if (this.IsOnServer)
                {
                    return this.dispatchRuntime.ManualAddressing;
                }
                else
                {
                    return this.shared.ManualAddressing;
                }
            }
            set
            {
                lock (this.ThisLock)
                {
                    if (this.IsOnServer)
                    {
                        string text = SR.GetString(SR.SFxSetManualAddresssingOnChannelDispatcher0);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(text));
                    }
                    else
                    {
                        this.InvalidateRuntime();
                        this.shared.ManualAddressing = value;
                    }
                }
            }
        }

        internal int MaxParameterInspectors
        {
            get
            {
                lock (this.ThisLock)
                {
                    int max = 0;

                    for (int i = 0; i < this.operations.Count; i++)
                        max = System.Math.Max(max, this.operations[i].ParameterInspectors.Count);

                    return max;
                }
            }
        }

        public ICollection<IClientMessageInspector> ClientMessageInspectors
        {
            get { return this.MessageInspectors; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new SynchronizedCollection<IClientMessageInspector> MessageInspectors
        {
            get { return this.messageInspectors; }
        }

        public ICollection<ClientOperation> ClientOperations
        {
            get { return this.Operations; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new SynchronizedKeyedCollection<string, ClientOperation> Operations
        {
            get { return this.operations; }
        }

        public IClientOperationSelector OperationSelector
        {
            get { return this.operationSelector; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.operationSelector = value;
                }
            }
        }

        internal object ThisLock
        {
            get { return this.shared; }
        }

        public ClientOperation UnhandledClientOperation
        {
            get { return this.unhandled; }
        }

        internal bool UseSynchronizationContext
        {
            get { return this.useSynchronizationContext; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.useSynchronizationContext = value;
                }
            }
        }

        internal T[] GetArray<T>(SynchronizedCollection<T> collection)
        {
            lock (collection.SyncRoot)
            {
                if (collection.Count == 0)
                {
                    return EmptyArray<T>.Instance;
                }
                else
                {
                    T[] array = new T[collection.Count];
                    collection.CopyTo(array, 0);
                    return array;
                }
            }
        }

        internal ImmutableClientRuntime GetRuntime()
        {
            lock (this.ThisLock)
            {
                if (this.runtime == null)
                    this.runtime = new ImmutableClientRuntime(this);

                return this.runtime;
            }
        }

        internal void InvalidateRuntime()
        {
            lock (this.ThisLock)
            {
                this.shared.ThrowIfImmutable();
                this.runtime = null;
            }
        }

        internal void LockDownProperties()
        {
            this.shared.LockDownProperties();
        }

        internal SynchronizedCollection<T> NewBehaviorCollection<T>()
        {
            return new ProxyBehaviorCollection<T>(this);
        }

        internal bool IsFault(ref Message reply)
        {
            if (reply == null)
            {
                return false;
            }
            if (reply.IsFault)
            {
                return true;
            }
            if (this.MessageVersionNoneFaultsEnabled && IsMessageVersionNoneFault(ref reply, this.MaxFaultSize))
            {
                return true;
            }

            return false;
        }

        internal static bool IsMessageVersionNoneFault(ref Message message, int maxFaultSize)
        {
            if (message.Version != MessageVersion.None || message.IsEmpty)
            {
                return false;
            }
            HttpResponseMessageProperty prop = message.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
            if (prop == null || prop.StatusCode != HttpStatusCode.InternalServerError)
            {
                return false;
            }
            using (MessageBuffer buffer = message.CreateBufferedCopy(maxFaultSize))
            {
                message.Close();
                message = buffer.CreateMessage();
                using (Message copy = buffer.CreateMessage())
                {
                    using (XmlDictionaryReader reader = copy.GetReaderAtBodyContents())
                    {
                        return reader.IsStartElement(XD.MessageDictionary.Fault, MessageVersion.None.Envelope.DictionaryNamespace);
                    }
                }
            }
        }

        class ProxyBehaviorCollection<T> : SynchronizedCollection<T>
        {
            ClientRuntime outer;

            internal ProxyBehaviorCollection(ClientRuntime outer)
                : base(outer.ThisLock)
            {
                this.outer = outer;
            }

            protected override void ClearItems()
            {
                this.outer.InvalidateRuntime();
                base.ClearItems();
            }

            protected override void InsertItem(int index, T item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }

                this.outer.InvalidateRuntime();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                this.outer.InvalidateRuntime();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, T item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }

                this.outer.InvalidateRuntime();
                base.SetItem(index, item);
            }
        }

        class OperationCollection : SynchronizedKeyedCollection<string, ClientOperation>
        {
            ClientRuntime outer;

            internal OperationCollection(ClientRuntime outer)
                : base(outer.ThisLock)
            {
                this.outer = outer;
            }

            protected override void ClearItems()
            {
                this.outer.InvalidateRuntime();
                base.ClearItems();
            }

            protected override string GetKeyForItem(ClientOperation item)
            {
                return item.Name;
            }

            protected override void InsertItem(int index, ClientOperation item)
            {
                if (item == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                if (item.Parent != this.outer)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxMismatchedOperationParent));

                this.outer.InvalidateRuntime();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                this.outer.InvalidateRuntime();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, ClientOperation item)
            {
                if (item == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                if (item.Parent != this.outer)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxMismatchedOperationParent));

                this.outer.InvalidateRuntime();
                base.SetItem(index, item);
            }

            internal void InternalClearItems() { this.ClearItems(); }
            internal string InternalGetKeyForItem(ClientOperation item) { return this.GetKeyForItem(item); }
            internal void InternalInsertItem(int index, ClientOperation item) { this.InsertItem(index, item); }
            internal void InternalRemoveItem(int index) { this.RemoveItem(index); }
            internal void InternalSetItem(int index, ClientOperation item) { this.SetItem(index, item); }
        }


        class OperationCollectionWrapper : KeyedCollection<string, ClientOperation>
        {
            OperationCollection inner;
            internal OperationCollectionWrapper(OperationCollection inner) { this.inner = inner; }
            protected override void ClearItems() { this.inner.InternalClearItems(); }
            protected override string GetKeyForItem(ClientOperation item) { return this.inner.InternalGetKeyForItem(item); }
            protected override void InsertItem(int index, ClientOperation item) { this.inner.InternalInsertItem(index, item); }
            protected override void RemoveItem(int index) { this.inner.InternalRemoveItem(index); }
            protected override void SetItem(int index, ClientOperation item) { this.inner.InternalSetItem(index, item); }
        }

    }
}
