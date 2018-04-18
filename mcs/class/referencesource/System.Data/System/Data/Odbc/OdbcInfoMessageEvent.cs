//------------------------------------------------------------------------------
// <copyright file="OdbcInfoMessageEvent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

using System;
using System.Data;
using System.Text;

namespace System.Data.Odbc {

    public delegate void OdbcInfoMessageEventHandler(object sender, OdbcInfoMessageEventArgs e);

    public sealed class OdbcInfoMessageEventArgs : System.EventArgs {
        private OdbcErrorCollection _errors;

        internal OdbcInfoMessageEventArgs(OdbcErrorCollection errors) {
            _errors = errors;
        }

        public OdbcErrorCollection Errors {
            get { return _errors; }
        }

        public string Message { // MDAC 84407
            get {
                StringBuilder builder = new StringBuilder();
                foreach(OdbcError error in Errors) {
                    if (0 < builder.Length) { builder.Append(Environment.NewLine); }
                    builder.Append(error.Message);
                }
                return builder.ToString();
            }
        }

        public override string ToString() {
            // MDAC 84407
            return Message;
            }
    }
}
