//------------------------------------------------------------------------------
// <copyright file="SqlNotificationRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="true">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Sql {

    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    
//      [System.ComponentModel.TypeConverterAttribute(typeof(System.Data.Sql.SqlNotificationRequest.SqlNotificationRequestConverter))]
    public sealed class SqlNotificationRequest {
        private string _userData;
        private string _options;
        private int    _timeout;

        public SqlNotificationRequest() 
                : this(null, null, SqlClient.SQL.SqlDependencyTimeoutDefault) {}

        public SqlNotificationRequest(string userData, string options, int timeout) {
            UserData = userData;
            Timeout  = timeout;    
            Options  = options;
        }

        public string Options {
            get {
                return _options;
            }
            set {
                if ((null != value) && (UInt16.MaxValue < value.Length)) {
                    throw ADP.ArgumentOutOfRange(String.Empty, ADP.ParameterService);
                }
                _options = value;
            }
        }

        public int Timeout {
            get {
                return _timeout;
            }
            set {
                if (0 > value) {
                    throw ADP.ArgumentOutOfRange(String.Empty, ADP.ParameterTimeout);
                }
                _timeout = value;
            }
        }

        public string UserData {
            get {
                return _userData;
            }
            set {
                if ((null != value) && (UInt16.MaxValue < value.Length)) {
                    throw ADP.ArgumentOutOfRange(String.Empty, ADP.ParameterUserData);
                }
                _userData = value;
            }
        }        
    }
}

