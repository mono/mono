//------------------------------------------------------------------------------
// <copyright file="OleDbInfoMessageEvent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.OleDb {

    using System;
    using System.Diagnostics;

    public sealed class OleDbInfoMessageEventArgs : System.EventArgs {
        readonly private OleDbException exception;

        internal OleDbInfoMessageEventArgs(OleDbException exception) {
            Debug.Assert(null != exception, "OleDbInfoMessageEventArgs without OleDbException"); 
            this.exception = exception;
        }

        public int ErrorCode {
            get {
                return this.exception.ErrorCode;
            }
        }

        public OleDbErrorCollection Errors {
            get {
                return this.exception.Errors;
            }
        }

        internal bool ShouldSerializeErrors() { // MDAC 65548
            return this.exception.ShouldSerializeErrors();
        }


        public string Message {
            get {
                return this.exception.Message;
            }
        }

        public string Source {
            get {
                return this.exception.Source;
            }
        }

        override public string ToString() {
            return Message;
        }
    }
}
