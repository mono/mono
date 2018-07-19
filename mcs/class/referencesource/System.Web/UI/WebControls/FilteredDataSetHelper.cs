//------------------------------------------------------------------------------
// <copyright file="FilteredDataSetHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Web.Util;

    /// <devdoc>
    /// Helper class for SqlDataSource and ObjectDataSource.
    /// </devdoc>
    internal static class FilteredDataSetHelper {

        public static DataView CreateFilteredDataView(DataTable table, string sortExpression, string filterExpression, IDictionary filterParameters) {
            Debug.Assert(table != null, "Did not expect null table");
            Debug.Assert(sortExpression != null, "Did not expect null sort expression");
            Debug.Assert(filterExpression != null, "Did not expect null filter expression");

            DataView dv = new DataView(table);
            // Set sort expression
            if (!String.IsNullOrEmpty(sortExpression)) {
                dv.Sort = sortExpression;
            }

            // Set filter expression
            if (!String.IsNullOrEmpty(filterExpression)) {
                bool hasNulls = false;
                Debug.Assert(filterParameters != null, "Did not expect null filter parameters when a filter expression was set");
                object[] values = new object[filterParameters.Count];
                int index = 0;
                foreach (DictionaryEntry de in filterParameters) {
                    if (de.Value == null) {
                        hasNulls = true;
                        break;
                    }
                    values[index] = de.Value;
                    index++;
                }
                filterExpression = String.Format(CultureInfo.InvariantCulture, filterExpression, values);
                // Filter expression should only be applied if there were no null parameters
                if (!hasNulls) {
                    dv.RowFilter = filterExpression;
                }
            }

            return dv;
        }

        public static DataTable GetDataTable(Control owner, object dataObject) {
            DataSet dataSet = dataObject as DataSet;
            if (dataSet != null) {
                if (dataSet.Tables.Count == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.FilteredDataSetHelper_DataSetHasNoTables, owner.ID));
                }
                return dataSet.Tables[0];
            }
            else {
                return dataObject as DataTable;
            }
        }
    }
}

