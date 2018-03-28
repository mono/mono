//------------------------------------------------------------------------------
// <copyright file="IPageAsyncTask.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    // Represents a testable (mockable) PageAsyncTask

    internal interface IPageAsyncTask {

        Task ExecuteAsync(object sender, EventArgs e, CancellationToken cancellationToken);

    }
}
