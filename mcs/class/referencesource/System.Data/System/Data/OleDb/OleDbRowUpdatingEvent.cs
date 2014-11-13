//------------------------------------------------------------------------------
// <copyright file="OleDbRowUpdatingEvent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.OleDb {

    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class OleDbRowUpdatingEventArgs : RowUpdatingEventArgs {

        public OleDbRowUpdatingEventArgs(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        : base(dataRow, command, statementType, tableMapping) {
        }


        new public OleDbCommand Command {
            get { return (base.Command as OleDbCommand); }
            set { base.Command = value; }
        }

        override protected IDbCommand BaseCommand {
            get { return base.BaseCommand; }
            set { base.BaseCommand = (value as OleDbCommand); }
        }
    }
}
