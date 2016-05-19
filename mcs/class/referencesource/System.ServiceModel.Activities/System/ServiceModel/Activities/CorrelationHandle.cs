//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Runtime.Serialization;

    [DataContract]
    public class CorrelationHandle : Handle
    {
        internal static readonly string StaticExecutionPropertyName = typeof(CorrelationHandle).FullName;

        static readonly Type requestReplyCorrelationInitializerType = typeof(RequestReplyCorrelationInitializer);

        // CorrelationHandles to support Context/Durable Duplex
        // For processing the CallBackContextMessageProperty
        static readonly Type callbackCorrelationInitializerType = typeof(CallbackCorrelationInitializer);  

        // This is for passing the Context information that we get in the reply message from the Server in the initial handshake 
        // to the next Sendmessage activity from the client to the server
        static readonly Type contextCorrelationInitializerType = typeof(ContextCorrelationInitializer);

        //// To get to the same instance on the server side( between SendReply and following Receive) and on the client side( between Send and following send)
        //static readonly Type followingContextCorrelationInitializerType = typeof(FollowingContextCorrelationInitializer);
        
        CorrelationCallbackContext callbackContext;
        CorrelationContext context;

        InstanceKey instanceKey;

        // This is never null when it matters because the CorrelationHandle sets this during OnInitialize
        NoPersistHandle noPersistHandle;

        // This is never null when it matters because the CorrelationHandle sets this during OnInitialize
        BookmarkScopeHandle bookmarkScopeHandle;

        public CorrelationHandle()
            : base()
        {
        }

        [DataMember(Name = "noPersistHandle")]
        internal NoPersistHandle SerializedNoPersistHandle
        {
            get { return this.noPersistHandle; }
            set { this.noPersistHandle = value; }
        }

        [DataMember(Name = "bookmarkScopeHandle")]
        internal BookmarkScopeHandle SerializedBookmarkScopeHandle
        {
            get { return this.bookmarkScopeHandle; }
            set { this.bookmarkScopeHandle = value; }
        }

        [DataMember(EmitDefaultValue = false)]
        internal Guid E2ETraceId
        {
            get;
            set;
        }

        // Used for durable correlation purposes
        internal InstanceKey InstanceKey
        {
            get
            {
                return this.instanceKey;
            }
            private set
            {
                this.instanceKey = value;
            }
        }

        
        // As a convenience, we let the same correlation handle that is used for durable
        // correlations be leveraged for a single outstanding transient (e.g. Request-Reply)
        // correlation. This is primarily used in the ambient correlation case, and is 
        // done this way since we cannot have two Execution Properties (i.e. activityContext.Properties)
        // with the same type at a given scope without a patch to the WF Runtime
        internal InstanceKey TransientInstanceKey
        {
            get;
            set;
        }

        [DataMember(Name = "InstanceKey", EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal SerializableInstanceKey SerializableInstanceKey
        {
            get
            {
                if (this.InstanceKey != null)
                {
                    return new SerializableInstanceKey(this.InstanceKey);
                }
                return null;
            }

            set
            {
                this.InstanceKey = value.ToInstanceKey();
            }
        }

        internal CorrelationRequestContext RequestContext
        {
            get;
            private set;
        }

        internal CorrelationResponseContext ResponseContext
        {
            get;
            private set;
        }

        [DataMember(EmitDefaultValue = false)]
        internal CorrelationCallbackContext CallbackContext
        {
            get
            {
                return this.callbackContext;
            }

            set
            {
                Fx.Assert(this.callbackContext == null || this.callbackContext == value, "cannot set two different callback contexts");
                this.callbackContext = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        internal CorrelationContext Context
        {
            get
            {
                return this.context;
            }

            set
            {
                Fx.Assert(this.context == null || this.context == value, "cannot set two different callback contexts");
                this.context = value;
            }
        }


        [DataMember(EmitDefaultValue = false)]
        internal BookmarkScope Scope
        {
            get;
            set;
        }

        protected override void OnInitialize(HandleInitializationContext context)
        {
            this.noPersistHandle = context.CreateAndInitializeHandle<NoPersistHandle>();
            this.bookmarkScopeHandle = context.CreateAndInitializeHandle<BookmarkScopeHandle>();
        }

        protected override void OnUninitialize(HandleInitializationContext context)
        {
            SendReceiveExtension sendReceiveExtension = context.GetExtension<SendReceiveExtension>();
            if (sendReceiveExtension != null)
            {
                if (this.InstanceKey != null)
                {
                    sendReceiveExtension.OnUninitializeCorrelation(this.InstanceKey);
                }
                if (this.TransientInstanceKey != null)
                {
                    sendReceiveExtension.OnUninitializeCorrelation(this.TransientInstanceKey);
                }
            }

            context.UninitializeHandle(this.noPersistHandle);
            context.UninitializeHandle(this.bookmarkScopeHandle);
        }

        internal BookmarkScope EnsureBookmarkScope(NativeActivityContext executionContext)
        {
            if (this.Scope == null)
            {
                this.Scope = executionContext.DefaultBookmarkScope;
            }
            return this.Scope;
        }

        internal bool TryRegisterRequestContext(NativeActivityContext executionContext, CorrelationRequestContext requestContext)
        {
            Fx.Assert(requestContext != null, "requires a valid requestContext");
            if (this.noPersistHandle == null)
            {
                return false;
            }
            if (this.RequestContext == null)
            {
                this.noPersistHandle.Enter(executionContext);
                this.RequestContext = requestContext;
                return true;
            }

            return object.ReferenceEquals(this.RequestContext, requestContext);
        }

        internal bool TryRegisterResponseContext(NativeActivityContext executionContext, CorrelationResponseContext responseContext)
        {
            Fx.Assert(responseContext != null, "requires a valid responseContext");
            if (this.noPersistHandle == null)
            {
                return false;
            }
            if (this.ResponseContext == null)
            {
                this.noPersistHandle.Enter(executionContext);
                this.ResponseContext = responseContext;
                return true;
            }

            return object.ReferenceEquals(this.ResponseContext, responseContext);
        }

        internal bool TryAcquireRequestContext(NativeActivityContext executionContext, out CorrelationRequestContext requestContext)
        {
            if (this.RequestContext != null)
            {
                // We have a context, and we should disassociate it from the correlation handle
                this.noPersistHandle.Exit(executionContext);
                requestContext = this.RequestContext;
                this.RequestContext = null;
                return true;
            }
            else
            {
                requestContext = null;
                return false;
            }
        }

        internal bool TryAcquireResponseContext(NativeActivityContext executionContext, out CorrelationResponseContext responseContext)
        {
            if (this.ResponseContext != null)
            {
                // We have a context, and we should disassociate it from the correlation handle
                this.noPersistHandle.Exit(executionContext);
                responseContext = this.ResponseContext;
                this.ResponseContext = null;
                return true;
            }
            else
            {
                responseContext = null;
                return false;
            }
        }

        internal void InitializeBookmarkScope(NativeActivityContext context, InstanceKey instanceKey)
        {
            Fx.Assert(context != null, "executionContext cannot be null");
            Fx.Assert(instanceKey != null, "instanceKey cannot be null");

            if (context.GetExtension<SendReceiveExtension>() != null)
            {
                if (this.InstanceKey != null && this.InstanceKey.Value != instanceKey.Value)
                {
                    throw FxTrace.Exception.AsError(
                        new InvalidOperationException(SR.CorrelationHandleInUse(this.InstanceKey.Value, instanceKey.Value)));
                }
                this.InstanceKey = instanceKey;
            }
            else
            {
                if (this.Scope == null)
                {
                    this.bookmarkScopeHandle.CreateBookmarkScope(context, instanceKey.Value);
                    this.Scope = this.bookmarkScopeHandle.BookmarkScope;
                }
                else
                {
                    if (this.Scope.IsInitialized)
                    {
                        if (this.Scope.Id != instanceKey.Value)
                        {
                            throw FxTrace.Exception.AsError(
                                new InvalidOperationException(SR.CorrelationHandleInUse(this.Scope.Id, instanceKey.Value)));
                        }
                    }
                    else
                    {
                        this.Scope.Initialize(context, instanceKey.Value);
                    }
                }
            }
        }

        internal bool IsInitalized()
        {
            if (this.Scope != null || this.CallbackContext != null || this.Context != null || this.ResponseContext != null || this.RequestContext != null || (this.InstanceKey != null && this.InstanceKey.IsValid))
            {
                return true;
            }

            return false;
        }

        internal static CorrelationHandle GetAmbientCorrelation(NativeActivityContext context)
        {
            return context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
        }

        internal static CorrelationHandle GetExplicitRequestReplyCorrelation(NativeActivityContext context, Collection<CorrelationInitializer> correlationInitializers)
        {
            return GetTypedCorrelationHandle(context, correlationInitializers, requestReplyCorrelationInitializerType);
        }

        internal static CorrelationHandle GetExplicitCallbackCorrelation(NativeActivityContext context, Collection<CorrelationInitializer> correlationInitializers)
        {
            return GetTypedCorrelationHandle(context, correlationInitializers, callbackCorrelationInitializerType);
        }

        internal static CorrelationHandle GetExplicitContextCorrelation(NativeActivityContext context, Collection<CorrelationInitializer> correlationInitializers)
        {
            return GetTypedCorrelationHandle(context, correlationInitializers, contextCorrelationInitializerType);
        }

        internal static CorrelationHandle GetTypedCorrelationHandle(NativeActivityContext context, Collection<CorrelationInitializer> correlationInitializers, Type correlationInitializerType)
        {
            CorrelationHandle typedCorrelationHandle = null;

            if (correlationInitializers != null && correlationInitializers.Count > 0)
            {
                foreach (CorrelationInitializer correlation in correlationInitializers)
                {
                    if (correlationInitializerType == correlation.GetType())
                    {
                        typedCorrelationHandle = correlation.CorrelationHandle.Get(context);
                        
                        // We return the first handle we find
                        break;
                    }
                }
            }

            return typedCorrelationHandle;
        }
    }
}
