//------------------------------------------------------------------------------
// <copyright file="DataRowView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System.Diagnostics;
    using System.ComponentModel;

    public class DataRowView : ICustomTypeDescriptor, System.ComponentModel.IEditableObject, System.ComponentModel.IDataErrorInfo, System.ComponentModel.INotifyPropertyChanged {

        private readonly DataView dataView;
        private readonly DataRow _row;
        private bool delayBeginEdit;

        private static PropertyDescriptorCollection zeroPropertyDescriptorCollection = new PropertyDescriptorCollection(null);

        /// <summary>
        /// When the PropertyChanged event happens, it must happen on the same DataRowView reference.
        /// This is so a generic event handler like Windows Presentation Foundation can redirect as appropriate.
        /// Having DataView.Equals is not sufficient for WPF, because two different instances may be equal but not equivalent.
        /// For DataRowView, if two instances are equal then they are equivalent.
        /// </summary>
        private System.ComponentModel.PropertyChangedEventHandler  onPropertyChanged;

        internal DataRowView(DataView dataView, DataRow row)
        {
            this.dataView = dataView;
            this._row = row;
        }

        /// <remarks>
        /// Checks for same reference instead of equivalent <see cref="DataView"/> or <see cref="Row"/>.
        /// 
        /// Necessary for ListChanged event handlers to use data structures that use the default to
        /// <see cref="Object.Equals(Object)"/> instead of <see cref="Object.ReferenceEquals"/>
        /// to understand if they need to add a <see cref="PropertyChanged"/> event handler.
        /// </remarks>
        /// <returns><see cref="Object.ReferenceEquals"/></returns>
        public override bool Equals(object other) {
            return Object.ReferenceEquals(this, other);
        }

        /// <returns>Hashcode of <see cref="Row"/></returns>
        public override Int32 GetHashCode() {
            // Everett compatability, must return hashcode for DataRow
            // this does prevent using this object in collections like Hashtable
            // which use the hashcode as an immutable value to identify this object
            // user could/should have used the DataRow property instead of the hashcode
            return Row.GetHashCode();
        }

        public DataView DataView {
            get {
                return dataView;
            }
        }

        internal int ObjectID {
            get { return _row.ObjectID; }
        }

        /// <summary>Gets or sets a value in specified column.</summary>
        /// <param name="ndx">Specified column index.</param>
        /// <remarks>Uses either <see cref="DataRowVersion.Default"/> or <see cref="DataRowVersion.Original"/> to access <see cref="Row"/></remarks>
        /// <exception cref="DataException"><see cref="System.Data.DataView.get_AllowEdit"/> when setting a value.</exception>
        /// <exception cref="IndexOutOfRangeException"><see cref="DataColumnCollection.get_Item(int)"/></exception>
        public object this[int ndx] {
            get {
                return Row[ndx, RowVersionDefault];
            }
            set {
                if (!dataView.AllowEdit && !IsNew) {
                    throw ExceptionBuilder.CanNotEdit();
                }
                SetColumnValue(dataView.Table.Columns[ndx], value);
            }
        }

        /// <summary>Gets the specified column value or related child view or sets a value in specified column.</summary>
        /// <param name="property">Specified column or relation name when getting.  Specified column name when setting.</param>
        /// <exception cref="ArgumentException"><see cref="DataColumnCollection.get_Item(string)"/> when <paramref name="property"/> is ambigous.</exception>
        /// <exception cref="ArgumentException">Unmatched <paramref name="property"/> when getting a value.</exception>
        /// <exception cref="DataException">Unmatched <paramref name="property"/> when setting a value.</exception>
        /// <exception cref="DataException"><see cref="System.Data.DataView.get_AllowEdit"/> when setting a value.</exception>
        public object this[string property] {
            get {
                DataColumn column = dataView.Table.Columns[property];
                if (null != column) {
                    return Row[column, RowVersionDefault];
                }
                else if (dataView.Table.DataSet != null && dataView.Table.DataSet.Relations.Contains(property)) {
                    return CreateChildView(property);
                }
                throw ExceptionBuilder.PropertyNotFound(property, dataView.Table.TableName);
            }
            set {
                DataColumn column = dataView.Table.Columns[property];
                if (null == column) {
                    throw ExceptionBuilder.SetFailed(property);
                }
                if (!dataView.AllowEdit && !IsNew) {
                    throw ExceptionBuilder.CanNotEdit();
                }
                SetColumnValue(column, value);
            }
        }

        // IDataErrorInfo stuff
        string System.ComponentModel.IDataErrorInfo.this[string colName] {
            get {
                return Row.GetColumnError(colName);
            }
        }

        string System.ComponentModel.IDataErrorInfo.Error {
            get {
                return Row.RowError;
            }
        }
        
        /// <summary>
        /// Gets the current version description of the <see cref="DataRow"/>
        /// in relation to <see cref="System.Data.DataView.get_RowStateFilter"/>
        /// </summary>
        /// <returns>Either <see cref="DataRowVersion.Current"/> or <see cref="DataRowVersion.Original"/></returns>
        public DataRowVersion RowVersion {
            get {
                return (RowVersionDefault & ~DataRowVersion.Proposed);
            }
        }

        /// <returns>Either <see cref="DataRowVersion.Default"/> or <see cref="DataRowVersion.Original"/></returns>
        private DataRowVersion RowVersionDefault {
            get {
                return Row.GetDefaultRowVersion(dataView.RowStateFilter);
            }
        }

        internal int GetRecord() {
            return Row.GetRecordFromVersion(RowVersionDefault);
        }

        internal bool HasRecord() {
            return Row.HasVersion(RowVersionDefault);
        }

        internal object GetColumnValue(DataColumn column) {
            return Row[column, RowVersionDefault];
        }

        internal void SetColumnValue(DataColumn column, object value) {
            if (delayBeginEdit) {
                delayBeginEdit = false;
                Row.BeginEdit();
            }
            if (DataRowVersion.Original == RowVersionDefault) {
                throw ExceptionBuilder.SetFailed(column.ColumnName);
            }
            Row[column] = value;
        }

        /// <summary>
        /// Returns a <see cref="System.Data.DataView"/>
        /// for the child <see cref="System.Data.DataTable"/>
        /// with the specified <see cref="System.Data.DataRelation"/>.
        /// </summary>
        /// <param name="relation">Specified <see cref="System.Data.DataRelation"/>.</param>
        /// <exception cref="ArgumentException">null or mismatch between <paramref name="relation"/> and <see cref="System.Data.DataView.get_Table"/>.</exception>
        public DataView CreateChildView(DataRelation relation, bool followParent) {
            if (relation == null || relation.ParentKey.Table != DataView.Table) {
                throw ExceptionBuilder.CreateChildView();
            }
            RelatedView childView;
            if (!followParent) {
                int record = GetRecord();
                object[] values = relation.ParentKey.GetKeyValues(record);
                childView = new RelatedView(relation.ChildColumnsReference, values);
            }
            else {
                childView = new RelatedView(this, relation.ParentKey, relation.ChildColumnsReference);
            }
            childView.SetIndex("", DataViewRowState.CurrentRows, null); // finish construction via RelatedView.SetIndex
            childView.SetDataViewManager(DataView.DataViewManager);
            return childView;
        }


        public DataView CreateChildView(DataRelation relation) {
            return CreateChildView(relation, followParent: false);
        }


        /// <summary><see cref="CreateChildView(DataRelation)"/></summary>
        /// <param name="relationName">Specified <see cref="System.Data.DataRelation"/> name.</param>
        /// <exception cref="ArgumentException">Unmatched <paramref name="relationName"/>.</exception>
        public DataView CreateChildView(string relationName, bool followParent) {
            return CreateChildView(DataView.Table.ChildRelations[relationName], followParent);
        }

        public DataView CreateChildView(string relationName) {
            return CreateChildView(relationName, followParent: false);
        }

        public DataRow Row {
            get {
                return _row;
            }
        }

        public void BeginEdit() {
            delayBeginEdit = true;
        }

        public void CancelEdit() {
            DataRow tmpRow = Row;
            if (IsNew) {
                dataView.FinishAddNew(false);
            }
            else {
                tmpRow.CancelEdit();
            }
            delayBeginEdit = false;
        }

        public void EndEdit() {
            if (IsNew) {
                dataView.FinishAddNew(true);
            }
            else {
                Row.EndEdit();
            }
            delayBeginEdit = false;
        }

        public bool IsNew {
            get {
                return (_row == dataView.addNewRow);
            }
        }

        public bool IsEdit {
            get {
                return (
                    Row.HasVersion(DataRowVersion.Proposed) ||  // It was edited or
                    delayBeginEdit);                            // DataRowView.BegingEdit() was called, but not edited yet.
            }
        }

        public void Delete() {
            dataView.Delete(Row);
        }

        #region ICustomTypeDescriptor

        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return new AttributeCollection((Attribute[])null);
        }

        string ICustomTypeDescriptor.GetClassName() {
            return null;
        }

        string ICustomTypeDescriptor.GetComponentName() {
            return null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return null;
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return new EventDescriptorCollection(null);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return new EventDescriptorCollection(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            return((ICustomTypeDescriptor)this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            return (dataView.Table != null ? dataView.Table.GetPropertyDescriptorCollection(attributes) : zeroPropertyDescriptorCollection);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            return this;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged {
            add {
                onPropertyChanged += value;
            }
            remove {
                onPropertyChanged -= value;
            }

        }

        internal void RaisePropertyChangedEvent (string propName){
            // Do not try catch, we would mask users bugs. if they throw we would catch
            if (onPropertyChanged != null) {
                onPropertyChanged (this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
