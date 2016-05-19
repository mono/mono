//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    interface IAspNetMessageProperty
    {
        Uri OriginalRequestUri { get; }
        IDisposable ApplyIntegrationContext();
        IDisposable Impersonate();
        void Close();
    }
}
