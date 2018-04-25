//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Diagnostics;

    abstract class DurableInstanceContextProvider : IInstanceContextProvider
    {
        ContextCache contextCache;

        bool isPerCall;
        ServiceHostBase serviceHostBase;

        protected DurableInstanceContextProvider(ServiceHostBase serviceHostBase, bool isPerCall)
        {
            if (serviceHostBase == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceHostBase");
            }

            this.serviceHostBase = serviceHostBase;

            if (serviceHostBase.Description.Behaviors.Find<ServiceThrottlingBehavior>() == null)
            {
                serviceHostBase.ServiceThrottle.MaxConcurrentInstances = (new ServiceThrottlingBehavior()).MaxConcurrentInstances;
            }
            this.contextCache = new ContextCache();
            this.isPerCall = isPerCall;
        }

        protected ContextCache Cache
        {
            get
            {
                return this.contextCache;
            }
        }

        public virtual InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (channel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channel");
            }

            Guid instanceId = GetInstanceIdFromMessage(message);
            InstanceContext result = null;

            if (instanceId != Guid.Empty) //Not an activation request.
            {
                if (contextCache.TryGetInstanceContext(instanceId, out result))
                {
                    lock (result.ThisLock)
                    {
                        if (!string.IsNullOrEmpty(channel.SessionId) && !result.IncomingChannels.Contains(channel))
                        {
                            result.IncomingChannels.Add(channel);
                        }
                    }
                    return result;
                }
            }
            return result;
        }

        public int GetReferenceCount(Guid instanceId)
        {
            return this.Cache.GetReferenceCount(instanceId);
        }

        public virtual void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
            if (instanceContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instanceContext");
            }
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (channel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channel");
            }

            Guid instanceId = GetInstanceIdFromMessage(message);
            DurableInstance durableInstance;
            if (instanceId == Guid.Empty) //Activation Request.
            {
                instanceId = Guid.NewGuid();
                durableInstance = this.OnCreateNewInstance(instanceId);
                message.Properties[DurableMessageDispatchInspector.NewDurableInstanceIdPropertyName] = instanceId;
            }
            else
            {
                durableInstance = this.OnGetExistingInstance(instanceId);
            }

            Fx.Assert(durableInstance != null, "Durable instance should never be null at this point.");
            durableInstance.Open();

            instanceContext.Extensions.Add(durableInstance);

            if (!string.IsNullOrEmpty(channel.SessionId))
            {
                instanceContext.IncomingChannels.Add(channel);
            }

            contextCache.AddInstanceContext(instanceId, instanceContext);

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string traceText = SR.GetString(SR.TraceCodeDICPInstanceContextCached, instanceId);
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.DICPInstanceContextCached, SR.GetString(SR.TraceCodeDICPInstanceContextCached), 
                    new StringTraceRecord("InstanceDetail", traceText),
                    this, null);
            }
        }

        public virtual bool IsIdle(InstanceContext instanceContext)
        {
            bool removed = false;

            if (instanceContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instanceContext");
            }

            DurableInstance durableInstance = instanceContext.Extensions.Find<DurableInstance>();

            if (durableInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.RequiredInstanceContextExtensionNotFound,
                    typeof(DurableInstance).Name)));
            }

            lock (instanceContext.ThisLock)
            {
                if (instanceContext.IncomingChannels.Count == 0)
                {
                    removed = contextCache.RemoveIfNotBusy(durableInstance.InstanceId, instanceContext);
                }
            }

            if (removed && DiagnosticUtility.ShouldTraceInformation)
            {
                string traceText = SR.GetString(SR.TraceCodeDICPInstanceContextRemovedFromCache, durableInstance.InstanceId);
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.DICPInstanceContextRemovedFromCache, SR.GetString(SR.TraceCodeDICPInstanceContextRemovedFromCache), 
                    new StringTraceRecord("InstanceDetail", traceText),
                    this, null);
            }
            return removed;
        }

        public virtual void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {

        }

        //Called by MessageInspector.BeforeReply
        internal void DecrementActivityCount(Guid instanceId)
        {
            contextCache.ReleaseReference(instanceId);
        }

        internal void UnbindAbortedInstance(InstanceContext instanceContext, Guid instanceId)
        {
            if (instanceContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instanceContext");
            }

            //We made our best effor to clean up the instancecontext out of our cache.
            //If another request already in middle of processing the request on InstanceContext
            //It will Fail with CommunicationException.
            this.contextCache.Remove(instanceId, instanceContext);
        }

        protected virtual Guid GetInstanceIdFromMessage(Message message)
        {
            if (!this.isPerCall)
            {
                ContextMessageProperty contextProperties = null;
                string instanceId = null;

                if (ContextMessageProperty.TryGet(message, out contextProperties))
                {
                    if (contextProperties.Context.TryGetValue(WellKnownContextProperties.InstanceId, out instanceId))
                    {
                        return Fx.CreateGuid(instanceId);
                    }
                }
            }
            return Guid.Empty;
        }

        protected abstract DurableInstance OnCreateNewInstance(Guid instanceId);
        protected abstract DurableInstance OnGetExistingInstance(Guid instanceId);

        //This class takes self contained lock, never calls out with lock taken.
        protected class ContextCache
        {
            Dictionary<Guid, ContextItem> contextCache;
            object lockObject = new object();

            public ContextCache()
            {
                contextCache = new Dictionary<Guid, ContextItem>();
            }

            public void AddInstanceContext(Guid instanceId, InstanceContext instanceContext)
            {
                ContextItem contextItem;
                int? referenceCount = null;

                lock (lockObject)
                {
                    if (!contextCache.TryGetValue(instanceId, out contextItem))
                    {
                        //This will be the case for activation request.
                        contextItem = new ContextItem(instanceId);
                        referenceCount = contextItem.AddReference();
                        contextCache.Add(instanceId, contextItem);
                    }
                }
                contextItem.InstanceContext = instanceContext;

                if (DiagnosticUtility.ShouldTraceInformation && referenceCount.HasValue)
                {
                    string traceText = SR2.GetString(SR2.DurableInstanceRefCountToInstanceContext, instanceId, referenceCount.Value);
                    TraceUtility.TraceEvent(TraceEventType.Information,
                        TraceCode.InstanceContextBoundToDurableInstance, SR.GetString(SR.TraceCodeInstanceContextBoundToDurableInstance), 
                        new StringTraceRecord("InstanceDetail", traceText),
                        this, null);
                }
            }

            public bool Contains(Guid instanceId, InstanceContext instanceContext)
            {
                ContextItem contextItem = null;

                lock (this.lockObject)
                {
                    if (contextCache.TryGetValue(instanceId, out contextItem))
                    {
                        return object.ReferenceEquals(contextItem.InstanceContext, instanceContext);
                    }
                    return false;
                }
            }

            public int GetReferenceCount(Guid instanceId)
            {
                int result = 0;

                lock (lockObject)
                {
                    ContextItem contextItem;
                    if (contextCache.TryGetValue(instanceId, out contextItem))
                    {
                        result = contextItem.ReferenceCount;
                    }
                }

                return result;
            }

            public void ReleaseReference(Guid instanceId)
            {
                int referenceCount = -1;
                ContextItem contextItem;

                lock (lockObject)
                {
                    if (contextCache.TryGetValue(instanceId, out contextItem))
                    {
                        referenceCount = contextItem.ReleaseReference();
                    }
                    else
                    {
                        Fx.Assert(false, "Cannot Release Reference of non exisiting items");
                    }
                }

                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    string traceText = SR2.GetString(SR2.DurableInstanceRefCountToInstanceContext, instanceId, referenceCount);
                    TraceUtility.TraceEvent(TraceEventType.Information,
                        TraceCode.InstanceContextDetachedFromDurableInstance, SR.GetString(SR.TraceCodeInstanceContextDetachedFromDurableInstance),
                        new StringTraceRecord("InstanceDetail", traceText),
                        this, null);
                }
            }

            public bool Remove(Guid instanceId, InstanceContext instanceContext)
            {
                lock (this.lockObject)
                {
                    ContextItem contextItem = null;
                    if (this.contextCache.TryGetValue(instanceId, out contextItem))
                    {
                        if (object.ReferenceEquals(instanceContext, contextItem.InstanceContext))
                        {
                            return this.contextCache.Remove(instanceId);
                        }
                    }
                    //InstanceContext is not in memory.
                    return false;
                }
            }

            public bool RemoveIfNotBusy(Guid instanceId, InstanceContext instanceContext)
            {
                lock (lockObject)
                {
                    ContextItem contextItem = null;
                    if (contextCache.TryGetValue(instanceId, out contextItem))
                    {
                        if (object.ReferenceEquals(contextItem.InstanceContext, instanceContext))
                        {
                            return (!contextItem.HasOutstandingReference)
                                && (contextCache.Remove(instanceId));
                        }
                    }
                    //InstanceContext is not in memory.
                    return true;
                }
            }

            //Helper method to call from GetExistingInstanceContext
            //returns true  : If InstanceContext is found in cache & guaranteed to stay in cache until ReleaseReference is called.
            //returns false : If InstanceContext is not found in cache;
            //               reference & slot is created for the ID;
            //               InitializeInstanceContext to call AddInstanceContext.
            public bool TryGetInstanceContext(Guid instanceId, out InstanceContext instanceContext)
            {
                ContextItem contextItem;
                instanceContext = null;
                int referenceCount = -1;

                try
                {
                    lock (lockObject)
                    {
                        if (!contextCache.TryGetValue(instanceId, out contextItem))
                        {
                            contextItem = new ContextItem(instanceId);
                            referenceCount = contextItem.AddReference();
                            contextCache.Add(instanceId, contextItem);
                            return false;
                        }
                        referenceCount = contextItem.AddReference();
                    }
                }
                finally
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        string traceText = SR2.GetString(SR2.DurableInstanceRefCountToInstanceContext, instanceId, referenceCount);
                        TraceUtility.TraceEvent(TraceEventType.Information,
                            TraceCode.InstanceContextBoundToDurableInstance, SR.GetString(SR.TraceCodeInstanceContextBoundToDurableInstance),
                            new StringTraceRecord("InstanceDetail", traceText),
                            this, null);
                    }
                }
                instanceContext = contextItem.InstanceContext;
                return true;
            }


            class ContextItem
            {
                InstanceContext context;
                Guid instanceId;
                object lockObject;
                int referenceCount;

                public ContextItem(Guid instanceId)
                {
                    lockObject = new object();
                    referenceCount = 0;
                    this.instanceId = instanceId;
                }

                public bool HasOutstandingReference
                {
                    get
                    {
                        return this.referenceCount > 0;
                    }
                }

                public InstanceContext InstanceContext
                {
                    get
                    {
                        if (this.context == null)
                        {
                            lock (this.lockObject)
                            {
                                if (this.context == null)
                                {
                                    Monitor.Wait(this.lockObject);
                                }
                            }
                        }
                        Fx.Assert(this.context != null, "Context cannot be null at this point");
                        return this.context;
                    }
                    set
                    {
                        if (value == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                        }
                        this.context = value;
                        lock (this.lockObject)
                        {
                            Monitor.PulseAll(this.lockObject);
                        }
                    }
                }

                public int ReferenceCount
                {
                    get
                    {
                        return this.referenceCount;
                    }
                }

                public int AddReference()
                {
                    //Called from higher locks taken
                    return ++this.referenceCount;
                }

                public int ReleaseReference()
                {
                    Fx.Assert(referenceCount > 0, "Reference count gone to negative");
                    //Called from higher locks taken
                    return --this.referenceCount;
                }
            }
        }
    }
}
