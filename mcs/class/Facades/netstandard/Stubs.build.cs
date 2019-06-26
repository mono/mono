namespace System.Data
{
    public enum AcceptRejectRule
    {
        Cascade = 1,
        None = 0,
    }
    [System.FlagsAttribute]
    public enum CommandBehavior
    {
        CloseConnection = 32,
        Default = 0,
        KeyInfo = 4,
        SchemaOnly = 2,
        SequentialAccess = 16,
        SingleResult = 1,
        SingleRow = 8,
    }
    public enum CommandType
    {
        StoredProcedure = 4,
        TableDirect = 512,
        Text = 1,
    }
    public enum ConflictOption
    {
        CompareAllSearchableValues = 1,
        CompareRowVersion = 2,
        OverwriteChanges = 3,
    }
    [System.FlagsAttribute]
    public enum ConnectionState
    {
        Broken = 16,
        Closed = 0,
        Connecting = 2,
        Executing = 4,
        Fetching = 8,
        Open = 1,
    }
    [System.ComponentModel.DefaultPropertyAttribute("ConstraintName")]
    [System.ComponentModel.TypeConverterAttribute("System.Data.ConstraintConverter")]
    public abstract partial class Constraint
    {
        protected Constraint() { }
        [System.ComponentModel.DefaultValueAttribute("")]
        public virtual string ConstraintName { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.PropertyCollection ExtendedProperties { get { throw null; } }
        public abstract System.Data.DataTable Table { get; }
        protected virtual System.Data.DataSet _DataSet { get { throw null; } }
        protected void CheckStateForProperty() { }
        protected internal void SetDataSet(System.Data.DataSet dataSet) { }
        public override string ToString() { throw null; }
    }
    [System.ComponentModel.DefaultEventAttribute("CollectionChanged")]
    public sealed partial class ConstraintCollection : System.Data.InternalDataCollectionBase
    {
        internal ConstraintCollection() { }
        public System.Data.Constraint this[int index] { get { throw null; } }
        public System.Data.Constraint this[string name] { get { throw null; } }
        protected override System.Collections.ArrayList List { get { throw null; } }
        public event System.ComponentModel.CollectionChangeEventHandler CollectionChanged { add { } remove { } }
        public void Add(System.Data.Constraint constraint) { }
        public System.Data.Constraint Add(string name, System.Data.DataColumn column, bool primaryKey) { throw null; }
        public System.Data.Constraint Add(string name, System.Data.DataColumn primaryKeyColumn, System.Data.DataColumn foreignKeyColumn) { throw null; }
        public System.Data.Constraint Add(string name, System.Data.DataColumn[] columns, bool primaryKey) { throw null; }
        public System.Data.Constraint Add(string name, System.Data.DataColumn[] primaryKeyColumns, System.Data.DataColumn[] foreignKeyColumns) { throw null; }
        public void AddRange(System.Data.Constraint[] constraints) { }
        public bool CanRemove(System.Data.Constraint constraint) { throw null; }
        public void Clear() { }
        public bool Contains(string name) { throw null; }
        public void CopyTo(System.Data.Constraint[] array, int index) { }
        public int IndexOf(System.Data.Constraint constraint) { throw null; }
        public int IndexOf(string constraintName) { throw null; }
        public void Remove(System.Data.Constraint constraint) { }
        public void Remove(string name) { }
        public void RemoveAt(int index) { }
    }
    [System.SerializableAttribute]
    public partial class ConstraintException : System.Data.DataException
    {
        public ConstraintException() { }
        protected ConstraintException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public ConstraintException(string s) { }
        public ConstraintException(string message, System.Exception innerException) { }
    }
    [System.ComponentModel.DefaultPropertyAttribute("ColumnName")]
    [System.ComponentModel.DesignTimeVisibleAttribute(false)]
    [System.ComponentModel.ToolboxItemAttribute(false)]
    public partial class DataColumn : System.ComponentModel.MarshalByValueComponent
    {
        public DataColumn() { }
        public DataColumn(string columnName) { }
        public DataColumn(string columnName, System.Type dataType) { }
        public DataColumn(string columnName, System.Type dataType, string expr) { }
        public DataColumn(string columnName, System.Type dataType, string expr, System.Data.MappingType type) { }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool AllowDBNull { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(false)]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public bool AutoIncrement { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute((long)0)]
        public long AutoIncrementSeed { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute((long)1)]
        public long AutoIncrementStep { get { throw null; } set { } }
        public string Caption { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.MappingType)(1))]
        public virtual System.Data.MappingType ColumnMapping { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute("")]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public string ColumnName { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(typeof(string))]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        [System.ComponentModel.TypeConverterAttribute("System.Data.ColumnTypeConverter")]
        public System.Type DataType { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.DataSetDateTime)(3))]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public System.Data.DataSetDateTime DateTimeMode { get { throw null; } set { } }
        [System.ComponentModel.TypeConverterAttribute("System.Data.DefaultValueTypeConverter")]
        public object DefaultValue { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute("")]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public string Expression { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.PropertyCollection ExtendedProperties { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute(-1)]
        public int MaxLength { get { throw null; } set { } }
        public string Namespace { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public int Ordinal { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string Prefix { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ReadOnly { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.DataTable Table { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public bool Unique { get { throw null; } set { } }
        protected internal void CheckNotAllowNull() { }
        protected void CheckUnique() { }
        protected virtual void OnPropertyChanging(System.ComponentModel.PropertyChangedEventArgs pcevent) { }
        protected internal void RaisePropertyChanging(string name) { }
        public void SetOrdinal(int ordinal) { }
        public override string ToString() { throw null; }
    }
    public partial class DataColumnChangeEventArgs : System.EventArgs
    {
        public DataColumnChangeEventArgs(System.Data.DataRow row, System.Data.DataColumn column, object value) { }
        public System.Data.DataColumn Column { get { throw null; } }
        public object ProposedValue { get { throw null; } set { } }
        public System.Data.DataRow Row { get { throw null; } }
    }
    public delegate void DataColumnChangeEventHandler(object sender, System.Data.DataColumnChangeEventArgs e);
    [System.ComponentModel.DefaultEventAttribute("CollectionChanged")]
    public sealed partial class DataColumnCollection : System.Data.InternalDataCollectionBase
    {
        internal DataColumnCollection() { }
        public System.Data.DataColumn this[int index] { get { throw null; } }
        public System.Data.DataColumn this[string name] { get { throw null; } }
        protected override System.Collections.ArrayList List { get { throw null; } }
        public event System.ComponentModel.CollectionChangeEventHandler CollectionChanged { add { } remove { } }
        public System.Data.DataColumn Add() { throw null; }
        public void Add(System.Data.DataColumn column) { }
        public System.Data.DataColumn Add(string columnName) { throw null; }
        public System.Data.DataColumn Add(string columnName, System.Type type) { throw null; }
        public System.Data.DataColumn Add(string columnName, System.Type type, string expression) { throw null; }
        public void AddRange(System.Data.DataColumn[] columns) { }
        public bool CanRemove(System.Data.DataColumn column) { throw null; }
        public void Clear() { }
        public bool Contains(string name) { throw null; }
        public void CopyTo(System.Data.DataColumn[] array, int index) { }
        public int IndexOf(System.Data.DataColumn column) { throw null; }
        public int IndexOf(string columnName) { throw null; }
        public void Remove(System.Data.DataColumn column) { }
        public void Remove(string name) { }
        public void RemoveAt(int index) { }
    }
    [System.SerializableAttribute]
    public partial class DataException : System.SystemException
    {
        public DataException() { }
        protected DataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public DataException(string s) { }
        public DataException(string s, System.Exception innerException) { }
    }
    [System.ComponentModel.DefaultPropertyAttribute("RelationName")]
    [System.ComponentModel.TypeConverterAttribute("System.Data.RelationshipConverter")]
    public partial class DataRelation
    {
        public DataRelation(string relationName, System.Data.DataColumn parentColumn, System.Data.DataColumn childColumn) { }
        public DataRelation(string relationName, System.Data.DataColumn parentColumn, System.Data.DataColumn childColumn, bool createConstraints) { }
        public DataRelation(string relationName, System.Data.DataColumn[] parentColumns, System.Data.DataColumn[] childColumns) { }
        public DataRelation(string relationName, System.Data.DataColumn[] parentColumns, System.Data.DataColumn[] childColumns, bool createConstraints) { }
        [System.ComponentModel.BrowsableAttribute(false)]
        public DataRelation(string relationName, string parentTableName, string parentTableNamespace, string childTableName, string childTableNamespace, string[] parentColumnNames, string[] childColumnNames, bool nested) { }
        [System.ComponentModel.BrowsableAttribute(false)]
        public DataRelation(string relationName, string parentTableName, string childTableName, string[] parentColumnNames, string[] childColumnNames, bool nested) { }
        public virtual System.Data.DataColumn[] ChildColumns { get { throw null; } }
        public virtual System.Data.ForeignKeyConstraint ChildKeyConstraint { get { throw null; } }
        public virtual System.Data.DataTable ChildTable { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public virtual System.Data.DataSet DataSet { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.PropertyCollection ExtendedProperties { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute(false)]
        public virtual bool Nested { get { throw null; } set { } }
        public virtual System.Data.DataColumn[] ParentColumns { get { throw null; } }
        public virtual System.Data.UniqueConstraint ParentKeyConstraint { get { throw null; } }
        public virtual System.Data.DataTable ParentTable { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public virtual string RelationName { get { throw null; } set { } }
        protected void CheckStateForProperty() { }
        protected internal void OnPropertyChanging(System.ComponentModel.PropertyChangedEventArgs pcevent) { }
        protected internal void RaisePropertyChanging(string name) { }
        public override string ToString() { throw null; }
    }
    [System.ComponentModel.DefaultEventAttribute("CollectionChanged")]
    [System.ComponentModel.DefaultPropertyAttribute("Table")]
    public abstract partial class DataRelationCollection : System.Data.InternalDataCollectionBase
    {
        protected DataRelationCollection() { }
        public abstract System.Data.DataRelation this[int index] { get; }
        public abstract System.Data.DataRelation this[string name] { get; }
        public event System.ComponentModel.CollectionChangeEventHandler CollectionChanged { add { } remove { } }
        public virtual System.Data.DataRelation Add(System.Data.DataColumn parentColumn, System.Data.DataColumn childColumn) { throw null; }
        public virtual System.Data.DataRelation Add(System.Data.DataColumn[] parentColumns, System.Data.DataColumn[] childColumns) { throw null; }
        public void Add(System.Data.DataRelation relation) { }
        public virtual System.Data.DataRelation Add(string name, System.Data.DataColumn parentColumn, System.Data.DataColumn childColumn) { throw null; }
        public virtual System.Data.DataRelation Add(string name, System.Data.DataColumn parentColumn, System.Data.DataColumn childColumn, bool createConstraints) { throw null; }
        public virtual System.Data.DataRelation Add(string name, System.Data.DataColumn[] parentColumns, System.Data.DataColumn[] childColumns) { throw null; }
        public virtual System.Data.DataRelation Add(string name, System.Data.DataColumn[] parentColumns, System.Data.DataColumn[] childColumns, bool createConstraints) { throw null; }
        protected virtual void AddCore(System.Data.DataRelation relation) { }
        public virtual void AddRange(System.Data.DataRelation[] relations) { }
        public virtual bool CanRemove(System.Data.DataRelation relation) { throw null; }
        public virtual void Clear() { }
        public virtual bool Contains(string name) { throw null; }
        public void CopyTo(System.Data.DataRelation[] array, int index) { }
        protected abstract System.Data.DataSet GetDataSet();
        public virtual int IndexOf(System.Data.DataRelation relation) { throw null; }
        public virtual int IndexOf(string relationName) { throw null; }
        protected virtual void OnCollectionChanged(System.ComponentModel.CollectionChangeEventArgs ccevent) { }
        protected virtual void OnCollectionChanging(System.ComponentModel.CollectionChangeEventArgs ccevent) { }
        public void Remove(System.Data.DataRelation relation) { }
        public void Remove(string name) { }
        public void RemoveAt(int index) { }
        protected virtual void RemoveCore(System.Data.DataRelation relation) { }
    }
    public partial class DataRow
    {
        protected internal DataRow(System.Data.DataRowBuilder builder) { }
        public bool HasErrors { get { throw null; } }
        public object this[System.Data.DataColumn column] { get { throw null; } set { } }
        public object this[System.Data.DataColumn column, System.Data.DataRowVersion version] { get { throw null; } }
        public object this[int columnIndex] { get { throw null; } set { } }
        public object this[int columnIndex, System.Data.DataRowVersion version] { get { throw null; } }
        public object this[string columnName] { get { throw null; } set { } }
        public object this[string columnName, System.Data.DataRowVersion version] { get { throw null; } }
        public object[] ItemArray { get { throw null; } set { } }
        public string RowError { get { throw null; } set { } }
        public System.Data.DataRowState RowState { get { throw null; } }
        public System.Data.DataTable Table { get { throw null; } }
        public void AcceptChanges() { }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public void BeginEdit() { }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public void CancelEdit() { }
        public void ClearErrors() { }
        public void Delete() { }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public void EndEdit() { }
        public System.Data.DataRow[] GetChildRows(System.Data.DataRelation relation) { throw null; }
        public System.Data.DataRow[] GetChildRows(System.Data.DataRelation relation, System.Data.DataRowVersion version) { throw null; }
        public System.Data.DataRow[] GetChildRows(string relationName) { throw null; }
        public System.Data.DataRow[] GetChildRows(string relationName, System.Data.DataRowVersion version) { throw null; }
        public string GetColumnError(System.Data.DataColumn column) { throw null; }
        public string GetColumnError(int columnIndex) { throw null; }
        public string GetColumnError(string columnName) { throw null; }
        public System.Data.DataColumn[] GetColumnsInError() { throw null; }
        public System.Data.DataRow GetParentRow(System.Data.DataRelation relation) { throw null; }
        public System.Data.DataRow GetParentRow(System.Data.DataRelation relation, System.Data.DataRowVersion version) { throw null; }
        public System.Data.DataRow GetParentRow(string relationName) { throw null; }
        public System.Data.DataRow GetParentRow(string relationName, System.Data.DataRowVersion version) { throw null; }
        public System.Data.DataRow[] GetParentRows(System.Data.DataRelation relation) { throw null; }
        public System.Data.DataRow[] GetParentRows(System.Data.DataRelation relation, System.Data.DataRowVersion version) { throw null; }
        public System.Data.DataRow[] GetParentRows(string relationName) { throw null; }
        public System.Data.DataRow[] GetParentRows(string relationName, System.Data.DataRowVersion version) { throw null; }
        public bool HasVersion(System.Data.DataRowVersion version) { throw null; }
        public bool IsNull(System.Data.DataColumn column) { throw null; }
        public bool IsNull(System.Data.DataColumn column, System.Data.DataRowVersion version) { throw null; }
        public bool IsNull(int columnIndex) { throw null; }
        public bool IsNull(string columnName) { throw null; }
        public void RejectChanges() { }
        public void SetAdded() { }
        public void SetColumnError(System.Data.DataColumn column, string error) { }
        public void SetColumnError(int columnIndex, string error) { }
        public void SetColumnError(string columnName, string error) { }
        public void SetModified() { }
        protected void SetNull(System.Data.DataColumn column) { }
        public void SetParentRow(System.Data.DataRow parentRow) { }
        public void SetParentRow(System.Data.DataRow parentRow, System.Data.DataRelation relation) { }
    }
    [System.FlagsAttribute]
    public enum DataRowAction
    {
        Add = 16,
        Change = 2,
        ChangeCurrentAndOriginal = 64,
        ChangeOriginal = 32,
        Commit = 8,
        Delete = 1,
        Nothing = 0,
        Rollback = 4,
    }
    public sealed partial class DataRowBuilder
    {
        internal DataRowBuilder() { }
    }
    public partial class DataRowChangeEventArgs : System.EventArgs
    {
        public DataRowChangeEventArgs(System.Data.DataRow row, System.Data.DataRowAction action) { }
        public System.Data.DataRowAction Action { get { throw null; } }
        public System.Data.DataRow Row { get { throw null; } }
    }
    public delegate void DataRowChangeEventHandler(object sender, System.Data.DataRowChangeEventArgs e);
    public sealed partial class DataRowCollection : System.Data.InternalDataCollectionBase
    {
        internal DataRowCollection() { }
        public override int Count { get { throw null; } }
        public System.Data.DataRow this[int index] { get { throw null; } }
        public void Add(System.Data.DataRow row) { }
        public System.Data.DataRow Add(params object[] values) { throw null; }
        public void Clear() { }
        public bool Contains(object key) { throw null; }
        public bool Contains(object[] keys) { throw null; }
        public override void CopyTo(System.Array ar, int index) { }
        public void CopyTo(System.Data.DataRow[] array, int index) { }
        public System.Data.DataRow Find(object key) { throw null; }
        public System.Data.DataRow Find(object[] keys) { throw null; }
        public override System.Collections.IEnumerator GetEnumerator() { throw null; }
        public int IndexOf(System.Data.DataRow row) { throw null; }
        public void InsertAt(System.Data.DataRow row, int pos) { }
        public void Remove(System.Data.DataRow row) { }
        public void RemoveAt(int index) { }
    }
    [System.FlagsAttribute]
    public enum DataRowState
    {
        Added = 4,
        Deleted = 8,
        Detached = 1,
        Modified = 16,
        Unchanged = 2,
    }
    public enum DataRowVersion
    {
        Current = 512,
        Default = 1536,
        Original = 256,
        Proposed = 1024,
    }
    public partial class DataRowView : System.ComponentModel.ICustomTypeDescriptor, System.ComponentModel.IDataErrorInfo, System.ComponentModel.IEditableObject, System.ComponentModel.INotifyPropertyChanged
    {
        internal DataRowView() { }
        public System.Data.DataView DataView { get { throw null; } }
        public bool IsEdit { get { throw null; } }
        public bool IsNew { get { throw null; } }
        public object this[int ndx] { get { throw null; } set { } }
        public object this[string property] { get { throw null; } set { } }
        public System.Data.DataRow Row { get { throw null; } }
        public System.Data.DataRowVersion RowVersion { get { throw null; } }
        string System.ComponentModel.IDataErrorInfo.Error { get { throw null; } }
        string System.ComponentModel.IDataErrorInfo.this[string colName] { get { throw null; } }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged { add { } remove { } }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public System.Data.DataView CreateChildView(System.Data.DataRelation relation) { throw null; }
        public System.Data.DataView CreateChildView(System.Data.DataRelation relation, bool followParent) { throw null; }
        public System.Data.DataView CreateChildView(string relationName) { throw null; }
        public System.Data.DataView CreateChildView(string relationName, bool followParent) { throw null; }
        public void Delete() { }
        public void EndEdit() { }
        public override bool Equals(object other) { throw null; }
        public override int GetHashCode() { throw null; }
        System.ComponentModel.AttributeCollection System.ComponentModel.ICustomTypeDescriptor.GetAttributes() { throw null; }
        string System.ComponentModel.ICustomTypeDescriptor.GetClassName() { throw null; }
        string System.ComponentModel.ICustomTypeDescriptor.GetComponentName() { throw null; }
        System.ComponentModel.TypeConverter System.ComponentModel.ICustomTypeDescriptor.GetConverter() { throw null; }
        System.ComponentModel.EventDescriptor System.ComponentModel.ICustomTypeDescriptor.GetDefaultEvent() { throw null; }
        System.ComponentModel.PropertyDescriptor System.ComponentModel.ICustomTypeDescriptor.GetDefaultProperty() { throw null; }
        object System.ComponentModel.ICustomTypeDescriptor.GetEditor(System.Type editorBaseType) { throw null; }
        System.ComponentModel.EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents() { throw null; }
        System.ComponentModel.EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents(System.Attribute[] attributes) { throw null; }
        System.ComponentModel.PropertyDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetProperties() { throw null; }
        System.ComponentModel.PropertyDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetProperties(System.Attribute[] attributes) { throw null; }
        object System.ComponentModel.ICustomTypeDescriptor.GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd) { throw null; }
    }
    [System.ComponentModel.DefaultPropertyAttribute("DataSetName")]
    [System.Xml.Serialization.XmlRootAttribute("DataSet")]
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetDataSetSchema")]
    [System.ComponentModel.ToolboxItemAttribute("Microsoft.VSDesigner.Data.VS.DataSetToolboxItem, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [System.SerializableAttribute]
    public partial class DataSet : System.ComponentModel.MarshalByValueComponent, System.ComponentModel.IListSource, System.ComponentModel.ISupportInitialize, System.ComponentModel.ISupportInitializeNotification, System.Runtime.Serialization.ISerializable, System.Xml.Serialization.IXmlSerializable
    {
        public DataSet() { }
        protected DataSet(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        protected DataSet(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context, bool ConstructSchema) { }
        public DataSet(string dataSetName) { }
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool CaseSensitive { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string DataSetName { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.DataViewManager DefaultViewManager { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool EnforceConstraints { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.PropertyCollection ExtendedProperties { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool HasErrors { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsInitialized { get { throw null; } }
        public System.Globalization.CultureInfo Locale { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string Namespace { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string Prefix { get { throw null; } set { } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(2))]
        public System.Data.DataRelationCollection Relations { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.SerializationFormat)(0))]
        public System.Data.SerializationFormat RemotingFormat { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public virtual System.Data.SchemaSerializationMode SchemaSerializationMode { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public override System.ComponentModel.ISite Site { get { throw null; } set { } }
        bool System.ComponentModel.IListSource.ContainsListCollection { get { throw null; } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(2))]
        public System.Data.DataTableCollection Tables { get { throw null; } }
        public event System.EventHandler Initialized { add { } remove { } }
        public event System.Data.MergeFailedEventHandler MergeFailed { add { } remove { } }
        public void AcceptChanges() { }
        public void BeginInit() { }
        public void Clear() { }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]public virtual System.Data.DataSet Clone() { throw null; }
        public System.Data.DataSet Copy() { throw null; }
        public System.Data.DataTableReader CreateDataReader() { throw null; }
        public System.Data.DataTableReader CreateDataReader(params System.Data.DataTable[] dataTables) { throw null; }
        protected System.Data.SchemaSerializationMode DetermineSchemaSerializationMode(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { throw null; }
        protected System.Data.SchemaSerializationMode DetermineSchemaSerializationMode(System.Xml.XmlReader reader) { throw null; }
        public void EndInit() { }
        public System.Data.DataSet GetChanges() { throw null; }
        public System.Data.DataSet GetChanges(System.Data.DataRowState rowStates) { throw null; }
        public static System.Xml.Schema.XmlSchemaComplexType GetDataSetSchema(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        protected virtual System.Xml.Schema.XmlSchema GetSchemaSerializable() { throw null; }
        protected void GetSerializationData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public string GetXml() { throw null; }
        public string GetXmlSchema() { throw null; }
        public bool HasChanges() { throw null; }
        public bool HasChanges(System.Data.DataRowState rowStates) { throw null; }
        public void InferXmlSchema(System.IO.Stream stream, string[] nsArray) { }
        public void InferXmlSchema(System.IO.TextReader reader, string[] nsArray) { }
        public void InferXmlSchema(string fileName, string[] nsArray) { }
        public void InferXmlSchema(System.Xml.XmlReader reader, string[] nsArray) { }
        protected virtual void InitializeDerivedDataSet() { }
        protected bool IsBinarySerialized(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { throw null; }
        public void Load(System.Data.IDataReader reader, System.Data.LoadOption loadOption, params System.Data.DataTable[] tables) { }
        public virtual void Load(System.Data.IDataReader reader, System.Data.LoadOption loadOption, System.Data.FillErrorEventHandler errorHandler, params System.Data.DataTable[] tables) { }
        public void Load(System.Data.IDataReader reader, System.Data.LoadOption loadOption, params string[] tables) { }
        public void Merge(System.Data.DataRow[] rows) { }
        public void Merge(System.Data.DataRow[] rows, bool preserveChanges, System.Data.MissingSchemaAction missingSchemaAction) { }
        public void Merge(System.Data.DataSet dataSet) { }
        public void Merge(System.Data.DataSet dataSet, bool preserveChanges) { }
        public void Merge(System.Data.DataSet dataSet, bool preserveChanges, System.Data.MissingSchemaAction missingSchemaAction) { }
        public void Merge(System.Data.DataTable table) { }
        public void Merge(System.Data.DataTable table, bool preserveChanges, System.Data.MissingSchemaAction missingSchemaAction) { }
        protected virtual void OnPropertyChanging(System.ComponentModel.PropertyChangedEventArgs pcevent) { }
        protected virtual void OnRemoveRelation(System.Data.DataRelation relation) { }
        protected internal virtual void OnRemoveTable(System.Data.DataTable table) { }
        protected internal void RaisePropertyChanging(string name) { }
        public System.Data.XmlReadMode ReadXml(System.IO.Stream stream) { throw null; }
        public System.Data.XmlReadMode ReadXml(System.IO.Stream stream, System.Data.XmlReadMode mode) { throw null; }
        public System.Data.XmlReadMode ReadXml(System.IO.TextReader reader) { throw null; }
        public System.Data.XmlReadMode ReadXml(System.IO.TextReader reader, System.Data.XmlReadMode mode) { throw null; }
        public System.Data.XmlReadMode ReadXml(string fileName) { throw null; }
        public System.Data.XmlReadMode ReadXml(string fileName, System.Data.XmlReadMode mode) { throw null; }
        public System.Data.XmlReadMode ReadXml(System.Xml.XmlReader reader) { throw null; }
        public System.Data.XmlReadMode ReadXml(System.Xml.XmlReader reader, System.Data.XmlReadMode mode) { throw null; }
        public void ReadXmlSchema(System.IO.Stream stream) { }
        public void ReadXmlSchema(System.IO.TextReader reader) { }
        public void ReadXmlSchema(string fileName) { }
        public void ReadXmlSchema(System.Xml.XmlReader reader) { }
        protected virtual void ReadXmlSerializable(System.Xml.XmlReader reader) { }
        public virtual void RejectChanges() { }
        public virtual void Reset() { }
        protected virtual bool ShouldSerializeRelations() { throw null; }
        protected virtual bool ShouldSerializeTables() { throw null; }
        System.Collections.IList System.ComponentModel.IListSource.GetList() { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public void WriteXml(System.IO.Stream stream) { }
        public void WriteXml(System.IO.Stream stream, System.Data.XmlWriteMode mode) { }
        public void WriteXml(System.IO.TextWriter writer) { }
        public void WriteXml(System.IO.TextWriter writer, System.Data.XmlWriteMode mode) { }
        public void WriteXml(string fileName) { }
        public void WriteXml(string fileName, System.Data.XmlWriteMode mode) { }
        public void WriteXml(System.Xml.XmlWriter writer) { }
        public void WriteXml(System.Xml.XmlWriter writer, System.Data.XmlWriteMode mode) { }
        public void WriteXmlSchema(System.IO.Stream stream) { }
        public void WriteXmlSchema(System.IO.Stream stream, System.Converter<System.Type, string> multipleTargetConverter) { }
        public void WriteXmlSchema(System.IO.TextWriter writer) { }
        public void WriteXmlSchema(System.IO.TextWriter writer, System.Converter<System.Type, string> multipleTargetConverter) { }
        public void WriteXmlSchema(string fileName) { }
        public void WriteXmlSchema(string fileName, System.Converter<System.Type, string> multipleTargetConverter) { }
        public void WriteXmlSchema(System.Xml.XmlWriter writer) { }
        public void WriteXmlSchema(System.Xml.XmlWriter writer, System.Converter<System.Type, string> multipleTargetConverter) { }
    }
    public enum DataSetDateTime
    {
        Local = 1,
        Unspecified = 2,
        UnspecifiedLocal = 3,
        Utc = 4,
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(32767))]
    [System.ObsoleteAttribute("DataSysDescriptionAttribute has been deprecated.  http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public partial class DataSysDescriptionAttribute : System.ComponentModel.DescriptionAttribute
    {
        [System.ObsoleteAttribute("DataSysDescriptionAttribute has been deprecated.  http://go.microsoft.com/fwlink/?linkid=14202", false)]
        public DataSysDescriptionAttribute(string description) { }
        public override string Description { get { throw null; } }
    }
    [System.ComponentModel.DefaultEventAttribute("RowChanging")]
    [System.ComponentModel.DefaultPropertyAttribute("TableName")]
    [System.ComponentModel.DesignTimeVisibleAttribute(false)]
    [System.ComponentModel.ToolboxItemAttribute(false)]
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetDataTableSchema")]
    [System.SerializableAttribute]
    public partial class DataTable : System.ComponentModel.MarshalByValueComponent, System.ComponentModel.IListSource, System.ComponentModel.ISupportInitialize, System.ComponentModel.ISupportInitializeNotification, System.Runtime.Serialization.ISerializable, System.Xml.Serialization.IXmlSerializable
    {
        protected internal bool fInitInProgress;
        public DataTable() { }
        protected DataTable(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public DataTable(string tableName) { }
        public DataTable(string tableName, string tableNamespace) { }
        public bool CaseSensitive { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.DataRelationCollection ChildRelations { get { throw null; } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(2))]
        public System.Data.DataColumnCollection Columns { get { throw null; } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(2))]
        public System.Data.ConstraintCollection Constraints { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.DataSet DataSet { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.DataView DefaultView { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string DisplayExpression { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.PropertyCollection ExtendedProperties { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool HasErrors { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsInitialized { get { throw null; } }
        public System.Globalization.CultureInfo Locale { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(50)]
        public int MinimumCapacity { get { throw null; } set { } }
        public string Namespace { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.DataRelationCollection ParentRelations { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string Prefix { get { throw null; } set { } }
        [System.ComponentModel.TypeConverterAttribute("System.Data.PrimaryKeyTypeConverter")]
        public System.Data.DataColumn[] PrimaryKey { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.SerializationFormat)(0))]
        public System.Data.SerializationFormat RemotingFormat { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.DataRowCollection Rows { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public override System.ComponentModel.ISite Site { get { throw null; } set { } }
        bool System.ComponentModel.IListSource.ContainsListCollection { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute("")]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public string TableName { get { throw null; } set { } }
        public event System.Data.DataColumnChangeEventHandler ColumnChanged { add { } remove { } }
        public event System.Data.DataColumnChangeEventHandler ColumnChanging { add { } remove { } }
        public event System.EventHandler Initialized { add { } remove { } }
        public event System.Data.DataRowChangeEventHandler RowChanged { add { } remove { } }
        public event System.Data.DataRowChangeEventHandler RowChanging { add { } remove { } }
        public event System.Data.DataRowChangeEventHandler RowDeleted { add { } remove { } }
        public event System.Data.DataRowChangeEventHandler RowDeleting { add { } remove { } }
        public event System.Data.DataTableClearEventHandler TableCleared { add { } remove { } }
        public event System.Data.DataTableClearEventHandler TableClearing { add { } remove { } }
        public event System.Data.DataTableNewRowEventHandler TableNewRow { add { } remove { } }
        public void AcceptChanges() { }
        public virtual void BeginInit() { }
        public void BeginLoadData() { }
        public void Clear() { }
        public virtual System.Data.DataTable Clone() { throw null; }
        public object Compute(string expression, string filter) { throw null; }
        public System.Data.DataTable Copy() { throw null; }
        public System.Data.DataTableReader CreateDataReader() { throw null; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]protected virtual System.Data.DataTable CreateInstance() { throw null; }
        public virtual void EndInit() { }
        public void EndLoadData() { }
        public System.Data.DataTable GetChanges() { throw null; }
        public System.Data.DataTable GetChanges(System.Data.DataRowState rowStates) { throw null; }
        public static System.Xml.Schema.XmlSchemaComplexType GetDataTableSchema(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public System.Data.DataRow[] GetErrors() { throw null; }
        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        protected virtual System.Type GetRowType() { throw null; }
        protected virtual System.Xml.Schema.XmlSchema GetSchema() { throw null; }
        public void ImportRow(System.Data.DataRow row) { }
        public void Load(System.Data.IDataReader reader) { }
        public void Load(System.Data.IDataReader reader, System.Data.LoadOption loadOption) { }
        public virtual void Load(System.Data.IDataReader reader, System.Data.LoadOption loadOption, System.Data.FillErrorEventHandler errorHandler) { }
        public System.Data.DataRow LoadDataRow(object[] values, bool fAcceptChanges) { throw null; }
        public System.Data.DataRow LoadDataRow(object[] values, System.Data.LoadOption loadOption) { throw null; }
        public void Merge(System.Data.DataTable table) { }
        public void Merge(System.Data.DataTable table, bool preserveChanges) { }
        public void Merge(System.Data.DataTable table, bool preserveChanges, System.Data.MissingSchemaAction missingSchemaAction) { }
        public System.Data.DataRow NewRow() { throw null; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]protected internal System.Data.DataRow[] NewRowArray(int size) { throw null; }
        protected virtual System.Data.DataRow NewRowFromBuilder(System.Data.DataRowBuilder builder) { throw null; }
        protected internal virtual void OnColumnChanged(System.Data.DataColumnChangeEventArgs e) { }
        protected internal virtual void OnColumnChanging(System.Data.DataColumnChangeEventArgs e) { }
        protected virtual void OnPropertyChanging(System.ComponentModel.PropertyChangedEventArgs pcevent) { }
        protected virtual void OnRemoveColumn(System.Data.DataColumn column) { }
        protected virtual void OnRowChanged(System.Data.DataRowChangeEventArgs e) { }
        protected virtual void OnRowChanging(System.Data.DataRowChangeEventArgs e) { }
        protected virtual void OnRowDeleted(System.Data.DataRowChangeEventArgs e) { }
        protected virtual void OnRowDeleting(System.Data.DataRowChangeEventArgs e) { }
        protected virtual void OnTableCleared(System.Data.DataTableClearEventArgs e) { }
        protected virtual void OnTableClearing(System.Data.DataTableClearEventArgs e) { }
        protected virtual void OnTableNewRow(System.Data.DataTableNewRowEventArgs e) { }
        public System.Data.XmlReadMode ReadXml(System.IO.Stream stream) { throw null; }
        public System.Data.XmlReadMode ReadXml(System.IO.TextReader reader) { throw null; }
        public System.Data.XmlReadMode ReadXml(string fileName) { throw null; }
        public System.Data.XmlReadMode ReadXml(System.Xml.XmlReader reader) { throw null; }
        public void ReadXmlSchema(System.IO.Stream stream) { }
        public void ReadXmlSchema(System.IO.TextReader reader) { }
        public void ReadXmlSchema(string fileName) { }
        public void ReadXmlSchema(System.Xml.XmlReader reader) { }
        protected virtual void ReadXmlSerializable(System.Xml.XmlReader reader) { }
        public void RejectChanges() { }
        public virtual void Reset() { }
        public System.Data.DataRow[] Select() { throw null; }
        public System.Data.DataRow[] Select(string filterExpression) { throw null; }
        public System.Data.DataRow[] Select(string filterExpression, string sort) { throw null; }
        public System.Data.DataRow[] Select(string filterExpression, string sort, System.Data.DataViewRowState recordStates) { throw null; }
        System.Collections.IList System.ComponentModel.IListSource.GetList() { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public override string ToString() { throw null; }
        public void WriteXml(System.IO.Stream stream) { }
        public void WriteXml(System.IO.Stream stream, bool writeHierarchy) { }
        public void WriteXml(System.IO.Stream stream, System.Data.XmlWriteMode mode) { }
        public void WriteXml(System.IO.Stream stream, System.Data.XmlWriteMode mode, bool writeHierarchy) { }
        public void WriteXml(System.IO.TextWriter writer) { }
        public void WriteXml(System.IO.TextWriter writer, bool writeHierarchy) { }
        public void WriteXml(System.IO.TextWriter writer, System.Data.XmlWriteMode mode) { }
        public void WriteXml(System.IO.TextWriter writer, System.Data.XmlWriteMode mode, bool writeHierarchy) { }
        public void WriteXml(string fileName) { }
        public void WriteXml(string fileName, bool writeHierarchy) { }
        public void WriteXml(string fileName, System.Data.XmlWriteMode mode) { }
        public void WriteXml(string fileName, System.Data.XmlWriteMode mode, bool writeHierarchy) { }
        public void WriteXml(System.Xml.XmlWriter writer) { }
        public void WriteXml(System.Xml.XmlWriter writer, bool writeHierarchy) { }
        public void WriteXml(System.Xml.XmlWriter writer, System.Data.XmlWriteMode mode) { }
        public void WriteXml(System.Xml.XmlWriter writer, System.Data.XmlWriteMode mode, bool writeHierarchy) { }
        public void WriteXmlSchema(System.IO.Stream stream) { }
        public void WriteXmlSchema(System.IO.Stream stream, bool writeHierarchy) { }
        public void WriteXmlSchema(System.IO.TextWriter writer) { }
        public void WriteXmlSchema(System.IO.TextWriter writer, bool writeHierarchy) { }
        public void WriteXmlSchema(string fileName) { }
        public void WriteXmlSchema(string fileName, bool writeHierarchy) { }
        public void WriteXmlSchema(System.Xml.XmlWriter writer) { }
        public void WriteXmlSchema(System.Xml.XmlWriter writer, bool writeHierarchy) { }
    }
    public sealed partial class DataTableClearEventArgs : System.EventArgs
    {
        public DataTableClearEventArgs(System.Data.DataTable dataTable) { }
        public System.Data.DataTable Table { get { throw null; } }
        public string TableName { get { throw null; } }
        public string TableNamespace { get { throw null; } }
    }
    public delegate void DataTableClearEventHandler(object sender, System.Data.DataTableClearEventArgs e);
    [System.ComponentModel.DefaultEventAttribute("CollectionChanged")]
    [System.ComponentModel.ListBindableAttribute(false)]
    public sealed partial class DataTableCollection : System.Data.InternalDataCollectionBase
    {
        internal DataTableCollection() { }
        public System.Data.DataTable this[int index] { get { throw null; } }
        public System.Data.DataTable this[string name] { get { throw null; } }
        public System.Data.DataTable this[string name, string tableNamespace] { get { throw null; } }
        protected override System.Collections.ArrayList List { get { throw null; } }
        public event System.ComponentModel.CollectionChangeEventHandler CollectionChanged { add { } remove { } }
        public event System.ComponentModel.CollectionChangeEventHandler CollectionChanging { add { } remove { } }
        public System.Data.DataTable Add() { throw null; }
        public void Add(System.Data.DataTable table) { }
        public System.Data.DataTable Add(string name) { throw null; }
        public System.Data.DataTable Add(string name, string tableNamespace) { throw null; }
        public void AddRange(System.Data.DataTable[] tables) { }
        public bool CanRemove(System.Data.DataTable table) { throw null; }
        public void Clear() { }
        public bool Contains(string name) { throw null; }
        public bool Contains(string name, string tableNamespace) { throw null; }
        public void CopyTo(System.Data.DataTable[] array, int index) { }
        public int IndexOf(System.Data.DataTable table) { throw null; }
        public int IndexOf(string tableName) { throw null; }
        public int IndexOf(string tableName, string tableNamespace) { throw null; }
        public void Remove(System.Data.DataTable table) { }
        public void Remove(string name) { }
        public void Remove(string name, string tableNamespace) { }
        public void RemoveAt(int index) { }
    }
    public sealed partial class DataTableNewRowEventArgs : System.EventArgs
    {
        public DataTableNewRowEventArgs(System.Data.DataRow dataRow) { }
        public System.Data.DataRow Row { get { throw null; } }
    }
    public delegate void DataTableNewRowEventHandler(object sender, System.Data.DataTableNewRowEventArgs e);
    public sealed partial class DataTableReader : System.Data.Common.DbDataReader
    {
        public DataTableReader(System.Data.DataTable dataTable) { }
        public DataTableReader(System.Data.DataTable[] dataTables) { }
        public override int Depth { get { throw null; } }
        public override int FieldCount { get { throw null; } }
        public override bool HasRows { get { throw null; } }
        public override bool IsClosed { get { throw null; } }
        public override object this[int ordinal] { get { throw null; } }
        public override object this[string name] { get { throw null; } }
        public override int RecordsAffected { get { throw null; } }
        public override void Close() { }
        public override bool GetBoolean(int ordinal) { throw null; }
        public override byte GetByte(int ordinal) { throw null; }
        public override long GetBytes(int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length) { throw null; }
        public override char GetChar(int ordinal) { throw null; }
        public override long GetChars(int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length) { throw null; }
        public override string GetDataTypeName(int ordinal) { throw null; }
        public override System.DateTime GetDateTime(int ordinal) { throw null; }
        public override decimal GetDecimal(int ordinal) { throw null; }
        public override double GetDouble(int ordinal) { throw null; }
        public override System.Collections.IEnumerator GetEnumerator() { throw null; }
        public override System.Type GetFieldType(int ordinal) { throw null; }
        public override float GetFloat(int ordinal) { throw null; }
        public override System.Guid GetGuid(int ordinal) { throw null; }
        public override short GetInt16(int ordinal) { throw null; }
        public override int GetInt32(int ordinal) { throw null; }
        public override long GetInt64(int ordinal) { throw null; }
        public override string GetName(int ordinal) { throw null; }
        public override int GetOrdinal(string name) { throw null; }
        public override System.Type GetProviderSpecificFieldType(int ordinal) { throw null; }
        public override object GetProviderSpecificValue(int ordinal) { throw null; }
        public override int GetProviderSpecificValues(object[] values) { throw null; }
        public override System.Data.DataTable GetSchemaTable() { throw null; }
        public override string GetString(int ordinal) { throw null; }
        public override object GetValue(int ordinal) { throw null; }
        public override int GetValues(object[] values) { throw null; }
        public override bool IsDBNull(int ordinal) { throw null; }
        public override bool NextResult() { throw null; }
        public override bool Read() { throw null; }
    }
    [System.ComponentModel.DefaultEventAttribute("PositionChanged")]
    [System.ComponentModel.DefaultPropertyAttribute("Table")]
    public partial class DataView : System.ComponentModel.MarshalByValueComponent, System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList, System.ComponentModel.IBindingList, System.ComponentModel.IBindingListView, System.ComponentModel.ISupportInitialize, System.ComponentModel.ISupportInitializeNotification, System.ComponentModel.ITypedList
    {
        public DataView() { }
        public DataView(System.Data.DataTable table) { }
        public DataView(System.Data.DataTable table, string RowFilter, string Sort, System.Data.DataViewRowState RowState) { }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool AllowDelete { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool AllowEdit { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool AllowNew { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(false)]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public bool ApplyDefaultSort { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public int Count { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.DataViewManager DataViewManager { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsInitialized { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        protected bool IsOpen { get { throw null; } }
        public System.Data.DataRowView this[int recordIndex] { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public virtual string RowFilter { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.DataViewRowState)(22))]
        public System.Data.DataViewRowState RowStateFilter { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string Sort { get { throw null; } set { } }
        bool System.Collections.ICollection.IsSynchronized { get { throw null; } }
        object System.Collections.ICollection.SyncRoot { get { throw null; } }
        bool System.Collections.IList.IsFixedSize { get { throw null; } }
        bool System.Collections.IList.IsReadOnly { get { throw null; } }
        object System.Collections.IList.this[int recordIndex] { get { throw null; } set { } }
        bool System.ComponentModel.IBindingList.AllowEdit { get { throw null; } }
        bool System.ComponentModel.IBindingList.AllowNew { get { throw null; } }
        bool System.ComponentModel.IBindingList.AllowRemove { get { throw null; } }
        bool System.ComponentModel.IBindingList.IsSorted { get { throw null; } }
        System.ComponentModel.ListSortDirection System.ComponentModel.IBindingList.SortDirection { get { throw null; } }
        System.ComponentModel.PropertyDescriptor System.ComponentModel.IBindingList.SortProperty { get { throw null; } }
        bool System.ComponentModel.IBindingList.SupportsChangeNotification { get { throw null; } }
        bool System.ComponentModel.IBindingList.SupportsSearching { get { throw null; } }
        bool System.ComponentModel.IBindingList.SupportsSorting { get { throw null; } }
        string System.ComponentModel.IBindingListView.Filter { get { throw null; } set { } }
        System.ComponentModel.ListSortDescriptionCollection System.ComponentModel.IBindingListView.SortDescriptions { get { throw null; } }
        bool System.ComponentModel.IBindingListView.SupportsAdvancedSorting { get { throw null; } }
        bool System.ComponentModel.IBindingListView.SupportsFiltering { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute(null)]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        [System.ComponentModel.TypeConverterAttribute("System.Data.DataTableTypeConverter")]
        public System.Data.DataTable Table { get { throw null; } set { } }
        public event System.EventHandler Initialized { add { } remove { } }
        public event System.ComponentModel.ListChangedEventHandler ListChanged { add { } remove { } }
        public virtual System.Data.DataRowView AddNew() { throw null; }
        public void BeginInit() { }
        protected void Close() { }
        protected virtual void ColumnCollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e) { }
        public void CopyTo(System.Array array, int index) { }
        public void Delete(int index) { }
        protected override void Dispose(bool disposing) { }
        public void EndInit() { }
        public virtual bool Equals(System.Data.DataView view) { throw null; }
        public int Find(object key) { throw null; }
        public int Find(object[] key) { throw null; }
        public System.Data.DataRowView[] FindRows(object key) { throw null; }
        public System.Data.DataRowView[] FindRows(object[] key) { throw null; }
        public System.Collections.IEnumerator GetEnumerator() { throw null; }
        protected virtual void IndexListChanged(object sender, System.ComponentModel.ListChangedEventArgs e) { }
        protected virtual void OnListChanged(System.ComponentModel.ListChangedEventArgs e) { }
        protected void Open() { }
        protected void Reset() { }
        int System.Collections.IList.Add(object value) { throw null; }
        void System.Collections.IList.Clear() { }
        bool System.Collections.IList.Contains(object value) { throw null; }
        int System.Collections.IList.IndexOf(object value) { throw null; }
        void System.Collections.IList.Insert(int index, object value) { }
        void System.Collections.IList.Remove(object value) { }
        void System.Collections.IList.RemoveAt(int index) { }
        void System.ComponentModel.IBindingList.AddIndex(System.ComponentModel.PropertyDescriptor property) { }
        object System.ComponentModel.IBindingList.AddNew() { throw null; }
        void System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor property, System.ComponentModel.ListSortDirection direction) { }
        int System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor property, object key) { throw null; }
        void System.ComponentModel.IBindingList.RemoveIndex(System.ComponentModel.PropertyDescriptor property) { }
        void System.ComponentModel.IBindingList.RemoveSort() { }
        void System.ComponentModel.IBindingListView.ApplySort(System.ComponentModel.ListSortDescriptionCollection sorts) { }
        void System.ComponentModel.IBindingListView.RemoveFilter() { }
        System.ComponentModel.PropertyDescriptorCollection System.ComponentModel.ITypedList.GetItemProperties(System.ComponentModel.PropertyDescriptor[] listAccessors) { throw null; }
        string System.ComponentModel.ITypedList.GetListName(System.ComponentModel.PropertyDescriptor[] listAccessors) { throw null; }
        public System.Data.DataTable ToTable() { throw null; }
        public System.Data.DataTable ToTable(bool distinct, params string[] columnNames) { throw null; }
        public System.Data.DataTable ToTable(string tableName) { throw null; }
        public System.Data.DataTable ToTable(string tableName, bool distinct, params string[] columnNames) { throw null; }
        protected void UpdateIndex() { }
        protected virtual void UpdateIndex(bool force) { }
    }
    public partial class DataViewManager : System.ComponentModel.MarshalByValueComponent, System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList, System.ComponentModel.IBindingList, System.ComponentModel.ITypedList
    {
        public DataViewManager() { }
        public DataViewManager(System.Data.DataSet dataSet) { }
        [System.ComponentModel.DefaultValueAttribute(null)]
        public System.Data.DataSet DataSet { get { throw null; } set { } }
        public string DataViewSettingCollectionString { get { throw null; } set { } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(2))]
        public System.Data.DataViewSettingCollection DataViewSettings { get { throw null; } }
        int System.Collections.ICollection.Count { get { throw null; } }
        bool System.Collections.ICollection.IsSynchronized { get { throw null; } }
        object System.Collections.ICollection.SyncRoot { get { throw null; } }
        bool System.Collections.IList.IsFixedSize { get { throw null; } }
        bool System.Collections.IList.IsReadOnly { get { throw null; } }
        object System.Collections.IList.this[int index] { get { throw null; } set { } }
        bool System.ComponentModel.IBindingList.AllowEdit { get { throw null; } }
        bool System.ComponentModel.IBindingList.AllowNew { get { throw null; } }
        bool System.ComponentModel.IBindingList.AllowRemove { get { throw null; } }
        bool System.ComponentModel.IBindingList.IsSorted { get { throw null; } }
        System.ComponentModel.ListSortDirection System.ComponentModel.IBindingList.SortDirection { get { throw null; } }
        System.ComponentModel.PropertyDescriptor System.ComponentModel.IBindingList.SortProperty { get { throw null; } }
        bool System.ComponentModel.IBindingList.SupportsChangeNotification { get { throw null; } }
        bool System.ComponentModel.IBindingList.SupportsSearching { get { throw null; } }
        bool System.ComponentModel.IBindingList.SupportsSorting { get { throw null; } }
        public event System.ComponentModel.ListChangedEventHandler ListChanged { add { } remove { } }
        public System.Data.DataView CreateDataView(System.Data.DataTable table) { throw null; }
        protected virtual void OnListChanged(System.ComponentModel.ListChangedEventArgs e) { }
        protected virtual void RelationCollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e) { }
        void System.Collections.ICollection.CopyTo(System.Array array, int index) { }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw null; }
        int System.Collections.IList.Add(object value) { throw null; }
        void System.Collections.IList.Clear() { }
        bool System.Collections.IList.Contains(object value) { throw null; }
        int System.Collections.IList.IndexOf(object value) { throw null; }
        void System.Collections.IList.Insert(int index, object value) { }
        void System.Collections.IList.Remove(object value) { }
        void System.Collections.IList.RemoveAt(int index) { }
        void System.ComponentModel.IBindingList.AddIndex(System.ComponentModel.PropertyDescriptor property) { }
        object System.ComponentModel.IBindingList.AddNew() { throw null; }
        void System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor property, System.ComponentModel.ListSortDirection direction) { }
        int System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor property, object key) { throw null; }
        void System.ComponentModel.IBindingList.RemoveIndex(System.ComponentModel.PropertyDescriptor property) { }
        void System.ComponentModel.IBindingList.RemoveSort() { }
        System.ComponentModel.PropertyDescriptorCollection System.ComponentModel.ITypedList.GetItemProperties(System.ComponentModel.PropertyDescriptor[] listAccessors) { throw null; }
        string System.ComponentModel.ITypedList.GetListName(System.ComponentModel.PropertyDescriptor[] listAccessors) { throw null; }
        protected virtual void TableCollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e) { }
    }
    [System.FlagsAttribute]
    public enum DataViewRowState
    {
        Added = 4,
        CurrentRows = 22,
        Deleted = 8,
        ModifiedCurrent = 16,
        ModifiedOriginal = 32,
        None = 0,
        OriginalRows = 42,
        Unchanged = 2,
    }
    [System.ComponentModel.TypeConverterAttribute(typeof(System.ComponentModel.ExpandableObjectConverter))]
    public partial class DataViewSetting
    {
        internal DataViewSetting() { }
        public bool ApplyDefaultSort { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.DataViewManager DataViewManager { get { throw null; } }
        public string RowFilter { get { throw null; } set { } }
        public System.Data.DataViewRowState RowStateFilter { get { throw null; } set { } }
        public string Sort { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Data.DataTable Table { get { throw null; } }
    }
    public partial class DataViewSettingCollection : System.Collections.ICollection, System.Collections.IEnumerable
    {
        internal DataViewSettingCollection() { }
        [System.ComponentModel.BrowsableAttribute(false)]
        public virtual int Count { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsReadOnly { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsSynchronized { get { throw null; } }
        public virtual System.Data.DataViewSetting this[System.Data.DataTable table] { get { throw null; } set { } }
        public virtual System.Data.DataViewSetting this[int index] { get { throw null; } set { } }
        public virtual System.Data.DataViewSetting this[string tableName] { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public object SyncRoot { get { throw null; } }
        public void CopyTo(System.Array ar, int index) { }
        public void CopyTo(System.Data.DataViewSetting[] ar, int index) { }
        public System.Collections.IEnumerator GetEnumerator() { throw null; }
    }
    [System.SerializableAttribute]
    public sealed partial class DBConcurrencyException : System.SystemException
    {
        public DBConcurrencyException() { }
        public DBConcurrencyException(string message) { }
        public DBConcurrencyException(string message, System.Exception inner) { }
        public DBConcurrencyException(string message, System.Exception inner, System.Data.DataRow[] dataRows) { }
        public System.Data.DataRow Row { get { throw null; } set { } }
        public int RowCount { get { throw null; } }
        public void CopyToRows(System.Data.DataRow[] array) { }
        public void CopyToRows(System.Data.DataRow[] array, int arrayIndex) { }
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
    public enum DbType
    {
        AnsiString = 0,
        AnsiStringFixedLength = 22,
        Binary = 1,
        Boolean = 3,
        Byte = 2,
        Currency = 4,
        Date = 5,
        DateTime = 6,
        DateTime2 = 26,
        DateTimeOffset = 27,
        Decimal = 7,
        Double = 8,
        Guid = 9,
        Int16 = 10,
        Int32 = 11,
        Int64 = 12,
        Object = 13,
        SByte = 14,
        Single = 15,
        String = 16,
        StringFixedLength = 23,
        Time = 17,
        UInt16 = 18,
        UInt32 = 19,
        UInt64 = 20,
        VarNumeric = 21,
        Xml = 25,
    }
    [System.SerializableAttribute]
    public partial class DeletedRowInaccessibleException : System.Data.DataException
    {
        public DeletedRowInaccessibleException() { }
        protected DeletedRowInaccessibleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public DeletedRowInaccessibleException(string s) { }
        public DeletedRowInaccessibleException(string message, System.Exception innerException) { }
    }
    [System.SerializableAttribute]
    public partial class DuplicateNameException : System.Data.DataException
    {
        public DuplicateNameException() { }
        protected DuplicateNameException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public DuplicateNameException(string s) { }
        public DuplicateNameException(string message, System.Exception innerException) { }
    }
    [System.SerializableAttribute]
    public partial class EvaluateException : System.Data.InvalidExpressionException
    {
        public EvaluateException() { }
        protected EvaluateException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public EvaluateException(string s) { }
        public EvaluateException(string message, System.Exception innerException) { }
    }
    public partial class FillErrorEventArgs : System.EventArgs
    {
        public FillErrorEventArgs(System.Data.DataTable dataTable, object[] values) { }
        public bool Continue { get { throw null; } set { } }
        public System.Data.DataTable DataTable { get { throw null; } }
        public System.Exception Errors { get { throw null; } set { } }
        public object[] Values { get { throw null; } }
    }
    public delegate void FillErrorEventHandler(object sender, System.Data.FillErrorEventArgs e);
    [System.ComponentModel.DefaultPropertyAttribute("ConstraintName")]
    public partial class ForeignKeyConstraint : System.Data.Constraint
    {
        public ForeignKeyConstraint(System.Data.DataColumn parentColumn, System.Data.DataColumn childColumn) { }
        public ForeignKeyConstraint(System.Data.DataColumn[] parentColumns, System.Data.DataColumn[] childColumns) { }
        public ForeignKeyConstraint(string constraintName, System.Data.DataColumn parentColumn, System.Data.DataColumn childColumn) { }
        public ForeignKeyConstraint(string constraintName, System.Data.DataColumn[] parentColumns, System.Data.DataColumn[] childColumns) { }
        [System.ComponentModel.BrowsableAttribute(false)]
        public ForeignKeyConstraint(string constraintName, string parentTableName, string parentTableNamespace, string[] parentColumnNames, string[] childColumnNames, System.Data.AcceptRejectRule acceptRejectRule, System.Data.Rule deleteRule, System.Data.Rule updateRule) { }
        [System.ComponentModel.BrowsableAttribute(false)]
        public ForeignKeyConstraint(string constraintName, string parentTableName, string[] parentColumnNames, string[] childColumnNames, System.Data.AcceptRejectRule acceptRejectRule, System.Data.Rule deleteRule, System.Data.Rule updateRule) { }
        [System.ComponentModel.DefaultValueAttribute((System.Data.AcceptRejectRule)(0))]
        public virtual System.Data.AcceptRejectRule AcceptRejectRule { get { throw null; } set { } }
        [System.ComponentModel.ReadOnlyAttribute(true)]
        public virtual System.Data.DataColumn[] Columns { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.Rule)(1))]
        public virtual System.Data.Rule DeleteRule { get { throw null; } set { } }
        [System.ComponentModel.ReadOnlyAttribute(true)]
        public virtual System.Data.DataColumn[] RelatedColumns { get { throw null; } }
        [System.ComponentModel.ReadOnlyAttribute(true)]
        public virtual System.Data.DataTable RelatedTable { get { throw null; } }
        [System.ComponentModel.ReadOnlyAttribute(true)]
        public override System.Data.DataTable Table { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.Rule)(1))]
        public virtual System.Data.Rule UpdateRule { get { throw null; } set { } }
        public override bool Equals(object key) { throw null; }
        public override int GetHashCode() { throw null; }
    }
    public partial interface IColumnMapping
    {
        string DataSetColumn { get; set; }
        string SourceColumn { get; set; }
    }
    public partial interface IColumnMappingCollection : System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList
    {
        object this[string index] { get; set; }
        System.Data.IColumnMapping Add(string sourceColumnName, string dataSetColumnName);
        bool Contains(string sourceColumnName);
        System.Data.IColumnMapping GetByDataSetColumn(string dataSetColumnName);
        int IndexOf(string sourceColumnName);
        void RemoveAt(string sourceColumnName);
    }
    public partial interface IDataAdapter
    {
        System.Data.MissingMappingAction MissingMappingAction { get; set; }
        System.Data.MissingSchemaAction MissingSchemaAction { get; set; }
        System.Data.ITableMappingCollection TableMappings { get; }
        int Fill(System.Data.DataSet dataSet);
        System.Data.DataTable[] FillSchema(System.Data.DataSet dataSet, System.Data.SchemaType schemaType);
        System.Data.IDataParameter[] GetFillParameters();
        int Update(System.Data.DataSet dataSet);
    }
    public partial interface IDataParameter
    {
        System.Data.DbType DbType { get; set; }
        System.Data.ParameterDirection Direction { get; set; }
        bool IsNullable { get; }
        string ParameterName { get; set; }
        string SourceColumn { get; set; }
        System.Data.DataRowVersion SourceVersion { get; set; }
        object Value { get; set; }
    }
    public partial interface IDataParameterCollection : System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList
    {
        object this[string parameterName] { get; set; }
        bool Contains(string parameterName);
        int IndexOf(string parameterName);
        void RemoveAt(string parameterName);
    }
    public partial interface IDataReader : System.Data.IDataRecord, System.IDisposable
    {
        int Depth { get; }
        bool IsClosed { get; }
        int RecordsAffected { get; }
        void Close();
        System.Data.DataTable GetSchemaTable();
        bool NextResult();
        bool Read();
    }
    public partial interface IDataRecord
    {
        int FieldCount { get; }
        object this[int i] { get; }
        object this[string name] { get; }
        bool GetBoolean(int i);
        byte GetByte(int i);
        long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length);
        char GetChar(int i);
        long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length);
        System.Data.IDataReader GetData(int i);
        string GetDataTypeName(int i);
        System.DateTime GetDateTime(int i);
        decimal GetDecimal(int i);
        double GetDouble(int i);
        System.Type GetFieldType(int i);
        float GetFloat(int i);
        System.Guid GetGuid(int i);
        short GetInt16(int i);
        int GetInt32(int i);
        long GetInt64(int i);
        string GetName(int i);
        int GetOrdinal(string name);
        string GetString(int i);
        object GetValue(int i);
        int GetValues(object[] values);
        bool IsDBNull(int i);
    }
    public partial interface IDbCommand : System.IDisposable
    {
        string CommandText { get; set; }
        int CommandTimeout { get; set; }
        System.Data.CommandType CommandType { get; set; }
        System.Data.IDbConnection Connection { get; set; }
        System.Data.IDataParameterCollection Parameters { get; }
        System.Data.IDbTransaction Transaction { get; set; }
        System.Data.UpdateRowSource UpdatedRowSource { get; set; }
        void Cancel();
        System.Data.IDbDataParameter CreateParameter();
        int ExecuteNonQuery();
        System.Data.IDataReader ExecuteReader();
        System.Data.IDataReader ExecuteReader(System.Data.CommandBehavior behavior);
        object ExecuteScalar();
        void Prepare();
    }
    public partial interface IDbConnection : System.IDisposable
    {
        string ConnectionString { get; set; }
        int ConnectionTimeout { get; }
        string Database { get; }
        System.Data.ConnectionState State { get; }
        System.Data.IDbTransaction BeginTransaction();
        System.Data.IDbTransaction BeginTransaction(System.Data.IsolationLevel il);
        void ChangeDatabase(string databaseName);
        void Close();
        System.Data.IDbCommand CreateCommand();
        void Open();
    }
    public partial interface IDbDataAdapter : System.Data.IDataAdapter
    {
        System.Data.IDbCommand DeleteCommand { get; set; }
        System.Data.IDbCommand InsertCommand { get; set; }
        System.Data.IDbCommand SelectCommand { get; set; }
        System.Data.IDbCommand UpdateCommand { get; set; }
    }
    public partial interface IDbDataParameter : System.Data.IDataParameter
    {
        byte Precision { get; set; }
        byte Scale { get; set; }
        int Size { get; set; }
    }
    public partial interface IDbTransaction : System.IDisposable
    {
        System.Data.IDbConnection Connection { get; }
        System.Data.IsolationLevel IsolationLevel { get; }
        void Commit();
        void Rollback();
    }
    [System.SerializableAttribute]
    public partial class InRowChangingEventException : System.Data.DataException
    {
        public InRowChangingEventException() { }
        protected InRowChangingEventException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public InRowChangingEventException(string s) { }
        public InRowChangingEventException(string message, System.Exception innerException) { }
    }
    public partial class InternalDataCollectionBase : System.Collections.ICollection, System.Collections.IEnumerable
    {
        public InternalDataCollectionBase() { }
        [System.ComponentModel.BrowsableAttribute(false)]
        public virtual int Count { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsReadOnly { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsSynchronized { get { throw null; } }
        protected virtual System.Collections.ArrayList List { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public object SyncRoot { get { throw null; } }
        public virtual void CopyTo(System.Array ar, int index) { }
        public virtual System.Collections.IEnumerator GetEnumerator() { throw null; }
    }
    [System.SerializableAttribute]
    public partial class InvalidConstraintException : System.Data.DataException
    {
        public InvalidConstraintException() { }
        protected InvalidConstraintException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public InvalidConstraintException(string s) { }
        public InvalidConstraintException(string message, System.Exception innerException) { }
    }
    [System.SerializableAttribute]
    public partial class InvalidExpressionException : System.Data.DataException
    {
        public InvalidExpressionException() { }
        protected InvalidExpressionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public InvalidExpressionException(string s) { }
        public InvalidExpressionException(string message, System.Exception innerException) { }
    }
    public enum IsolationLevel
    {
        Chaos = 16,
        ReadCommitted = 4096,
        ReadUncommitted = 256,
        RepeatableRead = 65536,
        Serializable = 1048576,
        Snapshot = 16777216,
        Unspecified = -1,
    }
    public partial interface ITableMapping
    {
        System.Data.IColumnMappingCollection ColumnMappings { get; }
        string DataSetTable { get; set; }
        string SourceTable { get; set; }
    }
    public partial interface ITableMappingCollection : System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList
    {
        object this[string index] { get; set; }
        System.Data.ITableMapping Add(string sourceTableName, string dataSetTableName);
        bool Contains(string sourceTableName);
        System.Data.ITableMapping GetByDataSetTable(string dataSetTableName);
        int IndexOf(string sourceTableName);
        void RemoveAt(string sourceTableName);
    }
    public enum KeyRestrictionBehavior
    {
        AllowOnly = 0,
        PreventUsage = 1,
    }
    public enum LoadOption
    {
        OverwriteChanges = 1,
        PreserveChanges = 2,
        Upsert = 3,
    }
    public enum MappingType
    {
        Attribute = 2,
        Element = 1,
        Hidden = 4,
        SimpleContent = 3,
    }
    public partial class MergeFailedEventArgs : System.EventArgs
    {
        public MergeFailedEventArgs(System.Data.DataTable table, string conflict) { }
        public string Conflict { get { throw null; } }
        public System.Data.DataTable Table { get { throw null; } }
    }
    public delegate void MergeFailedEventHandler(object sender, System.Data.MergeFailedEventArgs e);
    public enum MissingMappingAction
    {
        Error = 3,
        Ignore = 2,
        Passthrough = 1,
    }
    [System.SerializableAttribute]
    public partial class MissingPrimaryKeyException : System.Data.DataException
    {
        public MissingPrimaryKeyException() { }
        protected MissingPrimaryKeyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public MissingPrimaryKeyException(string s) { }
        public MissingPrimaryKeyException(string message, System.Exception innerException) { }
    }
    public enum MissingSchemaAction
    {
        Add = 1,
        AddWithKey = 4,
        Error = 3,
        Ignore = 2,
    }
    [System.SerializableAttribute]
    public partial class NoNullAllowedException : System.Data.DataException
    {
        public NoNullAllowedException() { }
        protected NoNullAllowedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public NoNullAllowedException(string s) { }
        public NoNullAllowedException(string message, System.Exception innerException) { }
    }
    [System.SerializableAttribute]
    public sealed partial class OperationAbortedException : System.SystemException
    {
        internal OperationAbortedException() { }
    }
    public enum ParameterDirection
    {
        Input = 1,
        InputOutput = 3,
        Output = 2,
        ReturnValue = 6,
    }
    [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
    [System.FlagsAttribute]
    [System.ObsoleteAttribute("PropertyAttributes has been deprecated.  http://go.microsoft.com/fwlink/?linkid=14202")]
    public enum PropertyAttributes
    {
        NotSupported = 0,
        Optional = 2,
        Read = 512,
        Required = 1,
        Write = 1024,
    }
    [System.SerializableAttribute]
    public partial class PropertyCollection : System.Collections.Hashtable, System.ICloneable
    {
        public PropertyCollection() { }
        protected PropertyCollection(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public override object Clone() { throw null; }
    }
    [System.SerializableAttribute]
    public partial class ReadOnlyException : System.Data.DataException
    {
        public ReadOnlyException() { }
        protected ReadOnlyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public ReadOnlyException(string s) { }
        public ReadOnlyException(string message, System.Exception innerException) { }
    }
    [System.SerializableAttribute]
    public partial class RowNotInTableException : System.Data.DataException
    {
        public RowNotInTableException() { }
        protected RowNotInTableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public RowNotInTableException(string s) { }
        public RowNotInTableException(string message, System.Exception innerException) { }
    }
    public enum Rule
    {
        Cascade = 1,
        None = 0,
        SetDefault = 3,
        SetNull = 2,
    }
    public enum SchemaSerializationMode
    {
        ExcludeSchema = 2,
        IncludeSchema = 1,
    }
    public enum SchemaType
    {
        Mapped = 2,
        Source = 1,
    }
    public enum SerializationFormat
    {
        Binary = 1,
        Xml = 0,
    }
    public enum SqlDbType
    {
        BigInt = 0,
        Binary = 1,
        Bit = 2,
        Char = 3,
        Date = 31,
        DateTime = 4,
        DateTime2 = 33,
        DateTimeOffset = 34,
        Decimal = 5,
        Float = 6,
        Image = 7,
        Int = 8,
        Money = 9,
        NChar = 10,
        NText = 11,
        NVarChar = 12,
        Real = 13,
        SmallDateTime = 15,
        SmallInt = 16,
        SmallMoney = 17,
        Structured = 30,
        Text = 18,
        Time = 32,
        Timestamp = 19,
        TinyInt = 20,
        Udt = 29,
        UniqueIdentifier = 14,
        VarBinary = 21,
        VarChar = 22,
        Variant = 23,
        Xml = 25,
    }
    public sealed partial class StateChangeEventArgs : System.EventArgs
    {
        public StateChangeEventArgs(System.Data.ConnectionState originalState, System.Data.ConnectionState currentState) { }
        public System.Data.ConnectionState CurrentState { get { throw null; } }
        public System.Data.ConnectionState OriginalState { get { throw null; } }
    }
    public delegate void StateChangeEventHandler(object sender, System.Data.StateChangeEventArgs e);
    public sealed partial class StatementCompletedEventArgs : System.EventArgs
    {
        public StatementCompletedEventArgs(int recordCount) { }
        public int RecordCount { get { throw null; } }
    }
    public delegate void StatementCompletedEventHandler(object sender, System.Data.StatementCompletedEventArgs e);
    public enum StatementType
    {
        Batch = 4,
        Delete = 3,
        Insert = 1,
        Select = 0,
        Update = 2,
    }
    [System.SerializableAttribute]
    public partial class StrongTypingException : System.Data.DataException
    {
        public StrongTypingException() { }
        protected StrongTypingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public StrongTypingException(string message) { }
        public StrongTypingException(string s, System.Exception innerException) { }
    }
    [System.SerializableAttribute]
    public partial class SyntaxErrorException : System.Data.InvalidExpressionException
    {
        public SyntaxErrorException() { }
        protected SyntaxErrorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public SyntaxErrorException(string s) { }
        public SyntaxErrorException(string message, System.Exception innerException) { }
    }
    [System.SerializableAttribute]
    public partial class TypedDataSetGeneratorException : System.Data.DataException
    {
        public TypedDataSetGeneratorException() { }
        public TypedDataSetGeneratorException(System.Collections.ArrayList list) { }
        protected TypedDataSetGeneratorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public TypedDataSetGeneratorException(string message) { }
        public TypedDataSetGeneratorException(string message, System.Exception innerException) { }
        public System.Collections.ArrayList ErrorList { get { throw null; } }
        [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags=(System.Security.Permissions.SecurityPermissionFlag)(128))]
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
    [System.ComponentModel.DefaultPropertyAttribute("ConstraintName")]
    public partial class UniqueConstraint : System.Data.Constraint
    {
        public UniqueConstraint(System.Data.DataColumn column) { }
        public UniqueConstraint(System.Data.DataColumn column, bool isPrimaryKey) { }
        public UniqueConstraint(System.Data.DataColumn[] columns) { }
        public UniqueConstraint(System.Data.DataColumn[] columns, bool isPrimaryKey) { }
        public UniqueConstraint(string name, System.Data.DataColumn column) { }
        public UniqueConstraint(string name, System.Data.DataColumn column, bool isPrimaryKey) { }
        public UniqueConstraint(string name, System.Data.DataColumn[] columns) { }
        public UniqueConstraint(string name, System.Data.DataColumn[] columns, bool isPrimaryKey) { }
        [System.ComponentModel.BrowsableAttribute(false)]
        public UniqueConstraint(string name, string[] columnNames, bool isPrimaryKey) { }
        [System.ComponentModel.ReadOnlyAttribute(true)]
        public virtual System.Data.DataColumn[] Columns { get { throw null; } }
        public bool IsPrimaryKey { get { throw null; } }
        [System.ComponentModel.ReadOnlyAttribute(true)]
        public override System.Data.DataTable Table { get { throw null; } }
        public override bool Equals(object key2) { throw null; }
        public override int GetHashCode() { throw null; }
    }
    public enum UpdateRowSource
    {
        Both = 3,
        FirstReturnedRecord = 2,
        None = 0,
        OutputParameters = 1,
    }
    public enum UpdateStatus
    {
        Continue = 0,
        ErrorsOccurred = 1,
        SkipAllRemainingRows = 3,
        SkipCurrentRow = 2,
    }
    [System.SerializableAttribute]
    public partial class VersionNotFoundException : System.Data.DataException
    {
        public VersionNotFoundException() { }
        protected VersionNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public VersionNotFoundException(string s) { }
        public VersionNotFoundException(string message, System.Exception innerException) { }
    }
    public enum XmlReadMode
    {
        Auto = 0,
        DiffGram = 4,
        Fragment = 5,
        IgnoreSchema = 2,
        InferSchema = 3,
        InferTypedSchema = 6,
        ReadSchema = 1,
    }
    public enum XmlWriteMode
    {
        DiffGram = 2,
        IgnoreSchema = 1,
        WriteSchema = 0,
    }
}
namespace System.Data.Common
{
    public enum CatalogLocation
    {
        End = 2,
        Start = 1,
    }
    public partial class DataAdapter : System.ComponentModel.Component, System.Data.IDataAdapter
    {
        protected DataAdapter() { }
        protected DataAdapter(System.Data.Common.DataAdapter from) { }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool AcceptChangesDuringFill { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool AcceptChangesDuringUpdate { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ContinueUpdateOnError { get { throw null; } set { } }
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public System.Data.LoadOption FillLoadOption { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.MissingMappingAction)(1))]
        public System.Data.MissingMappingAction MissingMappingAction { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.MissingSchemaAction)(1))]
        public System.Data.MissingSchemaAction MissingSchemaAction { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(false)]
        public virtual bool ReturnProviderSpecificTypes { get { throw null; } set { } }
        System.Data.ITableMappingCollection System.Data.IDataAdapter.TableMappings { get { throw null; } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(2))]
        public System.Data.Common.DataTableMappingCollection TableMappings { get { throw null; } }
        public event System.Data.FillErrorEventHandler FillError { add { } remove { } }
        [System.ObsoleteAttribute("CloneInternals() has been deprecated.  Use the DataAdapter(DataAdapter from) constructor.  http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual System.Data.Common.DataAdapter CloneInternals() { throw null; }
        protected virtual System.Data.Common.DataTableMappingCollection CreateTableMappings() { throw null; }
        protected override void Dispose(bool disposing) { }
        public virtual int Fill(System.Data.DataSet dataSet) { throw null; }
        protected virtual int Fill(System.Data.DataSet dataSet, string srcTable, System.Data.IDataReader dataReader, int startRecord, int maxRecords) { throw null; }
        protected virtual int Fill(System.Data.DataTable dataTable, System.Data.IDataReader dataReader) { throw null; }
        protected virtual int Fill(System.Data.DataTable[] dataTables, System.Data.IDataReader dataReader, int startRecord, int maxRecords) { throw null; }
        public virtual System.Data.DataTable[] FillSchema(System.Data.DataSet dataSet, System.Data.SchemaType schemaType) { throw null; }
        protected virtual System.Data.DataTable[] FillSchema(System.Data.DataSet dataSet, System.Data.SchemaType schemaType, string srcTable, System.Data.IDataReader dataReader) { throw null; }
        protected virtual System.Data.DataTable FillSchema(System.Data.DataTable dataTable, System.Data.SchemaType schemaType, System.Data.IDataReader dataReader) { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public virtual System.Data.IDataParameter[] GetFillParameters() { throw null; }
        protected bool HasTableMappings() { throw null; }
        protected virtual void OnFillError(System.Data.FillErrorEventArgs value) { }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public void ResetFillLoadOption() { }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public virtual bool ShouldSerializeAcceptChangesDuringFill() { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public virtual bool ShouldSerializeFillLoadOption() { throw null; }
        protected virtual bool ShouldSerializeTableMappings() { throw null; }
        public virtual int Update(System.Data.DataSet dataSet) { throw null; }
    }
    [System.ComponentModel.TypeConverterAttribute("System.Data.Common.DataColumnMapping.DataColumnMappingConverter")]
    public sealed partial class DataColumnMapping : System.MarshalByRefObject, System.Data.IColumnMapping, System.ICloneable
    {
        public DataColumnMapping() { }
        public DataColumnMapping(string sourceColumn, string dataSetColumn) { }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string DataSetColumn { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string SourceColumn { get { throw null; } set { } }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public System.Data.DataColumn GetDataColumnBySchemaAction(System.Data.DataTable dataTable, System.Type dataType, System.Data.MissingSchemaAction schemaAction) { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public static System.Data.DataColumn GetDataColumnBySchemaAction(string sourceColumn, string dataSetColumn, System.Data.DataTable dataTable, System.Type dataType, System.Data.MissingSchemaAction schemaAction) { throw null; }
        object System.ICloneable.Clone() { throw null; }
        public override string ToString() { throw null; }
    }
    public sealed partial class DataColumnMappingCollection : System.MarshalByRefObject, System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList, System.Data.IColumnMappingCollection
    {
        public DataColumnMappingCollection() { }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public int Count { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DataColumnMapping this[int index] { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DataColumnMapping this[string sourceColumn] { get { throw null; } set { } }
        bool System.Collections.ICollection.IsSynchronized { get { throw null; } }
        object System.Collections.ICollection.SyncRoot { get { throw null; } }
        bool System.Collections.IList.IsFixedSize { get { throw null; } }
        bool System.Collections.IList.IsReadOnly { get { throw null; } }
        object System.Collections.IList.this[int index] { get { throw null; } set { } }
        object System.Data.IColumnMappingCollection.this[string index] { get { throw null; } set { } }
        public int Add(object value) { throw null; }
        public System.Data.Common.DataColumnMapping Add(string sourceColumn, string dataSetColumn) { throw null; }
        public void AddRange(System.Array values) { }
        public void AddRange(System.Data.Common.DataColumnMapping[] values) { }
        public void Clear() { }
        public bool Contains(object value) { throw null; }
        public bool Contains(string value) { throw null; }
        public void CopyTo(System.Array array, int index) { }
        public void CopyTo(System.Data.Common.DataColumnMapping[] array, int index) { }
        public System.Data.Common.DataColumnMapping GetByDataSetColumn(string value) { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public static System.Data.Common.DataColumnMapping GetColumnMappingBySchemaAction(System.Data.Common.DataColumnMappingCollection columnMappings, string sourceColumn, System.Data.MissingMappingAction mappingAction) { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public static System.Data.DataColumn GetDataColumn(System.Data.Common.DataColumnMappingCollection columnMappings, string sourceColumn, System.Type dataType, System.Data.DataTable dataTable, System.Data.MissingMappingAction mappingAction, System.Data.MissingSchemaAction schemaAction) { throw null; }
        public System.Collections.IEnumerator GetEnumerator() { throw null; }
        public int IndexOf(object value) { throw null; }
        public int IndexOf(string sourceColumn) { throw null; }
        public int IndexOfDataSetColumn(string dataSetColumn) { throw null; }
        public void Insert(int index, System.Data.Common.DataColumnMapping value) { }
        public void Insert(int index, object value) { }
        public void Remove(System.Data.Common.DataColumnMapping value) { }
        public void Remove(object value) { }
        public void RemoveAt(int index) { }
        public void RemoveAt(string sourceColumn) { }
        System.Data.IColumnMapping System.Data.IColumnMappingCollection.Add(string sourceColumnName, string dataSetColumnName) { throw null; }
        System.Data.IColumnMapping System.Data.IColumnMappingCollection.GetByDataSetColumn(string dataSetColumnName) { throw null; }
    }
    [System.ComponentModel.TypeConverterAttribute("System.Data.Common.DataTableMapping.DataTableMappingConverter")]
    public sealed partial class DataTableMapping : System.MarshalByRefObject, System.Data.ITableMapping, System.ICloneable
    {
        public DataTableMapping() { }
        public DataTableMapping(string sourceTable, string dataSetTable) { }
        public DataTableMapping(string sourceTable, string dataSetTable, System.Data.Common.DataColumnMapping[] columnMappings) { }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(2))]
        public System.Data.Common.DataColumnMappingCollection ColumnMappings { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string DataSetTable { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public string SourceTable { get { throw null; } set { } }
        System.Data.IColumnMappingCollection System.Data.ITableMapping.ColumnMappings { get { throw null; } }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public System.Data.Common.DataColumnMapping GetColumnMappingBySchemaAction(string sourceColumn, System.Data.MissingMappingAction mappingAction) { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public System.Data.DataColumn GetDataColumn(string sourceColumn, System.Type dataType, System.Data.DataTable dataTable, System.Data.MissingMappingAction mappingAction, System.Data.MissingSchemaAction schemaAction) { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public System.Data.DataTable GetDataTableBySchemaAction(System.Data.DataSet dataSet, System.Data.MissingSchemaAction schemaAction) { throw null; }
        object System.ICloneable.Clone() { throw null; }
        public override string ToString() { throw null; }
    }
    [System.ComponentModel.ListBindableAttribute(false)]
    public sealed partial class DataTableMappingCollection : System.MarshalByRefObject, System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList, System.Data.ITableMappingCollection
    {
        public DataTableMappingCollection() { }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public int Count { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DataTableMapping this[int index] { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DataTableMapping this[string sourceTable] { get { throw null; } set { } }
        bool System.Collections.ICollection.IsSynchronized { get { throw null; } }
        object System.Collections.ICollection.SyncRoot { get { throw null; } }
        bool System.Collections.IList.IsFixedSize { get { throw null; } }
        bool System.Collections.IList.IsReadOnly { get { throw null; } }
        object System.Collections.IList.this[int index] { get { throw null; } set { } }
        object System.Data.ITableMappingCollection.this[string index] { get { throw null; } set { } }
        public int Add(object value) { throw null; }
        public System.Data.Common.DataTableMapping Add(string sourceTable, string dataSetTable) { throw null; }
        public void AddRange(System.Array values) { }
        public void AddRange(System.Data.Common.DataTableMapping[] values) { }
        public void Clear() { }
        public bool Contains(object value) { throw null; }
        public bool Contains(string value) { throw null; }
        public void CopyTo(System.Array array, int index) { }
        public void CopyTo(System.Data.Common.DataTableMapping[] array, int index) { }
        public System.Data.Common.DataTableMapping GetByDataSetTable(string dataSetTable) { throw null; }
        public System.Collections.IEnumerator GetEnumerator() { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public static System.Data.Common.DataTableMapping GetTableMappingBySchemaAction(System.Data.Common.DataTableMappingCollection tableMappings, string sourceTable, string dataSetTable, System.Data.MissingMappingAction mappingAction) { throw null; }
        public int IndexOf(object value) { throw null; }
        public int IndexOf(string sourceTable) { throw null; }
        public int IndexOfDataSetTable(string dataSetTable) { throw null; }
        public void Insert(int index, System.Data.Common.DataTableMapping value) { }
        public void Insert(int index, object value) { }
        public void Remove(System.Data.Common.DataTableMapping value) { }
        public void Remove(object value) { }
        public void RemoveAt(int index) { }
        public void RemoveAt(string sourceTable) { }
        System.Data.ITableMapping System.Data.ITableMappingCollection.Add(string sourceTableName, string dataSetTableName) { throw null; }
        System.Data.ITableMapping System.Data.ITableMappingCollection.GetByDataSetTable(string dataSetTableName) { throw null; }
    }
    public abstract partial class DbColumn
    {
        protected DbColumn() { }
        public System.Nullable<bool> AllowDBNull { get { throw null; } protected set { } }
        public string BaseCatalogName { get { throw null; } protected set { } }
        public string BaseColumnName { get { throw null; } protected set { } }
        public string BaseSchemaName { get { throw null; } protected set { } }
        public string BaseServerName { get { throw null; } protected set { } }
        public string BaseTableName { get { throw null; } protected set { } }
        public string ColumnName { get { throw null; } protected set { } }
        public System.Nullable<int> ColumnOrdinal { get { throw null; } protected set { } }
        public System.Nullable<int> ColumnSize { get { throw null; } protected set { } }
        public System.Type DataType { get { throw null; } protected set { } }
        public string DataTypeName { get { throw null; } protected set { } }
        public System.Nullable<bool> IsAliased { get { throw null; } protected set { } }
        public System.Nullable<bool> IsAutoIncrement { get { throw null; } protected set { } }
        public System.Nullable<bool> IsExpression { get { throw null; } protected set { } }
        public System.Nullable<bool> IsHidden { get { throw null; } protected set { } }
        public System.Nullable<bool> IsIdentity { get { throw null; } protected set { } }
        public System.Nullable<bool> IsKey { get { throw null; } protected set { } }
        public System.Nullable<bool> IsLong { get { throw null; } protected set { } }
        public System.Nullable<bool> IsReadOnly { get { throw null; } protected set { } }
        public System.Nullable<bool> IsUnique { get { throw null; } protected set { } }
        public virtual object this[string property] { get { throw null; } }
        public System.Nullable<int> NumericPrecision { get { throw null; } protected set { } }
        public System.Nullable<int> NumericScale { get { throw null; } protected set { } }
        public string UdtAssemblyQualifiedName { get { throw null; } protected set { } }
    }
    public abstract partial class DbCommand : System.ComponentModel.Component, System.Data.IDbCommand, System.IDisposable
    {
        protected DbCommand() { }
        [System.ComponentModel.DefaultValueAttribute("")]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public abstract string CommandText { get; set; }
        public abstract int CommandTimeout { get; set; }
        [System.ComponentModel.DefaultValueAttribute((System.Data.CommandType)(1))]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public abstract System.Data.CommandType CommandType { get; set; }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DefaultValueAttribute(null)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DbConnection Connection { get { throw null; } set { } }
        protected abstract System.Data.Common.DbConnection DbConnection { get; set; }
        protected abstract System.Data.Common.DbParameterCollection DbParameterCollection { get; }
        protected abstract System.Data.Common.DbTransaction DbTransaction { get; set; }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DefaultValueAttribute(true)]
        [System.ComponentModel.DesignOnlyAttribute(true)]
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public abstract bool DesignTimeVisible { get; set; }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DbParameterCollection Parameters { get { throw null; } }
        System.Data.IDbConnection System.Data.IDbCommand.Connection { get { throw null; } set { } }
        System.Data.IDataParameterCollection System.Data.IDbCommand.Parameters { get { throw null; } }
        System.Data.IDbTransaction System.Data.IDbCommand.Transaction { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DefaultValueAttribute(null)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DbTransaction Transaction { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.UpdateRowSource)(3))]
        public abstract System.Data.UpdateRowSource UpdatedRowSource { get; set; }
        public abstract void Cancel();
        protected abstract System.Data.Common.DbParameter CreateDbParameter();
        public System.Data.Common.DbParameter CreateParameter() { throw null; }
        protected abstract System.Data.Common.DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior);
        protected virtual System.Threading.Tasks.Task<System.Data.Common.DbDataReader> ExecuteDbDataReaderAsync(System.Data.CommandBehavior behavior, System.Threading.CancellationToken cancellationToken) { throw null; }
        public abstract int ExecuteNonQuery();
        public System.Threading.Tasks.Task<int> ExecuteNonQueryAsync() { throw null; }
        public virtual System.Threading.Tasks.Task<int> ExecuteNonQueryAsync(System.Threading.CancellationToken cancellationToken) { throw null; }
        public System.Data.Common.DbDataReader ExecuteReader() { throw null; }
        public System.Data.Common.DbDataReader ExecuteReader(System.Data.CommandBehavior behavior) { throw null; }
        public System.Threading.Tasks.Task<System.Data.Common.DbDataReader> ExecuteReaderAsync() { throw null; }
        public System.Threading.Tasks.Task<System.Data.Common.DbDataReader> ExecuteReaderAsync(System.Data.CommandBehavior behavior) { throw null; }
        public System.Threading.Tasks.Task<System.Data.Common.DbDataReader> ExecuteReaderAsync(System.Data.CommandBehavior behavior, System.Threading.CancellationToken cancellationToken) { throw null; }
        public System.Threading.Tasks.Task<System.Data.Common.DbDataReader> ExecuteReaderAsync(System.Threading.CancellationToken cancellationToken) { throw null; }
        public abstract object ExecuteScalar();
        public System.Threading.Tasks.Task<object> ExecuteScalarAsync() { throw null; }
        public virtual System.Threading.Tasks.Task<object> ExecuteScalarAsync(System.Threading.CancellationToken cancellationToken) { throw null; }
        public abstract void Prepare();
        System.Data.IDbDataParameter System.Data.IDbCommand.CreateParameter() { throw null; }
        System.Data.IDataReader System.Data.IDbCommand.ExecuteReader() { throw null; }
        System.Data.IDataReader System.Data.IDbCommand.ExecuteReader(System.Data.CommandBehavior behavior) { throw null; }
    }
    public abstract partial class DbCommandBuilder : System.ComponentModel.Component
    {
        protected DbCommandBuilder() { }
        [System.ComponentModel.DefaultValueAttribute((System.Data.Common.CatalogLocation)(1))]
        public virtual System.Data.Common.CatalogLocation CatalogLocation { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(".")]
        public virtual string CatalogSeparator { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute((System.Data.ConflictOption)(1))]
        public virtual System.Data.ConflictOption ConflictOption { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DbDataAdapter DataAdapter { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public virtual string QuotePrefix { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute("")]
        public virtual string QuoteSuffix { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(".")]
        public virtual string SchemaSeparator { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool SetAllValues { get { throw null; } set { } }
        protected abstract void ApplyParameterInfo(System.Data.Common.DbParameter parameter, System.Data.DataRow row, System.Data.StatementType statementType, bool whereClause);
        protected override void Dispose(bool disposing) { }
        public System.Data.Common.DbCommand GetDeleteCommand() { throw null; }
        public System.Data.Common.DbCommand GetDeleteCommand(bool useColumnsForParameterNames) { throw null; }
        public System.Data.Common.DbCommand GetInsertCommand() { throw null; }
        public System.Data.Common.DbCommand GetInsertCommand(bool useColumnsForParameterNames) { throw null; }
        protected abstract string GetParameterName(int parameterOrdinal);
        protected abstract string GetParameterName(string parameterName);
        protected abstract string GetParameterPlaceholder(int parameterOrdinal);
        protected virtual System.Data.DataTable GetSchemaTable(System.Data.Common.DbCommand sourceCommand) { throw null; }
        public System.Data.Common.DbCommand GetUpdateCommand() { throw null; }
        public System.Data.Common.DbCommand GetUpdateCommand(bool useColumnsForParameterNames) { throw null; }
        protected virtual System.Data.Common.DbCommand InitializeCommand(System.Data.Common.DbCommand command) { throw null; }
        public virtual string QuoteIdentifier(string unquotedIdentifier) { throw null; }
        public virtual void RefreshSchema() { }
        protected void RowUpdatingHandler(System.Data.Common.RowUpdatingEventArgs rowUpdatingEvent) { }
        protected abstract void SetRowUpdatingHandler(System.Data.Common.DbDataAdapter adapter);
        public virtual string UnquoteIdentifier(string quotedIdentifier) { throw null; }
    }
    public abstract partial class DbConnection : System.ComponentModel.Component, System.Data.IDbConnection, System.IDisposable
    {
        protected DbConnection() { }
        [System.ComponentModel.DefaultValueAttribute("")]
        [System.ComponentModel.RecommendedAsConfigurableAttribute(true)]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        [System.ComponentModel.SettingsBindableAttribute(true)]
        public abstract string ConnectionString { get; set; }
        public virtual int ConnectionTimeout { get { throw null; } }
        public abstract string Database { get; }
        public abstract string DataSource { get; }
        protected virtual System.Data.Common.DbProviderFactory DbProviderFactory { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public abstract string ServerVersion { get; }
        [System.ComponentModel.BrowsableAttribute(false)]
        public abstract System.Data.ConnectionState State { get; }
        public virtual event System.Data.StateChangeEventHandler StateChange { add { } remove { } }
        protected abstract System.Data.Common.DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel);
        public System.Data.Common.DbTransaction BeginTransaction() { throw null; }
        public System.Data.Common.DbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel) { throw null; }
        public abstract void ChangeDatabase(string databaseName);
        public abstract void Close();
        public System.Data.Common.DbCommand CreateCommand() { throw null; }
        protected abstract System.Data.Common.DbCommand CreateDbCommand();
        public virtual void EnlistTransaction(System.Transactions.Transaction transaction) { }
        public virtual System.Data.DataTable GetSchema() { throw null; }
        public virtual System.Data.DataTable GetSchema(string collectionName) { throw null; }
        public virtual System.Data.DataTable GetSchema(string collectionName, string[] restrictionValues) { throw null; }
        protected virtual void OnStateChange(System.Data.StateChangeEventArgs stateChange) { }
        public abstract void Open();
        public System.Threading.Tasks.Task OpenAsync() { throw null; }
        public virtual System.Threading.Tasks.Task OpenAsync(System.Threading.CancellationToken cancellationToken) { throw null; }
        System.Data.IDbTransaction System.Data.IDbConnection.BeginTransaction() { throw null; }
        System.Data.IDbTransaction System.Data.IDbConnection.BeginTransaction(System.Data.IsolationLevel isolationLevel) { throw null; }
        System.Data.IDbCommand System.Data.IDbConnection.CreateCommand() { throw null; }
    }
    public partial class DbConnectionStringBuilder : System.Collections.ICollection, System.Collections.IDictionary, System.Collections.IEnumerable, System.ComponentModel.ICustomTypeDescriptor
    {
        public DbConnectionStringBuilder() { }
        public DbConnectionStringBuilder(bool useOdbcRules) { }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        [System.ComponentModel.DesignOnlyAttribute(true)]
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public bool BrowsableConnectionString { get { throw null; } set { } }
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public string ConnectionString { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public virtual int Count { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public virtual bool IsFixedSize { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsReadOnly { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public virtual object this[string keyword] { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public virtual System.Collections.ICollection Keys { get { throw null; } }
        bool System.Collections.ICollection.IsSynchronized { get { throw null; } }
        object System.Collections.ICollection.SyncRoot { get { throw null; } }
        object System.Collections.IDictionary.this[object keyword] { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public virtual System.Collections.ICollection Values { get { throw null; } }
        public void Add(string keyword, object value) { }
        public static void AppendKeyValuePair(System.Text.StringBuilder builder, string keyword, string value) { }
        public static void AppendKeyValuePair(System.Text.StringBuilder builder, string keyword, string value, bool useOdbcRules) { }
        public virtual void Clear() { }
        protected internal void ClearPropertyDescriptors() { }
        public virtual bool ContainsKey(string keyword) { throw null; }
        public virtual bool EquivalentTo(System.Data.Common.DbConnectionStringBuilder connectionStringBuilder) { throw null; }
        protected virtual void GetProperties(System.Collections.Hashtable propertyDescriptors) { }
        public virtual bool Remove(string keyword) { throw null; }
        public virtual bool ShouldSerialize(string keyword) { throw null; }
        void System.Collections.ICollection.CopyTo(System.Array array, int index) { }
        void System.Collections.IDictionary.Add(object keyword, object value) { }
        bool System.Collections.IDictionary.Contains(object keyword) { throw null; }
        System.Collections.IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator() { throw null; }
        void System.Collections.IDictionary.Remove(object keyword) { }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw null; }
        System.ComponentModel.AttributeCollection System.ComponentModel.ICustomTypeDescriptor.GetAttributes() { throw null; }
        string System.ComponentModel.ICustomTypeDescriptor.GetClassName() { throw null; }
        string System.ComponentModel.ICustomTypeDescriptor.GetComponentName() { throw null; }
        System.ComponentModel.TypeConverter System.ComponentModel.ICustomTypeDescriptor.GetConverter() { throw null; }
        System.ComponentModel.EventDescriptor System.ComponentModel.ICustomTypeDescriptor.GetDefaultEvent() { throw null; }
        System.ComponentModel.PropertyDescriptor System.ComponentModel.ICustomTypeDescriptor.GetDefaultProperty() { throw null; }
        object System.ComponentModel.ICustomTypeDescriptor.GetEditor(System.Type editorBaseType) { throw null; }
        System.ComponentModel.EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents() { throw null; }
        System.ComponentModel.EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents(System.Attribute[] attributes) { throw null; }
        System.ComponentModel.PropertyDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetProperties() { throw null; }
        System.ComponentModel.PropertyDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetProperties(System.Attribute[] attributes) { throw null; }
        object System.ComponentModel.ICustomTypeDescriptor.GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd) { throw null; }
        public override string ToString() { throw null; }
        public virtual bool TryGetValue(string keyword, out object value) { value = default(object); throw null; }
    }
    public abstract partial class DbDataAdapter : System.Data.Common.DataAdapter, System.Data.IDataAdapter, System.Data.IDbDataAdapter, System.ICloneable
    {
        public const string DefaultSourceTableName = "Table";
        protected DbDataAdapter() { }
        protected DbDataAdapter(System.Data.Common.DbDataAdapter adapter) { }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DbCommand DeleteCommand { get { throw null; } set { } }
        protected internal System.Data.CommandBehavior FillCommandBehavior { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DbCommand InsertCommand { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DbCommand SelectCommand { get { throw null; } set { } }
        System.Data.IDbCommand System.Data.IDbDataAdapter.DeleteCommand { get { throw null; } set { } }
        System.Data.IDbCommand System.Data.IDbDataAdapter.InsertCommand { get { throw null; } set { } }
        System.Data.IDbCommand System.Data.IDbDataAdapter.SelectCommand { get { throw null; } set { } }
        System.Data.IDbCommand System.Data.IDbDataAdapter.UpdateCommand { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(1)]
        public virtual int UpdateBatchSize { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Data.Common.DbCommand UpdateCommand { get { throw null; } set { } }
        protected virtual int AddToBatch(System.Data.IDbCommand command) { throw null; }
        protected virtual void ClearBatch() { }
        protected virtual System.Data.Common.RowUpdatedEventArgs CreateRowUpdatedEvent(System.Data.DataRow dataRow, System.Data.IDbCommand command, System.Data.StatementType statementType, System.Data.Common.DataTableMapping tableMapping) { throw null; }
        protected virtual System.Data.Common.RowUpdatingEventArgs CreateRowUpdatingEvent(System.Data.DataRow dataRow, System.Data.IDbCommand command, System.Data.StatementType statementType, System.Data.Common.DataTableMapping tableMapping) { throw null; }
        protected override void Dispose(bool disposing) { }
        protected virtual int ExecuteBatch() { throw null; }
        public override int Fill(System.Data.DataSet dataSet) { throw null; }
        public int Fill(System.Data.DataSet dataSet, int startRecord, int maxRecords, string srcTable) { throw null; }
        protected virtual int Fill(System.Data.DataSet dataSet, int startRecord, int maxRecords, string srcTable, System.Data.IDbCommand command, System.Data.CommandBehavior behavior) { throw null; }
        public int Fill(System.Data.DataSet dataSet, string srcTable) { throw null; }
        public int Fill(System.Data.DataTable dataTable) { throw null; }
        protected virtual int Fill(System.Data.DataTable dataTable, System.Data.IDbCommand command, System.Data.CommandBehavior behavior) { throw null; }
        protected virtual int Fill(System.Data.DataTable[] dataTables, int startRecord, int maxRecords, System.Data.IDbCommand command, System.Data.CommandBehavior behavior) { throw null; }
        public int Fill(int startRecord, int maxRecords, params System.Data.DataTable[] dataTables) { throw null; }
        public override System.Data.DataTable[] FillSchema(System.Data.DataSet dataSet, System.Data.SchemaType schemaType) { throw null; }
        protected virtual System.Data.DataTable[] FillSchema(System.Data.DataSet dataSet, System.Data.SchemaType schemaType, System.Data.IDbCommand command, string srcTable, System.Data.CommandBehavior behavior) { throw null; }
        public System.Data.DataTable[] FillSchema(System.Data.DataSet dataSet, System.Data.SchemaType schemaType, string srcTable) { throw null; }
        public System.Data.DataTable FillSchema(System.Data.DataTable dataTable, System.Data.SchemaType schemaType) { throw null; }
        protected virtual System.Data.DataTable FillSchema(System.Data.DataTable dataTable, System.Data.SchemaType schemaType, System.Data.IDbCommand command, System.Data.CommandBehavior behavior) { throw null; }
        protected virtual System.Data.IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex) { throw null; }
        protected virtual bool GetBatchedRecordsAffected(int commandIdentifier, out int recordsAffected, out System.Exception error) { recordsAffected = default(int); error = default(System.Exception); throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public override System.Data.IDataParameter[] GetFillParameters() { throw null; }
        protected virtual void InitializeBatching() { }
        protected virtual void OnRowUpdated(System.Data.Common.RowUpdatedEventArgs value) { }
        protected virtual void OnRowUpdating(System.Data.Common.RowUpdatingEventArgs value) { }
        object System.ICloneable.Clone() { throw null; }
        protected virtual void TerminateBatching() { }
        public int Update(System.Data.DataRow[] dataRows) { throw null; }
        protected virtual int Update(System.Data.DataRow[] dataRows, System.Data.Common.DataTableMapping tableMapping) { throw null; }
        public override int Update(System.Data.DataSet dataSet) { throw null; }
        public int Update(System.Data.DataSet dataSet, string srcTable) { throw null; }
        public int Update(System.Data.DataTable dataTable) { throw null; }
    }
    [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, ControlEvidence=true, ControlPolicy=true)]
    [System.SerializableAttribute]
    public abstract partial class DBDataPermission : System.Security.CodeAccessPermission, System.Security.Permissions.IUnrestrictedPermission
    {
        [System.ObsoleteAttribute("DBDataPermission() has been deprecated.  Use the DBDataPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
        protected DBDataPermission() { }
        protected DBDataPermission(System.Data.Common.DBDataPermission permission) { }
        protected DBDataPermission(System.Data.Common.DBDataPermissionAttribute permissionAttribute) { }
        protected DBDataPermission(System.Security.Permissions.PermissionState state) { }
        [System.ObsoleteAttribute("DBDataPermission(PermissionState state,Boolean allowBlankPassword) has been deprecated.  Use the DBDataPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
        protected DBDataPermission(System.Security.Permissions.PermissionState state, bool allowBlankPassword) { }
        public bool AllowBlankPassword { get { throw null; } set { } }
        public virtual void Add(string connectionString, string restrictions, System.Data.KeyRestrictionBehavior behavior) { }
        protected void Clear() { }
        public override System.Security.IPermission Copy() { throw null; }
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
        protected virtual System.Data.Common.DBDataPermission CreateInstance() { throw null; }
        public override void FromXml(System.Security.SecurityElement securityElement) { }
        public override System.Security.IPermission Intersect(System.Security.IPermission target) { throw null; }
        public override bool IsSubsetOf(System.Security.IPermission target) { throw null; }
        public bool IsUnrestricted() { throw null; }
        public override System.Security.SecurityElement ToXml() { throw null; }
        public override System.Security.IPermission Union(System.Security.IPermission target) { throw null; }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(109), AllowMultiple=true, Inherited=false)]
    [System.SerializableAttribute]
    public abstract partial class DBDataPermissionAttribute : System.Security.Permissions.CodeAccessSecurityAttribute
    {
        protected DBDataPermissionAttribute(System.Security.Permissions.SecurityAction action) : base (default(System.Security.Permissions.SecurityAction)) { }
        public bool AllowBlankPassword { get { throw null; } set { } }
        public string ConnectionString { get { throw null; } set { } }
        public System.Data.KeyRestrictionBehavior KeyRestrictionBehavior { get { throw null; } set { } }
        public string KeyRestrictions { get { throw null; } set { } }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public bool ShouldSerializeConnectionString() { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public bool ShouldSerializeKeyRestrictions() { throw null; }
    }
    public abstract partial class DbDataReader : System.MarshalByRefObject, System.Collections.IEnumerable, System.Data.IDataReader, System.Data.IDataRecord, System.IDisposable
    {
        protected DbDataReader() { }
        public abstract int Depth { get; }
        public abstract int FieldCount { get; }
        public abstract bool HasRows { get; }
        public abstract bool IsClosed { get; }
        public abstract object this[int ordinal] { get; }
        public abstract object this[string name] { get; }
        public abstract int RecordsAffected { get; }
        public virtual int VisibleFieldCount { get { throw null; } }
        public virtual void Close() { }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public void Dispose() { }
        protected virtual void Dispose(bool disposing) { }
        public abstract bool GetBoolean(int ordinal);
        public abstract byte GetByte(int ordinal);
        public abstract long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length);
        public abstract char GetChar(int ordinal);
        public abstract long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length);
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public System.Data.Common.DbDataReader GetData(int ordinal) { throw null; }
        public abstract string GetDataTypeName(int ordinal);
        public abstract System.DateTime GetDateTime(int ordinal);
        protected virtual System.Data.Common.DbDataReader GetDbDataReader(int ordinal) { throw null; }
        public abstract decimal GetDecimal(int ordinal);
        public abstract double GetDouble(int ordinal);
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public abstract System.Collections.IEnumerator GetEnumerator();
        public abstract System.Type GetFieldType(int ordinal);
        public System.Threading.Tasks.Task<T> GetFieldValueAsync<T>(int ordinal) { throw null; }
        public virtual System.Threading.Tasks.Task<T> GetFieldValueAsync<T>(int ordinal, System.Threading.CancellationToken cancellationToken) { throw null; }
        public virtual T GetFieldValue<T>(int ordinal) { throw null; }
        public abstract float GetFloat(int ordinal);
        public abstract System.Guid GetGuid(int ordinal);
        public abstract short GetInt16(int ordinal);
        public abstract int GetInt32(int ordinal);
        public abstract long GetInt64(int ordinal);
        public abstract string GetName(int ordinal);
        public abstract int GetOrdinal(string name);
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public virtual System.Type GetProviderSpecificFieldType(int ordinal) { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public virtual object GetProviderSpecificValue(int ordinal) { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public virtual int GetProviderSpecificValues(object[] values) { throw null; }
        public virtual System.Data.DataTable GetSchemaTable() { throw null; }
        public virtual System.IO.Stream GetStream(int ordinal) { throw null; }
        public abstract string GetString(int ordinal);
        public virtual System.IO.TextReader GetTextReader(int ordinal) { throw null; }
        public abstract object GetValue(int ordinal);
        public abstract int GetValues(object[] values);
        public abstract bool IsDBNull(int ordinal);
        public System.Threading.Tasks.Task<bool> IsDBNullAsync(int ordinal) { throw null; }
        public virtual System.Threading.Tasks.Task<bool> IsDBNullAsync(int ordinal, System.Threading.CancellationToken cancellationToken) { throw null; }
        public abstract bool NextResult();
        public System.Threading.Tasks.Task<bool> NextResultAsync() { throw null; }
        public virtual System.Threading.Tasks.Task<bool> NextResultAsync(System.Threading.CancellationToken cancellationToken) { throw null; }
        public abstract bool Read();
        public System.Threading.Tasks.Task<bool> ReadAsync() { throw null; }
        public virtual System.Threading.Tasks.Task<bool> ReadAsync(System.Threading.CancellationToken cancellationToken) { throw null; }
        System.Data.IDataReader System.Data.IDataRecord.GetData(int ordinal) { throw null; }
    }
    public static partial class DbDataReaderExtensions
    {
        public static bool CanGetColumnSchema(this System.Data.Common.DbDataReader reader) { throw null; }
        public static System.Collections.ObjectModel.ReadOnlyCollection<System.Data.Common.DbColumn> GetColumnSchema(this System.Data.Common.DbDataReader reader) { throw null; }
    }
    public abstract partial class DbDataRecord : System.ComponentModel.ICustomTypeDescriptor, System.Data.IDataRecord
    {
        protected DbDataRecord() { }
        public abstract int FieldCount { get; }
        public abstract object this[int i] { get; }
        public abstract object this[string name] { get; }
        public abstract bool GetBoolean(int i);
        public abstract byte GetByte(int i);
        public abstract long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length);
        public abstract char GetChar(int i);
        public abstract long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length);
        public System.Data.IDataReader GetData(int i) { throw null; }
        public abstract string GetDataTypeName(int i);
        public abstract System.DateTime GetDateTime(int i);
        protected virtual System.Data.Common.DbDataReader GetDbDataReader(int i) { throw null; }
        public abstract decimal GetDecimal(int i);
        public abstract double GetDouble(int i);
        public abstract System.Type GetFieldType(int i);
        public abstract float GetFloat(int i);
        public abstract System.Guid GetGuid(int i);
        public abstract short GetInt16(int i);
        public abstract int GetInt32(int i);
        public abstract long GetInt64(int i);
        public abstract string GetName(int i);
        public abstract int GetOrdinal(string name);
        public abstract string GetString(int i);
        public abstract object GetValue(int i);
        public abstract int GetValues(object[] values);
        public abstract bool IsDBNull(int i);
        System.ComponentModel.AttributeCollection System.ComponentModel.ICustomTypeDescriptor.GetAttributes() { throw null; }
        string System.ComponentModel.ICustomTypeDescriptor.GetClassName() { throw null; }
        string System.ComponentModel.ICustomTypeDescriptor.GetComponentName() { throw null; }
        System.ComponentModel.TypeConverter System.ComponentModel.ICustomTypeDescriptor.GetConverter() { throw null; }
        System.ComponentModel.EventDescriptor System.ComponentModel.ICustomTypeDescriptor.GetDefaultEvent() { throw null; }
        System.ComponentModel.PropertyDescriptor System.ComponentModel.ICustomTypeDescriptor.GetDefaultProperty() { throw null; }
        object System.ComponentModel.ICustomTypeDescriptor.GetEditor(System.Type editorBaseType) { throw null; }
        System.ComponentModel.EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents() { throw null; }
        System.ComponentModel.EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents(System.Attribute[] attributes) { throw null; }
        System.ComponentModel.PropertyDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetProperties() { throw null; }
        System.ComponentModel.PropertyDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetProperties(System.Attribute[] attributes) { throw null; }
        object System.ComponentModel.ICustomTypeDescriptor.GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd) { throw null; }
    }
    public abstract partial class DbDataSourceEnumerator
    {
        protected DbDataSourceEnumerator() { }
        public abstract System.Data.DataTable GetDataSources();
    }
    public partial class DbEnumerator : System.Collections.IEnumerator
    {
        public DbEnumerator(System.Data.Common.DbDataReader reader) { }
        public DbEnumerator(System.Data.Common.DbDataReader reader, bool closeReader) { }
        public DbEnumerator(System.Data.IDataReader reader) { }
        public DbEnumerator(System.Data.IDataReader reader, bool closeReader) { }
        public object Current { get { throw null; } }
        public bool MoveNext() { throw null; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public void Reset() { }
    }
    [System.SerializableAttribute]
    public abstract partial class DbException : System.Runtime.InteropServices.ExternalException
    {
        protected DbException() { }
        protected DbException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        protected DbException(string message) { }
        protected DbException(string message, System.Exception innerException) { }
        protected DbException(string message, int errorCode) { }
    }
    public static partial class DbMetaDataCollectionNames
    {
        public static readonly string DataSourceInformation;
        public static readonly string DataTypes;
        public static readonly string MetaDataCollections;
        public static readonly string ReservedWords;
        public static readonly string Restrictions;
    }
    public static partial class DbMetaDataColumnNames
    {
        public static readonly string CollectionName;
        public static readonly string ColumnSize;
        public static readonly string CompositeIdentifierSeparatorPattern;
        public static readonly string CreateFormat;
        public static readonly string CreateParameters;
        public static readonly string DataSourceProductName;
        public static readonly string DataSourceProductVersion;
        public static readonly string DataSourceProductVersionNormalized;
        public static readonly string DataType;
        public static readonly string GroupByBehavior;
        public static readonly string IdentifierCase;
        public static readonly string IdentifierPattern;
        public static readonly string IsAutoIncrementable;
        public static readonly string IsBestMatch;
        public static readonly string IsCaseSensitive;
        public static readonly string IsConcurrencyType;
        public static readonly string IsFixedLength;
        public static readonly string IsFixedPrecisionScale;
        public static readonly string IsLiteralSupported;
        public static readonly string IsLong;
        public static readonly string IsNullable;
        public static readonly string IsSearchable;
        public static readonly string IsSearchableWithLike;
        public static readonly string IsUnsigned;
        public static readonly string LiteralPrefix;
        public static readonly string LiteralSuffix;
        public static readonly string MaximumScale;
        public static readonly string MinimumScale;
        public static readonly string NumberOfIdentifierParts;
        public static readonly string NumberOfRestrictions;
        public static readonly string OrderByColumnsInSelect;
        public static readonly string ParameterMarkerFormat;
        public static readonly string ParameterMarkerPattern;
        public static readonly string ParameterNameMaxLength;
        public static readonly string ParameterNamePattern;
        public static readonly string ProviderDbType;
        public static readonly string QuotedIdentifierCase;
        public static readonly string QuotedIdentifierPattern;
        public static readonly string ReservedWord;
        public static readonly string StatementSeparatorPattern;
        public static readonly string StringLiteralPattern;
        public static readonly string SupportedJoinOperators;
        public static readonly string TypeName;
    }
    public abstract partial class DbParameter : System.MarshalByRefObject, System.Data.IDataParameter, System.Data.IDbDataParameter
    {
        protected DbParameter() { }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public abstract System.Data.DbType DbType { get; set; }
        [System.ComponentModel.DefaultValueAttribute((System.Data.ParameterDirection)(1))]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public abstract System.Data.ParameterDirection Direction { get; set; }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignOnlyAttribute(true)]
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public abstract bool IsNullable { get; set; }
        [System.ComponentModel.DefaultValueAttribute("")]
        public abstract string ParameterName { get; set; }
        public virtual byte Precision { get { throw null; } set { } }
        public virtual byte Scale { get { throw null; } set { } }
        public abstract int Size { get; set; }
        [System.ComponentModel.DefaultValueAttribute("")]
        public abstract string SourceColumn { get; set; }
        [System.ComponentModel.DefaultValueAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public abstract bool SourceColumnNullMapping { get; set; }
        [System.ComponentModel.DefaultValueAttribute((System.Data.DataRowVersion)(512))]
        public virtual System.Data.DataRowVersion SourceVersion { get { throw null; } set { } }
        byte System.Data.IDbDataParameter.Precision { get { throw null; } set { } }
        byte System.Data.IDbDataParameter.Scale { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(null)]
        [System.ComponentModel.RefreshPropertiesAttribute((System.ComponentModel.RefreshProperties)(1))]
        public abstract object Value { get; set; }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public abstract void ResetDbType();
    }
    public abstract partial class DbParameterCollection : System.MarshalByRefObject, System.Collections.ICollection, System.Collections.IEnumerable, System.Collections.IList, System.Data.IDataParameterCollection
    {
        protected DbParameterCollection() { }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public abstract int Count { get; }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public virtual bool IsFixedSize { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public virtual bool IsReadOnly { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public virtual bool IsSynchronized { get { throw null; } }
        public System.Data.Common.DbParameter this[int index] { get { throw null; } set { } }
        public System.Data.Common.DbParameter this[string parameterName] { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public abstract object SyncRoot { get; }
        object System.Collections.IList.this[int index] { get { throw null; } set { } }
        object System.Data.IDataParameterCollection.this[string parameterName] { get { throw null; } set { } }
        public abstract int Add(object value);
        public abstract void AddRange(System.Array values);
        public abstract void Clear();
        public abstract bool Contains(object value);
        public abstract bool Contains(string value);
        public abstract void CopyTo(System.Array array, int index);
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public abstract System.Collections.IEnumerator GetEnumerator();
        protected abstract System.Data.Common.DbParameter GetParameter(int index);
        protected abstract System.Data.Common.DbParameter GetParameter(string parameterName);
        public abstract int IndexOf(object value);
        public abstract int IndexOf(string parameterName);
        public abstract void Insert(int index, object value);
        public abstract void Remove(object value);
        public abstract void RemoveAt(int index);
        public abstract void RemoveAt(string parameterName);
        protected abstract void SetParameter(int index, System.Data.Common.DbParameter value);
        protected abstract void SetParameter(string parameterName, System.Data.Common.DbParameter value);
    }
    public abstract partial class DbProviderFactory
    {
        protected DbProviderFactory() { }
        public virtual bool CanCreateDataSourceEnumerator { get { throw null; } }
        public virtual System.Data.Common.DbCommand CreateCommand() { throw null; }
        public virtual System.Data.Common.DbCommandBuilder CreateCommandBuilder() { throw null; }
        public virtual System.Data.Common.DbConnection CreateConnection() { throw null; }
        public virtual System.Data.Common.DbConnectionStringBuilder CreateConnectionStringBuilder() { throw null; }
        public virtual System.Data.Common.DbDataAdapter CreateDataAdapter() { throw null; }
        public virtual System.Data.Common.DbDataSourceEnumerator CreateDataSourceEnumerator() { throw null; }
        public virtual System.Data.Common.DbParameter CreateParameter() { throw null; }
        public virtual System.Security.CodeAccessPermission CreatePermission(System.Security.Permissions.PermissionState state) { throw null; }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(128), AllowMultiple=false, Inherited=true)]
    [System.SerializableAttribute]
    public sealed partial class DbProviderSpecificTypePropertyAttribute : System.Attribute
    {
        public DbProviderSpecificTypePropertyAttribute(bool isProviderSpecificTypeProperty) { }
        public bool IsProviderSpecificTypeProperty { get { throw null; } }
    }
    public abstract partial class DbTransaction : System.MarshalByRefObject, System.Data.IDbTransaction, System.IDisposable
    {
        protected DbTransaction() { }
        public System.Data.Common.DbConnection Connection { get { throw null; } }
        protected abstract System.Data.Common.DbConnection DbConnection { get; }
        public abstract System.Data.IsolationLevel IsolationLevel { get; }
        System.Data.IDbConnection System.Data.IDbTransaction.Connection { get { throw null; } }
        public abstract void Commit();
        public void Dispose() { }
        protected virtual void Dispose(bool disposing) { }
        public abstract void Rollback();
    }
    public enum GroupByBehavior
    {
        ExactMatch = 4,
        MustContainAll = 3,
        NotSupported = 1,
        Unknown = 0,
        Unrelated = 2,
    }
    public partial interface IDbColumnSchemaGenerator
    {
        System.Collections.ObjectModel.ReadOnlyCollection<System.Data.Common.DbColumn> GetColumnSchema();
    }
    public enum IdentifierCase
    {
        Insensitive = 1,
        Sensitive = 2,
        Unknown = 0,
    }
    public partial class RowUpdatedEventArgs : System.EventArgs
    {
        public RowUpdatedEventArgs(System.Data.DataRow dataRow, System.Data.IDbCommand command, System.Data.StatementType statementType, System.Data.Common.DataTableMapping tableMapping) { }
        public System.Data.IDbCommand Command { get { throw null; } }
        public System.Exception Errors { get { throw null; } set { } }
        public int RecordsAffected { get { throw null; } }
        public System.Data.DataRow Row { get { throw null; } }
        public int RowCount { get { throw null; } }
        public System.Data.StatementType StatementType { get { throw null; } }
        public System.Data.UpdateStatus Status { get { throw null; } set { } }
        public System.Data.Common.DataTableMapping TableMapping { get { throw null; } }
        public void CopyToRows(System.Data.DataRow[] array) { }
        public void CopyToRows(System.Data.DataRow[] array, int arrayIndex) { }
    }
    public partial class RowUpdatingEventArgs : System.EventArgs
    {
        public RowUpdatingEventArgs(System.Data.DataRow dataRow, System.Data.IDbCommand command, System.Data.StatementType statementType, System.Data.Common.DataTableMapping tableMapping) { }
        protected virtual System.Data.IDbCommand BaseCommand { get { throw null; } set { } }
        public System.Data.IDbCommand Command { get { throw null; } set { } }
        public System.Exception Errors { get { throw null; } set { } }
        public System.Data.DataRow Row { get { throw null; } }
        public System.Data.StatementType StatementType { get { throw null; } }
        public System.Data.UpdateStatus Status { get { throw null; } set { } }
        public System.Data.Common.DataTableMapping TableMapping { get { throw null; } }
    }
    public static partial class SchemaTableColumn
    {
        public static readonly string AllowDBNull;
        public static readonly string BaseColumnName;
        public static readonly string BaseSchemaName;
        public static readonly string BaseTableName;
        public static readonly string ColumnName;
        public static readonly string ColumnOrdinal;
        public static readonly string ColumnSize;
        public static readonly string DataType;
        public static readonly string IsAliased;
        public static readonly string IsExpression;
        public static readonly string IsKey;
        public static readonly string IsLong;
        public static readonly string IsUnique;
        public static readonly string NonVersionedProviderType;
        public static readonly string NumericPrecision;
        public static readonly string NumericScale;
        public static readonly string ProviderType;
    }
    public static partial class SchemaTableOptionalColumn
    {
        public static readonly string AutoIncrementSeed;
        public static readonly string AutoIncrementStep;
        public static readonly string BaseCatalogName;
        public static readonly string BaseColumnNamespace;
        public static readonly string BaseServerName;
        public static readonly string BaseTableNamespace;
        public static readonly string ColumnMapping;
        public static readonly string DefaultValue;
        public static readonly string Expression;
        public static readonly string IsAutoIncrement;
        public static readonly string IsHidden;
        public static readonly string IsReadOnly;
        public static readonly string IsRowVersion;
        public static readonly string ProviderSpecificDataType;
    }
    [System.FlagsAttribute]
    public enum SupportedJoinOperators
    {
        FullOuter = 8,
        Inner = 1,
        LeftOuter = 2,
        None = 0,
        RightOuter = 4,
    }
}
namespace System.Data.SqlTypes
{
    public partial interface INullable
    {
        bool IsNull { get; }
    }
    [System.SerializableAttribute]
    public sealed partial class SqlAlreadyFilledException : System.Data.SqlTypes.SqlTypeException
    {
        public SqlAlreadyFilledException() { }
        public SqlAlreadyFilledException(string message) { }
        public SqlAlreadyFilledException(string message, System.Exception e) { }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlBinary : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlBinary Null;
        public SqlBinary(byte[] value) { throw null;}
        public bool IsNull { get { throw null; } }
        public byte this[int index] { get { throw null; } }
        public int Length { get { throw null; } }
        public byte[] Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlBinary Add(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlBinary value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlBinary Concat(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static System.Data.SqlTypes.SqlBinary operator +(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static explicit operator byte[] (System.Data.SqlTypes.SqlBinary x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBinary (System.Data.SqlTypes.SqlGuid x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlBinary (byte[] x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlBinary x, System.Data.SqlTypes.SqlBinary y) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlGuid ToSqlGuid() { throw null; }
        public override string ToString() { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlBoolean : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlBoolean False;
        public static readonly System.Data.SqlTypes.SqlBoolean Null;
        public static readonly System.Data.SqlTypes.SqlBoolean One;
        public static readonly System.Data.SqlTypes.SqlBoolean True;
        public static readonly System.Data.SqlTypes.SqlBoolean Zero;
        public SqlBoolean(bool value) { throw null;}
        public SqlBoolean(int value) { throw null;}
        public byte ByteValue { get { throw null; } }
        public bool IsFalse { get { throw null; } }
        public bool IsNull { get { throw null; } }
        public bool IsTrue { get { throw null; } }
        public bool Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlBoolean And(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlBoolean value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEquals(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEquals(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean OnesComplement(System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator &(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator |(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ^(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static explicit operator bool (System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBoolean (System.Data.SqlTypes.SqlByte x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBoolean (System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBoolean (System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBoolean (System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBoolean (System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBoolean (System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBoolean (System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBoolean (System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBoolean (System.Data.SqlTypes.SqlString x) { throw null; }
        public static bool operator false(System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlBoolean (bool x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !(System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ~(System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static bool operator true(System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Or(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Parse(string s) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlByte ToSqlByte() { throw null; }
        public System.Data.SqlTypes.SqlDecimal ToSqlDecimal() { throw null; }
        public System.Data.SqlTypes.SqlDouble ToSqlDouble() { throw null; }
        public System.Data.SqlTypes.SqlInt16 ToSqlInt16() { throw null; }
        public System.Data.SqlTypes.SqlInt32 ToSqlInt32() { throw null; }
        public System.Data.SqlTypes.SqlInt64 ToSqlInt64() { throw null; }
        public System.Data.SqlTypes.SqlMoney ToSqlMoney() { throw null; }
        public System.Data.SqlTypes.SqlSingle ToSqlSingle() { throw null; }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Xor(System.Data.SqlTypes.SqlBoolean x, System.Data.SqlTypes.SqlBoolean y) { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlByte : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlByte MaxValue;
        public static readonly System.Data.SqlTypes.SqlByte MinValue;
        public static readonly System.Data.SqlTypes.SqlByte Null;
        public static readonly System.Data.SqlTypes.SqlByte Zero;
        public SqlByte(byte value) { throw null;}
        public bool IsNull { get { throw null; } }
        public byte Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlByte Add(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte BitwiseAnd(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte BitwiseOr(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlByte value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlByte Divide(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte Mod(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte Modulus(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte Multiply(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte OnesComplement(System.Data.SqlTypes.SqlByte x) { throw null; }
        public static System.Data.SqlTypes.SqlByte operator +(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte operator &(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte operator |(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte operator /(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte operator ^(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlByte (System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static explicit operator byte (System.Data.SqlTypes.SqlByte x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlByte (System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlByte (System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlByte (System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlByte (System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlByte (System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlByte (System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlByte (System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlByte (System.Data.SqlTypes.SqlString x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlByte (byte x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte operator %(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte operator *(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte operator ~(System.Data.SqlTypes.SqlByte x) { throw null; }
        public static System.Data.SqlTypes.SqlByte operator -(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        public static System.Data.SqlTypes.SqlByte Parse(string s) { throw null; }
        public static System.Data.SqlTypes.SqlByte Subtract(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlBoolean ToSqlBoolean() { throw null; }
        public System.Data.SqlTypes.SqlDecimal ToSqlDecimal() { throw null; }
        public System.Data.SqlTypes.SqlDouble ToSqlDouble() { throw null; }
        public System.Data.SqlTypes.SqlInt16 ToSqlInt16() { throw null; }
        public System.Data.SqlTypes.SqlInt32 ToSqlInt32() { throw null; }
        public System.Data.SqlTypes.SqlInt64 ToSqlInt64() { throw null; }
        public System.Data.SqlTypes.SqlMoney ToSqlMoney() { throw null; }
        public System.Data.SqlTypes.SqlSingle ToSqlSingle() { throw null; }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
        public static System.Data.SqlTypes.SqlByte Xor(System.Data.SqlTypes.SqlByte x, System.Data.SqlTypes.SqlByte y) { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    public sealed partial class SqlBytes : System.Data.SqlTypes.INullable, System.Runtime.Serialization.ISerializable, System.Xml.Serialization.IXmlSerializable
    {
        public SqlBytes() { }
        public SqlBytes(byte[] buffer) { }
        public SqlBytes(System.Data.SqlTypes.SqlBinary value) { }
        public SqlBytes(System.IO.Stream s) { }
        public byte[] Buffer { get { throw null; } }
        public bool IsNull { get { throw null; } }
        public byte this[long offset] { get { throw null; } set { } }
        public long Length { get { throw null; } }
        public long MaxLength { get { throw null; } }
        public static System.Data.SqlTypes.SqlBytes Null { get { throw null; } }
        public System.Data.SqlTypes.StorageState Storage { get { throw null; } }
        public System.IO.Stream Stream { get { throw null; } set { } }
        public byte[] Value { get { throw null; } }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBytes (System.Data.SqlTypes.SqlBinary value) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlBinary (System.Data.SqlTypes.SqlBytes value) { throw null; }
        public long Read(long offset, byte[] buffer, int offsetInBuffer, int count) { throw null; }
        public void SetLength(long value) { }
        public void SetNull() { }
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader r) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlBinary ToSqlBinary() { throw null; }
        public void Write(long offset, byte[] buffer, int offsetInBuffer, int count) { }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    public sealed partial class SqlChars : System.Data.SqlTypes.INullable, System.Runtime.Serialization.ISerializable, System.Xml.Serialization.IXmlSerializable
    {
        public SqlChars() { }
        public SqlChars(char[] buffer) { }
        public SqlChars(System.Data.SqlTypes.SqlString value) { }
        public char[] Buffer { get { throw null; } }
        public bool IsNull { get { throw null; } }
        public char this[long offset] { get { throw null; } set { } }
        public long Length { get { throw null; } }
        public long MaxLength { get { throw null; } }
        public static System.Data.SqlTypes.SqlChars Null { get { throw null; } }
        public System.Data.SqlTypes.StorageState Storage { get { throw null; } }
        public char[] Value { get { throw null; } }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlChars value) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlChars (System.Data.SqlTypes.SqlString value) { throw null; }
        public long Read(long offset, char[] buffer, int offsetInBuffer, int count) { throw null; }
        public void SetLength(long value) { }
        public void SetNull() { }
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader r) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public void Write(long offset, char[] buffer, int offsetInBuffer, int count) { }
    }
    [System.FlagsAttribute]
    public enum SqlCompareOptions
    {
        BinarySort = 32768,
        BinarySort2 = 16384,
        IgnoreCase = 1,
        IgnoreKanaType = 8,
        IgnoreNonSpace = 2,
        IgnoreWidth = 16,
        None = 0,
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlDateTime : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlDateTime MaxValue;
        public static readonly System.Data.SqlTypes.SqlDateTime MinValue;
        public static readonly System.Data.SqlTypes.SqlDateTime Null;
        public static readonly int SQLTicksPerHour;
        public static readonly int SQLTicksPerMinute;
        public static readonly int SQLTicksPerSecond;
        public SqlDateTime(System.DateTime value) { throw null;}
        public SqlDateTime(int dayTicks, int timeTicks) { throw null;}
        public SqlDateTime(int year, int month, int day) { throw null;}
        public SqlDateTime(int year, int month, int day, int hour, int minute, int second) { throw null;}
        public SqlDateTime(int year, int month, int day, int hour, int minute, int second, double millisecond) { throw null;}
        public SqlDateTime(int year, int month, int day, int hour, int minute, int second, int bilisecond) { throw null;}
        public int DayTicks { get { throw null; } }
        public bool IsNull { get { throw null; } }
        public int TimeTicks { get { throw null; } }
        public System.DateTime Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlDateTime Add(System.Data.SqlTypes.SqlDateTime x, System.TimeSpan t) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlDateTime value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static System.Data.SqlTypes.SqlDateTime operator +(System.Data.SqlTypes.SqlDateTime x, System.TimeSpan t) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static explicit operator System.DateTime (System.Data.SqlTypes.SqlDateTime x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlDateTime (System.Data.SqlTypes.SqlString x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDateTime (System.DateTime value) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlDateTime x, System.Data.SqlTypes.SqlDateTime y) { throw null; }
        public static System.Data.SqlTypes.SqlDateTime operator -(System.Data.SqlTypes.SqlDateTime x, System.TimeSpan t) { throw null; }
        public static System.Data.SqlTypes.SqlDateTime Parse(string s) { throw null; }
        public static System.Data.SqlTypes.SqlDateTime Subtract(System.Data.SqlTypes.SqlDateTime x, System.TimeSpan t) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlDecimal : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly byte MaxPrecision;
        public static readonly byte MaxScale;
        public static readonly System.Data.SqlTypes.SqlDecimal MaxValue;
        public static readonly System.Data.SqlTypes.SqlDecimal MinValue;
        public static readonly System.Data.SqlTypes.SqlDecimal Null;
        public SqlDecimal(byte bPrecision, byte bScale, bool fPositive, int data1, int data2, int data3, int data4) { throw null;}
        public SqlDecimal(byte bPrecision, byte bScale, bool fPositive, int[] bits) { throw null;}
        public SqlDecimal(decimal value) { throw null;}
        public SqlDecimal(double dVal) { throw null;}
        public SqlDecimal(int value) { throw null;}
        public SqlDecimal(long value) { throw null;}
        public byte[] BinData { get { throw null; } }
        public int[] Data { get { throw null; } }
        public bool IsNull { get { throw null; } }
        public bool IsPositive { get { throw null; } }
        public byte Precision { get { throw null; } }
        public byte Scale { get { throw null; } }
        public decimal Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlDecimal Abs(System.Data.SqlTypes.SqlDecimal n) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal Add(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal AdjustScale(System.Data.SqlTypes.SqlDecimal n, int digits, bool fRound) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal Ceiling(System.Data.SqlTypes.SqlDecimal n) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlDecimal value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal ConvertToPrecScale(System.Data.SqlTypes.SqlDecimal n, int precision, int scale) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal Divide(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal Floor(System.Data.SqlTypes.SqlDecimal n) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal Multiply(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal operator +(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal operator /(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlDecimal (System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static explicit operator decimal (System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlDecimal (System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlDecimal (System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlDecimal (System.Data.SqlTypes.SqlString x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlDecimal (double x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDecimal (System.Data.SqlTypes.SqlByte x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDecimal (System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDecimal (System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDecimal (System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDecimal (System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDecimal (decimal x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDecimal (long x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal operator *(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal operator -(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal operator -(System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal Parse(string s) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal Power(System.Data.SqlTypes.SqlDecimal n, double exp) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal Round(System.Data.SqlTypes.SqlDecimal n, int position) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 Sign(System.Data.SqlTypes.SqlDecimal n) { throw null; }
        public static System.Data.SqlTypes.SqlDecimal Subtract(System.Data.SqlTypes.SqlDecimal x, System.Data.SqlTypes.SqlDecimal y) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public double ToDouble() { throw null; }
        public System.Data.SqlTypes.SqlBoolean ToSqlBoolean() { throw null; }
        public System.Data.SqlTypes.SqlByte ToSqlByte() { throw null; }
        public System.Data.SqlTypes.SqlDouble ToSqlDouble() { throw null; }
        public System.Data.SqlTypes.SqlInt16 ToSqlInt16() { throw null; }
        public System.Data.SqlTypes.SqlInt32 ToSqlInt32() { throw null; }
        public System.Data.SqlTypes.SqlInt64 ToSqlInt64() { throw null; }
        public System.Data.SqlTypes.SqlMoney ToSqlMoney() { throw null; }
        public System.Data.SqlTypes.SqlSingle ToSqlSingle() { throw null; }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
        public static System.Data.SqlTypes.SqlDecimal Truncate(System.Data.SqlTypes.SqlDecimal n, int position) { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlDouble : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlDouble MaxValue;
        public static readonly System.Data.SqlTypes.SqlDouble MinValue;
        public static readonly System.Data.SqlTypes.SqlDouble Null;
        public static readonly System.Data.SqlTypes.SqlDouble Zero;
        public SqlDouble(double value) { throw null;}
        public bool IsNull { get { throw null; } }
        public double Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlDouble Add(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlDouble value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlDouble Divide(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlDouble Multiply(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlDouble operator +(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlDouble operator /(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlDouble (System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static explicit operator double (System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlDouble (System.Data.SqlTypes.SqlString x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDouble (System.Data.SqlTypes.SqlByte x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDouble (System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDouble (System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDouble (System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDouble (System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDouble (System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDouble (System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlDouble (double x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlDouble operator *(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlDouble operator -(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        public static System.Data.SqlTypes.SqlDouble operator -(System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static System.Data.SqlTypes.SqlDouble Parse(string s) { throw null; }
        public static System.Data.SqlTypes.SqlDouble Subtract(System.Data.SqlTypes.SqlDouble x, System.Data.SqlTypes.SqlDouble y) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlBoolean ToSqlBoolean() { throw null; }
        public System.Data.SqlTypes.SqlByte ToSqlByte() { throw null; }
        public System.Data.SqlTypes.SqlDecimal ToSqlDecimal() { throw null; }
        public System.Data.SqlTypes.SqlInt16 ToSqlInt16() { throw null; }
        public System.Data.SqlTypes.SqlInt32 ToSqlInt32() { throw null; }
        public System.Data.SqlTypes.SqlInt64 ToSqlInt64() { throw null; }
        public System.Data.SqlTypes.SqlMoney ToSqlMoney() { throw null; }
        public System.Data.SqlTypes.SqlSingle ToSqlSingle() { throw null; }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlGuid : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlGuid Null;
        public SqlGuid(byte[] value) { throw null;}
        public SqlGuid(System.Guid g) { throw null;}
        public SqlGuid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k) { throw null;}
        public SqlGuid(string s) { throw null;}
        public bool IsNull { get { throw null; } }
        public System.Guid Value { get { throw null; } }
        public int CompareTo(System.Data.SqlTypes.SqlGuid value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlGuid (System.Data.SqlTypes.SqlBinary x) { throw null; }
        public static explicit operator System.Guid (System.Data.SqlTypes.SqlGuid x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlGuid (System.Data.SqlTypes.SqlString x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlGuid (System.Guid x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlGuid x, System.Data.SqlTypes.SqlGuid y) { throw null; }
        public static System.Data.SqlTypes.SqlGuid Parse(string s) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public byte[] ToByteArray() { throw null; }
        public System.Data.SqlTypes.SqlBinary ToSqlBinary() { throw null; }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlInt16 : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlInt16 MaxValue;
        public static readonly System.Data.SqlTypes.SqlInt16 MinValue;
        public static readonly System.Data.SqlTypes.SqlInt16 Null;
        public static readonly System.Data.SqlTypes.SqlInt16 Zero;
        public SqlInt16(short value) { throw null;}
        public bool IsNull { get { throw null; } }
        public short Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlInt16 Add(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 BitwiseAnd(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 BitwiseOr(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlInt16 value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 Divide(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 Mod(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 Modulus(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 Multiply(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 OnesComplement(System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 operator +(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 operator &(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 operator |(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 operator /(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 operator ^(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt16 (System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt16 (System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt16 (System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static explicit operator short (System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt16 (System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt16 (System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt16 (System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt16 (System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt16 (System.Data.SqlTypes.SqlString x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlInt16 (System.Data.SqlTypes.SqlByte x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlInt16 (short x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 operator %(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 operator *(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 operator ~(System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 operator -(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 operator -(System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 Parse(string s) { throw null; }
        public static System.Data.SqlTypes.SqlInt16 Subtract(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlBoolean ToSqlBoolean() { throw null; }
        public System.Data.SqlTypes.SqlByte ToSqlByte() { throw null; }
        public System.Data.SqlTypes.SqlDecimal ToSqlDecimal() { throw null; }
        public System.Data.SqlTypes.SqlDouble ToSqlDouble() { throw null; }
        public System.Data.SqlTypes.SqlInt32 ToSqlInt32() { throw null; }
        public System.Data.SqlTypes.SqlInt64 ToSqlInt64() { throw null; }
        public System.Data.SqlTypes.SqlMoney ToSqlMoney() { throw null; }
        public System.Data.SqlTypes.SqlSingle ToSqlSingle() { throw null; }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
        public static System.Data.SqlTypes.SqlInt16 Xor(System.Data.SqlTypes.SqlInt16 x, System.Data.SqlTypes.SqlInt16 y) { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlInt32 : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlInt32 MaxValue;
        public static readonly System.Data.SqlTypes.SqlInt32 MinValue;
        public static readonly System.Data.SqlTypes.SqlInt32 Null;
        public static readonly System.Data.SqlTypes.SqlInt32 Zero;
        public SqlInt32(int value) { throw null;}
        public bool IsNull { get { throw null; } }
        public int Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlInt32 Add(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 BitwiseAnd(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 BitwiseOr(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlInt32 value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 Divide(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 Mod(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 Modulus(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 Multiply(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 OnesComplement(System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 operator +(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 operator &(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 operator |(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 operator /(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 operator ^(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt32 (System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt32 (System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt32 (System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static explicit operator int (System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt32 (System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt32 (System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt32 (System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt32 (System.Data.SqlTypes.SqlString x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlInt32 (System.Data.SqlTypes.SqlByte x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlInt32 (System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlInt32 (int x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 operator %(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 operator *(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 operator ~(System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 operator -(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 operator -(System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 Parse(string s) { throw null; }
        public static System.Data.SqlTypes.SqlInt32 Subtract(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlBoolean ToSqlBoolean() { throw null; }
        public System.Data.SqlTypes.SqlByte ToSqlByte() { throw null; }
        public System.Data.SqlTypes.SqlDecimal ToSqlDecimal() { throw null; }
        public System.Data.SqlTypes.SqlDouble ToSqlDouble() { throw null; }
        public System.Data.SqlTypes.SqlInt16 ToSqlInt16() { throw null; }
        public System.Data.SqlTypes.SqlInt64 ToSqlInt64() { throw null; }
        public System.Data.SqlTypes.SqlMoney ToSqlMoney() { throw null; }
        public System.Data.SqlTypes.SqlSingle ToSqlSingle() { throw null; }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
        public static System.Data.SqlTypes.SqlInt32 Xor(System.Data.SqlTypes.SqlInt32 x, System.Data.SqlTypes.SqlInt32 y) { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlInt64 : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlInt64 MaxValue;
        public static readonly System.Data.SqlTypes.SqlInt64 MinValue;
        public static readonly System.Data.SqlTypes.SqlInt64 Null;
        public static readonly System.Data.SqlTypes.SqlInt64 Zero;
        public SqlInt64(long value) { throw null;}
        public bool IsNull { get { throw null; } }
        public long Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlInt64 Add(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 BitwiseAnd(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 BitwiseOr(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlInt64 value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 Divide(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 Mod(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 Modulus(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 Multiply(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 OnesComplement(System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 operator +(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 operator &(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 operator |(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 operator /(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 operator ^(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt64 (System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt64 (System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt64 (System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static explicit operator long (System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt64 (System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt64 (System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlInt64 (System.Data.SqlTypes.SqlString x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlInt64 (System.Data.SqlTypes.SqlByte x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlInt64 (System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlInt64 (System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlInt64 (long x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 operator %(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 operator *(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 operator ~(System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 operator -(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 operator -(System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 Parse(string s) { throw null; }
        public static System.Data.SqlTypes.SqlInt64 Subtract(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlBoolean ToSqlBoolean() { throw null; }
        public System.Data.SqlTypes.SqlByte ToSqlByte() { throw null; }
        public System.Data.SqlTypes.SqlDecimal ToSqlDecimal() { throw null; }
        public System.Data.SqlTypes.SqlDouble ToSqlDouble() { throw null; }
        public System.Data.SqlTypes.SqlInt16 ToSqlInt16() { throw null; }
        public System.Data.SqlTypes.SqlInt32 ToSqlInt32() { throw null; }
        public System.Data.SqlTypes.SqlMoney ToSqlMoney() { throw null; }
        public System.Data.SqlTypes.SqlSingle ToSqlSingle() { throw null; }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
        public static System.Data.SqlTypes.SqlInt64 Xor(System.Data.SqlTypes.SqlInt64 x, System.Data.SqlTypes.SqlInt64 y) { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlMoney : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlMoney MaxValue;
        public static readonly System.Data.SqlTypes.SqlMoney MinValue;
        public static readonly System.Data.SqlTypes.SqlMoney Null;
        public static readonly System.Data.SqlTypes.SqlMoney Zero;
        public SqlMoney(decimal value) { throw null;}
        public SqlMoney(double value) { throw null;}
        public SqlMoney(int value) { throw null;}
        public SqlMoney(long value) { throw null;}
        public bool IsNull { get { throw null; } }
        public decimal Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlMoney Add(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlMoney value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlMoney Divide(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlMoney Multiply(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlMoney operator +(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlMoney operator /(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlMoney (System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlMoney (System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlMoney (System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static explicit operator decimal (System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlMoney (System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlMoney (System.Data.SqlTypes.SqlString x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlMoney (double x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlMoney (System.Data.SqlTypes.SqlByte x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlMoney (System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlMoney (System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlMoney (System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlMoney (decimal x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlMoney (long x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlMoney operator *(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlMoney operator -(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        public static System.Data.SqlTypes.SqlMoney operator -(System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static System.Data.SqlTypes.SqlMoney Parse(string s) { throw null; }
        public static System.Data.SqlTypes.SqlMoney Subtract(System.Data.SqlTypes.SqlMoney x, System.Data.SqlTypes.SqlMoney y) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public decimal ToDecimal() { throw null; }
        public double ToDouble() { throw null; }
        public int ToInt32() { throw null; }
        public long ToInt64() { throw null; }
        public System.Data.SqlTypes.SqlBoolean ToSqlBoolean() { throw null; }
        public System.Data.SqlTypes.SqlByte ToSqlByte() { throw null; }
        public System.Data.SqlTypes.SqlDecimal ToSqlDecimal() { throw null; }
        public System.Data.SqlTypes.SqlDouble ToSqlDouble() { throw null; }
        public System.Data.SqlTypes.SqlInt16 ToSqlInt16() { throw null; }
        public System.Data.SqlTypes.SqlInt32 ToSqlInt32() { throw null; }
        public System.Data.SqlTypes.SqlInt64 ToSqlInt64() { throw null; }
        public System.Data.SqlTypes.SqlSingle ToSqlSingle() { throw null; }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
    }
    [System.SerializableAttribute]
    public sealed partial class SqlNotFilledException : System.Data.SqlTypes.SqlTypeException
    {
        public SqlNotFilledException() { }
        public SqlNotFilledException(string message) { }
        public SqlNotFilledException(string message, System.Exception e) { }
    }
    [System.SerializableAttribute]
    public sealed partial class SqlNullValueException : System.Data.SqlTypes.SqlTypeException
    {
        public SqlNullValueException() { }
        public SqlNullValueException(string message) { }
        public SqlNullValueException(string message, System.Exception e) { }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlSingle : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly System.Data.SqlTypes.SqlSingle MaxValue;
        public static readonly System.Data.SqlTypes.SqlSingle MinValue;
        public static readonly System.Data.SqlTypes.SqlSingle Null;
        public static readonly System.Data.SqlTypes.SqlSingle Zero;
        public SqlSingle(double value) { throw null;}
        public SqlSingle(float value) { throw null;}
        public bool IsNull { get { throw null; } }
        public float Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlSingle Add(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlSingle value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlSingle Divide(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlSingle Multiply(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlSingle operator +(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlSingle operator /(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlSingle (System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlSingle (System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static explicit operator float (System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlSingle (System.Data.SqlTypes.SqlString x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlSingle (System.Data.SqlTypes.SqlByte x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlSingle (System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlSingle (System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlSingle (System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlSingle (System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlSingle (System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlSingle (float x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlSingle operator *(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlSingle operator -(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        public static System.Data.SqlTypes.SqlSingle operator -(System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static System.Data.SqlTypes.SqlSingle Parse(string s) { throw null; }
        public static System.Data.SqlTypes.SqlSingle Subtract(System.Data.SqlTypes.SqlSingle x, System.Data.SqlTypes.SqlSingle y) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlBoolean ToSqlBoolean() { throw null; }
        public System.Data.SqlTypes.SqlByte ToSqlByte() { throw null; }
        public System.Data.SqlTypes.SqlDecimal ToSqlDecimal() { throw null; }
        public System.Data.SqlTypes.SqlDouble ToSqlDouble() { throw null; }
        public System.Data.SqlTypes.SqlInt16 ToSqlInt16() { throw null; }
        public System.Data.SqlTypes.SqlInt32 ToSqlInt32() { throw null; }
        public System.Data.SqlTypes.SqlInt64 ToSqlInt64() { throw null; }
        public System.Data.SqlTypes.SqlMoney ToSqlMoney() { throw null; }
        public System.Data.SqlTypes.SqlString ToSqlString() { throw null; }
        public override string ToString() { throw null; }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SqlString : System.Data.SqlTypes.INullable, System.IComparable, System.Xml.Serialization.IXmlSerializable
    {
        public static readonly int BinarySort;
        public static readonly int BinarySort2;
        public static readonly int IgnoreCase;
        public static readonly int IgnoreKanaType;
        public static readonly int IgnoreNonSpace;
        public static readonly int IgnoreWidth;
        public static readonly System.Data.SqlTypes.SqlString Null;
        public SqlString(int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions, byte[] data) { throw null;}
        public SqlString(int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions, byte[] data, bool fUnicode) { throw null;}
        public SqlString(int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions, byte[] data, int index, int count) { throw null;}
        public SqlString(int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions, byte[] data, int index, int count, bool fUnicode) { throw null;}
        public SqlString(string data) { throw null;}
        public SqlString(string data, int lcid) { throw null;}
        public SqlString(string data, int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions) { throw null;}
        public System.Globalization.CompareInfo CompareInfo { get { throw null; } }
        public System.Globalization.CultureInfo CultureInfo { get { throw null; } }
        public bool IsNull { get { throw null; } }
        public int LCID { get { throw null; } }
        public System.Data.SqlTypes.SqlCompareOptions SqlCompareOptions { get { throw null; } }
        public string Value { get { throw null; } }
        public static System.Data.SqlTypes.SqlString Add(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public System.Data.SqlTypes.SqlString Clone() { throw null; }
        public static System.Globalization.CompareOptions CompareOptionsFromSqlCompareOptions(System.Data.SqlTypes.SqlCompareOptions compareOptions) { throw null; }
        public int CompareTo(System.Data.SqlTypes.SqlString value) { throw null; }
        public int CompareTo(object value) { throw null; }
        public static System.Data.SqlTypes.SqlString Concat(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean Equals(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public override bool Equals(object value) { throw null; }
        public override int GetHashCode() { throw null; }
        public byte[] GetNonUnicodeBytes() { throw null; }
        public byte[] GetUnicodeBytes() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThan(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean GreaterThanOrEqual(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThan(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean LessThanOrEqual(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean NotEquals(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static System.Data.SqlTypes.SqlString operator +(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator ==(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlBoolean x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlByte x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlDateTime x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlDecimal x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlDouble x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlGuid x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlInt16 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlInt32 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlInt64 x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlMoney x) { throw null; }
        public static explicit operator System.Data.SqlTypes.SqlString (System.Data.SqlTypes.SqlSingle x) { throw null; }
        public static explicit operator string (System.Data.SqlTypes.SqlString x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator >=(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static implicit operator System.Data.SqlTypes.SqlString (string x) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator !=(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        public static System.Data.SqlTypes.SqlBoolean operator <=(System.Data.SqlTypes.SqlString x, System.Data.SqlTypes.SqlString y) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
        public System.Data.SqlTypes.SqlBoolean ToSqlBoolean() { throw null; }
        public System.Data.SqlTypes.SqlByte ToSqlByte() { throw null; }
        public System.Data.SqlTypes.SqlDateTime ToSqlDateTime() { throw null; }
        public System.Data.SqlTypes.SqlDecimal ToSqlDecimal() { throw null; }
        public System.Data.SqlTypes.SqlDouble ToSqlDouble() { throw null; }
        public System.Data.SqlTypes.SqlGuid ToSqlGuid() { throw null; }
        public System.Data.SqlTypes.SqlInt16 ToSqlInt16() { throw null; }
        public System.Data.SqlTypes.SqlInt32 ToSqlInt32() { throw null; }
        public System.Data.SqlTypes.SqlInt64 ToSqlInt64() { throw null; }
        public System.Data.SqlTypes.SqlMoney ToSqlMoney() { throw null; }
        public System.Data.SqlTypes.SqlSingle ToSqlSingle() { throw null; }
        public override string ToString() { throw null; }
    }
    [System.SerializableAttribute]
    public sealed partial class SqlTruncateException : System.Data.SqlTypes.SqlTypeException
    {
        public SqlTruncateException() { }
        public SqlTruncateException(string message) { }
        public SqlTruncateException(string message, System.Exception e) { }
    }
    [System.SerializableAttribute]
    public partial class SqlTypeException : System.SystemException
    {
        public SqlTypeException() { }
        protected SqlTypeException(System.Runtime.Serialization.SerializationInfo si, System.Runtime.Serialization.StreamingContext sc) { }
        public SqlTypeException(string message) { }
        public SqlTypeException(string message, System.Exception e) { }
    }
    [System.Xml.Serialization.XmlSchemaProviderAttribute("GetXsdType")]
    [System.SerializableAttribute]
    public sealed partial class SqlXml : System.Data.SqlTypes.INullable, System.Xml.Serialization.IXmlSerializable
    {
        public SqlXml() { }
        public SqlXml(System.IO.Stream value) { }
        public SqlXml(System.Xml.XmlReader value) { }
        public bool IsNull { get { throw null; } }
        public static System.Data.SqlTypes.SqlXml Null { get { throw null; } }
        public string Value { get { throw null; } }
        public System.Xml.XmlReader CreateReader() { throw null; }
        public static System.Xml.XmlQualifiedName GetXsdType(System.Xml.Schema.XmlSchemaSet schemaSet) { throw null; }
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema() { throw null; }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader r) { }
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) { }
    }
    public enum StorageState
    {
        Buffer = 0,
        Stream = 1,
        UnmanagedBuffer = 2,
    }
}
namespace System.Runtime.Serialization
{
    [System.AttributeUsageAttribute((System.AttributeTargets)(12), Inherited=false, AllowMultiple=false)]
    public sealed partial class CollectionDataContractAttribute : System.Attribute
    {
        public CollectionDataContractAttribute() { }
        public bool IsItemNameSetExplicitly { get { throw null; } }
        public bool IsKeyNameSetExplicitly { get { throw null; } }
        public bool IsNameSetExplicitly { get { throw null; } }
        public bool IsNamespaceSetExplicitly { get { throw null; } }
        public bool IsReference { get { throw null; } set { } }
        public bool IsReferenceSetExplicitly { get { throw null; } }
        public bool IsValueNameSetExplicitly { get { throw null; } }
        public string ItemName { get { throw null; } set { } }
        public string KeyName { get { throw null; } set { } }
        public string Name { get { throw null; } set { } }
        public string Namespace { get { throw null; } set { } }
        public string ValueName { get { throw null; } set { } }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(3), Inherited=false, AllowMultiple=true)]
    public sealed partial class ContractNamespaceAttribute : System.Attribute
    {
        public ContractNamespaceAttribute(string contractNamespace) { }
        public string ClrNamespace { get { throw null; } set { } }
        public string ContractNamespace { get { throw null; } }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(28), Inherited=false, AllowMultiple=false)]
    public sealed partial class DataContractAttribute : System.Attribute
    {
        public DataContractAttribute() { }
        public bool IsNameSetExplicitly { get { throw null; } }
        public bool IsNamespaceSetExplicitly { get { throw null; } }
        public bool IsReference { get { throw null; } set { } }
        public bool IsReferenceSetExplicitly { get { throw null; } }
        public string Name { get { throw null; } set { } }
        public string Namespace { get { throw null; } set { } }
    }
    public abstract partial class DataContractResolver
    {
        protected DataContractResolver() { }
        public abstract System.Type ResolveName(string typeName, string typeNamespace, System.Type declaredType, System.Runtime.Serialization.DataContractResolver knownTypeResolver);
        public abstract bool TryResolveType(System.Type type, System.Type declaredType, System.Runtime.Serialization.DataContractResolver knownTypeResolver, out System.Xml.XmlDictionaryString typeName, out System.Xml.XmlDictionaryString typeNamespace);
    }
    public sealed partial class DataContractSerializer : System.Runtime.Serialization.XmlObjectSerializer
    {
        public DataContractSerializer(System.Type type) { }
        public DataContractSerializer(System.Type type, System.Collections.Generic.IEnumerable<System.Type> knownTypes) { }
        public DataContractSerializer(System.Type type, System.Collections.Generic.IEnumerable<System.Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, bool preserveObjectReferences, System.Runtime.Serialization.IDataContractSurrogate dataContractSurrogate) { }
        public DataContractSerializer(System.Type type, System.Collections.Generic.IEnumerable<System.Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, bool preserveObjectReferences, System.Runtime.Serialization.IDataContractSurrogate dataContractSurrogate, System.Runtime.Serialization.DataContractResolver dataContractResolver) { }
        public DataContractSerializer(System.Type type, System.Runtime.Serialization.DataContractSerializerSettings settings) { }
        public DataContractSerializer(System.Type type, string rootName, string rootNamespace) { }
        public DataContractSerializer(System.Type type, string rootName, string rootNamespace, System.Collections.Generic.IEnumerable<System.Type> knownTypes) { }
        public DataContractSerializer(System.Type type, string rootName, string rootNamespace, System.Collections.Generic.IEnumerable<System.Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, bool preserveObjectReferences, System.Runtime.Serialization.IDataContractSurrogate dataContractSurrogate) { }
        public DataContractSerializer(System.Type type, string rootName, string rootNamespace, System.Collections.Generic.IEnumerable<System.Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, bool preserveObjectReferences, System.Runtime.Serialization.IDataContractSurrogate dataContractSurrogate, System.Runtime.Serialization.DataContractResolver dataContractResolver) { }
        public DataContractSerializer(System.Type type, System.Xml.XmlDictionaryString rootName, System.Xml.XmlDictionaryString rootNamespace) { }
        public DataContractSerializer(System.Type type, System.Xml.XmlDictionaryString rootName, System.Xml.XmlDictionaryString rootNamespace, System.Collections.Generic.IEnumerable<System.Type> knownTypes) { }
        public DataContractSerializer(System.Type type, System.Xml.XmlDictionaryString rootName, System.Xml.XmlDictionaryString rootNamespace, System.Collections.Generic.IEnumerable<System.Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, bool preserveObjectReferences, System.Runtime.Serialization.IDataContractSurrogate dataContractSurrogate) { }
        public DataContractSerializer(System.Type type, System.Xml.XmlDictionaryString rootName, System.Xml.XmlDictionaryString rootNamespace, System.Collections.Generic.IEnumerable<System.Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, bool preserveObjectReferences, System.Runtime.Serialization.IDataContractSurrogate dataContractSurrogate, System.Runtime.Serialization.DataContractResolver dataContractResolver) { }
        public System.Runtime.Serialization.DataContractResolver DataContractResolver { get { throw null; } }
        public System.Runtime.Serialization.IDataContractSurrogate DataContractSurrogate { get { throw null; } }
        public bool IgnoreExtensionDataObject { get { throw null; } }
        public System.Collections.ObjectModel.ReadOnlyCollection<System.Type> KnownTypes { get { throw null; } }
        public int MaxItemsInObjectGraph { get { throw null; } }
        public bool PreserveObjectReferences { get { throw null; } }
        public bool SerializeReadOnlyTypes { get { throw null; } }
        public override bool IsStartObject(System.Xml.XmlDictionaryReader reader) { throw null; }
        public override bool IsStartObject(System.Xml.XmlReader reader) { throw null; }
        public override object ReadObject(System.Xml.XmlDictionaryReader reader, bool verifyObjectName) { throw null; }
        public object ReadObject(System.Xml.XmlDictionaryReader reader, bool verifyObjectName, System.Runtime.Serialization.DataContractResolver dataContractResolver) { throw null; }
        public override object ReadObject(System.Xml.XmlReader reader) { throw null; }
        public override object ReadObject(System.Xml.XmlReader reader, bool verifyObjectName) { throw null; }
        public override void WriteEndObject(System.Xml.XmlDictionaryWriter writer) { }
        public override void WriteEndObject(System.Xml.XmlWriter writer) { }
        public void WriteObject(System.Xml.XmlDictionaryWriter writer, object graph, System.Runtime.Serialization.DataContractResolver dataContractResolver) { }
        public override void WriteObject(System.Xml.XmlWriter writer, object graph) { }
        public override void WriteObjectContent(System.Xml.XmlDictionaryWriter writer, object graph) { }
        public override void WriteObjectContent(System.Xml.XmlWriter writer, object graph) { }
        public override void WriteStartObject(System.Xml.XmlDictionaryWriter writer, object graph) { }
        public override void WriteStartObject(System.Xml.XmlWriter writer, object graph) { }
    }
    public static partial class DataContractSerializerExtensions
    {
        public static System.Runtime.Serialization.ISerializationSurrogateProvider GetSerializationSurrogateProvider(this System.Runtime.Serialization.DataContractSerializer serializer) { throw null; }
        public static void SetSerializationSurrogateProvider(this System.Runtime.Serialization.DataContractSerializer serializer, System.Runtime.Serialization.ISerializationSurrogateProvider provider) { }
    }
    public partial class DataContractSerializerSettings
    {
        public DataContractSerializerSettings() { }
        public System.Runtime.Serialization.DataContractResolver DataContractResolver { get { throw null; } set { } }
        public System.Runtime.Serialization.IDataContractSurrogate DataContractSurrogate { get { throw null; } set { } }
        public bool IgnoreExtensionDataObject { get { throw null; } set { } }
        public System.Collections.Generic.IEnumerable<System.Type> KnownTypes { get { throw null; } set { } }
        public int MaxItemsInObjectGraph { get { throw null; } set { } }
        public bool PreserveObjectReferences { get { throw null; } set { } }
        public System.Xml.XmlDictionaryString RootName { get { throw null; } set { } }
        public System.Xml.XmlDictionaryString RootNamespace { get { throw null; } set { } }
        public bool SerializeReadOnlyTypes { get { throw null; } set { } }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(384), Inherited=false, AllowMultiple=false)]
    public sealed partial class DataMemberAttribute : System.Attribute
    {
        public DataMemberAttribute() { }
        public bool EmitDefaultValue { get { throw null; } set { } }
        public bool IsNameSetExplicitly { get { throw null; } }
        public bool IsRequired { get { throw null; } set { } }
        public string Name { get { throw null; } set { } }
        public int Order { get { throw null; } set { } }
    }
    public partial class DateTimeFormat
    {
        public DateTimeFormat(string formatString) { }
        public DateTimeFormat(string formatString, System.IFormatProvider formatProvider) { }
        public System.Globalization.DateTimeStyles DateTimeStyles { get { throw null; } set { } }
        public System.IFormatProvider FormatProvider { get { throw null; } }
        public string FormatString { get { throw null; } }
    }
    public enum EmitTypeInformation
    {
        Always = 1,
        AsNeeded = 0,
        Never = 2,
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(256), Inherited=false, AllowMultiple=false)]
    public sealed partial class EnumMemberAttribute : System.Attribute
    {
        public EnumMemberAttribute() { }
        public bool IsValueSetExplicitly { get { throw null; } }
        public string Value { get { throw null; } set { } }
    }
    public partial class ExportOptions
    {
        public ExportOptions() { }
        public System.Runtime.Serialization.IDataContractSurrogate DataContractSurrogate { get { throw null; } set { } }
        public System.Collections.ObjectModel.Collection<System.Type> KnownTypes { get { throw null; } }
    }
    public sealed partial class ExtensionDataObject
    {
        internal ExtensionDataObject() { }
    }
    public partial interface IDataContractSurrogate
    {
        object GetCustomDataToExport(System.Reflection.MemberInfo memberInfo, System.Type dataContractType);
        object GetCustomDataToExport(System.Type clrType, System.Type dataContractType);
        System.Type GetDataContractType(System.Type type);
        object GetDeserializedObject(object obj, System.Type targetType);
        void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<System.Type> customDataTypes);
        object GetObjectToSerialize(object obj, System.Type targetType);
        System.Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData);
    }
    public partial interface IExtensibleDataObject
    {
        System.Runtime.Serialization.ExtensionDataObject ExtensionData { get; set; }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(384), Inherited=false, AllowMultiple=false)]
    public sealed partial class IgnoreDataMemberAttribute : System.Attribute
    {
        public IgnoreDataMemberAttribute() { }
    }
    [System.SerializableAttribute]
    public partial class InvalidDataContractException : System.Exception
    {
        public InvalidDataContractException() { }
        protected InvalidDataContractException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public InvalidDataContractException(string message) { }
        public InvalidDataContractException(string message, System.Exception innerException) { }
    }
    public partial interface ISerializationSurrogateProvider
    {
        object GetDeserializedObject(object obj, System.Type targetType);
        object GetObjectToSerialize(object obj, System.Type targetType);
        System.Type GetSurrogateType(System.Type type);
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(12), Inherited=true, AllowMultiple=true)]
    public sealed partial class KnownTypeAttribute : System.Attribute
    {
        public KnownTypeAttribute(string methodName) { }
        public KnownTypeAttribute(System.Type type) { }
        public string MethodName { get { throw null; } }
        public System.Type Type { get { throw null; } }
    }
    public sealed partial class NetDataContractSerializer : System.Runtime.Serialization.XmlObjectSerializer, System.Runtime.Serialization.IFormatter
    {
        public NetDataContractSerializer() { }
        public NetDataContractSerializer(System.Runtime.Serialization.StreamingContext context) { }
        public NetDataContractSerializer(System.Runtime.Serialization.StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, System.Runtime.Serialization.Formatters.FormatterAssemblyStyle assemblyFormat, System.Runtime.Serialization.ISurrogateSelector surrogateSelector) { }
        public NetDataContractSerializer(string rootName, string rootNamespace) { }
        public NetDataContractSerializer(string rootName, string rootNamespace, System.Runtime.Serialization.StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, System.Runtime.Serialization.Formatters.FormatterAssemblyStyle assemblyFormat, System.Runtime.Serialization.ISurrogateSelector surrogateSelector) { }
        public NetDataContractSerializer(System.Xml.XmlDictionaryString rootName, System.Xml.XmlDictionaryString rootNamespace) { }
        public NetDataContractSerializer(System.Xml.XmlDictionaryString rootName, System.Xml.XmlDictionaryString rootNamespace, System.Runtime.Serialization.StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, System.Runtime.Serialization.Formatters.FormatterAssemblyStyle assemblyFormat, System.Runtime.Serialization.ISurrogateSelector surrogateSelector) { }
        public System.Runtime.Serialization.Formatters.FormatterAssemblyStyle AssemblyFormat { get { throw null; } set { } }
        public System.Runtime.Serialization.SerializationBinder Binder { get { throw null; } set { } }
        public System.Runtime.Serialization.StreamingContext Context { get { throw null; } set { } }
        public bool IgnoreExtensionDataObject { get { throw null; } }
        public int MaxItemsInObjectGraph { get { throw null; } }
        public System.Runtime.Serialization.ISurrogateSelector SurrogateSelector { get { throw null; } set { } }
        public object Deserialize(System.IO.Stream stream) { throw null; }
        public override bool IsStartObject(System.Xml.XmlDictionaryReader reader) { throw null; }
        public override bool IsStartObject(System.Xml.XmlReader reader) { throw null; }
        public override object ReadObject(System.Xml.XmlDictionaryReader reader, bool verifyObjectName) { throw null; }
        public override object ReadObject(System.Xml.XmlReader reader) { throw null; }
        public override object ReadObject(System.Xml.XmlReader reader, bool verifyObjectName) { throw null; }
        public void Serialize(System.IO.Stream stream, object graph) { }
        public override void WriteEndObject(System.Xml.XmlDictionaryWriter writer) { }
        public override void WriteEndObject(System.Xml.XmlWriter writer) { }
        public override void WriteObject(System.Xml.XmlWriter writer, object graph) { }
        public override void WriteObjectContent(System.Xml.XmlDictionaryWriter writer, object graph) { }
        public override void WriteObjectContent(System.Xml.XmlWriter writer, object graph) { }
        public override void WriteStartObject(System.Xml.XmlDictionaryWriter writer, object graph) { }
        public override void WriteStartObject(System.Xml.XmlWriter writer, object graph) { }
    }
    public abstract partial class XmlObjectSerializer
    {
        protected XmlObjectSerializer() { }
        public abstract bool IsStartObject(System.Xml.XmlDictionaryReader reader);
        public virtual bool IsStartObject(System.Xml.XmlReader reader) { throw null; }
        public virtual object ReadObject(System.IO.Stream stream) { throw null; }
        public virtual object ReadObject(System.Xml.XmlDictionaryReader reader) { throw null; }
        public abstract object ReadObject(System.Xml.XmlDictionaryReader reader, bool verifyObjectName);
        public virtual object ReadObject(System.Xml.XmlReader reader) { throw null; }
        public virtual object ReadObject(System.Xml.XmlReader reader, bool verifyObjectName) { throw null; }
        public abstract void WriteEndObject(System.Xml.XmlDictionaryWriter writer);
        public virtual void WriteEndObject(System.Xml.XmlWriter writer) { }
        public virtual void WriteObject(System.IO.Stream stream, object graph) { }
        public virtual void WriteObject(System.Xml.XmlDictionaryWriter writer, object graph) { }
        public virtual void WriteObject(System.Xml.XmlWriter writer, object graph) { }
        public abstract void WriteObjectContent(System.Xml.XmlDictionaryWriter writer, object graph);
        public virtual void WriteObjectContent(System.Xml.XmlWriter writer, object graph) { }
        public abstract void WriteStartObject(System.Xml.XmlDictionaryWriter writer, object graph);
        public virtual void WriteStartObject(System.Xml.XmlWriter writer, object graph) { }
    }
    public static partial class XmlSerializableServices
    {
        public static void AddDefaultSchema(System.Xml.Schema.XmlSchemaSet schemas, System.Xml.XmlQualifiedName typeQName) { }
        public static System.Xml.XmlNode[] ReadNodes(System.Xml.XmlReader xmlReader) { throw null; }
        public static void WriteNodes(System.Xml.XmlWriter xmlWriter, System.Xml.XmlNode[] nodes) { }
    }
    public static partial class XPathQueryGenerator
    {
        public static string CreateFromDataContractSerializer(System.Type type, System.Reflection.MemberInfo[] pathToMember, System.Text.StringBuilder rootElementXpath, out System.Xml.XmlNamespaceManager namespaces) { namespaces = default(System.Xml.XmlNamespaceManager); throw null; }
        public static string CreateFromDataContractSerializer(System.Type type, System.Reflection.MemberInfo[] pathToMember, out System.Xml.XmlNamespaceManager namespaces) { namespaces = default(System.Xml.XmlNamespaceManager); throw null; }
    }
    public partial class XsdDataContractExporter
    {
        public XsdDataContractExporter() { }
        public XsdDataContractExporter(System.Xml.Schema.XmlSchemaSet schemas) { }
        public System.Runtime.Serialization.ExportOptions Options { get { throw null; } set { } }
        public System.Xml.Schema.XmlSchemaSet Schemas { get { throw null; } }
        public bool CanExport(System.Collections.Generic.ICollection<System.Reflection.Assembly> assemblies) { throw null; }
        public bool CanExport(System.Collections.Generic.ICollection<System.Type> types) { throw null; }
        public bool CanExport(System.Type type) { throw null; }
        public void Export(System.Collections.Generic.ICollection<System.Reflection.Assembly> assemblies) { }
        public void Export(System.Collections.Generic.ICollection<System.Type> types) { }
        public void Export(System.Type type) { }
        public System.Xml.XmlQualifiedName GetRootElementName(System.Type type) { throw null; }
        public System.Xml.Schema.XmlSchemaType GetSchemaType(System.Type type) { throw null; }
        public System.Xml.XmlQualifiedName GetSchemaTypeName(System.Type type) { throw null; }
    }
}
namespace System.Runtime.Serialization.Json
{
    public sealed partial class DataContractJsonSerializer : System.Runtime.Serialization.XmlObjectSerializer
    {
        public DataContractJsonSerializer(System.Type type) { }
        public DataContractJsonSerializer(System.Type type, System.Collections.Generic.IEnumerable<System.Type> knownTypes) { }
        public DataContractJsonSerializer(System.Type type, System.Collections.Generic.IEnumerable<System.Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, System.Runtime.Serialization.IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation) { }
        public DataContractJsonSerializer(System.Type type, System.Runtime.Serialization.Json.DataContractJsonSerializerSettings settings) { }
        public DataContractJsonSerializer(System.Type type, string rootName) { }
        public DataContractJsonSerializer(System.Type type, string rootName, System.Collections.Generic.IEnumerable<System.Type> knownTypes) { }
        public DataContractJsonSerializer(System.Type type, string rootName, System.Collections.Generic.IEnumerable<System.Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, System.Runtime.Serialization.IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation) { }
        public DataContractJsonSerializer(System.Type type, System.Xml.XmlDictionaryString rootName) { }
        public DataContractJsonSerializer(System.Type type, System.Xml.XmlDictionaryString rootName, System.Collections.Generic.IEnumerable<System.Type> knownTypes) { }
        public DataContractJsonSerializer(System.Type type, System.Xml.XmlDictionaryString rootName, System.Collections.Generic.IEnumerable<System.Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, System.Runtime.Serialization.IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation) { }
        public System.Runtime.Serialization.IDataContractSurrogate DataContractSurrogate { get { throw null; } }
        public System.Runtime.Serialization.DateTimeFormat DateTimeFormat { get { throw null; } }
        public System.Runtime.Serialization.EmitTypeInformation EmitTypeInformation { get { throw null; } }
        public bool IgnoreExtensionDataObject { get { throw null; } }
        public System.Collections.ObjectModel.ReadOnlyCollection<System.Type> KnownTypes { get { throw null; } }
        public int MaxItemsInObjectGraph { get { throw null; } }
        public bool SerializeReadOnlyTypes { get { throw null; } }
        public bool UseSimpleDictionaryFormat { get { throw null; } }
        public override bool IsStartObject(System.Xml.XmlDictionaryReader reader) { throw null; }
        public override bool IsStartObject(System.Xml.XmlReader reader) { throw null; }
        public override object ReadObject(System.IO.Stream stream) { throw null; }
        public override object ReadObject(System.Xml.XmlDictionaryReader reader) { throw null; }
        public override object ReadObject(System.Xml.XmlDictionaryReader reader, bool verifyObjectName) { throw null; }
        public override object ReadObject(System.Xml.XmlReader reader) { throw null; }
        public override object ReadObject(System.Xml.XmlReader reader, bool verifyObjectName) { throw null; }
        public override void WriteEndObject(System.Xml.XmlDictionaryWriter writer) { }
        public override void WriteEndObject(System.Xml.XmlWriter writer) { }
        public override void WriteObject(System.IO.Stream stream, object graph) { }
        public override void WriteObject(System.Xml.XmlDictionaryWriter writer, object graph) { }
        public override void WriteObject(System.Xml.XmlWriter writer, object graph) { }
        public override void WriteObjectContent(System.Xml.XmlDictionaryWriter writer, object graph) { }
        public override void WriteObjectContent(System.Xml.XmlWriter writer, object graph) { }
        public override void WriteStartObject(System.Xml.XmlDictionaryWriter writer, object graph) { }
        public override void WriteStartObject(System.Xml.XmlWriter writer, object graph) { }
    }
    public partial class DataContractJsonSerializerSettings
    {
        public DataContractJsonSerializerSettings() { }
        public System.Runtime.Serialization.IDataContractSurrogate DataContractSurrogate { get { throw null; } set { } }
        public System.Runtime.Serialization.DateTimeFormat DateTimeFormat { get { throw null; } set { } }
        public System.Runtime.Serialization.EmitTypeInformation EmitTypeInformation { get { throw null; } set { } }
        public bool IgnoreExtensionDataObject { get { throw null; } set { } }
        public System.Collections.Generic.IEnumerable<System.Type> KnownTypes { get { throw null; } set { } }
        public int MaxItemsInObjectGraph { get { throw null; } set { } }
        public string RootName { get { throw null; } set { } }
        public bool SerializeReadOnlyTypes { get { throw null; } set { } }
        public bool UseSimpleDictionaryFormat { get { throw null; } set { } }
    }
    public partial interface IXmlJsonReaderInitializer
    {
        void SetInput(byte[] buffer, int offset, int count, System.Text.Encoding encoding, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.OnXmlDictionaryReaderClose onClose);
        void SetInput(System.IO.Stream stream, System.Text.Encoding encoding, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.OnXmlDictionaryReaderClose onClose);
    }
    public partial interface IXmlJsonWriterInitializer
    {
        void SetOutput(System.IO.Stream stream, System.Text.Encoding encoding, bool ownsStream);
    }
    public static partial class JsonReaderWriterFactory
    {
        public static System.Xml.XmlDictionaryReader CreateJsonReader(byte[] buffer, int offset, int count, System.Text.Encoding encoding, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.OnXmlDictionaryReaderClose onClose) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateJsonReader(byte[] buffer, int offset, int count, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateJsonReader(byte[] buffer, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateJsonReader(System.IO.Stream stream, System.Text.Encoding encoding, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.OnXmlDictionaryReaderClose onClose) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateJsonReader(System.IO.Stream stream, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateJsonWriter(System.IO.Stream stream) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateJsonWriter(System.IO.Stream stream, System.Text.Encoding encoding) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateJsonWriter(System.IO.Stream stream, System.Text.Encoding encoding, bool ownsStream) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateJsonWriter(System.IO.Stream stream, System.Text.Encoding encoding, bool ownsStream, bool indent) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateJsonWriter(System.IO.Stream stream, System.Text.Encoding encoding, bool ownsStream, bool indent, string indentChars) { throw null; }
    }
}
namespace System.Transactions
{
    [System.SerializableAttribute]
    public sealed partial class CommittableTransaction : System.Transactions.Transaction, System.IAsyncResult, System.IDisposable, System.Runtime.Serialization.ISerializable
    {
        public CommittableTransaction() { }
        public CommittableTransaction(System.TimeSpan timeout) { }
        public CommittableTransaction(System.Transactions.TransactionOptions options) { }
        object System.IAsyncResult.AsyncState { get { throw null; } }
        System.Threading.WaitHandle System.IAsyncResult.AsyncWaitHandle { get { throw null; } }
        bool System.IAsyncResult.CompletedSynchronously { get { throw null; } }
        bool System.IAsyncResult.IsCompleted { get { throw null; } }
        public System.IAsyncResult BeginCommit(System.AsyncCallback asyncCallback, object asyncState) { throw null; }
        public void Commit() { }
        public void EndCommit(System.IAsyncResult asyncResult) { }
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
    public enum DependentCloneOption
    {
        BlockCommitUntilComplete = 0,
        RollbackIfNotComplete = 1,
    }
    [System.SerializableAttribute]
    public sealed partial class DependentTransaction : System.Transactions.Transaction, System.Runtime.Serialization.ISerializable
    {
        internal DependentTransaction() { }
        public void Complete() { }
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
    public partial class Enlistment
    {
        internal Enlistment() { }
        public void Done() { }
    }
    [System.FlagsAttribute]
    public enum EnlistmentOptions
    {
        EnlistDuringPrepareRequired = 1,
        None = 0,
    }
    public enum EnterpriseServicesInteropOption
    {
        Automatic = 1,
        Full = 2,
        None = 0,
    }
    public delegate System.Transactions.Transaction HostCurrentTransactionCallback();
    [System.Runtime.InteropServices.InterfaceTypeAttribute((System.Runtime.InteropServices.ComInterfaceType)(1))]
    public partial interface IDtcTransaction
    {
        void Abort(System.IntPtr reason, int retaining, int async);
        void Commit(int retaining, int commitType, int reserved);
        void GetTransactionInfo(System.IntPtr transactionInformation);
    }
    public partial interface IEnlistmentNotification
    {
        void Commit(System.Transactions.Enlistment enlistment);
        void InDoubt(System.Transactions.Enlistment enlistment);
        void Prepare(System.Transactions.PreparingEnlistment preparingEnlistment);
        void Rollback(System.Transactions.Enlistment enlistment);
    }
    public partial interface IPromotableSinglePhaseNotification : System.Transactions.ITransactionPromoter
    {
        void Initialize();
        void Rollback(System.Transactions.SinglePhaseEnlistment singlePhaseEnlistment);
        void SinglePhaseCommit(System.Transactions.SinglePhaseEnlistment singlePhaseEnlistment);
    }
    public partial interface ISimpleTransactionSuperior : System.Transactions.ITransactionPromoter
    {
        void Rollback();
    }
    public partial interface ISinglePhaseNotification : System.Transactions.IEnlistmentNotification
    {
        void SinglePhaseCommit(System.Transactions.SinglePhaseEnlistment singlePhaseEnlistment);
    }
    public enum IsolationLevel
    {
        Chaos = 5,
        ReadCommitted = 2,
        ReadUncommitted = 3,
        RepeatableRead = 1,
        Serializable = 0,
        Snapshot = 4,
        Unspecified = 6,
    }
    public partial interface ITransactionPromoter
    {
        byte[] Promote();
    }
    public partial class PreparingEnlistment : System.Transactions.Enlistment
    {
        internal PreparingEnlistment() { }
        public void ForceRollback() { }
        public void ForceRollback(System.Exception e) { }
        public void Prepared() { }
        public byte[] RecoveryInformation() { throw null; }
    }
    public partial class SinglePhaseEnlistment : System.Transactions.Enlistment
    {
        internal SinglePhaseEnlistment() { }
        public void Aborted() { }
        public void Aborted(System.Exception e) { }
        public void Committed() { }
        public void InDoubt() { }
        public void InDoubt(System.Exception e) { }
    }
    [System.SerializableAttribute]
    public sealed partial class SubordinateTransaction : System.Transactions.Transaction
    {
        public SubordinateTransaction(System.Transactions.IsolationLevel isoLevel, System.Transactions.ISimpleTransactionSuperior superior) { }
    }
    [System.SerializableAttribute]
    public partial class Transaction : System.IDisposable, System.Runtime.Serialization.ISerializable
    {
        internal Transaction() { }
        public static System.Transactions.Transaction Current { get { throw null; } set { } }
        public System.Transactions.IsolationLevel IsolationLevel { get { throw null; } }
        public System.Guid PromoterType { get { throw null; } }
        public System.Transactions.TransactionInformation TransactionInformation { get { throw null; } }
        public event System.Transactions.TransactionCompletedEventHandler TransactionCompleted { add { } remove { } }
        protected System.IAsyncResult BeginCommitInternal(System.AsyncCallback callback) { throw null; }
        public System.Transactions.Transaction Clone() { throw null; }
        public System.Transactions.DependentTransaction DependentClone(System.Transactions.DependentCloneOption cloneOption) { throw null; }
        public void Dispose() { }
        protected void EndCommitInternal(System.IAsyncResult ar) { }
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand)]
        public System.Transactions.Enlistment EnlistDurable(System.Guid resourceManagerIdentifier, System.Transactions.IEnlistmentNotification enlistmentNotification, System.Transactions.EnlistmentOptions enlistmentOptions) { throw null; }
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand)]
        public System.Transactions.Enlistment EnlistDurable(System.Guid resourceManagerIdentifier, System.Transactions.ISinglePhaseNotification singlePhaseNotification, System.Transactions.EnlistmentOptions enlistmentOptions) { throw null; }
        public bool EnlistPromotableSinglePhase(System.Transactions.IPromotableSinglePhaseNotification promotableSinglePhaseNotification) { throw null; }
        public bool EnlistPromotableSinglePhase(System.Transactions.IPromotableSinglePhaseNotification promotableSinglePhaseNotification, System.Guid promoterType) { throw null; }
        public System.Transactions.Enlistment EnlistVolatile(System.Transactions.IEnlistmentNotification enlistmentNotification, System.Transactions.EnlistmentOptions enlistmentOptions) { throw null; }
        public System.Transactions.Enlistment EnlistVolatile(System.Transactions.ISinglePhaseNotification singlePhaseNotification, System.Transactions.EnlistmentOptions enlistmentOptions) { throw null; }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public byte[] GetPromotedToken() { throw null; }
        public static bool operator ==(System.Transactions.Transaction x, System.Transactions.Transaction y) { throw null; }
        public static bool operator !=(System.Transactions.Transaction x, System.Transactions.Transaction y) { throw null; }
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand)]
        public System.Transactions.Enlistment PromoteAndEnlistDurable(System.Guid manager, System.Transactions.IPromotableSinglePhaseNotification promotableNotification, System.Transactions.ISinglePhaseNotification notification, System.Transactions.EnlistmentOptions options) { throw null; }
        public void Rollback() { }
        public void Rollback(System.Exception e) { }
        public void SetDistributedTransactionIdentifier(System.Transactions.IPromotableSinglePhaseNotification promotableNotification, System.Guid distributedTransactionIdentifier) { }
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
    [System.SerializableAttribute]
    public partial class TransactionAbortedException : System.Transactions.TransactionException
    {
        public TransactionAbortedException() { }
        protected TransactionAbortedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public TransactionAbortedException(string message) { }
        public TransactionAbortedException(string message, System.Exception innerException) { }
    }
    public delegate void TransactionCompletedEventHandler(object sender, System.Transactions.TransactionEventArgs e);
    public partial class TransactionEventArgs : System.EventArgs
    {
        public TransactionEventArgs() { }
        public System.Transactions.Transaction Transaction { get { throw null; } }
    }
    [System.SerializableAttribute]
    public partial class TransactionException : System.SystemException
    {
        public TransactionException() { }
        protected TransactionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public TransactionException(string message) { }
        public TransactionException(string message, System.Exception innerException) { }
    }
    [System.SerializableAttribute]
    public partial class TransactionInDoubtException : System.Transactions.TransactionException
    {
        public TransactionInDoubtException() { }
        protected TransactionInDoubtException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public TransactionInDoubtException(string message) { }
        public TransactionInDoubtException(string message, System.Exception innerException) { }
    }
    public partial class TransactionInformation
    {
        internal TransactionInformation() { }
        public System.DateTime CreationTime { get { throw null; } }
        public System.Guid DistributedIdentifier { get { throw null; } }
        public string LocalIdentifier { get { throw null; } }
        public System.Transactions.TransactionStatus Status { get { throw null; } }
    }
    public static partial class TransactionInterop
    {
        public static readonly System.Guid PromoterTypeDtc;
        public static System.Transactions.IDtcTransaction GetDtcTransaction(System.Transactions.Transaction transaction) { throw null; }
        public static byte[] GetExportCookie(System.Transactions.Transaction transaction, byte[] whereabouts) { throw null; }
        public static System.Transactions.Transaction GetTransactionFromDtcTransaction(System.Transactions.IDtcTransaction transactionNative) { throw null; }
        public static System.Transactions.Transaction GetTransactionFromExportCookie(byte[] cookie) { throw null; }
        public static System.Transactions.Transaction GetTransactionFromTransmitterPropagationToken(byte[] propagationToken) { throw null; }
        public static byte[] GetTransmitterPropagationToken(System.Transactions.Transaction transaction) { throw null; }
        public static byte[] GetWhereabouts() { throw null; }
    }
    public static partial class TransactionManager
    {
        public static System.TimeSpan DefaultTimeout { get { throw null; } }
        public static System.Transactions.HostCurrentTransactionCallback HostCurrentCallback { get { throw null; } set { } }
        public static System.TimeSpan MaximumTimeout { get { throw null; } }
        public static event System.Transactions.TransactionStartedEventHandler DistributedTransactionStarted { add { } remove { } }
        public static void RecoveryComplete(System.Guid resourceManagerIdentifier) { }
        public static System.Transactions.Enlistment Reenlist(System.Guid resourceManagerIdentifier, byte[] recoveryInformation, System.Transactions.IEnlistmentNotification enlistmentNotification) { throw null; }
    }
    [System.SerializableAttribute]
    public partial class TransactionManagerCommunicationException : System.Transactions.TransactionException
    {
        public TransactionManagerCommunicationException() { }
        protected TransactionManagerCommunicationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public TransactionManagerCommunicationException(string message) { }
        public TransactionManagerCommunicationException(string message, System.Exception innerException) { }
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct TransactionOptions
    {
        public System.Transactions.IsolationLevel IsolationLevel { get { throw null; } set { } }
        public System.TimeSpan Timeout { get { throw null; } set { } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static bool operator ==(System.Transactions.TransactionOptions x, System.Transactions.TransactionOptions y) { throw null; }
        public static bool operator !=(System.Transactions.TransactionOptions x, System.Transactions.TransactionOptions y) { throw null; }
    }
    [System.SerializableAttribute]
    public partial class TransactionPromotionException : System.Transactions.TransactionException
    {
        public TransactionPromotionException() { }
        protected TransactionPromotionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public TransactionPromotionException(string message) { }
        public TransactionPromotionException(string message, System.Exception innerException) { }
    }
    public sealed partial class TransactionScope : System.IDisposable
    {
        public TransactionScope() { }
        public TransactionScope(System.Transactions.Transaction transactionToUse) { }
        public TransactionScope(System.Transactions.Transaction transactionToUse, System.TimeSpan scopeTimeout) { }
        public TransactionScope(System.Transactions.Transaction transactionToUse, System.TimeSpan scopeTimeout, System.Transactions.EnterpriseServicesInteropOption interopOption) { }
        public TransactionScope(System.Transactions.Transaction transactionToUse, System.TimeSpan scopeTimeout, System.Transactions.TransactionScopeAsyncFlowOption asyncFlowOption) { }
        public TransactionScope(System.Transactions.Transaction transactionToUse, System.Transactions.TransactionScopeAsyncFlowOption asyncFlowOption) { }
        public TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption asyncFlowOption) { }
        public TransactionScope(System.Transactions.TransactionScopeOption scopeOption) { }
        public TransactionScope(System.Transactions.TransactionScopeOption scopeOption, System.TimeSpan scopeTimeout) { }
        public TransactionScope(System.Transactions.TransactionScopeOption scopeOption, System.TimeSpan scopeTimeout, System.Transactions.TransactionScopeAsyncFlowOption asyncFlow) { }
        public TransactionScope(System.Transactions.TransactionScopeOption scopeOption, System.Transactions.TransactionOptions transactionOptions) { }
        public TransactionScope(System.Transactions.TransactionScopeOption scopeOption, System.Transactions.TransactionOptions transactionOptions, System.Transactions.EnterpriseServicesInteropOption interopOption) { }
        public TransactionScope(System.Transactions.TransactionScopeOption scopeOption, System.Transactions.TransactionOptions transactionOptions, System.Transactions.TransactionScopeAsyncFlowOption asyncFlowOption) { }
        public TransactionScope(System.Transactions.TransactionScopeOption option, System.Transactions.TransactionScopeAsyncFlowOption asyncFlow) { }
        public void Complete() { }
        public void Dispose() { }
    }
    public enum TransactionScopeAsyncFlowOption
    {
        Enabled = 1,
        Suppress = 0,
    }
    public enum TransactionScopeOption
    {
        Required = 0,
        RequiresNew = 1,
        Suppress = 2,
    }
    public delegate void TransactionStartedEventHandler(object sender, System.Transactions.TransactionEventArgs e);
    public enum TransactionStatus
    {
        Aborted = 2,
        Active = 0,
        Committed = 1,
        InDoubt = 3,
    }
}
namespace System.Xml
{
    public partial interface IFragmentCapableXmlDictionaryWriter
    {
        bool CanFragment { get; }
        void EndFragment();
        void StartFragment(System.IO.Stream stream, bool generateSelfContainedTextFragment);
        void WriteFragment(byte[] buffer, int offset, int count);
    }
    public partial interface IStreamProvider
    {
        System.IO.Stream GetStream();
        void ReleaseStream(System.IO.Stream stream);
    }
    public partial interface IXmlBinaryReaderInitializer
    {
        void SetInput(byte[] buffer, int offset, int count, System.Xml.IXmlDictionary dictionary, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.XmlBinaryReaderSession session, System.Xml.OnXmlDictionaryReaderClose onClose);
        void SetInput(System.IO.Stream stream, System.Xml.IXmlDictionary dictionary, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.XmlBinaryReaderSession session, System.Xml.OnXmlDictionaryReaderClose onClose);
    }
    public partial interface IXmlBinaryWriterInitializer
    {
        void SetOutput(System.IO.Stream stream, System.Xml.IXmlDictionary dictionary, System.Xml.XmlBinaryWriterSession session, bool ownsStream);
    }
    public partial interface IXmlDictionary
    {
        bool TryLookup(int key, out System.Xml.XmlDictionaryString result);
        bool TryLookup(string value, out System.Xml.XmlDictionaryString result);
        bool TryLookup(System.Xml.XmlDictionaryString value, out System.Xml.XmlDictionaryString result);
    }
    public partial interface IXmlMtomReaderInitializer
    {
        void SetInput(byte[] buffer, int offset, int count, System.Text.Encoding[] encodings, string contentType, System.Xml.XmlDictionaryReaderQuotas quotas, int maxBufferSize, System.Xml.OnXmlDictionaryReaderClose onClose);
        void SetInput(System.IO.Stream stream, System.Text.Encoding[] encodings, string contentType, System.Xml.XmlDictionaryReaderQuotas quotas, int maxBufferSize, System.Xml.OnXmlDictionaryReaderClose onClose);
    }
    public partial interface IXmlMtomWriterInitializer
    {
        void SetOutput(System.IO.Stream stream, System.Text.Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream);
    }
    public partial interface IXmlTextReaderInitializer
    {
        void SetInput(byte[] buffer, int offset, int count, System.Text.Encoding encoding, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.OnXmlDictionaryReaderClose onClose);
        void SetInput(System.IO.Stream stream, System.Text.Encoding encoding, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.OnXmlDictionaryReaderClose onClose);
    }
    public partial interface IXmlTextWriterInitializer
    {
        void SetOutput(System.IO.Stream stream, System.Text.Encoding encoding, bool ownsStream);
    }
    public delegate void OnXmlDictionaryReaderClose(System.Xml.XmlDictionaryReader reader);
    public partial class UniqueId
    {
        public UniqueId() { }
        public UniqueId(byte[] guid) { }
        [System.Security.SecuritySafeCriticalAttribute]
        public UniqueId(byte[] guid, int offset) { }
        [System.Security.SecuritySafeCriticalAttribute]
        public UniqueId(char[] chars, int offset, int count) { }
        public UniqueId(System.Guid guid) { }
        [System.Security.SecuritySafeCriticalAttribute]
        public UniqueId(string value) { }
        public int CharArrayLength { [System.Security.SecuritySafeCriticalAttribute]get { throw null; } }
        public bool IsGuid { get { throw null; } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static bool operator ==(System.Xml.UniqueId id1, System.Xml.UniqueId id2) { throw null; }
        public static bool operator !=(System.Xml.UniqueId id1, System.Xml.UniqueId id2) { throw null; }
        [System.Security.SecuritySafeCriticalAttribute]
        public int ToCharArray(char[] chars, int offset) { throw null; }
        [System.Security.SecuritySafeCriticalAttribute]
        public override string ToString() { throw null; }
        [System.Security.SecuritySafeCriticalAttribute]
        public bool TryGetGuid(byte[] buffer, int offset) { throw null; }
        public bool TryGetGuid(out System.Guid guid) { guid = default(System.Guid); throw null; }
    }
    public partial class XmlBinaryReaderSession : System.Xml.IXmlDictionary
    {
        public XmlBinaryReaderSession() { }
        public System.Xml.XmlDictionaryString Add(int id, string value) { throw null; }
        public void Clear() { }
        public bool TryLookup(int key, out System.Xml.XmlDictionaryString result) { result = default(System.Xml.XmlDictionaryString); throw null; }
        public bool TryLookup(string value, out System.Xml.XmlDictionaryString result) { result = default(System.Xml.XmlDictionaryString); throw null; }
        public bool TryLookup(System.Xml.XmlDictionaryString value, out System.Xml.XmlDictionaryString result) { result = default(System.Xml.XmlDictionaryString); throw null; }
    }
    public partial class XmlBinaryWriterSession
    {
        public XmlBinaryWriterSession() { }
        public void Reset() { }
        public virtual bool TryAdd(System.Xml.XmlDictionaryString value, out int key) { key = default(int); throw null; }
    }
    public partial class XmlDictionary : System.Xml.IXmlDictionary
    {
        public XmlDictionary() { }
        public XmlDictionary(int capacity) { }
        public static System.Xml.IXmlDictionary Empty { get { throw null; } }
        public virtual System.Xml.XmlDictionaryString Add(string value) { throw null; }
        public virtual bool TryLookup(int key, out System.Xml.XmlDictionaryString result) { result = default(System.Xml.XmlDictionaryString); throw null; }
        public virtual bool TryLookup(string value, out System.Xml.XmlDictionaryString result) { result = default(System.Xml.XmlDictionaryString); throw null; }
        public virtual bool TryLookup(System.Xml.XmlDictionaryString value, out System.Xml.XmlDictionaryString result) { result = default(System.Xml.XmlDictionaryString); throw null; }
    }
    public abstract partial class XmlDictionaryReader : System.Xml.XmlReader
    {
        protected XmlDictionaryReader() { }
        public virtual bool CanCanonicalize { get { throw null; } }
        public virtual System.Xml.XmlDictionaryReaderQuotas Quotas { get { throw null; } }
        public static System.Xml.XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, System.Xml.IXmlDictionary dictionary, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, System.Xml.IXmlDictionary dictionary, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.XmlBinaryReaderSession session) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, System.Xml.IXmlDictionary dictionary, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.XmlBinaryReaderSession session, System.Xml.OnXmlDictionaryReaderClose onClose) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateBinaryReader(byte[] buffer, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateBinaryReader(System.IO.Stream stream, System.Xml.IXmlDictionary dictionary, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateBinaryReader(System.IO.Stream stream, System.Xml.IXmlDictionary dictionary, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.XmlBinaryReaderSession session) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateBinaryReader(System.IO.Stream stream, System.Xml.IXmlDictionary dictionary, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.XmlBinaryReaderSession session, System.Xml.OnXmlDictionaryReaderClose onClose) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateBinaryReader(System.IO.Stream stream, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateDictionaryReader(System.Xml.XmlReader reader) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, System.Text.Encoding encoding, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, System.Text.Encoding[] encodings, string contentType, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, System.Text.Encoding[] encodings, string contentType, System.Xml.XmlDictionaryReaderQuotas quotas, int maxBufferSize, System.Xml.OnXmlDictionaryReaderClose onClose) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, System.Text.Encoding[] encodings, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateMtomReader(System.IO.Stream stream, System.Text.Encoding encoding, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateMtomReader(System.IO.Stream stream, System.Text.Encoding[] encodings, string contentType, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateMtomReader(System.IO.Stream stream, System.Text.Encoding[] encodings, string contentType, System.Xml.XmlDictionaryReaderQuotas quotas, int maxBufferSize, System.Xml.OnXmlDictionaryReaderClose onClose) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateMtomReader(System.IO.Stream stream, System.Text.Encoding[] encodings, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateTextReader(byte[] buffer, int offset, int count, System.Text.Encoding encoding, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.OnXmlDictionaryReaderClose onClose) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateTextReader(byte[] buffer, int offset, int count, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateTextReader(byte[] buffer, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateTextReader(System.IO.Stream stream, System.Text.Encoding encoding, System.Xml.XmlDictionaryReaderQuotas quotas, System.Xml.OnXmlDictionaryReaderClose onClose) { throw null; }
        public static System.Xml.XmlDictionaryReader CreateTextReader(System.IO.Stream stream, System.Xml.XmlDictionaryReaderQuotas quotas) { throw null; }
        public virtual void EndCanonicalization() { }
        public virtual string GetAttribute(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual void GetNonAtomizedNames(out string localName, out string namespaceUri) { localName = default(string); namespaceUri = default(string); }
        public virtual int IndexOfLocalName(string[] localNames, string namespaceUri) { throw null; }
        public virtual int IndexOfLocalName(System.Xml.XmlDictionaryString[] localNames, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual bool IsLocalName(string localName) { throw null; }
        public virtual bool IsLocalName(System.Xml.XmlDictionaryString localName) { throw null; }
        public virtual bool IsNamespaceUri(string namespaceUri) { throw null; }
        public virtual bool IsNamespaceUri(System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual bool IsStartArray(out System.Type type) { type = default(System.Type); throw null; }
        public virtual bool IsStartElement(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        protected bool IsTextNode(System.Xml.XmlNodeType nodeType) { throw null; }
        public virtual void MoveToStartElement() { }
        public virtual void MoveToStartElement(string name) { }
        public virtual void MoveToStartElement(string localName, string namespaceUri) { }
        public virtual void MoveToStartElement(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { }
        public virtual int ReadArray(string localName, string namespaceUri, bool[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(string localName, string namespaceUri, System.DateTime[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(string localName, string namespaceUri, decimal[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(string localName, string namespaceUri, double[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(string localName, string namespaceUri, System.Guid[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(string localName, string namespaceUri, short[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(string localName, string namespaceUri, int[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(string localName, string namespaceUri, long[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(string localName, string namespaceUri, float[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(string localName, string namespaceUri, System.TimeSpan[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, bool[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, System.DateTime[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, decimal[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, double[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, System.Guid[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, short[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, int[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, long[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, float[] array, int offset, int count) { throw null; }
        public virtual int ReadArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, System.TimeSpan[] array, int offset, int count) { throw null; }
        public virtual bool[] ReadBooleanArray(string localName, string namespaceUri) { throw null; }
        public virtual bool[] ReadBooleanArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public override object ReadContentAs(System.Type type, System.Xml.IXmlNamespaceResolver namespaceResolver) { throw null; }
        public virtual byte[] ReadContentAsBase64() { throw null; }
        public virtual byte[] ReadContentAsBinHex() { throw null; }
        protected byte[] ReadContentAsBinHex(int maxByteArrayContentLength) { throw null; }
        public virtual int ReadContentAsChars(char[] chars, int offset, int count) { throw null; }
        public override decimal ReadContentAsDecimal() { throw null; }
        public override float ReadContentAsFloat() { throw null; }
        public virtual System.Guid ReadContentAsGuid() { throw null; }
        public virtual void ReadContentAsQualifiedName(out string localName, out string namespaceUri) { localName = default(string); namespaceUri = default(string); }
        public override string ReadContentAsString() { throw null; }
        protected string ReadContentAsString(int maxStringContentLength) { throw null; }
        public virtual string ReadContentAsString(string[] strings, out int index) { index = default(int); throw null; }
        public virtual string ReadContentAsString(System.Xml.XmlDictionaryString[] strings, out int index) { index = default(int); throw null; }
        public virtual System.TimeSpan ReadContentAsTimeSpan() { throw null; }
        public virtual System.Xml.UniqueId ReadContentAsUniqueId() { throw null; }
        public virtual System.DateTime[] ReadDateTimeArray(string localName, string namespaceUri) { throw null; }
        public virtual System.DateTime[] ReadDateTimeArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual decimal[] ReadDecimalArray(string localName, string namespaceUri) { throw null; }
        public virtual decimal[] ReadDecimalArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual double[] ReadDoubleArray(string localName, string namespaceUri) { throw null; }
        public virtual double[] ReadDoubleArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual byte[] ReadElementContentAsBase64() { throw null; }
        public virtual byte[] ReadElementContentAsBinHex() { throw null; }
        public override bool ReadElementContentAsBoolean() { throw null; }
        public override System.DateTime ReadElementContentAsDateTime() { throw null; }
        public override decimal ReadElementContentAsDecimal() { throw null; }
        public override double ReadElementContentAsDouble() { throw null; }
        public override float ReadElementContentAsFloat() { throw null; }
        public virtual System.Guid ReadElementContentAsGuid() { throw null; }
        public override int ReadElementContentAsInt() { throw null; }
        public override long ReadElementContentAsLong() { throw null; }
        public override string ReadElementContentAsString() { throw null; }
        public virtual System.TimeSpan ReadElementContentAsTimeSpan() { throw null; }
        public virtual System.Xml.UniqueId ReadElementContentAsUniqueId() { throw null; }
        public virtual void ReadFullStartElement() { }
        public virtual void ReadFullStartElement(string name) { }
        public virtual void ReadFullStartElement(string localName, string namespaceUri) { }
        public virtual void ReadFullStartElement(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { }
        public virtual System.Guid[] ReadGuidArray(string localName, string namespaceUri) { throw null; }
        public virtual System.Guid[] ReadGuidArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual short[] ReadInt16Array(string localName, string namespaceUri) { throw null; }
        public virtual short[] ReadInt16Array(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual int[] ReadInt32Array(string localName, string namespaceUri) { throw null; }
        public virtual int[] ReadInt32Array(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual long[] ReadInt64Array(string localName, string namespaceUri) { throw null; }
        public virtual long[] ReadInt64Array(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual float[] ReadSingleArray(string localName, string namespaceUri) { throw null; }
        public virtual float[] ReadSingleArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual void ReadStartElement(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { }
        public override string ReadString() { throw null; }
        protected string ReadString(int maxStringContentLength) { throw null; }
        public virtual System.TimeSpan[] ReadTimeSpanArray(string localName, string namespaceUri) { throw null; }
        public virtual System.TimeSpan[] ReadTimeSpanArray(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { throw null; }
        public virtual int ReadValueAsBase64(byte[] buffer, int offset, int count) { throw null; }
        public virtual void StartCanonicalization(System.IO.Stream stream, bool includeComments, string[] inclusivePrefixes) { }
        public virtual bool TryGetArrayLength(out int count) { count = default(int); throw null; }
        public virtual bool TryGetBase64ContentLength(out int length) { length = default(int); throw null; }
        public virtual bool TryGetLocalNameAsDictionaryString(out System.Xml.XmlDictionaryString localName) { localName = default(System.Xml.XmlDictionaryString); throw null; }
        public virtual bool TryGetNamespaceUriAsDictionaryString(out System.Xml.XmlDictionaryString namespaceUri) { namespaceUri = default(System.Xml.XmlDictionaryString); throw null; }
        public virtual bool TryGetValueAsDictionaryString(out System.Xml.XmlDictionaryString value) { value = default(System.Xml.XmlDictionaryString); throw null; }
    }
    public sealed partial class XmlDictionaryReaderQuotas
    {
        public XmlDictionaryReaderQuotas() { }
        public static System.Xml.XmlDictionaryReaderQuotas Max { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute(16384)]
        public int MaxArrayLength { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(4096)]
        public int MaxBytesPerRead { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(32)]
        public int MaxDepth { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(16384)]
        public int MaxNameTableCharCount { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(8192)]
        public int MaxStringContentLength { get { throw null; } set { } }
        public System.Xml.XmlDictionaryReaderQuotaTypes ModifiedQuotas { get { throw null; } }
        public void CopyTo(System.Xml.XmlDictionaryReaderQuotas quotas) { }
    }
    [System.FlagsAttribute]
    public enum XmlDictionaryReaderQuotaTypes
    {
        MaxArrayLength = 4,
        MaxBytesPerRead = 8,
        MaxDepth = 1,
        MaxNameTableCharCount = 16,
        MaxStringContentLength = 2,
    }
    public partial class XmlDictionaryString
    {
        public XmlDictionaryString(System.Xml.IXmlDictionary dictionary, string value, int key) { }
        public System.Xml.IXmlDictionary Dictionary { get { throw null; } }
        public static System.Xml.XmlDictionaryString Empty { get { throw null; } }
        public int Key { get { throw null; } }
        public string Value { get { throw null; } }
        public override string ToString() { throw null; }
    }
    public abstract partial class XmlDictionaryWriter : System.Xml.XmlWriter
    {
        protected XmlDictionaryWriter() { }
        public virtual bool CanCanonicalize { get { throw null; } }
        public static System.Xml.XmlDictionaryWriter CreateBinaryWriter(System.IO.Stream stream) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateBinaryWriter(System.IO.Stream stream, System.Xml.IXmlDictionary dictionary) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateBinaryWriter(System.IO.Stream stream, System.Xml.IXmlDictionary dictionary, System.Xml.XmlBinaryWriterSession session) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateBinaryWriter(System.IO.Stream stream, System.Xml.IXmlDictionary dictionary, System.Xml.XmlBinaryWriterSession session, bool ownsStream) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateDictionaryWriter(System.Xml.XmlWriter writer) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateMtomWriter(System.IO.Stream stream, System.Text.Encoding encoding, int maxSizeInBytes, string startInfo) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateMtomWriter(System.IO.Stream stream, System.Text.Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateTextWriter(System.IO.Stream stream) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateTextWriter(System.IO.Stream stream, System.Text.Encoding encoding) { throw null; }
        public static System.Xml.XmlDictionaryWriter CreateTextWriter(System.IO.Stream stream, System.Text.Encoding encoding, bool ownsStream) { throw null; }
        public virtual void EndCanonicalization() { }
        public virtual void StartCanonicalization(System.IO.Stream stream, bool includeComments, string[] inclusivePrefixes) { }
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, bool[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, System.DateTime[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, decimal[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, double[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, System.Guid[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, short[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, int[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, long[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, float[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, System.TimeSpan[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, bool[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, System.DateTime[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, decimal[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, double[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, System.Guid[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, short[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, int[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, long[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, float[] array, int offset, int count) { }
        public virtual void WriteArray(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, System.TimeSpan[] array, int offset, int count) { }
        public void WriteAttributeString(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, string value) { }
        public void WriteAttributeString(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, string value) { }
        public override System.Threading.Tasks.Task WriteBase64Async(byte[] buffer, int index, int count) { throw null; }
        public void WriteElementString(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, string value) { }
        public void WriteElementString(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri, string value) { }
        public virtual void WriteNode(System.Xml.XmlDictionaryReader reader, bool defattr) { }
        public override void WriteNode(System.Xml.XmlReader reader, bool defattr) { }
        public virtual void WriteQualifiedName(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { }
        public virtual void WriteStartAttribute(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { }
        public void WriteStartAttribute(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { }
        public virtual void WriteStartElement(string prefix, System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { }
        public void WriteStartElement(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString namespaceUri) { }
        public virtual void WriteString(System.Xml.XmlDictionaryString value) { }
        protected virtual void WriteTextNode(System.Xml.XmlDictionaryReader reader, bool isAttribute) { }
        public virtual void WriteValue(System.Guid value) { }
        public virtual void WriteValue(System.TimeSpan value) { }
        public virtual void WriteValue(System.Xml.IStreamProvider value) { }
        public virtual void WriteValue(System.Xml.UniqueId value) { }
        public virtual void WriteValue(System.Xml.XmlDictionaryString value) { }
        public virtual System.Threading.Tasks.Task WriteValueAsync(System.Xml.IStreamProvider value) { throw null; }
        public virtual void WriteXmlAttribute(string localName, string value) { }
        public virtual void WriteXmlAttribute(System.Xml.XmlDictionaryString localName, System.Xml.XmlDictionaryString value) { }
        public virtual void WriteXmlnsAttribute(string prefix, string namespaceUri) { }
        public virtual void WriteXmlnsAttribute(string prefix, System.Xml.XmlDictionaryString namespaceUri) { }
    }
}
namespace System.Web
{
    public sealed partial class HttpUtility
    {
        public HttpUtility() { }
        public static string HtmlAttributeEncode(string s) { throw null; }
        public static void HtmlAttributeEncode(string s, System.IO.TextWriter output) { }
        public static string HtmlDecode(string s) { throw null; }
        public static void HtmlDecode(string s, System.IO.TextWriter output) { }
        public static string HtmlEncode(object value) { throw null; }
        public static string HtmlEncode(string s) { throw null; }
        public static void HtmlEncode(string s, System.IO.TextWriter output) { }
        public static string JavaScriptStringEncode(string value) { throw null; }
        public static string JavaScriptStringEncode(string value, bool addDoubleQuotes) { throw null; }
        public static System.Collections.Specialized.NameValueCollection ParseQueryString(string query) { throw null; }
        public static System.Collections.Specialized.NameValueCollection ParseQueryString(string query, System.Text.Encoding encoding) { throw null; }
        public static string UrlDecode(byte[] bytes, int offset, int count, System.Text.Encoding e) { throw null; }
        public static string UrlDecode(byte[] bytes, System.Text.Encoding e) { throw null; }
        public static string UrlDecode(string str) { throw null; }
        public static string UrlDecode(string str, System.Text.Encoding e) { throw null; }
        public static byte[] UrlDecodeToBytes(byte[] bytes) { throw null; }
        public static byte[] UrlDecodeToBytes(byte[] bytes, int offset, int count) { throw null; }
        public static byte[] UrlDecodeToBytes(string str) { throw null; }
        public static byte[] UrlDecodeToBytes(string str, System.Text.Encoding e) { throw null; }
        public static string UrlEncode(byte[] bytes) { throw null; }
        public static string UrlEncode(byte[] bytes, int offset, int count) { throw null; }
        public static string UrlEncode(string str) { throw null; }
        public static string UrlEncode(string str, System.Text.Encoding e) { throw null; }
        public static byte[] UrlEncodeToBytes(byte[] bytes) { throw null; }
        public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count) { throw null; }
        public static byte[] UrlEncodeToBytes(string str) { throw null; }
        public static byte[] UrlEncodeToBytes(string str, System.Text.Encoding e) { throw null; }
        public static string UrlEncodeUnicode(string str) { throw null; }
        public static byte[] UrlEncodeUnicodeToBytes(string str) { throw null; }
        public static string UrlPathEncode(string str) { throw null; }
    }
}