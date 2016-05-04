//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    class SecurityListenerSettingsLifetimeManager
    {
        SecurityProtocolFactory securityProtocolFactory;
        SecuritySessionServerSettings sessionSettings;
        bool sessionMode;
        IChannelListener innerListener;
        int referenceCount;

        public SecurityListenerSettingsLifetimeManager(SecurityProtocolFactory securityProtocolFactory, SecuritySessionServerSettings sessionSettings, bool sessionMode, IChannelListener innerListener)
        {
            this.securityProtocolFactory = securityProtocolFactory;
            this.sessionSettings = sessionSettings;
            this.sessionMode = sessionMode;
            this.innerListener = innerListener;
            // have a reference right from the start so that the state can be aborted before open
            referenceCount = 1;
        }

        public void Abort()
        {
            if (Interlocked.Decrement(ref this.referenceCount) == 0)
            {
                AbortCore();
            }
        }

        public void AddReference()
        {
            Interlocked.Increment(ref this.referenceCount);
        }

        public void Open(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.securityProtocolFactory != null)
            {
                this.securityProtocolFactory.Open(false, timeoutHelper.RemainingTime());
            }
            if (this.sessionMode && this.sessionSettings != null)
            {
                this.sessionSettings.Open(timeoutHelper.RemainingTime());
            } 

            this.innerListener.Open(timeoutHelper.RemainingTime());

            this.SetBufferManager();        
        }

        void SetBufferManager()
        {
            ITransportFactorySettings transportSettings = this.innerListener.GetProperty<ITransportFactorySettings>();
            if (transportSettings == null)
                return;

            BufferManager bufferManager = transportSettings.BufferManager;
            if (bufferManager == null)
                return;

            if (this.securityProtocolFactory != null)
            {
                this.securityProtocolFactory.StreamBufferManager = bufferManager;
            }

            if (this.sessionMode && this.sessionSettings != null && this.sessionSettings.SessionProtocolFactory != null)
            {
                this.sessionSettings.SessionProtocolFactory.StreamBufferManager = bufferManager;
            }
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            List<OperationWithTimeoutBeginCallback> beginOperations = new List<OperationWithTimeoutBeginCallback>(3);
            List<OperationEndCallback> endOperations = new List<OperationEndCallback>(3);
            if (this.securityProtocolFactory != null)
            {
                beginOperations.Add(new OperationWithTimeoutBeginCallback(this.BeginOpenSecurityProtocolFactory));
                endOperations.Add(new OperationEndCallback(this.EndOpenSecurityProtocolFactory));
            }
            if (this.sessionMode && this.sessionSettings != null)
            {
                beginOperations.Add(new OperationWithTimeoutBeginCallback(this.sessionSettings.BeginOpen));
                endOperations.Add(new OperationEndCallback(this.sessionSettings.EndOpen));
            }
            beginOperations.Add(new OperationWithTimeoutBeginCallback(this.innerListener.BeginOpen));
            endOperations.Add(new OperationEndCallback(this.innerListener.EndOpen));

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations.ToArray(), endOperations.ToArray(), callback, state);
        }

        public void EndOpen(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
            this.SetBufferManager();
        }

        IAsyncResult BeginOpenSecurityProtocolFactory(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.securityProtocolFactory.BeginOpen(false, timeout, callback, state);
        }

        void EndOpenSecurityProtocolFactory(IAsyncResult result)
        {
            this.securityProtocolFactory.EndOpen(result);
        }

        public void Close(TimeSpan timeout)
        {
            if (Interlocked.Decrement(ref this.referenceCount) == 0)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                bool throwing = true;
                try
                {
                    if (this.securityProtocolFactory != null)
                    {
                        this.securityProtocolFactory.Close(false, timeoutHelper.RemainingTime());
                    }
                    if (sessionMode && sessionSettings != null)
                    {
                        this.sessionSettings.Close(timeoutHelper.RemainingTime());
                    }
                    this.innerListener.Close(timeoutHelper.RemainingTime());
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        AbortCore();
                    }
                }
            }
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (Interlocked.Decrement(ref this.referenceCount) == 0)
            {
                bool throwing = true;
                try
                {
                    List<OperationWithTimeoutBeginCallback> beginOperations = new List<OperationWithTimeoutBeginCallback>(3);
                    List<OperationEndCallback> endOperations = new List<OperationEndCallback>(3);
                    if (this.securityProtocolFactory != null)
                    {
                        beginOperations.Add(new OperationWithTimeoutBeginCallback(this.securityProtocolFactory.BeginClose));
                        endOperations.Add(new OperationEndCallback(this.securityProtocolFactory.EndClose));
                    }
                    if (this.sessionMode && this.sessionSettings != null)
                    {
                        beginOperations.Add(new OperationWithTimeoutBeginCallback(this.sessionSettings.BeginClose));
                        endOperations.Add(new OperationEndCallback(this.sessionSettings.EndClose));
                    }
                    beginOperations.Add(new OperationWithTimeoutBeginCallback(this.innerListener.BeginClose));
                    endOperations.Add(new OperationEndCallback(this.innerListener.EndClose));
                    IAsyncResult result = OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations.ToArray(), endOperations.ToArray(), callback, state);
                    throwing = false;
                    return result;
                }
                finally
                {
                    if (throwing)
                    {
                        AbortCore();
                    }
                }
            }
            else
            {
                return new DummyCloseAsyncResult(callback, state);
            }
        }

        public void EndClose(IAsyncResult result)
        {
            if (result is DummyCloseAsyncResult)
            {
                DummyCloseAsyncResult.End(result);
            }
            else
            {
                bool throwing = true;
                try
                {
                    OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        AbortCore();
                    }
                }
            }
        }

        void AbortCore()
        {
            if (this.securityProtocolFactory != null)
            {
                this.securityProtocolFactory.Close(true, TimeSpan.Zero);
            }
            if (sessionMode && this.sessionSettings != null)
            {
                this.sessionSettings.Abort();
            }
            this.innerListener.Abort();
        }

        class DummyCloseAsyncResult : CompletedAsyncResult
        {
            public DummyCloseAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }

            new public static void End(IAsyncResult result)
            {
                AsyncResult.End<DummyCloseAsyncResult>(result);
            }
        }
    }
}
