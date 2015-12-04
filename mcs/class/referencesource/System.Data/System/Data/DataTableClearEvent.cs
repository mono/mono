//------------------------------------------------------------------------------
// <copyright file="DataTableClearEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;

    public sealed class DataTableClearEventArgs : EventArgs {

        private readonly DataTable dataTable;

        public DataTableClearEventArgs(DataTable dataTable) {
            this.dataTable = dataTable;
        }

        public DataTable Table{
            get {
                return dataTable;
            }
        }
        
        public string TableName{
            get {
                return dataTable.TableName;
            }
        }
        
        public string TableNamespace{
            get {
                return dataTable.Namespace;
            }
        }

    }
}
