//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    interface IMsmqMessagePool : IDisposable
    {
        MsmqInputMessage TakeMessage();
        void ReturnMessage(MsmqInputMessage message);
    }

    sealed class MsmqInputMessagePool
        : SynchronizedDisposablePool<MsmqInputMessage>, IMsmqMessagePool
    {
        int maxPoolSize;

        internal MsmqInputMessagePool(int maxPoolSize)
            : base(maxPoolSize)
        {
            this.maxPoolSize = maxPoolSize;
        }

        MsmqInputMessage IMsmqMessagePool.TakeMessage()
        {
            MsmqInputMessage message = this.Take();
            if (null == message)
                message = new MsmqInputMessage();
            return message;
        }

        void IMsmqMessagePool.ReturnMessage(MsmqInputMessage message)
        {
            if (!this.Return(message))
            {
                MsmqDiagnostics.PoolFull(this.maxPoolSize);
                message.Dispose();
            }
        }
    }
}
