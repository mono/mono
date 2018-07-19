//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System.ServiceModel.Channels;

    sealed class MsmqIntegrationMessagePool
        : SynchronizedDisposablePool<MsmqIntegrationInputMessage>, IMsmqMessagePool
    {
        int maxPoolSize;

        internal MsmqIntegrationMessagePool(int maxPoolSize)
            : base(maxPoolSize)
        {
            this.maxPoolSize = maxPoolSize;
        }

        MsmqInputMessage IMsmqMessagePool.TakeMessage()
        {
            MsmqIntegrationInputMessage message = this.Take();
            if (null == message)
                message = new MsmqIntegrationInputMessage();
            return message;
        }

        void IMsmqMessagePool.ReturnMessage(MsmqInputMessage message)
        {
            if (! this.Return(message as MsmqIntegrationInputMessage))
            {
                MsmqDiagnostics.PoolFull(this.maxPoolSize);
                message.Dispose();
            }
        }
    }
}
