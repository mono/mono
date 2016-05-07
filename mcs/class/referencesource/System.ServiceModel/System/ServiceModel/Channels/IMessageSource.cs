//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Threading;

    enum AsyncReceiveResult
    {
        Completed,
        Pending,
    }

    interface IMessageSource
    {
        AsyncReceiveResult BeginReceive(TimeSpan timeout, WaitCallback callback, object state);
        Message EndReceive();
        Message Receive(TimeSpan timeout);

        AsyncReceiveResult BeginWaitForMessage(TimeSpan timeout, WaitCallback callback, object state);
        bool EndWaitForMessage();
        bool WaitForMessage(TimeSpan timeout);
    }
}
