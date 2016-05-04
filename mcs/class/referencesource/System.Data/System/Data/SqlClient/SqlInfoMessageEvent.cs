//------------------------------------------------------------------------------
// <copyright file="SqlInfoMessageEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    using System;

    public sealed class SqlInfoMessageEventArgs : System.EventArgs {
        private SqlException exception;

        internal SqlInfoMessageEventArgs(SqlException exception) {
            this.exception = exception;
        }

        public SqlErrorCollection Errors {
            get { return exception.Errors;}
        }

        /*virtual protected*/private bool ShouldSerializeErrors() { // MDAC 65548
            return (null != exception) && (0 < exception.Errors.Count);
        }

        public string Message { // MDAC 68482
            get { return exception.Message; }
        }

        public string Source { // MDAC 68482
            get { return exception.Source;}
        }

        override public string ToString() { // MDAC 68482
            return Message;
        }
    }
}
