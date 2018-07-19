//------------------------------------------------------------------------------
// <copyright file="DataViewManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.ComponentModel;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Xml;

    [
    Designer("Microsoft.VSDesigner.Data.VS.DataViewManagerDesigner, " + AssemblyRef.MicrosoftVSDesigner)
    ]
    public class DataViewManager : MarshalByValueComponent, IBindingList, System.ComponentModel.ITypedList {
        private DataViewSettingCollection dataViewSettingsCollection;
        private DataSet dataSet;
        private DataViewManagerListItemTypeDescriptor item;
        private bool locked;
        internal int nViews = 0;

        private System.ComponentModel.ListChangedEventHandler onListChanged;

        private static NotSupportedException NotSupported = new NotSupportedException();

        public DataViewManager() : this(null, false) {}

        public DataViewManager(DataSet dataSet) : this(dataSet, false) {}

        internal DataViewManager(DataSet dataSet, bool locked) {
            GC.SuppressFinalize(this);
            this.dataSet = dataSet;
            if (this.dataSet != null) {
                this.dataSet.Tables.CollectionChanged += new CollectionChangeEventHandler(TableCollectionChanged);
                this.dataSet.Relations.CollectionChanged += new CollectionChangeEventHandler(RelationCollectionChanged);
            }
            this.locked = locked;
            this.item = new DataViewManagerListItemTypeDescriptor(this);
            this.dataViewSettingsCollection = new DataViewSettingCollection(this);
        }

        [
        DefaultValue(null),
        ResDescriptionAttribute(Res.DataViewManagerDataSetDescr)
        ]
        public DataSet DataSet {
            get {
                return dataSet;
            }
            set {
                if (value == null)
                    throw ExceptionBuilder.SetFailed("DataSet to null");

                if (locked)
                    throw ExceptionBuilder.SetDataSetFailed();

                if (dataSet != null) {
                    if (nViews > 0)
                        throw ExceptionBuilder.CanNotSetDataSet();

                    this.dataSet.Tables.CollectionChanged -= new CollectionChangeEventHandler(TableCollectionChanged);
                    this.dataSet.Relations.CollectionChanged -= new CollectionChangeEventHandler(RelationCollectionChanged);
                }

                this.dataSet = value;
                this.dataSet.Tables.CollectionChanged += new CollectionChangeEventHandler(TableCollectionChanged);
                this.dataSet.Relations.CollectionChanged += new CollectionChangeEventHandler(RelationCollectionChanged);
                this.dataViewSettingsCollection = new DataViewSettingCollection(this);
                item.Reset();
            }
        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ResDescriptionAttribute(Res.DataViewManagerTableSettingsDescr)
        ]
        public DataViewSettingCollection DataViewSettings {
            get {
                return dataViewSettingsCollection;
            }
        }

        public string DataViewSettingCollectionString {
            get {
                if (dataSet == null)
                    return "";

                StringBuilder builder = new StringBuilder();
                builder.Append("<DataViewSettingCollectionString>");
                foreach (DataTable dt in dataSet.Tables) {
                    DataViewSetting ds = dataViewSettingsCollection[dt];
                    builder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "<{0} Sort=\"{1}\" RowFilter=\"{2}\" RowStateFilter=\"{3}\"/>", dt.EncodedTableName, ds.Sort, ds.RowFilter, ds.RowStateFilter);
                }
                builder.Append("</DataViewSettingCollectionString>");
                return builder.ToString();
            }
            set {
                if (value == null || value.Length == 0)
                    return;

                XmlTextReader r = new XmlTextReader(new StringReader(value));
                r.WhitespaceHandling = WhitespaceHandling.None;
                r.Read();
                if (r.Name != "DataViewSettingCollectionString")
                    throw ExceptionBuilder.SetFailed("DataViewSettingCollectionString");
                while (r.Read()) {
                    if (r.NodeType != XmlNodeType.Element)
                        continue;

                    string table = XmlConvert.DecodeName(r.LocalName);
                    if (r.MoveToAttribute("Sort"))
                        dataViewSettingsCollection[table].Sort = r.Value;

                    if (r.MoveToAttribute("RowFilter"))
                        dataViewSettingsCollection[table].RowFilter = r.Value;

                    if (r.MoveToAttribute("RowStateFilter"))
                        dataViewSettingsCollection[table].RowStateFilter = (DataViewRowState)Enum.Parse(typeof(DataViewRowState),r.Value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            DataViewManagerListItemTypeDescriptor[] items = new DataViewManagerListItemTypeDescriptor[1];
            ((ICollection)this).CopyTo(items, 0);
            return items.GetEnumerator();
        }

        int ICollection.Count {
            get {
                return 1;
            }
        }

        object ICollection.SyncRoot {
            get {
                return this;
            }
        }

        bool ICollection.IsSynchronized {
            get {
                return false;
            }
        }

        bool IList.IsReadOnly {
            get {
                return true;
            }
        }

        bool IList.IsFixedSize {
            get {
                return true;
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            array.SetValue((object)(new DataViewManagerListItemTypeDescriptor(this)), index);
        }

        object IList.this[int index] {
            get {
                return item;
            }
            set {
                throw ExceptionBuilder.CannotModifyCollection();
            }
        }

        int IList.Add(object value) {
            throw ExceptionBuilder.CannotModifyCollection();
        }

        void IList.Clear() {
            throw ExceptionBuilder.CannotModifyCollection();
        }

        bool IList.Contains(object value) {
            return(value == item);
        }

        int IList.IndexOf(object value) {
            return(value == item) ? 1 : -1;
        }

        void IList.Insert(int index, object value) {
            throw ExceptionBuilder.CannotModifyCollection();
        }

        void IList.Remove(object value) {
            throw ExceptionBuilder.CannotModifyCollection();
        }

        void IList.RemoveAt(int index) {
            throw ExceptionBuilder.CannotModifyCollection();
        }

        // ------------- IBindingList: ---------------------------

        bool IBindingList.AllowNew {
            get {
                return false;
            }
        }
        object IBindingList.AddNew() {
            throw NotSupported;
        }

        bool IBindingList.AllowEdit {
            get {
                return false;
            }
        }

        bool IBindingList.AllowRemove {
            get {
                return false;
            }
        }

        bool IBindingList.SupportsChangeNotification {
            get {
                return true;
            }
        }

        bool IBindingList.SupportsSearching {
            get {
                return false;
            }
        }

        bool IBindingList.SupportsSorting {
            get {
                return false;
            }
        }

        bool IBindingList.IsSorted {
            get {
                throw NotSupported;
            }
        }

        PropertyDescriptor IBindingList.SortProperty {
            get {
                throw NotSupported;
            }
        }

        ListSortDirection IBindingList.SortDirection {
            get {
                throw NotSupported;
            }
        }

        public event System.ComponentModel.ListChangedEventHandler ListChanged {
            add {
                onListChanged += value;
            }
            remove {
                onListChanged -= value;
            }
        }

        void IBindingList.AddIndex(PropertyDescriptor property) {
            // no operation
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction) {
            throw NotSupported;
        }

        int IBindingList.Find(PropertyDescriptor property, object key) {
                    throw NotSupported;
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property) {
            // no operation
        }

        void IBindingList.RemoveSort() {
            throw NotSupported;
        }

        /*
        string IBindingList.GetListName() {
            return ((System.Data.ITypedList)this).GetListName(null);
        }
        string IBindingList.GetListName(PropertyDescriptor[] listAccessors) {
            return ((System.Data.ITypedList)this).GetListName(listAccessors);
        }
        */

        // Microsoft: GetListName and GetItemProperties almost the same in DataView and DataViewManager
        string System.ComponentModel.ITypedList.GetListName(PropertyDescriptor[] listAccessors) {
            DataSet dataSet = DataSet;
            if (dataSet == null)
                throw ExceptionBuilder.CanNotUseDataViewManager();

            if (listAccessors == null || listAccessors.Length == 0) {
                return dataSet.DataSetName;
            }
            else {
                DataTable table = dataSet.FindTable(null, listAccessors, 0);
                if (table != null) {
                    return table.TableName;
                }
            }
            return String.Empty;
        }

        PropertyDescriptorCollection System.ComponentModel.ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors) {
            DataSet dataSet = DataSet;
            if (dataSet == null)
                throw ExceptionBuilder.CanNotUseDataViewManager();

            if (listAccessors == null || listAccessors.Length == 0) {
                return((ICustomTypeDescriptor)(new DataViewManagerListItemTypeDescriptor(this))).GetProperties();
            }
            else {
                DataTable table = dataSet.FindTable(null, listAccessors, 0);
                if (table != null) {
                    return table.GetPropertyDescriptorCollection(null);
                }
            }
            return new PropertyDescriptorCollection(null);
        }

        public DataView CreateDataView(DataTable table) {
            if (dataSet == null)
                throw ExceptionBuilder.CanNotUseDataViewManager();

            DataView dataView = new DataView(table);
            dataView.SetDataViewManager(this);
            return dataView;
        }

        protected virtual void OnListChanged(ListChangedEventArgs e) {
            try {
                if (onListChanged != null) {
                    onListChanged(this, e);
                }
            }
            catch (Exception f) {
                // 
                if (!Common.ADP.IsCatchableExceptionType(f)) {
                    throw;
                }
                ExceptionBuilder.TraceExceptionWithoutRethrow(f);
                // ignore the exception
            }
        }

        protected virtual void TableCollectionChanged(object sender, CollectionChangeEventArgs e) {
             PropertyDescriptor NullProp = null;
             OnListChanged(
                 e.Action == CollectionChangeAction.Add ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataTablePropertyDescriptor((System.Data.DataTable)e.Element)) :
                 e.Action == CollectionChangeAction.Refresh ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, NullProp) :
                 e.Action == CollectionChangeAction.Remove ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataTablePropertyDescriptor((System.Data.DataTable)e.Element)) :
                 /*default*/ null
             );
        }

        protected virtual void RelationCollectionChanged(object sender, CollectionChangeEventArgs e) {
            DataRelationPropertyDescriptor NullProp = null;
            OnListChanged(
                e.Action == CollectionChangeAction.Add ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataRelationPropertyDescriptor((System.Data.DataRelation)e.Element)) :
                e.Action == CollectionChangeAction.Refresh ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, NullProp):
                e.Action == CollectionChangeAction.Remove ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataRelationPropertyDescriptor((System.Data.DataRelation)e.Element)) :
            /*default*/ null
            );
        }
    }
}
