//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class SingletonInstanceContextProvider : InstanceContextProviderBase
    {
        InstanceContext singleton;
        object thisLock;

        internal SingletonInstanceContextProvider(DispatchRuntime dispatchRuntime)
            : base(dispatchRuntime)
        {
            this.thisLock = new Object();
        }

        internal InstanceContext SingletonInstance
        {
            get
            {
                if (this.singleton == null)
                {
                    lock (this.thisLock)
                    {
                        if (this.singleton == null)
                        {
                            InstanceContext instanceContext = this.DispatchRuntime.SingletonInstanceContext;

                            if (instanceContext == null)
                            {
                                instanceContext = new InstanceContext(this.DispatchRuntime.ChannelDispatcher.Host, false);
                                instanceContext.Open();
                            }
                            else if (instanceContext.State != CommunicationState.Opened)
                            {
                                // we need to lock against the instance context for open since two different endpoints could
                                // share the same instance context, but different providers. So the provider lock does not guard
                                // the open process
                                lock (instanceContext.ThisLock)
                                {
                                    if (instanceContext.State != CommunicationState.Opened)
                                    {
                                        instanceContext.Open();
                                    }
                                }
                            }

                            //Set the IsUsercreated flag to false for singleton mode even in cases when users create their own runtime.
                            instanceContext.IsUserCreated = false;

                            //Delay assigning the potentially newly created InstanceContext (till after its opened) to this.Singleton 
                            //to ensure that it is opened only once.
                            this.singleton = instanceContext;
                        }
                    }
                }
                return this.singleton;
            }
        }

        #region IInstanceContextProvider Members

        public override InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            ServiceChannel serviceChannel = this.GetServiceChannelFromProxy(channel);
            if (serviceChannel != null && serviceChannel.HasSession)
            {
                this.SingletonInstance.BindIncomingChannel(serviceChannel);
            }
            return this.SingletonInstance;
        }

        public override void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
            //no-op
        }

        public override bool IsIdle(InstanceContext instanceContext)
        {
            //By default return false
            return false;
        }

        public override void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
            //no-op
        }

        #endregion
    }
}
