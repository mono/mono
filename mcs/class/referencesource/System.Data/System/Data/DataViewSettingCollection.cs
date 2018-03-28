//------------------------------------------------------------------------------
// <copyright file="DataViewSettingCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Collections;

    [
    Editor("Microsoft.VSDesigner.Data.Design.DataViewSettingsCollectionEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    ]
    public class DataViewSettingCollection : ICollection {
        private readonly DataViewManager dataViewManager;
        private readonly Hashtable list = new Hashtable();

        internal DataViewSettingCollection(DataViewManager dataViewManager) {
            if (dataViewManager == null) {
                throw ExceptionBuilder.ArgumentNull("dataViewManager");
            }
            this.dataViewManager = dataViewManager;
        }

        public virtual DataViewSetting this[DataTable table] {
            get {
                if (table == null) {
                    throw ExceptionBuilder.ArgumentNull("table");
                }
                DataViewSetting dataViewSetting = (DataViewSetting) list[table];
                if(dataViewSetting == null) {
                    dataViewSetting = new DataViewSetting();
                    this[table] = dataViewSetting;
                }
                return dataViewSetting;
            }
            set {
                if (table == null) {
                    throw ExceptionBuilder.ArgumentNull("table");
                }
                value.SetDataViewManager(dataViewManager);
                value.SetDataTable(table);
                list[table] = value;
            }
        }

        private DataTable GetTable(string tableName) {
            DataTable dt = null;
            DataSet ds = dataViewManager.DataSet;
            if(ds != null) {
                dt = ds.Tables[tableName];
            }
            return dt;
        }

        private DataTable GetTable(int index) {
            DataTable dt = null;
            DataSet ds = dataViewManager.DataSet;
            if(ds != null) {
                dt = ds.Tables[index];
            }
            return dt;
        }

        public virtual DataViewSetting this[string tableName] {
            get {
                DataTable dt = GetTable(tableName);
                if(dt != null) {
                    return this[dt];
                }
                return null;
            }
        }

        public virtual DataViewSetting this[int index] {
            get {
                DataTable dt = GetTable(index);
                if(dt != null) {
                    return this[dt];
                }
                return null;
            }
            set {
                DataTable dt = GetTable(index);
                if(dt != null) {
                    this[dt] = value;
                }else {
                    // throw excaption here.
                }
            }
        }

        // ----------- ICollection -------------------------
        public void CopyTo(Array ar, int index) {
            System.Collections.IEnumerator Enumerator = GetEnumerator();
            while (Enumerator.MoveNext()) {
                ar.SetValue(Enumerator.Current, index++);
            }
        }

        public void CopyTo(DataViewSetting[] ar, int index) {
            System.Collections.IEnumerator Enumerator = GetEnumerator();
            while (Enumerator.MoveNext()) {
                ar.SetValue(Enumerator.Current, index++);
            }
        }

        [Browsable(false)]
        public virtual int Count {
            get {
                DataSet ds = dataViewManager.DataSet;
                return (ds == null) ? 0 : ds.Tables.Count;
            }
        }

        public IEnumerator GetEnumerator() {
            // I have to do something here.
            return new DataViewSettingsEnumerator(dataViewManager);
        }

        [
        Browsable(false)
        ]
        public bool IsReadOnly {
            get {
                return true;
            }
        }

        [Browsable(false)]
        public bool IsSynchronized {
            get {
                // so the user will know that it has to lock this object
                return false;
            }
        }

        [Browsable(false)]
        public object SyncRoot {
            get {
                return this;
            }
        }

        internal void Remove(DataTable table) {
            list.Remove(table);
        }

        private sealed class DataViewSettingsEnumerator : IEnumerator {
            DataViewSettingCollection dataViewSettings;
            IEnumerator                tableEnumerator;
            public DataViewSettingsEnumerator(DataViewManager dvm) {
                DataSet ds = dvm.DataSet;
                if(ds != null) {
                    dataViewSettings = dvm.DataViewSettings;
                    tableEnumerator  = dvm.DataSet.Tables.GetEnumerator();
                }else {
                    dataViewSettings = null;
                    tableEnumerator  = DataSet.zeroTables.GetEnumerator();
                }
            }
            public bool MoveNext() {
                return tableEnumerator.MoveNext();
            }
            public void Reset() {
                tableEnumerator.Reset();
            }
            public object Current {
                get {
                    return dataViewSettings[(DataTable) tableEnumerator.Current];
                }
            }
        }
    }
}

