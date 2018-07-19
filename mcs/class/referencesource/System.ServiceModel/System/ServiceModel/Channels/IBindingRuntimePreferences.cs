//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    // This is an optional interface that a binding can implement to specify preferences about 
    // how it should be used by a runtime.
    public interface IBindingRuntimePreferences
    {
        bool ReceiveSynchronously { get; }
    }
}

