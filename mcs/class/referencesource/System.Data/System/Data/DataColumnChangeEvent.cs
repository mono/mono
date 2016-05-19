//------------------------------------------------------------------------------
// <copyright file="DataColumnChangeEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    /// <devdoc>
    ///    <para>
    ///       Provides data for the <see cref='System.Data.DataTable.ColumnChanging'/> event.
    ///    </para>
    /// </devdoc>
    public class DataColumnChangeEventArgs : EventArgs {

        private readonly DataRow _row;
        private DataColumn _column;
        private object _proposedValue;

        internal DataColumnChangeEventArgs(DataRow row) {
            _row = row;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.DataColumnChangeEventArgs'/> class.
        ///    </para>
        /// </devdoc>
        public DataColumnChangeEventArgs(DataRow row, DataColumn column, object value) {
            _row = row;
            _column = column;
            _proposedValue = value;
        }

        /// <devdoc>
        ///    <para>Gets the column whose value is changing.</para>
        /// </devdoc>
        public DataColumn Column {
            get {
                return _column;
            }
        }

        /// <devdoc>
        ///    <para>Gets the row whose value is changing.</para>
        /// </devdoc>
        public DataRow Row {
            get {
                return _row;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the proposed value.</para>
        /// </devdoc>
        public object ProposedValue {
            get {
                return _proposedValue;
            }
            set {
                _proposedValue = value;
            }
        }
        
        internal void InitializeColumnChangeEvent(DataColumn column, object value) {            
            _column = column;
            _proposedValue = value;
        }
    }
}
