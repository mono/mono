//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    public interface IErrorHandler
    {
        void ProvideFault(Exception error, MessageVersion version, ref Message fault);
        bool HandleError(Exception error);
    }
}
