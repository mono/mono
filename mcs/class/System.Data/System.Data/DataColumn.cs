//
// System.Data.DataColumn.cs
//
// Author:
//   Franklin Wise (gracenote@earthlink.net)
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Copyright 2002, Franklin Wise
// (C) Chris Podurgiel
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Daniel Morgan, 2002, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.ComponentModel;
using System.Reflection;
using System.Collections;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Globalization;
using Mono.Data.SqlExpressions;

namespace System.Data {
	internal delegate void DelegateColumnValueChange (DataColumn column, DataRow row, object proposedValue);

	/// <summary>
	/// Summary description for DataColumn.
	/// </summary>

	[Editor ("Microsoft.VSDesigner.Data.Design.DataColumnEditor, " + Consts.AssemblyMicrosoft_VSDesigner,
		 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	[ToolboxItem (false)]
	[DefaultProperty ("ColumnName")]
	[DesignTimeVisible (false)]
	public class DataColumn : MarshalByValueComponent {
#region Events
		EventHandlerList _eventHandlers = new EventHandlerList ();

		//used for constraint validation
		//if an exception is fired during this event the change should be canceled
		//[MonoTODO]
		//internal event DelegateColumnValueChange ValidateColumnValueChange;

		//used for FK Constraint Cascading rules
		//[MonoTODO]
		//internal event DelegateColumnValueChange ColumnValueChanging;

		static readonly object _propertyChangedKey = new object ();
		internal event PropertyChangedEventHandler PropertyChanged {
			add { _eventHandlers.AddHandler (_propertyChangedKey, value); }
			remove { _eventHandlers.RemoveHandler (_propertyChangedKey, value); }
		}

		#endregion //Events

		#region Fields

		private bool _allowDBNull = true;
		private bool _autoIncrement;
		private long _autoIncrementSeed;
		private long _autoIncrementStep = 1;
		private long _nextAutoIncrementValue;
		private string _caption;
		private MappingType _columnMapping;
		private string _columnName = String.Empty;
		private object _defaultValue = GetDefaultValueForType (null);
		private string _expression;
		private IExpression _compiledExpression;
		private PropertyCollection _extendedProperties = new PropertyCollection ();
		private int _maxLength = -1; //-1 represents no length limit
		private string _nameSpace;
		private int _ordinal = -1; //-1 represents not part of a collection
		private string _prefix = String.Empty;
		private bool _readOnly;
		private DataTable _table;
		private bool _unique;
		private DataContainer _dataContainer;

		#endregion // Fields

		#region Constructors

		public DataColumn ()
			: this (String.Empty, typeof (string), String.Empty, MappingType.Element)
		{
		}

		//TODO: Ctor init vars directly
		public DataColumn (string columnName)
			: this (columnName, typeof (string), String.Empty, MappingType.Element)
		{
		}

		public DataColumn (string columnName, Type dataType)
			: this (columnName, dataType, String.Empty, MappingType.Element)
		{
		}

		public DataColumn (string columnName, Type dataType, string expr)
			: this (columnName, dataType, expr, MappingType.Element)
		{
		}

		public DataColumn (string columnName, Type dataType, string expr, MappingType type)
		{
			ColumnName = columnName == null ? String.Empty : columnName;

			if (dataType == null)
				throw new ArgumentNullException ("dataType");

			DataType = dataType;
			Expression = expr == null ? String.Empty : expr;
			ColumnMapping = type;
		}
		#endregion

		#region Properties

		internal object this [int index] {
			get { return DataContainer [index]; }
			set {
				if (!(value == null && AutoIncrement)) {
					try {
						DataContainer [index] = value;
					} catch(Exception e) {
						throw new ArgumentException (
							String.Format (
								"{0}. Couldn't store <{1}> in Column named '{2}'. Expected type is {3}.",
								e.Message, value, ColumnName, DataType.Name),
							e);
					}
				}

				if (AutoIncrement && !DataContainer.IsNull (index)) {
					long value64 = Convert.ToInt64 (value);
					UpdateAutoIncrementValue (value64);
				}
			}
		}

#if NET_2_0
		DataSetDateTime _datetimeMode = DataSetDateTime.UnspecifiedLocal;
		[DefaultValue (DataSetDateTime.UnspecifiedLocal)]
		[RefreshProperties (RefreshProperties.All)]
		public DataSetDateTime DateTimeMode {
			get { return _datetimeMode; }
			set {
				if (DataType != typeof (DateTime))
					throw new InvalidOperationException ("The DateTimeMode can be set only on DataColumns of type DateTime.");

				if (!Enum.IsDefined (typeof (DataSetDateTime), value))
					throw new InvalidEnumArgumentException (
						string.Format (
							CultureInfo.InvariantCulture, "The {0} enumeration value, {1}, is invalid",
							typeof (DataSetDateTime).Name, value));

				if (_datetimeMode == value)
					return;
				if (_table == null || _table.Rows.Count == 0) {
					_datetimeMode = value;
					return;
				}
				if ((_datetimeMode == DataSetDateTime.Unspecified || _datetimeMode == DataSetDateTime.UnspecifiedLocal)
					&& (value == DataSetDateTime.Unspecified || value == DataSetDateTime.UnspecifiedLocal)) {
					_datetimeMode = value;
					return;
				}

				throw new InvalidOperationException (
					String.Format ("Cannot change DateTimeMode from '{0}' to '{1}' once the table has data.",
						       _datetimeMode, value));
			}
		}
#endif

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates whether null values are allowed in this column.")]
#endif
		[DefaultValue (true)]
		public bool AllowDBNull {
			get { return _allowDBNull; }
			set {
				if (!value && null != _table) {
					for (int r = 0; r < _table.Rows.Count; r++) {
						DataRow row = _table.Rows [r];
						DataRowVersion version = row.HasVersion (DataRowVersion.Default) ?
							DataRowVersion.Default : DataRowVersion.Original;
						if (row.IsNull (this, version))
							throw new DataException ("Column '" + ColumnName + "' has null values in it.");
						//TODO: do we also check different versions of the row??
					}
				}

				_allowDBNull = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the column automatically increments the value of the column for new rows added to the table.
		/// </summary>
		/// <remarks>
		///		If the type of this column is not Int16, Int32, or Int64 when this property is set,
		///		the DataType property is coerced to Int32. An exception is generated if this is a computed column
		///		(that is, the Expression property is set.) The incremented value is used only if the row's value for this column,
		///		when added to the columns collection, is equal to the default value.
		///	</remarks>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates whether the column automatically increments itself for new rows added to the table.  The type of this column must be Int16, Int32, or Int64.")]
#endif
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.All)]
		public bool AutoIncrement {
			get { return _autoIncrement; }
			set {
				if (value) {
					//Can't be true if this is a computed column
					if (Expression != string.Empty)
						throw new ArgumentException ("Can not Auto Increment a computed column.");

					if (DefaultValue != DBNull.Value)
						throw new ArgumentException ("Can not set AutoIncrement while default value exists for this column.");

					if (!CanAutoIncrement (DataType))
						DataType = typeof (Int32);
				}

				if (_table != null)
					_table.Columns.UpdateAutoIncrement (this, value);
				_autoIncrement = value;
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the starting value for an AutoIncrement column.")]
#endif
		[DefaultValue (0)]
		public long AutoIncrementSeed {
			get { return _autoIncrementSeed; }
			set {
				_autoIncrementSeed = value;
				_nextAutoIncrementValue = _autoIncrementSeed;
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the increment used by an AutoIncrement column.")]
#endif
		[DefaultValue (1)]
		public long AutoIncrementStep {
			get { return _autoIncrementStep; }
			set { _autoIncrementStep = value; }
		}

		internal void UpdateAutoIncrementValue (long value64)
		{
			if (_autoIncrementStep > 0) {
				if (value64 >= _nextAutoIncrementValue) {
					_nextAutoIncrementValue = value64;
					AutoIncrementValue ();
				}
			} else if (value64 <= _nextAutoIncrementValue) {
				_nextAutoIncrementValue = value64;
				AutoIncrementValue ();
			}
		}

		internal long AutoIncrementValue ()
		{
			long currentValue = _nextAutoIncrementValue;
			_nextAutoIncrementValue += AutoIncrementStep;
			return currentValue;
		}

		internal long GetAutoIncrementValue ()
		{
			return _nextAutoIncrementValue;
		}

		internal void SetDefaultValue (int index)
		{
			if (AutoIncrement)
				this [index] = _nextAutoIncrementValue;
			else
				DataContainer.CopyValue (Table.DefaultValuesRowIndex, index);
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the default user-interface caption for this column.")]
#endif
		public string Caption {
			get { return _caption == null ? ColumnName : _caption; }
			set { _caption = value == null ? String.Empty : value; }
		}

#if !NET_2_0
		[DataSysDescription ("Indicates how this column persists in XML: as an attribute, element, simple content node, or nothing.")]
#endif
		[DefaultValue (MappingType.Element)]
		public virtual MappingType ColumnMapping {
			get { return _columnMapping; }
			set { _columnMapping = value; }
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the name used to look up this column in the Columns collection of a DataTable.")]
#endif
		[RefreshProperties (RefreshProperties.All)]
		[DefaultValue ("")]
		public string ColumnName {
			get { return _columnName; }
			set {
				if (value == null)
					value = String.Empty;

				CultureInfo info = Table != null ? Table.Locale : CultureInfo.CurrentCulture;
				if (String.Compare (value, _columnName, true, info) != 0) {
					if (Table != null) {
						if (value.Length == 0)
							throw new ArgumentException ("ColumnName is required when it is part of a DataTable.");

						Table.Columns.RegisterName (value, this);
						if (_columnName.Length > 0)
							Table.Columns.UnregisterName (_columnName);
					}

					RaisePropertyChanging ("ColumnName");
					_columnName = value;

					if (Table != null)
						Table.ResetPropertyDescriptorsCache ();
				} else if (String.Compare (value, _columnName, false, info) != 0) {
					RaisePropertyChanging ("ColumnName");
					_columnName = value;

					if (Table != null)
						Table.ResetPropertyDescriptorsCache ();
				}
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the type of data stored in this column.")]
#endif
		[DefaultValue (typeof (string))]
		[RefreshProperties (RefreshProperties.All)]
		[TypeConverterAttribute (typeof (ColumnTypeConverter))]
		public Type DataType {
			get { return DataContainer.Type; }
			set {
				if (value == null)
					return;

				if (_dataContainer != null) {
					if (value == _dataContainer.Type)
						return;

					// check if data already exists can we change the datatype
					if (_dataContainer.Capacity > 0)
						throw new ArgumentException ("The column already has data stored.");
				}

				if (null != GetParentRelation () || null != GetChildRelation ())
					throw new InvalidConstraintException ("Cannot change datatype when column is part of a relation");

				Type prevType = _dataContainer != null ? _dataContainer.Type : null; // current

#if NET_2_0
				if (_dataContainer != null && _dataContainer.Type == typeof (DateTime))
					_datetimeMode = DataSetDateTime.UnspecifiedLocal;
#endif
				_dataContainer = DataContainer.Create (value, this);

				//Check AutoIncrement status, make compatible datatype
				if(AutoIncrement == true) {
					// we want to check that the datatype is supported?					
					if (!CanAutoIncrement (value))
						AutoIncrement = false;
				}

				if (DefaultValue != GetDefaultValueForType (prevType))
					SetDefaultValue (DefaultValue, true);
				else
					_defaultValue = GetDefaultValueForType (DataType);
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>When AutoIncrement is set to true, there can be no default value.</remarks>
		/// <exception cref="System.InvalidCastException"></exception>
		/// <exception cref="System.ArgumentException"></exception>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the default column value used when adding new rows to the table.")]
#endif
		[TypeConverterAttribute (typeof (System.Data.DefaultValueTypeConverter))]
		public object DefaultValue {
			get { return _defaultValue; }

			set {
				if (AutoIncrement)
					throw new ArgumentException ("Can not set default value while AutoIncrement is true on this column.");
				SetDefaultValue (value, false);
			}
		}

		void SetDefaultValue (object value, bool forcedTypeCheck)
		{
			if (forcedTypeCheck || !this._defaultValue.Equals (value)) {
				if (value == null || value == DBNull.Value)
					_defaultValue = GetDefaultValueForType (DataType);
				else if (DataType.IsInstanceOfType (value))
					_defaultValue = value;
				else
					try {
						_defaultValue = Convert.ChangeType (value, DataType);
					} catch (InvalidCastException) {
						string msg = String.Format ("Default Value of type '{0}' is not compatible with column type '{1}'", value.GetType (), DataType);
#if NET_2_0
						throw new DataException (msg);
#else
						throw new ArgumentException (msg);
#endif
					}
			}

			// store default value in the table if already belongs to
			if (Table != null && Table.DefaultValuesRowIndex != -1)
				DataContainer [Table.DefaultValuesRowIndex] = _defaultValue;
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the value that this column computes for each row based on other columns instead of taking user input.")]
#endif
		[DefaultValue ("")]
		[RefreshProperties (RefreshProperties.All)]
		public string Expression {
			get { return _expression; }
			set {
				if (value == null)
					value = String.Empty;

				CompileExpression (value);
				_expression = value;
			}
		}

		internal void CompileExpression ()
		{
			CompileExpression (_expression);
		}

		internal void CompileExpression (string expression)
		{
			if (expression != String.Empty) {
				if (AutoIncrement || Unique)
					throw new ArgumentException ("Cannot create an expression on a column that has AutoIncrement or Unique.");

				if (Table != null) {
					for (int i = 0; i < Table.Constraints.Count; i++) {
						if (Table.Constraints [i].IsColumnContained (this))
							throw new ArgumentException (
								String.Format (
									"Cannot set Expression property on column {0}, because it is a part of a constraint.",
									ColumnName));
					}
				}

				Parser parser = new Parser ();
				IExpression compiledExpression = parser.Compile (expression);

				if (Table != null) {
					if (compiledExpression.DependsOn (this))
						throw new ArgumentException ("Cannot set Expression property due to circular reference in the expression.");
					// Check if expression is ok
					if (Table.Rows.Count == 0)
						compiledExpression.Eval (Table.NewRow ());
					else
						compiledExpression.Eval (Table.Rows [0]);
				}
				ReadOnly = true;
				_compiledExpression = compiledExpression;
			} else {
				_compiledExpression = null;
				if (Table != null) {
					int defaultValuesRowIndex = Table.DefaultValuesRowIndex;
					if (defaultValuesRowIndex != -1)
						DataContainer.FillValues (defaultValuesRowIndex);
				}
			}
		}

		internal IExpression CompiledExpression {
			get { return _compiledExpression; }
		}

		[Browsable (false)]
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("The collection that holds custom user information.")]
#endif
		public PropertyCollection ExtendedProperties {
			get { return _extendedProperties; }
#if NET_2_0
			internal set { _extendedProperties = value; }
#endif
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the maximum length of the value this column allows. ")]
#endif
		[DefaultValue (-1)] //Default == -1 no max length
		public int MaxLength {
			get { return _maxLength; }
			set {
				if (value >= 0 && _columnMapping == MappingType.SimpleContent)
					throw new ArgumentException (
						String.Format (
							"Cannot set MaxLength property on '{0}' column which is mapped to SimpleContent.",
							ColumnName));
				//only applies to string columns
				_maxLength = value;
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the XML uri for elements or attributes stored in this column.")]
#endif
		public string Namespace {
			get {
				if (_nameSpace != null)
					return _nameSpace;
				if (Table != null && _columnMapping != MappingType.Attribute)
					return Table.Namespace;
				return String.Empty;
			}
			set { _nameSpace = value; }
		}

		//Need a good way to set the Ordinal when the column is added to a columnCollection.
		[Browsable (false)]
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the index of this column in the Columns collection.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Ordinal {
			get { return _ordinal; }
#if NET_2_0
			internal set { _ordinal = value; }
#endif
		}

#if NET_2_0
		public void SetOrdinal (int ordinal)
		{
			if (_ordinal == -1)
				throw new ArgumentException ("Column must belong to a table.");
			_table.Columns.MoveColumn (_ordinal, ordinal);
			_ordinal = ordinal;
		}
#else
		internal void SetOrdinal(int ordinal)
		{
			_ordinal = ordinal;
		}
#endif

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the Prefix used for this DataColumn in xml representation.")]
#endif
		[DefaultValue ("")]
		public string Prefix {
			get { return _prefix; }
			set { _prefix = value == null ? String.Empty : value; }
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates whether this column allows changes once a row has been added to the table.")]
#endif
		[DefaultValue (false)]
		public bool ReadOnly {
			get { return _readOnly; }
			set { _readOnly = value; }
		}

		[Browsable (false)]
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Returns the DataTable to which this column belongs.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataTable Table {
			get { return _table; }
#if NET_2_0
			internal set { _table = value; }
#endif
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates whether this column should restrict its values in the rows of the table to be unique.")]
#endif
		[DefaultValue (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Unique {
			get { return _unique; }
			set {
				if (_unique == value)
					return;

				// Set the property value, so that when adding/removing the constraint
				// we dont run into recursive issues.
				_unique = value;

				if (_table == null)
					return;

				try {
					if (value) {
						if (Expression != null && Expression != String.Empty)
							throw new ArgumentException ("Cannot change Unique property for the expression column.");

						_table.Constraints.Add (null, this, false);
					} else {

						UniqueConstraint uc = UniqueConstraint.GetUniqueConstraintForColumnSet (
							_table.Constraints, new DataColumn[] {this});
						_table.Constraints.Remove (uc);
					}
				} catch (Exception e) {
					_unique = !value;
					throw e;
				}
			}
		}

		internal DataContainer DataContainer {
			get { return _dataContainer; }
		}

		internal static bool CanAutoIncrement (Type type)
		{
			switch (Type.GetTypeCode (type)) {
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
					return true;
			}

			return false;
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		internal DataColumn Clone ()
		{
			DataColumn copy = new DataColumn ();

			// Copy all the properties of column
			copy._allowDBNull = _allowDBNull;
			copy._autoIncrement = _autoIncrement;
			copy._autoIncrementSeed = _autoIncrementSeed;
			copy._autoIncrementStep = _autoIncrementStep;
			copy._caption = _caption;
			copy._columnMapping = _columnMapping;
			copy._columnName = _columnName;
			//Copy.Container
			copy.DataType = DataType;
			copy._defaultValue = _defaultValue;
			// Do not use the Expression property to set the expression, the caller of Clone 
			// must explicitly call CompileExpression on the returned DataColumn to update compiledExpression (if any).
			copy._expression = _expression;
			//Copy.ExtendedProperties
			foreach (object key in _extendedProperties.Keys)
				copy.ExtendedProperties.Add (key, ExtendedProperties[key]);
			copy._maxLength = _maxLength;
			copy._nameSpace = _nameSpace;
			copy._prefix = _prefix;
			copy._readOnly = _readOnly;
			//Copy.Site
			//we do not copy the unique value - it will be copyied when copying the constraints.
			//Copy.Unique = Column.Unique;
#if NET_2_0
			if (DataType == typeof (DateTime))
				copy.DateTimeMode = _datetimeMode;
#endif

			return copy;
		}

		/// <summary>
		///  Sets unique true whithout creating Constraint
		/// </summary>
		internal void SetUnique ()
		{
			_unique = true;
		}

		[MonoTODO]
		internal void AssertCanAddToCollection ()
		{
			//Check if Default Value is set and AutoInc is set
		}

		[MonoTODO]
		protected internal void CheckNotAllowNull ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void CheckUnique ()
		{
			throw new NotImplementedException ();
		}

		protected internal virtual void
		OnPropertyChanging (PropertyChangedEventArgs pcevent)
		{
			PropertyChangedEventHandler eh = _eventHandlers [_propertyChangedKey] as PropertyChangedEventHandler;

			if (eh != null)
				eh (this, pcevent);
		}

		protected internal void RaisePropertyChanging (string name)
		{
			PropertyChangedEventArgs e = new PropertyChangedEventArgs (name);
			OnPropertyChanging (e);
		}

		/// <summary>
		/// Gets the Expression of the column, if one exists.
		/// </summary>
		/// <returns>The Expression value, if the property is set;
		/// otherwise, the ColumnName property.</returns>
		public override string ToString ()
		{
			if (_expression != string.Empty)
				return ColumnName + " + " + _expression;

			return ColumnName;
		}

		internal void SetTable (DataTable table)
		{
			if(_table != null)
				throw new ArgumentException ("The column already belongs to a different table");

			_table = table;
			// this will get called by DataTable
			// and DataColumnCollection
			if (_unique) {
				// if the DataColumn is marked as Unique and then
				// added to a DataTable , then a UniqueConstraint
				// should be created
				UniqueConstraint uc = new UniqueConstraint (this);
				_table.Constraints.Add (uc);
			}

			// allocate space in the column data container
			DataContainer.Capacity = _table.RecordCache.CurrentCapacity;

			int defaultValuesRowIndex = _table.DefaultValuesRowIndex;
			if (defaultValuesRowIndex != -1) {
				// store default value in the table
				DataContainer [defaultValuesRowIndex] = _defaultValue;
				// Set all the values in data container to default
				// it's cheaper that raise event on each row.
				DataContainer.FillValues (defaultValuesRowIndex);
			}
		}

		// Returns true if all the same collumns are in columnSet and compareSet
		internal static bool AreColumnSetsTheSame (DataColumn [] columnSet, DataColumn [] compareSet)
		{
			if (null == columnSet && null == compareSet)
				return true;

			if (null == columnSet || null == compareSet)
				return false;

			if (columnSet.Length != compareSet.Length)
				return false;

			foreach (DataColumn col in columnSet) {
				bool matchFound = false;
				foreach (DataColumn compare in compareSet) {
					if (col == compare)
						matchFound = true;
				}
				if (!matchFound)
					return false;
			}
			return true;
		}

		internal int CompareValues (int index1, int index2)
		{
			return DataContainer.CompareValues (index1, index2);
		}

		/// <summary>
		///     Returns the data relation, which contains this column.
		///     This searches in current table's parent relations.
		/// <summary>
		/// <returns>
		///     DataRelation if found otherwise null.
		/// </returns>
		private DataRelation GetParentRelation ()
		{
			if (_table == null)
				return null;
			foreach (DataRelation rel in _table.ParentRelations)
				if (rel.Contains (this))
					return rel;
			return null;
		}


		/// <summary>
		///     Returns the data relation, which contains this column.
		///     This searches in current table's child relations.
		/// <summary>
		/// <returns>
		///     DataRelation if found otherwise null.
		/// </returns>
		private DataRelation GetChildRelation ()
		{
			if (_table == null)
				return null;
			foreach (DataRelation rel in _table.ChildRelations)
				if (rel.Contains (this))
					return rel;
			return null;
		}

		internal void ResetColumnInfo ()
		{
			_ordinal = -1;
			_table = null;
			if (_compiledExpression != null)
				_compiledExpression.ResetExpression ();
		}

		internal bool DataTypeMatches (DataColumn col)
		{
			if (DataType != col.DataType)
				return false;
#if NET_2_0
			if (DataType != typeof (DateTime))
				return true;

			if (DateTimeMode == col.DateTimeMode)
				return true;

			if (DateTimeMode == DataSetDateTime.Local || DateTimeMode == DataSetDateTime.Utc)
				return false;

			if (col.DateTimeMode == DataSetDateTime.Local || col.DateTimeMode == DataSetDateTime.Utc)
				return false;
#endif
			return true;
		}

		internal static object GetDefaultValueForType (Type type)
		{
#if NET_2_0
			if (type == null)
				return DBNull.Value;
			if (type.Namespace == "System.Data.SqlTypes" && type.Assembly == typeof (DataColumn).Assembly) {
				// For SqlXxx types, set SqlXxx.Null instead of DBNull.Value.
				if (type == typeof (SqlBinary))
					return SqlBinary.Null;
				if (type == typeof (SqlBoolean))
					return SqlBoolean.Null;
				if (type == typeof (SqlByte))
					return SqlByte.Null;
				if (type == typeof (SqlBytes))
					return SqlBytes.Null;
				if (type == typeof (SqlChars))
					return SqlChars.Null;
				if (type == typeof (SqlDateTime))
					return SqlDateTime.Null;
				if (type == typeof (SqlDecimal))
					return SqlDecimal.Null;
				if (type == typeof (SqlDouble))
					return SqlDouble.Null;
				if (type == typeof (SqlGuid))
					return SqlGuid.Null;
				if (type == typeof (SqlInt16))
					return SqlInt16.Null;
				if (type == typeof (SqlInt32))
					return SqlInt32.Null;
				if (type == typeof (SqlInt64))
					return SqlInt64.Null;
				if (type == typeof (SqlMoney))
					return SqlMoney.Null;
				if (type == typeof (SqlSingle))
					return SqlSingle.Null;
				if (type == typeof (SqlString))
					return SqlString.Null;
				if (type == typeof (SqlXml))
					return SqlXml.Null;
			}
#endif
			return DBNull.Value;
		}

		#endregion // Methods
	}
}
