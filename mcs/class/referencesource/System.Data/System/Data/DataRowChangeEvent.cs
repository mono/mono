//------------------------------------------------------------------------------
// <copyright file="DataRowChangeEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;

    public class DataRowChangeEventArgs : EventArgs {

        private DataRow row;
        private DataRowAction action;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.DataRowChangeEventArgs'/> class.
        ///    </para>
        /// </devdoc>
        public DataRowChangeEventArgs(DataRow row, DataRowAction action) {
            this.row = row;
            this.action = action;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the row upon which an action has occurred.
        ///    </para>
        /// </devdoc>
        public DataRow Row {
            get {
                return row;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the action that has occurred on a <see cref='System.Data.DataRow'/>.
        ///    </para>
        /// </devdoc>
        public DataRowAction Action {
            get {
                return action;
            }
        }
    }
}
