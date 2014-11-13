//------------------------------------------------------------------------------
// <copyright file="SqlDataSourceSelectingEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;

    public class SqlDataSourceSelectingEventArgs : SqlDataSourceCommandEventArgs {

        private DataSourceSelectArguments _arguments;
        /*private bool _executingSelectCount;*/



        public SqlDataSourceSelectingEventArgs(DbCommand command, DataSourceSelectArguments arguments /*, bool executingSelectCount*/) : base(command) {
            _arguments = arguments;
            //_executingSelectCount = executingSelectCount;
        }



        public DataSourceSelectArguments Arguments {
            get {
                return _arguments;
            }
        }


        /*public bool ExecutingSelectCount {
            get {
                return _executingSelectCount;
            }
        }*/
    }
}

