//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    enum ListenerExceptionStatus
    {
        Success,
        PathTooLong,
        RegistrationQuotaExceeded,
        ProtocolUnsupported,
        ConflictingRegistration,
        FailedToListen,
        VersionUnsupported,
        InvalidArgument,
    }
}
