//------------------------------------------------------------------------------
// <copyright file="DataColumn.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Xml;
    using System.Data.Common;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Collections;
    using System.Globalization;
    using System.Data.SqlTypes;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <devdoc>
    ///    <para>
    ///       Represents one column of data in a <see cref='System.Data.DataTable'/>.
    ///    </para>
    /// </devdoc>
    [
    ToolboxItem(false),
    DesignTimeVisible(false),
    DefaultProperty("ColumnName"),
    Editor("Microsoft.VSDesigner.Data.Design.DataColumnEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    ]
 public class DataColumn : MarshalByValueComponent {

        // properties
        private bool allowNull = true;
        private string caption = null;
        private string _columnName = null;
        private Type dataType = null;
        private StorageType _storageType;
        internal object defaultValue = DBNull.Value;             // DefaultValue Converter
        private DataSetDateTime _dateTimeMode = DataSetDateTime.UnspecifiedLocal;
        private DataExpression expression = null;
        private int maxLength = -1;
        private int _ordinal = -1;
        private bool readOnly = false;
        internal Index sortIndex = null;
        internal DataTable table = null;
        private bool unique = false;
        internal MappingType columnMapping = MappingType.Element;
        internal int _hashCode;

        internal int errors;
        private bool isSqlType = false;
        private bool implementsINullable = false;
        private bool implementsIChangeTracking = false;
        private bool implementsIRevertibleChangeTracking = false;
        private bool implementsIXMLSerializable = false;

        private bool defaultValueIsNull = true;

        // list of columns whose expression consume values from this column
        internal List<DataColumn> dependentColumns = null;

        // collections
        internal PropertyCollection extendedProperties = null;

        // events
        private PropertyChangedEventHandler onPropertyChangingDelegate = null;

        // state
        private DataStorage _storage;

        /// <summary>represents current value to return, usage pattern is .get_Current then MoveAfter</summary>
        private AutoIncrementValue autoInc;

        //
        // The _columnClass member is the class for the unfoliated virtual nodes in the XML.
        //
        internal string _columnUri = null;
        private string _columnPrefix = "";
        internal string encodedColumnName = null;

        // 
        internal string dttype = "";        // The type specified in dt:type attribute
        internal SimpleType simpleType = null;

        private static int _objectTypeCount; // Bid counter
        private readonly int _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of a <see cref='System.Data.DataColumn'/>
        ///       class.
        ///    </para>
        /// </devdoc>
        public DataColumn() : this(null, typeof(string), null, MappingType.Element) {
        }

        /// <devdoc>
        ///    <para>
        ///       Inititalizes a new instance of the <see cref='System.Data.DataColumn'/> class
        ///       using the specified column name.
        ///    </para>
        /// </devdoc>
        public DataColumn(string columnName) : this(columnName, typeof(string), null, MappingType.Element) {
        }

        /// <devdoc>
        ///    <para>
        ///       Inititalizes a new instance of the <see cref='System.Data.DataColumn'/> class
        ///       using the specified column name and data type.
        ///    </para>
        /// </devdoc>
        public DataColumn(string columnName, Type dataType) : this(columnName, dataType, null, MappingType.Element) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance
        ///       of the <see cref='System.Data.DataColumn'/> class
        ///       using the specified name, data type, and expression.
        ///    </para>
        /// </devdoc>
        public DataColumn(string columnName, Type dataType, string expr) : this(columnName, dataType, expr, MappingType.Element) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.DataColumn'/> class
        ///       using
        ///       the specified name, data type, expression, and value that determines whether the
        ///       column is an attribute.
        ///    </para>
        /// </devdoc>
        public DataColumn(string columnName, Type dataType, string expr, MappingType type) {
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataColumn.DataColumn|API> %d#, columnName='%ls', expr='%ls', type=%d{ds.MappingType}\n",
                          ObjectID, columnName, expr, (int)type);

            if (dataType == null) {
                throw ExceptionBuilder.ArgumentNull("dataType");
            }

            StorageType typeCode = DataStorage.GetStorageType(dataType);
            if (DataStorage.ImplementsINullableValue(typeCode, dataType)) {
                throw ExceptionBuilder.ColumnTypeNotSupported();
            }
            _columnName = columnName ?? string.Empty;

            SimpleType stype = SimpleType.CreateSimpleType(typeCode, dataType);
            if (null != stype) {
                this.SimpleType = stype;
            }
            UpdateColumnType(dataType, typeCode);

            if ((null != expr) && (0 < expr.Length)) {
                // @perfnote: its a performance hit to set Expression to the empty str when we know it will come out null
                this.Expression = expr;
            }
            this.columnMapping = type;
        }


        private void UpdateColumnType(Type type, StorageType typeCode) {
            dataType = type;
            _storageType = typeCode;
            if (StorageType.DateTime != typeCode) { // revert _dateTimeMode back to default, when column type is changed
                _dateTimeMode = DataSetDateTime.UnspecifiedLocal;
            }
            DataStorage.ImplementsInterfaces(
                                typeCode, type,
                                out isSqlType,
                                out implementsINullable,
                                out implementsIXMLSerializable,
                                out implementsIChangeTracking,
                                out implementsIRevertibleChangeTracking);

            if (!isSqlType && implementsINullable) {
                SqlUdtStorage.GetStaticNullForUdtType(type);
            }
        }

        // PUBLIC PROPERTIES

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether null
        ///       values are
        ///       allowed in this column for rows belonging to the table.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(true),
        ResDescriptionAttribute(Res.DataColumnAllowNullDescr)
        ]
        public bool AllowDBNull {
            get {
                return allowNull;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataColumn.set_AllowDBNull|API> %d#, %d{bool}\n", ObjectID, value);
                try {
                    if (allowNull != value) {
                        if (table != null) {
                            if (!value && table.EnforceConstraints)
                                CheckNotAllowNull();
                        }
                        this.allowNull = value;
                    }
                    // 
                }
                finally {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets a value indicating whether the column automatically increments the value of the column for new
        ///       rows added to the table.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        RefreshProperties(RefreshProperties.All),
        DefaultValue(false),
        ResDescriptionAttribute(Res.DataColumnAutoIncrementDescr)
        ]
        public bool AutoIncrement {
            get {
                return ((null != autoInc) && (autoInc.Auto));
            }
            set {
                Bid.Trace("<ds.DataColumn.set_AutoIncrement|API> %d#, %d{bool}\n", ObjectID, value);
                if (this.AutoIncrement != value) {
                    if (value) {
                        if (expression != null) {
                            throw ExceptionBuilder.AutoIncrementAndExpression();
                        }
                        //                        if (defaultValue != null && defaultValue != DBNull.Value) {
                        if (!DefaultValueIsNull) {
                            throw ExceptionBuilder.AutoIncrementAndDefaultValue();
                        }
                        if (!IsAutoIncrementType(DataType)) {
                            if (HasData) {
                                throw ExceptionBuilder.AutoIncrementCannotSetIfHasData(DataType.Name);
                            }
                            DataType = typeof(int);
                        }
                    }

                    this.AutoInc.Auto = value;
                }
            }
        }

        internal object AutoIncrementCurrent {
            get { return ((null != this.autoInc) ? this.autoInc.Current : this.AutoIncrementSeed); }
            set {
                if ((System.Numerics.BigInteger)this.AutoIncrementSeed != BigIntegerStorage.ConvertToBigInteger(value, this.FormatProvider)) {
                    this.AutoInc.SetCurrent(value, this.FormatProvider);
                }
            }
        }

        internal AutoIncrementValue AutoInc {
            get {
                return (this.autoInc ?? (this.autoInc = ((this.DataType == typeof(System.Numerics.BigInteger)) 
                                                         ? (AutoIncrementValue)new AutoIncrementBigInteger()
                                                         : new AutoIncrementInt64())));
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       or sets the starting value for a column that has its
        ///    <see cref='System.Data.DataColumn.AutoIncrement'/> property
        ///       set to <see langword='true'/>
        ///       .
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue((Int64)0),
        ResDescriptionAttribute(Res.DataColumnAutoIncrementSeedDescr)
        ]
        public Int64 AutoIncrementSeed {
            get {
                return ((null != this.autoInc) ? this.autoInc.Seed : 0L);
            }
            set {
                Bid.Trace("<ds.DataColumn.set_AutoIncrementSeed|API> %d#, %I64d\n", ObjectID, value);
                if (this.AutoIncrementSeed != value) {
                    this.AutoInc.Seed = value;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the increment used by a column with its <see cref='System.Data.DataColumn.AutoIncrement'/>
        ///       property set to <see langword='true'/>
        ///       .
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue((Int64)1),
        ResDescriptionAttribute(Res.DataColumnAutoIncrementStepDescr)
        ]
        public Int64 AutoIncrementStep {
            get {
                return ((null != this.autoInc) ? this.autoInc.Step : 1L);
            }
            set {
                Bid.Trace("<ds.DataColumn.set_AutoIncrementStep|API> %d#, %I64d\n", ObjectID, value);
                if (this.AutoIncrementStep != value) {
                    this.AutoInc.Step = value;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the caption for this column.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataColumnCaptionDescr)
        ]
        public string Caption {
            get {
                return (caption != null) ? caption : _columnName;
            }
            set {
                if (value == null)
                    value = "";

                if (caption == null || String.Compare(caption, value, true, Locale) != 0) {
                    caption = value;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Resets the <see cref='System.Data.DataColumn.Caption'/> property to its previous value, or
        ///       to <see langword='null'/> .
        ///    </para>
        /// </devdoc>
        private void ResetCaption() {
            if (caption != null) {
                caption = null;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the <see cref='System.Data.DataColumn.Caption'/> has been explicitly set.
        ///    </para>
        /// </devdoc>
        private bool ShouldSerializeCaption() {
            return (caption != null);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the name of the column within the <see cref='System.Data.DataColumnCollection'/>.
        ///    </para>
        /// </devdoc>
        [
        RefreshProperties(RefreshProperties.All),
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(""),
        ResDescriptionAttribute(Res.DataColumnColumnNameDescr)
        ]
        public string ColumnName {
            get {
                return _columnName;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataColumn.set_ColumnName|API> %d#, '%ls'\n", ObjectID, value);
                try {
                    if (value == null) {
                        value = "";
                    }

                    if (String.Compare(_columnName, value, true, Locale) != 0) {
                        if (table != null) {
                            if (value.Length == 0)
                                throw ExceptionBuilder.ColumnNameRequired();

                            table.Columns.RegisterColumnName(value, this);
                            if (_columnName.Length != 0)
                                table.Columns.UnregisterName(_columnName);
                        }

                        RaisePropertyChanging("ColumnName");
                        _columnName = value;
                        encodedColumnName = null;
                        if (table != null) {
                            table.Columns.OnColumnPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                        }
                    }
                    else if (_columnName != value) {
                        RaisePropertyChanging("ColumnName");
                        _columnName = value;
                        encodedColumnName = null;
                        if (table != null) {
                            table.Columns.OnColumnPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                        }
                    }
                }
                finally {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }

        internal string EncodedColumnName {
            get {
                if (this.encodedColumnName == null) {
                    this.encodedColumnName = XmlConvert.EncodeLocalName(this.ColumnName);
                }
                Debug.Assert(this.encodedColumnName != null && this.encodedColumnName.Length != 0);
                return this.encodedColumnName;
            }
        }

        internal IFormatProvider FormatProvider {
            get {
                // used for formating/parsing not comparing
                return ((null != table) ? table.FormatProvider : CultureInfo.CurrentCulture);
            }
        }

        internal CultureInfo Locale {
            get {
                // used for comparing not formating/parsing
                return ((null != table) ? table.Locale : CultureInfo.CurrentCulture);
            }
        }

        internal int ObjectID {
            get {
                return _objectID;
            }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(""),
        ResDescriptionAttribute(Res.DataColumnPrefixDescr)
        ]
        public string Prefix {
            get { return _columnPrefix; }
            set {
                if (value == null)
                    value = "";
                Bid.Trace("<ds.DataColumn.set_Prefix|API> %d#, '%ls'\n", ObjectID, value);

                if ((XmlConvert.DecodeName(value) == value) && (XmlConvert.EncodeName(value) != value))
                    throw ExceptionBuilder.InvalidPrefix(value);

                _columnPrefix = value;

            }
        }

        // Return the field value as a string. If the field value is NULL, then NULL is return.
        // If the column type is string and it's value is empty, then the empty string is returned.
        // If the column type is not string, or the column type is string and the value is not empty string, then a non-empty string is returned
        // This method does not throw any formatting exceptions, since we can always format the field value to a string.
        internal string GetColumnValueAsString(DataRow row, DataRowVersion version) {

            object objValue = this[row.GetRecordFromVersion(version)];

            if (DataStorage.IsObjectNull(objValue)) {
                return null;
            }

            string value = ConvertObjectToXml(objValue);
            Debug.Assert(value != null);

            return value;
        }

        /// <devdoc>
        /// Whether this column computes values.
        /// </devdoc>
        internal bool Computed {
            get {
                return this.expression != null;
            }
        }

        /// <devdoc>
        /// The internal expression object that computes the values.
        /// </devdoc>
        internal DataExpression DataExpression {
            get {
                return this.expression;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       The type
        ///       of data stored in thecolumn.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(typeof(string)),
        RefreshProperties(RefreshProperties.All),
        TypeConverter(typeof(ColumnTypeConverter)),
        ResDescriptionAttribute(Res.DataColumnDataTypeDescr)
        ]
        public Type DataType {
            get {
                return dataType;
            }
            set {
                if (dataType != value) {
                    if (HasData) {
                        throw ExceptionBuilder.CantChangeDataType();
                    }
                    if (value == null) {
                        throw ExceptionBuilder.NullDataType();
                    }
                    StorageType typeCode = DataStorage.GetStorageType(value);
                    if (DataStorage.ImplementsINullableValue(typeCode, value)) {
                        throw ExceptionBuilder.ColumnTypeNotSupported();
                    }
                    if (table != null && IsInRelation()) {
                        throw ExceptionBuilder.ColumnsTypeMismatch();
                    }
                    if (typeCode == StorageType.BigInteger && this.expression != null)
                    {
                        throw ExprException.UnsupportedDataType(value);
                    }

                    // If the DefualtValue is different from the Column DataType, we will coerce the value to the DataType
                    if (!DefaultValueIsNull) {
                        try {
                            if (this.defaultValue is System.Numerics.BigInteger) {
                                this.defaultValue = BigIntegerStorage.ConvertFromBigInteger((System.Numerics.BigInteger)this.defaultValue, value, this.FormatProvider);
                            }
                            else if (typeof(System.Numerics.BigInteger) == value) {
                                this.defaultValue = BigIntegerStorage.ConvertToBigInteger(this.defaultValue, this.FormatProvider);
                            }
                            else if (typeof(string) == value) { // since string types can be null in value! DO NOT REMOVE THIS 
                                defaultValue = DefaultValue.ToString();
                            }
                            else if (typeof(SqlString) == value) { // since string types can be null in value! DO NOT REMOVE THIS 
                                defaultValue = SqlConvert.ConvertToSqlString(DefaultValue);
                            }
                            else if (typeof(object) != value) {
                                DefaultValue = SqlConvert.ChangeTypeForDefaultValue(DefaultValue, value, FormatProvider);
                            }
                        }
                        catch (InvalidCastException ex) {
                            throw ExceptionBuilder.DefaultValueDataType(ColumnName, DefaultValue.GetType(), value, ex);
                        }
                        catch (FormatException ex) {
                            throw ExceptionBuilder.DefaultValueDataType(ColumnName, DefaultValue.GetType(), value, ex);
                        }
                    }

                    if (this.ColumnMapping == MappingType.SimpleContent)
                        if (value == typeof(Char))
                            throw ExceptionBuilder.CannotSetSimpleContentType(ColumnName, value);

                    SimpleType = SimpleType.CreateSimpleType(typeCode, value);
                    if (StorageType.String == typeCode) {
                        maxLength = -1;
                    }
                    UpdateColumnType(value, typeCode);
                    XmlDataType = null;

                    if (AutoIncrement) {
                        if (!IsAutoIncrementType(value)) {
                            AutoIncrement = false;
                        }

                        if (null != this.autoInc) {
                            // if you already have data you can't change the data type
                            // if you don't have data - you wouldn't have incremented AutoIncrementCurrent.
                            AutoIncrementValue inc = this.autoInc;
                            this.autoInc = null;
                            this.AutoInc.Auto = inc.Auto; // recreate with correct datatype
                            this.AutoInc.Seed = inc.Seed;
                            this.AutoInc.Step = inc.Step;
                            if (this.autoInc.DataType == inc.DataType) {
                                this.autoInc.Current = inc.Current;
                            }
                            else if (inc.DataType == typeof(Int64)) {
                                this.AutoInc.Current = (System.Numerics.BigInteger)(long)inc.Current;
                            }
                            else {
                                this.AutoInc.Current = checked((long)(System.Numerics.BigInteger)inc.Current);
                            }
                        }
                    }
                }
            }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(DataSetDateTime.UnspecifiedLocal),
        RefreshProperties(RefreshProperties.All),
        ResDescriptionAttribute(Res.DataColumnDateTimeModeDescr)
        ]
        public DataSetDateTime DateTimeMode {
            get {
                return _dateTimeMode;
            }
            set {
                if (_dateTimeMode != value) {
                    if (DataType != typeof(DateTime) && value != DataSetDateTime.UnspecifiedLocal) { //Check for column being DateTime. If the column is not DateTime make sure the value that is being is only the default[UnspecifiedLocal].
                        throw ExceptionBuilder.CannotSetDateTimeModeForNonDateTimeColumns();
                    }
                    switch (value) {
                    case DataSetDateTime.Utc:
                    case DataSetDateTime.Local:
                        if (HasData) {
                            throw ExceptionBuilder.CantChangeDateTimeMode(_dateTimeMode, value);
                        }
                        break;
                    case DataSetDateTime.Unspecified:
                    case DataSetDateTime.UnspecifiedLocal:
                        if (_dateTimeMode == DataSetDateTime.Unspecified || _dateTimeMode == DataSetDateTime.UnspecifiedLocal) {
                            break;
                        }
                        if (HasData) {
                            throw ExceptionBuilder.CantChangeDateTimeMode(_dateTimeMode, value);
                        }
                        break;
                    default:
                        throw ExceptionBuilder.InvalidDateTimeMode(value);
                    }
                    _dateTimeMode = value;
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the default value for the
        ///       column when creating new rows.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataColumnDefaultValueDescr),
        TypeConverter(typeof(DefaultValueTypeConverter))
        ]
        public object DefaultValue {
            get {
                Debug.Assert(defaultValue != null, "It should not have been set to null.");
                if (defaultValue == DBNull.Value && this.implementsINullable) { // for perf I dont access property
                    if (_storage != null)
                        defaultValue = _storage.NullValue;
                    else if (this.isSqlType)
                            defaultValue = SqlConvert.ChangeTypeForDefaultValue(defaultValue, this.dataType, FormatProvider);
                    else if (this.implementsINullable) {
                        System.Reflection.PropertyInfo propInfo = this.dataType.GetProperty("Null", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (propInfo != null)
                            defaultValue = propInfo.GetValue(null, null);
                    }
                }

                return defaultValue;
            }
            set {
                Bid.Trace("<ds.DataColumn.set_DefaultValue|API> %d#\n", ObjectID);
                if (defaultValue == null || !DefaultValue.Equals(value)) {
                    if (AutoIncrement) {
                        throw ExceptionBuilder.DefaultValueAndAutoIncrement();
                    }

                    object newDefaultValue = (value == null) ? DBNull.Value : value;
                    if (newDefaultValue != DBNull.Value && DataType != typeof(Object)) {
                        // If the DefualtValue is different from the Column DataType, we will coerce the value to the DataType
                        try {
                            newDefaultValue = SqlConvert.ChangeTypeForDefaultValue(newDefaultValue, DataType, FormatProvider);
                        }
                        catch (InvalidCastException ex) {
                            throw ExceptionBuilder.DefaultValueColumnDataType(ColumnName, newDefaultValue.GetType(), DataType, ex);
                        }
                    }
                    defaultValue = newDefaultValue;
                    // SQL BU Defect Tracking 401640:  should not assign any value until conversion is successful.
                    defaultValueIsNull = ((newDefaultValue == DBNull.Value) || (this.ImplementsINullable && DataStorage.IsObjectSqlNull(newDefaultValue))) ? true : false;
                }
            }
        }

        internal bool DefaultValueIsNull {
            get {
                return defaultValueIsNull;
            }
        }

        internal void BindExpression() {
            this.DataExpression.Bind(this.table);
        }

        /// <devdoc>
        ///    <para>Gets
        ///       or sets the expresssion used to either filter rows, calculate the column's
        ///       value, or create an aggregate column.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        RefreshProperties(RefreshProperties.All),
        DefaultValue(""),
        ResDescriptionAttribute(Res.DataColumnExpressionDescr)
        ]
        public string Expression {
            get {
                return (this.expression == null ? "" : this.expression.Expression);
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataColumn.set_Expression|API> %d#, '%ls'\n", ObjectID, value);

                if (value == null) {
                    value = "";
                }

                try {
                    DataExpression newExpression = null;
                    if (value.Length > 0) {
                        DataExpression testExpression = new DataExpression(this.table, value, this.dataType);
                        if (testExpression.HasValue) {
                            newExpression = testExpression;
                        }
                    }

                    if (expression == null && newExpression != null) {
                        if (AutoIncrement || Unique) {
                            throw ExceptionBuilder.ExpressionAndUnique();
                        }

                        // We need to make sure the column is not involved in any Constriants
                        if (table != null) {
                            for (int i = 0; i < table.Constraints.Count; i++) {
                                if (table.Constraints[i].ContainsColumn(this)) {
                                    throw ExceptionBuilder.ExpressionAndConstraint(this, table.Constraints[i]);
                                }
                            }
                        }

                        bool oldReadOnly = ReadOnly;
                        try {
                            ReadOnly = true;
                        }
                        catch (ReadOnlyException e) {
                            ExceptionBuilder.TraceExceptionForCapture(e);
                            ReadOnly = oldReadOnly;
                            throw ExceptionBuilder.ExpressionAndReadOnly();
                        }
                    }

                    // re-calculate the evaluation queue
                    if (this.table != null) {
                        if (newExpression != null && newExpression.DependsOn(this)) {
                            throw ExceptionBuilder.ExpressionCircular();
                        }
                        HandleDependentColumnList(expression, newExpression);
                        //hold onto oldExpression in case of error applying new Expression.
                        DataExpression oldExpression = this.expression;
                        this.expression = newExpression;

                        // because the column is attached to a table we need to re-calc values
                        try {
                            if (newExpression == null) {
                                for (int i = 0; i < table.RecordCapacity; i++) {
                                    InitializeRecord(i);
                                }
                            }
                            else {
                                this.table.EvaluateExpressions(this);
                            }
                            // SQLBU 501916: DataTable internal index is corrupted:'5'
                            this.table.ResetInternalIndexes(this);
                            this.table.EvaluateDependentExpressions(this);
                        }
                        catch (Exception e1) {
                            // 
                            if (!ADP.IsCatchableExceptionType(e1)) {
                                throw;
                            }
                            ExceptionBuilder.TraceExceptionForCapture(e1);
                            try {
                                // in the case of error we need to set the column expression to the old value
                                this.expression = oldExpression;
                                HandleDependentColumnList(newExpression, expression);
                                if (oldExpression == null) {
                                    for (int i = 0; i < table.RecordCapacity; i++) {
                                        InitializeRecord(i);
                                    }
                                }
                                else {
                                    this.table.EvaluateExpressions(this);
                                }
                                this.table.ResetInternalIndexes(this);
                                this.table.EvaluateDependentExpressions(this);
                            }
                            catch (Exception e2) {
                                // 
                                if (!ADP.IsCatchableExceptionType(e2)) {
                                    throw;
                                }
                                ExceptionBuilder.TraceExceptionWithoutRethrow(e2);
                            }
                            throw;
                        }
                    }
                    else {
                        //if column is not attached to a table, just set.
                        this.expression = newExpression;
                    }
                }
                finally {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets the collection of custom user information.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        Browsable(false),
        ResDescriptionAttribute(Res.ExtendedPropertiesDescr)
        ]
        public PropertyCollection ExtendedProperties {
            get {
                if (extendedProperties == null) {
                    extendedProperties = new PropertyCollection();
                }
                return extendedProperties;
            }
        }

        /// <devdoc>
        /// Indicates whether this column is now storing data.
        /// </devdoc>
        internal bool HasData {
            get {
                return (_storage != null);
            }
        }

        internal bool ImplementsINullable {
            get {
                return implementsINullable;
            }
        }

        internal bool ImplementsIChangeTracking {
            get {
                return implementsIChangeTracking;
            }
        }

        internal bool ImplementsIRevertibleChangeTracking {
            get {
                return implementsIRevertibleChangeTracking;
            }
        }

        internal bool IsCloneable {
            get {
                Debug.Assert(null != _storage, "no storage");
                return _storage.IsCloneable;
            }
        }

        internal bool IsStringType {
            get {
                Debug.Assert(null != _storage, "no storage");
                return _storage.IsStringType;
            }
        }

        internal bool IsValueType {
            get {
                Debug.Assert(null != _storage, "no storage");
                return _storage.IsValueType;
            }
        }

        internal bool IsSqlType {
            get {
                return isSqlType;
            }
        }

        private void SetMaxLengthSimpleType() {
            if (this.simpleType != null) {
                Debug.Assert(this.simpleType.CanHaveMaxLength(), "expected simpleType to be string");

                this.simpleType.MaxLength = maxLength;
                // check if we reset the simpleType back to plain string
                if (this.simpleType.IsPlainString()) {
                    this.simpleType = null;
                }
                else {
                    // Named Simple Type's Name should not be null
                    if (this.simpleType.Name != null && this.dttype != null) {
                        // if MaxLength is changed, we need to make  namedsimpletype annonymous simpletype
                        this.simpleType.ConvertToAnnonymousSimpleType();
                        this.dttype = null;
                    }
                }
            }
            else if (-1 < maxLength) {
                this.SimpleType = SimpleType.CreateLimitedStringType(maxLength);
            }
        }
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataColumnMaxLengthDescr),
        DefaultValue(-1)
        ]
        public int MaxLength {
            get {
                return maxLength;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataColumn.set_MaxLength|API> %d#, %d\n", ObjectID, value);

                try {
                    if (maxLength != value) {
                        if (this.ColumnMapping == MappingType.SimpleContent) {
                            throw ExceptionBuilder.CannotSetMaxLength2(this);
                        }
                        if ((DataType != typeof(string)) && (DataType != typeof(SqlString))) {
                            throw ExceptionBuilder.HasToBeStringType(this);
                        }
                        int oldValue = maxLength;
                        maxLength = Math.Max(value, -1);

                        if (((oldValue < 0) || (value < oldValue)) && (null != table) && table.EnforceConstraints) {
                            if (!CheckMaxLength()) {
                                maxLength = oldValue;
                                throw ExceptionBuilder.CannotSetMaxLength(this, value);
                            }
                        }
                        SetMaxLengthSimpleType();
                    }
                }
                finally {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataColumnNamespaceDescr)
        ]
        public string Namespace {
            get {
                if (_columnUri == null) {
                    if (Table != null && columnMapping != MappingType.Attribute) {
                        return Table.Namespace;
                    }
                    return "";
                }
                return _columnUri;
            }
            set {
                Bid.Trace("<ds.DataColumn.set_Namespace|API> %d#, '%ls'\n", ObjectID, value);

                if (_columnUri != value) {
                    if (columnMapping != MappingType.SimpleContent) {
                        RaisePropertyChanging("Namespace");
                        _columnUri = value;
                    }
                    else if (value != this.Namespace) {
                        throw ExceptionBuilder.CannotChangeNamespace(this.ColumnName);
                    }
                }
            }
        }

        private bool ShouldSerializeNamespace() {
            return (_columnUri != null);
        }

        private void ResetNamespace() {
            this.Namespace = null;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the position of the column in the <see cref='System.Data.DataColumnCollection'/>
        ///       collection.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.DataColumnOrdinalDescr)
        ]
        public int Ordinal {
            get {
                return _ordinal;
            }
        }

        public void SetOrdinal(int ordinal) {
            if (_ordinal == -1) {
                throw ExceptionBuilder.ColumnNotInAnyTable();
            }

            if (this._ordinal != ordinal) {
                table.Columns.MoveTo(this, ordinal);
            }
        }

        internal void SetOrdinalInternal(int ordinal) {
            // 
            if (this._ordinal != ordinal) {
                if (Unique && this._ordinal != -1 && ordinal == -1) {
                    UniqueConstraint key = table.Constraints.FindKeyConstraint(this);
                    if (key != null)
                        table.Constraints.Remove(key);
                }
                // SQLBU 429176: remove the sortIndex when DataColumn is removed
                if ((null != sortIndex) && (-1 == ordinal)) {
                    Debug.Assert(2 <= sortIndex.RefCount, "bad sortIndex refcount");
                    sortIndex.RemoveRef();
                    sortIndex.RemoveRef(); // second should remove it from index collection
                    sortIndex = null;
                }
                int originalOrdinal = this._ordinal;
                this._ordinal = ordinal;
                if (originalOrdinal == -1 && this._ordinal != -1) {
                    if (Unique) {
                        UniqueConstraint key = new UniqueConstraint(this);
                        table.Constraints.Add(key);
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value
        ///       indicating whether the column allows changes once a row has been added to the table.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(false),
        ResDescriptionAttribute(Res.DataColumnReadOnlyDescr)
        ]
        public bool ReadOnly {
            get {
                return readOnly;
            }
            set {
                Bid.Trace("<ds.DataColumn.set_ReadOnly|API> %d#, %d{bool}\n", ObjectID, value);
                if (readOnly != value) {
                    if (!value && expression != null) {
                        throw ExceptionBuilder.ReadOnlyAndExpression();
                    }
                    this.readOnly = value;
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // don't have debugger view expand this
        private Index SortIndex {
            get {
                if (sortIndex == null) {
                    IndexField[] indexDesc = new IndexField[] { new IndexField(this, false) };
                    sortIndex = table.GetIndex(indexDesc, DataViewRowState.CurrentRows, (IFilter)null);
                    sortIndex.AddRef();
                }
                return sortIndex;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the <see cref='System.Data.DataTable'/> to which the column belongs to.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.DataColumnDataTableDescr)
        ]
        public DataTable Table {
            get {
                return table;
            }
        }

        /// <devdoc>
        /// Internal mechanism for changing the table pointer.
        /// </devdoc>
        internal void SetTable(DataTable table) {
            if (this.table != table) {
                if (this.Computed)
                    if ((table == null) ||
                        (!table.fInitInProgress && ((table.DataSet == null) || (!table.DataSet.fIsSchemaLoading && !table.DataSet.fInitInProgress)))) {
                        // We need to re-bind all expression columns.
                        this.DataExpression.Bind(table);
                    }

                if (Unique && this.table != null) {
                    UniqueConstraint constraint = table.Constraints.FindKeyConstraint(this);
                    if (constraint != null)
                        table.Constraints.CanRemove(constraint, true);
                }
                this.table = table;
                _storage = null; // empty out storage for reuse.
            }
        }

        private DataRow GetDataRow(int index) {
            return table.recordManager[index];
        }

        /// <devdoc>
        /// This is how data is pushed in and out of the column.
        /// </devdoc>
        internal object this[int record] {
            get {
                table.recordManager.VerifyRecord(record);
                Debug.Assert(null != _storage, "null storage");
                return _storage.Get(record);
            }
            set {
                try {
                    table.recordManager.VerifyRecord(record);
                    Debug.Assert(null != _storage, "no storage");
                    Debug.Assert(null != value, "setting null, expecting dbnull");
                    _storage.Set(record, value);
                    Debug.Assert(null != this.table, "storage with no DataTable on column");
                }
                catch (Exception e) {
                    ExceptionBuilder.TraceExceptionForCapture(e);
                    throw ExceptionBuilder.SetFailed(value, this, DataType, e);
                }

                if (AutoIncrement) {
                    if (!_storage.IsNull(record)) {
                        this.AutoInc.SetCurrentAndIncrement(_storage.Get(record));
                    }
                }
                if (Computed) {// if and only if it is Expression column, we will cache LastChangedColumn, otherwise DO NOT
                    DataRow dr = GetDataRow(record);
                    if (dr != null) {
                        // at initialization time (datatable.NewRow(), we would fill the storage with default value, but at that time we wont have datarow)
                        dr.LastChangedColumn = this;
                    }
                }
            }
        }

        internal void InitializeRecord(int record) {
            Debug.Assert(null != _storage, "no storage");
            _storage.Set(record, DefaultValue);
        }

        internal void SetValue(int record, object value) { // just silently set the value
            try {
                Debug.Assert(null != value, "setting null, expecting dbnull");
                Debug.Assert(null != this.table, "storage with no DataTable on column");
                Debug.Assert(null != _storage, "no storage");
                _storage.Set(record, value);
            }
            catch (Exception e) {
                ExceptionBuilder.TraceExceptionForCapture(e);
                throw ExceptionBuilder.SetFailed(value, this, DataType, e);
            }

            DataRow dr = GetDataRow(record);
            if (dr != null) {  // at initialization time (datatable.NewRow(), we would fill the storage with default value, but at that time we wont have datarow)
                dr.LastChangedColumn = this;
            }
        }

        internal void FreeRecord(int record) {
            Debug.Assert(null != _storage, "no storage");
            _storage.Set(record, _storage.NullValue);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the values in each row of the column must be unique.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(false),
        ResDescriptionAttribute(Res.DataColumnUniqueDescr),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool Unique {
            get {
                return unique;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataColumn.set_Unique|API> %d#, %d{bool}\n", ObjectID, value);
                try {
                    if (unique != value) {
                        if (value && expression != null) {
                            throw ExceptionBuilder.UniqueAndExpression();
                        }
                        UniqueConstraint oldConstraint = null;
                        if (table != null) {
                            if (value)
                                CheckUnique();
                            else {
                                for (System.Collections.IEnumerator e = Table.Constraints.GetEnumerator(); e.MoveNext(); ) {
                                    UniqueConstraint o = (e.Current as UniqueConstraint);
                                    if ((null != o) && (o.ColumnsReference.Length == 1) && (o.ColumnsReference[0] == this))
                                        oldConstraint = o;
                                }
                                Debug.Assert(oldConstraint != null, "Should have found a column to remove from the collection.");
                                table.Constraints.CanRemove(oldConstraint, true);
                            }
                        }

                        this.unique = value;

                        if (table != null) {
                            if (value) {
                                // This should not fail due to a duplicate constraint. unique would have
                                // already been true if there was an existed UniqueConstraint for this column

                                UniqueConstraint constraint = new UniqueConstraint(this);
                                Debug.Assert(table.Constraints.FindKeyConstraint(this) == null, "Should not be a duplication constraint in collection");
                                table.Constraints.Add(constraint);
                            }
                            else
                            {
                                table.Constraints.Remove(oldConstraint);
                                // 
                            }
                        }
                    }
                }
                finally {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }


        // FxCop Rule; getter not used!  WebData 101301; so changing from Property to method
        internal void InternalUnique(bool value) {
            this.unique = value;
        }

        internal string XmlDataType {
            get {
                return dttype;
            }
            set {
                dttype = value;
            }
        }

        internal SimpleType SimpleType {
            get {
                return simpleType;
            }
            set {
                simpleType = value;
                // there is a change, since we are supporting hierarchy(bacause of Names Simple Type) old check (just one leel base check) is wrong
                if (value != null && value.CanHaveMaxLength())
                    maxLength = simpleType.MaxLength;// this is temp solution, since we dont let simple content to have
                //maxlength set but for simple type we want to set it, after coming to decision about it , we should
                // use MaxLength property
            }
        }

        /// <devdoc>
        /// <para>Gets the <see cref='System.Data.MappingType'/> of the column.</para>
        /// </devdoc>
        [
        DefaultValue(MappingType.Element),
        ResDescriptionAttribute(Res.DataColumnMappingDescr)
        ]
        public virtual MappingType ColumnMapping {
            get {
                return columnMapping;
            }
            set {
                Bid.Trace("<ds.DataColumn.set_ColumnMapping|API> %d#, %d{ds.MappingType}\n", ObjectID, (int)value);
                if (value != columnMapping) {

                    if (value == MappingType.SimpleContent && table != null) {
                        int threshold = 0;
                        if (columnMapping == MappingType.Element)
                            threshold = 1;
                        if (this.dataType == typeof(Char))
                            throw ExceptionBuilder.CannotSetSimpleContent(ColumnName, this.dataType);

                        if (table.XmlText != null && table.XmlText != this)
                            throw ExceptionBuilder.CannotAddColumn3();
                        if (table.ElementColumnCount > threshold)
                            throw ExceptionBuilder.CannotAddColumn4(this.ColumnName);
                    }

                    RaisePropertyChanging("ColumnMapping");

                    if (table != null) {
                        if (columnMapping == MappingType.SimpleContent)
                            table.xmlText = null;

                        if (value == MappingType.Element)
                            table.ElementColumnCount++;
                        else if (columnMapping == MappingType.Element)
                            table.ElementColumnCount--;
                    }

                    columnMapping = value;
                    if (value == MappingType.SimpleContent) {
                        _columnUri = null;
                        if (table != null) {
                            table.XmlText = this;
                        }
                        this.SimpleType = null;
                    }
                }
            }
        }

        internal event PropertyChangedEventHandler PropertyChanging {
            add {
                onPropertyChangingDelegate += value;
            }
            remove {
                onPropertyChangingDelegate -= value;
            }
        }

        internal void CheckColumnConstraint(DataRow row, DataRowAction action) {
            if (table.UpdatingCurrent(row, action)) {
                CheckNullable(row);
                CheckMaxLength(row);
            }
        }

        internal bool CheckMaxLength() {
            if ((0 <= maxLength) && (null != Table) && (0 < Table.Rows.Count)) {
                Debug.Assert(IsStringType, "not a String or SqlString column");
                foreach (DataRow dr in Table.Rows) {
                    if (dr.HasVersion(DataRowVersion.Current)) {
                        if (maxLength < GetStringLength(dr.GetCurrentRecordNo())) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        internal void CheckMaxLength(DataRow dr) {
            if (0 <= maxLength) {
                Debug.Assert(IsStringType, "not a String or SqlString column");
                if (maxLength < GetStringLength(dr.GetDefaultRecord())) {
                    throw ExceptionBuilder.LongerThanMaxLength(this);
                }
            }
        }

        internal protected void CheckNotAllowNull() {
            if (_storage == null)
                return;

            if (sortIndex != null) {
                if (sortIndex.IsKeyInIndex(_storage.NullValue)) {// here we do use strong typed NULL for Sql types
                    throw ExceptionBuilder.NullKeyValues(ColumnName);
                }
            }
            else { // since we do not have index, we so sequential search
                foreach (DataRow dr in this.table.Rows) {
                    if (dr.RowState == DataRowState.Deleted)
                        continue;
                    if (!implementsINullable) {
                        if (dr[this] == DBNull.Value) {
                            throw ExceptionBuilder.NullKeyValues(ColumnName);
                        }
                    }
                    else {
                        if (DataStorage.IsObjectNull(dr[this])) {
                            throw ExceptionBuilder.NullKeyValues(ColumnName);
                        }
                    }
                }
            }
        }

        internal void CheckNullable(DataRow row) {
            if (!AllowDBNull) {
                Debug.Assert(null != _storage, "no storage");
                if (_storage.IsNull(row.GetDefaultRecord())) {
                    throw ExceptionBuilder.NullValues(ColumnName);
                }
            }
        }

        protected void CheckUnique() {
            if (!SortIndex.CheckUnique()) {
                // Throws an exception and the name of any column if its Unique property set to
                // True and non-unique values are found in the column.
                throw ExceptionBuilder.NonUniqueValues(ColumnName);
            }
        }

        internal int Compare(int record1, int record2) {
            Debug.Assert(null != _storage, "null storage");
            return _storage.Compare(record1, record2);
        }

        internal bool CompareValueTo(int record1, object value, bool checkType) {
            // this method is used to make sure value and exact type match.
            int valuesMatch = CompareValueTo(record1, value);
            // if values match according to storage, do extra checks for exact compare
            if (valuesMatch == 0) {
                Type leftType = value.GetType();
                Type rightType = _storage.Get(record1).GetType();
                // if strings, then do exact character by character check
                if (leftType == typeof(System.String) && rightType == typeof(System.String)) {
                    return String.CompareOrdinal((string)_storage.Get(record1), (string)value) == 0 ? true : false;
                }
                // make sure same type
                else if (leftType == rightType) {
                    return true;
                }
            }
            return false;
        }

        internal int CompareValueTo(int record1, object value) {
            Debug.Assert(null != _storage, "null storage");
            return _storage.CompareValueTo(record1, value);
        }

        internal object ConvertValue(object value) {
            Debug.Assert(null != _storage, "null storage");
            return _storage.ConvertValue(value);
        }

        internal void Copy(int srcRecordNo, int dstRecordNo) {
            Debug.Assert(null != _storage, "null storage");
            _storage.Copy(srcRecordNo, dstRecordNo);
        }

        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)] 
        internal DataColumn Clone() {
            DataColumn clone = (DataColumn)Activator.CreateInstance(this.GetType());
            // set All properties
            //            clone.columnMapping = columnMapping;

            clone.SimpleType = SimpleType;

            clone.allowNull = allowNull;
            if (null != this.autoInc) {
                clone.autoInc = this.autoInc.Clone();
            }
            clone.caption = caption;
            clone.ColumnName = ColumnName;
            clone._columnUri = _columnUri;
            clone._columnPrefix = _columnPrefix;
            clone.DataType = DataType;
            clone.defaultValue = defaultValue;
            clone.defaultValueIsNull = ((defaultValue == DBNull.Value) || (clone.ImplementsINullable && DataStorage.IsObjectSqlNull(defaultValue))) ? true : false;
            clone.columnMapping = columnMapping;// clone column Mapping since we dont let MaxLength to be set throu API
            // 
            clone.readOnly = readOnly;
            clone.MaxLength = MaxLength;
            clone.dttype = dttype;
            clone._dateTimeMode = _dateTimeMode;


            // so if we have set it, we should continue preserving the information

            // ...Extended Properties
            if (this.extendedProperties != null) {
                foreach (Object key in this.extendedProperties.Keys) {
                    clone.ExtendedProperties[key] = this.extendedProperties[key];
                }
            }

            return clone;
        }

        /// <devdoc>
        ///    <para>Finds a relation that this column is the sole child of or null.</para>
        /// </devdoc>
        internal DataRelation FindParentRelation() {
            DataRelation[] parentRelations = new DataRelation[Table.ParentRelations.Count];
            Table.ParentRelations.CopyTo(parentRelations, 0);

            for (int i = 0; i < parentRelations.Length; i++) {
                DataRelation relation = parentRelations[i];
                DataKey key = relation.ChildKey;
                if (key.ColumnsReference.Length == 1 && key.ColumnsReference[0] == this) {
                    return relation;
                }
            }
            // should we throw an exception?
            return null;
        }


        internal object GetAggregateValue(int[] records, AggregateType kind) {
            if (_storage == null) {
                if (kind == AggregateType.Count)
                    return 0;
                else
                    return DBNull.Value;
            }
            return _storage.Aggregate(records, kind);
        }

        private int GetStringLength(int record) {
            Debug.Assert(null != _storage, "no storage");
            return _storage.GetStringLength(record);
        }

        internal void Init(int record) {
            if (AutoIncrement) {
                object value = this.autoInc.Current;
                this.autoInc.MoveAfter();
                Debug.Assert(null != _storage, "no storage");
                _storage.Set(record, value);
            }
            else
                this[record] = defaultValue;
        }

        internal static bool IsAutoIncrementType(Type dataType) {
            return ((dataType == typeof(Int32)) || (dataType == typeof(Int64)) || (dataType == typeof(Int16)) || (dataType == typeof(Decimal)) || (dataType == typeof(System.Numerics.BigInteger)) ||
                   (dataType == typeof(SqlInt32)) || (dataType == typeof(SqlInt64)) || (dataType == typeof(SqlInt16)) || (dataType == typeof(SqlDecimal)));
        }

        private bool IsColumnMappingValid(StorageType typeCode, MappingType mapping) {
            if ((mapping != MappingType.Element) && DataStorage.IsTypeCustomType(typeCode)) {
                return false;
            }
            return true;
        }

        internal bool IsCustomType {
            get {
                if (null != _storage)
                    return _storage.IsCustomDefinedType;
                return DataStorage.IsTypeCustomType(DataType);
            }
        }

        internal bool IsValueCustomTypeInstance(object value) {
            // if instance is not a storage supported type (built in or SQL types)
            return (DataStorage.IsTypeCustomType(value.GetType()) && !(value is Type));
        }

        internal bool ImplementsIXMLSerializable {
            get {
                return implementsIXMLSerializable;
            }
        }

        internal bool IsNull(int record) {
            Debug.Assert(null != _storage, "no storage");
            return _storage.IsNull(record);
        }

        /// <devdoc>
        ///      Returns true if this column is a part of a Parent or Child key for a relation.
        /// </devdoc>
        internal bool IsInRelation() {
            DataKey key;
            DataRelationCollection rels = table.ParentRelations;

            Debug.Assert(rels != null, "Invalid ParentRelations");
            for (int i = 0; i < rels.Count; i++) {
                key = rels[i].ChildKey;
                Debug.Assert(key.HasValue, "Invalid child key (null)");
                if (key.ContainsColumn(this)) {
                    return true;
                }
            }
            rels = table.ChildRelations;
            Debug.Assert(rels != null, "Invalid ChildRelations");
            for (int i = 0; i < rels.Count; i++) {
                key = rels[i].ParentKey;
                Debug.Assert(key.HasValue, "Invalid parent key (null)");
                if (key.ContainsColumn(this)) {
                    return true;
                }
            }
            return false;
        }

        internal bool IsMaxLengthViolated() {
            if (MaxLength < 0)
                return true;

            bool error = false;
            object value;
            string errorText = null;

            foreach (DataRow dr in Table.Rows) {
                if (dr.HasVersion(DataRowVersion.Current)) {
                    value = dr[this];
                    if (!this.isSqlType) {
                        if (value != null && value != DBNull.Value && ((string)value).Length > MaxLength) {
                            if (errorText == null) {
                                errorText = ExceptionBuilder.MaxLengthViolationText(this.ColumnName);
                            }
                            dr.RowError = errorText;
                            dr.SetColumnError(this, errorText);
                            error = true;
                        }
                    }
                    else {
                        if (!DataStorage.IsObjectNull(value) && ((SqlString)value).Value.Length > MaxLength) {
                            if (errorText == null) {
                                errorText = ExceptionBuilder.MaxLengthViolationText(this.ColumnName);
                            }
                            dr.RowError = errorText;
                            dr.SetColumnError(this, errorText);
                            error = true;
                        }
                    }
                }
            }
            return error;
        }

        internal bool IsNotAllowDBNullViolated() {//
            Index index = this.SortIndex;
            DataRow[] rows = index.GetRows(index.FindRecords(DBNull.Value));
            for (int i = 0; i < rows.Length; i++) {
                string errorText = ExceptionBuilder.NotAllowDBNullViolationText(this.ColumnName);
                rows[i].RowError = errorText;
                rows[i].SetColumnError(this, errorText);
            }
            return (rows.Length > 0);
        }

        internal void FinishInitInProgress() {
            if (this.Computed)
                BindExpression();
        }

        protected virtual void OnPropertyChanging(PropertyChangedEventArgs pcevent) {
            if (onPropertyChangingDelegate != null)
                onPropertyChangingDelegate(this, pcevent);
        }

        protected internal void RaisePropertyChanging(string name) {
            OnPropertyChanging(new PropertyChangedEventArgs(name));
        }

        private void InsureStorage() {
            if (_storage == null) {
                _storage = DataStorage.CreateStorage(this, dataType, _storageType);
            }
        }

        internal void SetCapacity(int capacity) {
            InsureStorage();
            _storage.SetCapacity(capacity);
        }

        private bool ShouldSerializeDefaultValue() {
            return (!DefaultValueIsNull);
        }

        internal void OnSetDataSet() {
        }

        // Returns the <see cref='System.Data.DataColumn.Expression'/> of the column, if one exists.
        public override string ToString() {
            if (this.expression == null)
                return this.ColumnName;
            else
                return this.ColumnName + " + " + this.Expression;

        }


        internal object ConvertXmlToObject(string s) {
            Debug.Assert(s != null, "Caller is resposible for missing element/attribure case");
            InsureStorage();
            return _storage.ConvertXmlToObject(s);
        }

        internal object ConvertXmlToObject(XmlReader xmlReader, XmlRootAttribute xmlAttrib) {
            InsureStorage();
            return _storage.ConvertXmlToObject(xmlReader, xmlAttrib);
        }


        internal string ConvertObjectToXml(object value) {
            Debug.Assert(value != null && (value != DBNull.Value), "Caller is resposible for checking on DBNull");
            InsureStorage();
            return _storage.ConvertObjectToXml(value);
        }

        internal void ConvertObjectToXml(object value, XmlWriter xmlWriter, XmlRootAttribute xmlAttrib) {
            Debug.Assert(value != null && (value != DBNull.Value), "Caller is resposible for checking on DBNull");
            InsureStorage();
            _storage.ConvertObjectToXml(value, xmlWriter, xmlAttrib);
        }

        internal object GetEmptyColumnStore(int recordCount) {
            InsureStorage();
            return _storage.GetEmptyStorageInternal(recordCount);
        }

        internal void CopyValueIntoStore(int record, object store, BitArray nullbits, int storeIndex) {
            Debug.Assert(null != _storage, "no storage");
            _storage.CopyValueInternal(record, store, nullbits, storeIndex);
        }

        internal void SetStorage(object store, BitArray nullbits) {
            InsureStorage();
            _storage.SetStorageInternal(store, nullbits);
        }

        internal void AddDependentColumn(DataColumn expressionColumn) {
            if (dependentColumns == null) {
                dependentColumns = new List<DataColumn>();
            }
            Debug.Assert(!dependentColumns.Contains(expressionColumn), "duplicate column - expected to be unique");
            dependentColumns.Add(expressionColumn);
            this.table.AddDependentColumn(expressionColumn);
        }

        internal void RemoveDependentColumn(DataColumn expressionColumn) {
            if (dependentColumns != null && dependentColumns.Contains(expressionColumn)) {
                dependentColumns.Remove(expressionColumn);
            }
            this.table.RemoveDependentColumn(expressionColumn);
        }

        internal void HandleDependentColumnList(DataExpression oldExpression, DataExpression newExpression) {
            DataColumn[] dependency;
            // remove this column from the dependentColumn list of the columns this column depends on.
            if (oldExpression != null) {
                dependency = oldExpression.GetDependency();
                foreach (DataColumn col in dependency) {
                    Debug.Assert(null != col, "null datacolumn in expression dependencies");
                    col.RemoveDependentColumn(this);
                    if (col.table != this.table) {
                        this.table.RemoveDependentColumn(this);
                    }
                }
                this.table.RemoveDependentColumn(this);
            }

            if (newExpression != null) {
                // get the list of columns that this expression depends on
                dependency = newExpression.GetDependency();
                // add this column to dependent column list of each column this column depends on
                foreach (DataColumn col in dependency) {
                    col.AddDependentColumn(this);
                    if (col.table != this.table) {
                        this.table.AddDependentColumn(this);
                    }
                }
                this.table.AddDependentColumn(this);
            }
        }
    }

    internal abstract class AutoIncrementValue {
        private bool auto;

        internal bool Auto {
            get { return this.auto; }
            set { this.auto = value; }
        }
        internal abstract object Current { get; set; }
        internal abstract long Seed { get; set; }
        internal abstract long Step { get; set; }
        internal abstract Type DataType { get; }

        internal abstract void SetCurrent(object value, IFormatProvider formatProvider);
        internal abstract void SetCurrentAndIncrement(object value);
        internal abstract void MoveAfter();

        internal AutoIncrementValue Clone() {
            AutoIncrementValue clone = (this is AutoIncrementInt64) ? (AutoIncrementValue)new AutoIncrementInt64() : (AutoIncrementValue)new AutoIncrementBigInteger();
            clone.Auto = this.Auto;
            clone.Seed = this.Seed;
            clone.Step = this.Step;
            clone.Current = this.Current;
            return clone;
        }
    }

    /// <summary>the auto stepped value with Int64 representation</summary>
    /// <remarks>use unchecked behavior for Dev10 
    internal sealed class AutoIncrementInt64 : AutoIncrementValue {
        /// <summary>the last returned auto incremented value</summary>
        private System.Int64 current;

        /// <summary>the initial value use to set current</summary>
        private System.Int64 seed;

        /// <summary>the value by which to offset the next value</summary>
        private System.Int64 step = 1;

        /// <summary>Gets and sets the current auto incremented value to use</summary>
        internal override object Current {
            get { return this.current; }
            set { this.current = (Int64)value; }
        }

        internal override Type DataType { get { return typeof(System.Int64); } }

        /// <summary>Get and sets the initial seed value.</summary>
        internal override long Seed {
            get { return this.seed; }
            set {
                if ((this.current == this.seed) || this.BoundaryCheck(value)) {
                    this.current = value;
                }
                this.seed = value;
            }
        }

        /// <summary>Get and sets the stepping value.</summary>
        /// <exception cref="ArugmentException">if value is 0</exception>
        internal override long Step {
            get { return this.step; }
            set {
                if (0 == value) {
                    throw ExceptionBuilder.AutoIncrementSeed();
                }
                if (this.step != value) {
                    if (this.current != this.Seed) {
                        this.current = unchecked(this.current - this.step + value);
                    }
                    this.step = value;
                }
            }
        }

        internal override void MoveAfter() {
            this.current = unchecked(this.current + this.step);
        }

        internal override void SetCurrent(object value, IFormatProvider formatProvider) {
            this.current = Convert.ToInt64(value, formatProvider);
        }

        internal override void SetCurrentAndIncrement(object value) {
            Debug.Assert(null != value && DataColumn.IsAutoIncrementType(value.GetType()) && !(value is System.Numerics.BigInteger), "unexpected value for autoincrement");
            System.Int64 v = (Int64)SqlConvert.ChangeType2(value, StorageType.Int64, typeof(Int64), CultureInfo.InvariantCulture);
            if (this.BoundaryCheck(v)) {
                this.current = unchecked(v + this.step);
            }
        }

        private bool BoundaryCheck(System.Numerics.BigInteger value) {
            return (((this.step < 0) && (value <= this.current)) || ((0 < this.step) && (this.current <= value)));
        }
    }

    /// <summary>the auto stepped value with BigInteger representation</summary>
    internal sealed class AutoIncrementBigInteger : AutoIncrementValue {
        /// <summary>the current auto incremented value to use</summary>
        private System.Numerics.BigInteger current;

        /// <summary>the initial value use to set current</summary>
        private System.Int64 seed;

        /// <summary>the value by which to offset the next value</summary>
        private System.Numerics.BigInteger step = 1;

        /// <summary>Gets and sets the current auto incremented value to use</summary>
        internal override object Current {
            get { return this.current; }
            set { this.current = (System.Numerics.BigInteger)value; }
        }

        internal override Type DataType { get { return typeof(System.Numerics.BigInteger); } }

        /// <summary>Get and sets the initial seed value.</summary>
        internal override long Seed {
            get { return this.seed; }
            set {
                if ((this.current == this.seed) || this.BoundaryCheck(value)) {
                    this.current = value;
                }
                this.seed = value;
            }
        }

        /// <summary>Get and sets the stepping value.</summary>
        /// <exception cref="ArugmentException">if value is 0</exception>
        internal override long Step {
            get { return (long)this.step; }
            set {
                if (0 == value) {
                    throw ExceptionBuilder.AutoIncrementSeed();
                }
                if (this.step != value) {
                    if (this.current != this.Seed) {
                        this.current = checked(this.current - this.step + value);
                    }
                    this.step = value;
                }
            }
        }

        internal override void MoveAfter() {
            this.current = checked(this.current + this.step);
        }

        internal override void SetCurrent(object value, IFormatProvider formatProvider) {
            this.current = BigIntegerStorage.ConvertToBigInteger(value, formatProvider);
        }

        internal override void SetCurrentAndIncrement(object value) {
            System.Numerics.BigInteger v = (System.Numerics.BigInteger)value;
            if (this.BoundaryCheck(v)) {
                this.current = v + this.step;
            }
        }

        private bool BoundaryCheck(System.Numerics.BigInteger value) {
            return (((this.step < 0) && (value <= this.current)) || ((0 < this.step) && (this.current <= value)));
        }
    }
}
