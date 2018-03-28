//------------------------------------------------------------------------------
// <copyright file="IRequestCompletedNotifier.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;

    // Abstraction that allows test code to see if a request has been completed

    internal interface IRequestCompletedNotifier {

        bool IsRequestCompleted { get; }

    }
}
