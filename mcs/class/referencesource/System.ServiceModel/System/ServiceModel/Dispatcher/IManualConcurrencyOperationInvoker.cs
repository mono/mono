//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;

    interface IManualConcurrencyOperationInvoker : IOperationInvoker
    {
        bool OwnsFormatter { get; }

        object Invoke(object instance, object[] inputs, IInvokeReceivedNotification notification, out object[] outputs);

        IAsyncResult InvokeBegin(object instance, object[] inputs, IInvokeReceivedNotification notification, AsyncCallback callback, object state);
   }
}
