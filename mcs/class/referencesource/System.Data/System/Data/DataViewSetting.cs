//------------------------------------------------------------------------------
// <copyright file="DataViewSetting.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.ComponentModel;

    [
    TypeConverter((typeof(ExpandableObjectConverter))),
    ]
    public class DataViewSetting {
        DataViewManager dataViewManager;
        DataTable       table;
        string sort      = "";
        string rowFilter = "";
        DataViewRowState rowStateFilter = DataViewRowState.CurrentRows;
        bool applyDefaultSort = false;

        internal DataViewSetting() {}

        internal DataViewSetting(string sort, string rowFilter, DataViewRowState rowStateFilter) {
            this.sort = sort;
            this.rowFilter = rowFilter;
            this.rowStateFilter = rowStateFilter; 
        }

        public bool ApplyDefaultSort {
            get {
                return applyDefaultSort;
            }
            set {
                if (applyDefaultSort != value) {
                    applyDefaultSort = value;
                }
            }
        }

        [Browsable(false)]
        public DataViewManager DataViewManager {
            get {
                return dataViewManager;
            }
        }

        internal void SetDataViewManager(DataViewManager dataViewManager) {
            if(this.dataViewManager != dataViewManager) {
                if(this.dataViewManager != null) {
                    // throw exception here;
                }
                this.dataViewManager = dataViewManager;
            }
        }

        [Browsable(false)]
        public DataTable Table {
            get {
                return table;
            }
        }

        internal void SetDataTable(DataTable table) {
            if(this.table != table) {
                if(this.table != null) {
                    // throw exception here;
                }
                this.table = table;
            }
        }

        public string RowFilter {
            get {
                return rowFilter;
            }
            set {
                if (value == null)
                    value = "";

                if (this.rowFilter != value) {
                    this.rowFilter = value;
                }
            }
        }

        public DataViewRowState RowStateFilter {
            get {
                return rowStateFilter;
            }
            set {
                if (this.rowStateFilter != value) {
                    this.rowStateFilter = value;
                }
            }
        }

        public string Sort {
            get {
                return sort;
            }
            set {
                if (value == null)
                    value = "";

                if (this.sort != value) {
                    this.sort = value;
                }
            }
        }
    }
}
