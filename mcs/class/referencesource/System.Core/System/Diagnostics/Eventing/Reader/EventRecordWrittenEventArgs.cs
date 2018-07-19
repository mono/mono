// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: EventRecordWrittenEventArgs
**
** Purpose: 
** The EventArgs class for an EventLogWatcher notification.
**
============================================================*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.Diagnostics.Eventing.Reader {

    /// <summary>
    /// the custom event handler args.
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class EventRecordWrittenEventArgs : EventArgs {

        private EventRecord record;
        private Exception exception;

        internal EventRecordWrittenEventArgs(EventLogRecord record) { this.record = record; }
        internal EventRecordWrittenEventArgs(Exception exception) { this.exception = exception; }
 
        /// <summary>
        /// The EventRecord being notified.  
        /// NOTE: If non null, then caller is required to call Dispose().
        /// </summary>
        public EventRecord EventRecord { 
            get { return this.record; } 
        }

        /// <summary>
        /// If any error occured during subscription, this will be non-null.
        /// After a notification containing an exception, no more notifications will
        /// be made for this subscription.
        /// </summary>
        public Exception EventException {
            get{ return this.exception; }
        }
    }

}
