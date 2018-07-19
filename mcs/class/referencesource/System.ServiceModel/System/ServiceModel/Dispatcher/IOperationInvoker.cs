//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;

    public interface IOperationInvoker
    {
        bool IsSynchronous { get; }

        object[] AllocateInputs();

        object Invoke(object instance, object[] inputs, out object[] outputs);

        IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state);

        object InvokeEnd(object instance, out object[] outputs, IAsyncResult result);
    }
}
