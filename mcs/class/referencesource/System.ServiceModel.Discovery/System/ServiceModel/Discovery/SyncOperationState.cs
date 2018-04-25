//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.ComponentModel;
    using System.Runtime;
    using System.Threading;

    class SyncOperationState
    {
        AsyncCompletedEventArgs eventArgs;

        [Fx.Tag.SynchronizationObject()]
        ManualResetEvent waitEvent;

        public SyncOperationState()
        {
            this.waitEvent = new ManualResetEvent(false);
            this.eventArgs = null;
        }

        public AsyncCompletedEventArgs EventArgs
        {
            get
            {
                return this.eventArgs;
            }
            set
            {
                this.eventArgs = value;
            }
        }

        public ManualResetEvent WaitEvent
        {
            get
            {
                return this.waitEvent;
            }
        }
    }
}
