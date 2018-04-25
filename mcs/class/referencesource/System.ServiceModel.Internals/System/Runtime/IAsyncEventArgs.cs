// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime
{
    interface IAsyncEventArgs
    {
        object AsyncState { get; }

        Exception Exception { get; }
    }
}
