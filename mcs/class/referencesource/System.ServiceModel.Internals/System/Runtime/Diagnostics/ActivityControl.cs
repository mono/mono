// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime.Diagnostics
{
    /// <summary>
    /// enum ActivityControl
    /// </summary>
    internal enum ActivityControl : uint
    {
        /// <summary>
        /// Get the activity Id from the thread
        /// </summary>
        EVENT_ACTIVITY_CTRL_GET_ID = 1,

        /// <summary>
        /// Set the activity Id to the thread
        /// </summary>
        EVENT_ACTIVITY_CTRL_SET_ID = 2,

        /// <summary>
        /// Create the activity Id
        /// </summary>
        EVENT_ACTIVITY_CTRL_CREATE_ID = 3,

        /// <summary>
        /// Get the activity Id from the thread and set it
        /// </summary>
        EVENT_ACTIVITY_CTRL_GET_SET_ID = 4,

        /// <summary>
        /// Create an activity Id and set it to the thread
        /// </summary>
        EVENT_ACTIVITY_CTRL_CREATE_SET_ID = 5
    }
}
