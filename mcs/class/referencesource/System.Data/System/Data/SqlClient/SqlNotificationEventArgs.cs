//------------------------------------------------------------------------------
// <copyright file="SqlNotificationEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    using System;
    using System.ComponentModel;
    using System.Collections;
    using System.Data;

 public class SqlNotificationEventArgs : EventArgs {
        private SqlNotificationType _type;
        private SqlNotificationInfo _info;
        private SqlNotificationSource _source;

        public SqlNotificationEventArgs(SqlNotificationType type, SqlNotificationInfo info, SqlNotificationSource source) {
            _info = info;
            _source = source;
            _type = type;
        }

        public SqlNotificationType Type {
            get {
                return _type;
            }
        }

        public SqlNotificationInfo Info {
            get {
                return _info;
            }
        }

        public SqlNotificationSource Source {
            get {
                return _source;
            }
        }

        internal static SqlNotificationEventArgs NotifyError = new SqlNotificationEventArgs(SqlNotificationType.Subscribe, SqlNotificationInfo.Error, SqlNotificationSource.Object);
    }
}


