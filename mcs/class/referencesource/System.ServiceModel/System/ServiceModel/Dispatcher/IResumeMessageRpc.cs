//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Threading;

    interface IResumeMessageRpc
    {
        InstanceContext GetMessageInstanceContext();

        void Resume();
        void Resume(out bool alreadyResumedNoLock);
        void Resume(IAsyncResult result);
        void Resume(object instance);
        void SignalConditionalResume(IAsyncResult result);
    }
}
