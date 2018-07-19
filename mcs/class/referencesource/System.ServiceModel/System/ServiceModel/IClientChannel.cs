//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Net.Security;
    using System.ServiceModel.Security.Tokens;

    public interface IClientChannel : IContextChannel, IDisposable
    {
        bool AllowInitializationUI { get; set; }
        bool DidInteractiveInitialization { get; }
        Uri Via { get; }

        event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived;

        void DisplayInitializationUI();
        IAsyncResult BeginDisplayInitializationUI(AsyncCallback callback, object state);
        void EndDisplayInitializationUI(IAsyncResult result);
    }
}
