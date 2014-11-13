//------------------------------------------------------------------------------
// <copyright file="OleDbRowUpdatedEvent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.OleDb {

    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class OleDbRowUpdatedEventArgs : RowUpdatedEventArgs {

        public OleDbRowUpdatedEventArgs(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        : base(dataRow, command, statementType, tableMapping) {
        }

        new public OleDbCommand Command {
            get {
                return(OleDbCommand) base.Command;
            }
        }
    }
}
