//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Persistence;

    abstract class DurableInstance : CommunicationObject, IExtension<InstanceContext>
    {
        DurableInstanceContextProvider instanceContextProvider;
        Guid instanceId;

        protected DurableInstance(DurableInstanceContextProvider instanceContextProvider, Guid instanceId)
        {
            if (instanceContextProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instanceContextProvider");
            }

            this.instanceId = instanceId;
            this.instanceContextProvider = instanceContextProvider;
        }

        public Guid InstanceId
        {
            get
            {
                return this.instanceId;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return PersistenceProvider.DefaultOpenClosePersistenceTimout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return PersistenceProvider.DefaultOpenClosePersistenceTimout; }
        }

        public void DecrementActivityCount()
        {
            instanceContextProvider.DecrementActivityCount(this.instanceId);
        }

        void IExtension<InstanceContext>.Attach(InstanceContext owner)
        {
        }

        void IExtension<InstanceContext>.Detach(InstanceContext owner)
        {
        }

        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }
    }
}
