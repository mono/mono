//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    interface ILockingQueue
    {
        void DeleteMessage(long lookupId, TimeSpan timeout);
        void UnlockMessage(long lookupId, TimeSpan timeout);
    }
}
