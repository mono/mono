//------------------------------------------------------------------------------
// <copyright file="DataTableNewRowEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;

    public sealed class DataTableNewRowEventArgs : EventArgs {

        private readonly DataRow dataRow;

        public DataTableNewRowEventArgs(DataRow dataRow) {
            this.dataRow = dataRow;
        }

        public DataRow Row{
            get {
                return dataRow;
            }
        }
    }
}
