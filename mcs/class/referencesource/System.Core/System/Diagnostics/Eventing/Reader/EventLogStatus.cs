// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: EventLogStatus
**
** Purpose: 
** This public class describes the status of a particular
** log with respect to an instantiated EventLogReader.  
** Since it is possible to instantiate an EventLogReader
** with a query containing multiple logs and the reader can 
** be configured to tolerate errors in attaching to those logs,
** this class allows the user to determine exactly what the status
** of those logs is.
============================================================*/
using System;

namespace System.Diagnostics.Eventing.Reader{

    /// <summary>
    /// Describes the status of a particular log with respect to 
    /// an instantiated EventLogReader.  Since it is possible to 
    /// instantiate an EventLogReader with a query containing 
    /// multiple logs and the reader can be configured to tolerate 
    /// errors in attaching to those logs, this class allows the 
    /// user to determine exactly what the status of those logs is.
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class EventLogStatus {
        private string channelName;
        private int win32ErrorCode;

        internal EventLogStatus(string channelName, int win32ErrorCode) {
            this.channelName = channelName;
            this.win32ErrorCode = win32ErrorCode;
        }

        public string LogName {
            get { return this.channelName; }
        }

        public int StatusCode {
            get { return this.win32ErrorCode; }
        }
    }

}
