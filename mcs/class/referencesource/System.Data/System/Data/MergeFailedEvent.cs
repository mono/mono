//------------------------------------------------------------------------------
// <copyright file="MergeFailedEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class MergeFailedEventArgs : EventArgs {
        private DataTable table;
        private string conflict;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public MergeFailedEventArgs(DataTable table, string conflict) {
            this.table = table;
            this.conflict = conflict;
        }

        /// <devdoc>
        /// <para>Gets the name of the <see cref='System.Data.DataTable'/>.</para>
        /// </devdoc>
        public DataTable Table {
            get { return table; }
        }

        /// <devdoc>
        ///    <para>Gets a description of the merge conflict.</para>
        /// </devdoc>
        public string Conflict {
            get { return conflict; }
        }
    }
}
