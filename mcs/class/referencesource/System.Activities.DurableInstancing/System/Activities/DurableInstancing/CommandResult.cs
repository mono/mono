//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    enum CommandResult
    {
        Success = 0,
        InstanceNotFound = 1,
        InstanceLockNotAcquired = 2,
        KeyAlreadyExists = 3,
        KeyNotFound = 4,
        InstanceAlreadyExists = 5,
        InstanceLockLost = 6,
        InstanceCompleted = 7,
        KeyDisassociated = 8,
        StaleInstanceVersion = 10,
        HostLockExpired = 11,
        HostLockNotFound = 12,
        CleanupInProgress = 13,
        InstanceAlreadyLockedToOwner = 14,
        IdentityNotFound = 15,
        Unknown = 99
    };
}
