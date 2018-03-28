//------------------------------------------------------------------------------
// <copyright file="IPerfCounters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;

    // Provides an abstraction into the performance counter system.

    internal interface IPerfCounters {

        void IncrementCounter(AppPerfCounter counter);
        void IncrementCounter(AppPerfCounter counter, int value);

        void DecrementCounter(AppPerfCounter counter);

        void SetCounter(AppPerfCounter counter, int value);

    }
}
