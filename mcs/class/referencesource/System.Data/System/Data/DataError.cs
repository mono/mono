//------------------------------------------------------------------------------
// <copyright file="DataError.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>               
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;

    /// <devdoc>
    /// <para>Represents an custom error that can be associated with a <see cref='System.Data.DataRow'/>.</para>
    /// </devdoc>
   internal sealed class DataError {
        private string rowError = String.Empty;

        // column-level errors
        private int count;
        private ColumnError[] errorList;
        internal const int initialCapacity = 1;

        internal DataError() {
        }

        internal DataError(string rowError) {
            SetText(rowError);
        }

        internal string Text {
            get {
                return rowError;
            }
            set {
                SetText(value);
            }
        }

        internal bool HasErrors {
            get {
                return(rowError.Length != 0 || count != 0);
            }
        }

        //
        // this method resets the error to the new value.
        //
        internal void SetColumnError(DataColumn column, string error) {
            Debug.Assert(column != null, "Invalid (null) argument");
            Debug.Assert(column.Table != null, "Invalid (loose) column");
            if (error == null || error.Length == 0) {
                // remove error from the collection
                Clear(column);
            }
            else {
                if (errorList == null) {
                    errorList = new ColumnError[initialCapacity];
                }
                int i = IndexOf(column);
                errorList[i].column = column;
                errorList[i].error = error;
                column.errors++;
                if (i == count)
                    count++;
            }
        }

        internal string GetColumnError(DataColumn column) {
            for (int i = 0; i < count; i++) {
                if (errorList[i].column == column) {
                    return errorList[i].error;
                }
            }
            return String.Empty;
        }

        internal void Clear(DataColumn column) {
            if (count == 0)
                return;

            for (int i = 0; i < count; i++) {
                if (errorList[i].column == column) {
                    System.Array.Copy(errorList, i+1, errorList, i, count-i-1);
                    count--;
                    column.errors--;
                    Debug.Assert(column.errors >= 0, "missing error counts");
                }
            }
        }

        internal void Clear() {
            for (int i = 0; i < count; i++) {
                errorList[i].column.errors--;
                Debug.Assert(errorList[i].column.errors >= 0, "missing error counts");
            }
            count = 0;
            rowError = String.Empty;
        }

        internal DataColumn[] GetColumnsInError() {
            DataColumn[] cols = new DataColumn[count];

            for (int i = 0; i < count; i++) {
                cols[i] = errorList[i].column;
            }
            return cols;
        }

        /// <devdoc>
        /// <para>Sets the error message for the <see cref='System.Data.DataError'/>.</para>
        /// </devdoc>
        private void SetText(string errorText) {
            if (null == errorText) {
                errorText = String.Empty;
            }
            rowError = errorText;
        }

        internal int IndexOf (DataColumn column) {
            // try to find the column
            for (int i = 0; i < count; i++) {
                if (errorList[i].column == column) {
                    return i;
                }
            }

            if (count >= errorList.Length) {
                int newCapacity = Math.Min(count*2, column.Table.Columns.Count);
                ColumnError[] biggerList = new ColumnError[newCapacity];
                System.Array.Copy(errorList, 0, biggerList, 0, count);
                errorList = biggerList;
            }
            return count;
        }

        internal struct ColumnError {
            internal DataColumn column;
            internal string error;
        };
    }
}
