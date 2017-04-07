//------------------------------------------------------------------------------
// <copyright file="OdbcRowUpdatingEvent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Common;       //DbDataAdapter
using System.ComponentModel;    //Component

namespace System.Data.Odbc
{
    /////////////////////////////////////////////////////////////////////////
    // Event Handlers
    //
    /////////////////////////////////////////////////////////////////////////
    public delegate void OdbcRowUpdatingEventHandler(object sender, OdbcRowUpdatingEventArgs e);

    public delegate void OdbcRowUpdatedEventHandler(object sender, OdbcRowUpdatedEventArgs e);

    /////////////////////////////////////////////////////////////////////////
    // OdbcRowUpdatingEventArgs
    //
    /////////////////////////////////////////////////////////////////////////
    public sealed class OdbcRowUpdatingEventArgs : RowUpdatingEventArgs
    {
        public OdbcRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        : base(row, command, statementType, tableMapping)
        {
            }

        new public OdbcCommand Command {
            get { return (base.Command as OdbcCommand); }
            set {
                base.Command = value; }
        }

        override protected IDbCommand BaseCommand {
            get { return base.BaseCommand; }
            set { base.BaseCommand = (value as OdbcCommand); }
        }
    }

    /////////////////////////////////////////////////////////////////////////
    // OdbcRowUpdatedEventArgs
    //
    /////////////////////////////////////////////////////////////////////////
    public sealed class OdbcRowUpdatedEventArgs : RowUpdatedEventArgs
    {
        public OdbcRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        : base(row, command, statementType, tableMapping)
        {
        }

        new public OdbcCommand Command {
            get {   return(OdbcCommand) base.Command;   }
        }
    }
}
