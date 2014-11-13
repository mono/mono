//------------------------------------------------------------------------------
// <copyright file="SqlDataSourceCommandEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;

    public class SqlDataSourceCommandEventArgs : CancelEventArgs {

        private DbCommand _command;



        public SqlDataSourceCommandEventArgs(DbCommand command) : base() {
            _command = command;
        }



        public DbCommand Command {
            get {
                return _command;
            }
        }
    }
}

