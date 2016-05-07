// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime
{
    enum AsyncCompletionResult
    {
        /// <summary>
        /// Inidicates that the operation has been queued for completion.
        /// </summary>
        Queued,

        /// <summary>
        /// Indicates the operation has completed.
        /// </summary>
        Completed,
    }
}
