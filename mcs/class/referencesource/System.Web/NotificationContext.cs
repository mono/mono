//------------------------------------------------------------------------------
// <copyright file="NotificationContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {

    internal class NotificationContext {
        internal NotificationContext(int flags, bool isReEntry) {
            CurrentNotificationFlags = flags;
            IsReEntry = isReEntry;
        }
        internal bool IsPostNotification;
        internal RequestNotification CurrentNotification;
        internal int CurrentModuleIndex;
        internal int CurrentModuleEventIndex;
        internal int CurrentNotificationFlags;
        internal HttpAsyncResult AsyncResult;
        internal bool PendingAsyncCompletion;
        internal Exception Error;
        internal bool RequestCompleted;
        internal bool IsReEntry; // Currently, we only re-enter for SendResponse notifications
    }
}
