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
using Mono.Data.SqlExpressions;

namespace System.Data {
	internal delegate void DelegateColumnValueChange(DataColumn column, DataRow row, object proposedValue);
	
	/// <summary>
	/// Summary description for DataColumn.
	/// </summary>

	[Editor]
	[ToolboxItem (false)]
	[DefaultProperty ("ColumnName")]
	[DesignTimeVisible (false)]
	public class DataColumn : MarshalByValueComponent
	{		
		#region Events
		[MonoTODO]
		//used for constraint validation
		//if an exception is fired during this event the change should be canceled
		internal event DelegateColumnValueChange ValidateColumnValueChange;

		//used for FK Constraint Cascading rules
		internal event DelegateColumnValueChange ColumnValueChanging;
		#endregion //Events
		
		#region Fields

		private bool _allowDBNull = true;
		private bool _autoIncrement;
		private long _autoIncrementSeed;
		private long _autoIncrementStep = 1;
		private long _nextAutoIncrementValue;
		private string _caption;
		private MappingType _columnMapping;
		private string _columnName;
		private object _defaultValue = DBNull.Value;
		private string expression;
		private IExpression compiledExpression;
		private PropertyCollection _extendedProperties = new PropertyCollection ();
		private int maxLength = -1; //-1 represents no length limit
		private string nameSpace = String.Empty;
		private int _ordinal = -1; //-1 represents not part of a collection
		private string prefix = String.Empty;
		private bool readOnly;
		private DataTable _table;
		private bool unique;
		private AbstractDataContainer _dataContainer;

		#endregion // Fields

		#region Constructors

		public DataColumn() : this(String.Empty, typeof (string), String.Empty, MappingType.Element)
		{
		}

		//TODO: Ctor init vars directly
		public DataColumn(string columnName): this(columnName, typeof (string), String.Empty, MappingType.Element)
		{
		}

		public DataColumn(string columnName, Type dataType): this(columnName, dataType, String.Empty, MappingType.Element)
		{
		}

		public DataColumn( string columnName, Type dataType, 
			string expr): this(columnName, dataType, expr, MappingType.Element)
		{
		}

		public DataColumn(string columnName, Type dataType, 
			string expr, MappingType type)
		{
			ColumnName = (columnName == null ? String.Empty : columnName);
			
			if(dataType == null) {
				throw new ArgumentNullException("dataType can't be null.");
			}
			
			DataType = dataType;
			Expression = expr == null ? String.Empty : expr;
			ColumnMapping = type;
		}
		#endregion

		#region Properties

		internal object this[int index] {
			get {
				return DataContainer[index];
			}
			set {
				if ( !(value == null && AutoIncrement) ) {
					DataContainer[index] = value;
				}

				if ( AutoIncrement && !DataContainer.IsNull(index) ) {
					long value64 = Convert.ToInt64(value);
					UpdateAutoIncrementValue(value64);
				}
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether null values are allowed in this column.")]
		[DefaultValue (true)]
		public bool AllowDBNull
		{
			get {
				return _allowDBNull;
			}
			set {
				//TODO: If we are a part of the table and this value changes
				//we need to validate that all the existing values conform to the new setting

				if (true == value)
				{
					_allowDBNull = true;
					return;
				}
				
				//if Value == false case
				if (null != _table)
				{
					if (_table.Rows.Count > 0)
					{
						bool nullsFound = false;
						for(int r = 0; r < _table.Rows.Count; r++) {
							DataRow row = _table.Rows[r];
							if(row.IsNull(this)) {
								nullsFound = true;
								break;
							}
						}
						
						if (nullsFound)
							throw new DataException("Column '" + ColumnName + "' has null values in it.");
						//TODO: Validate no null values exist
						//do we also check different versions of the row??
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
		[DataSysDescription ("Indicates whether the column automatically increments itself for new rows added to the table.  The type of this column must be Int16, Int32, or Int64.")]
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.All)]
		public bool AutoIncrement
		{
			get {
				return _autoIncrement;
			}
			set {
				if(value == true)
				{
					//Can't be true if this is a computed column
					if (Expression != string.Empty)
					{
						throw new ArgumentException("Can not Auto Increment a computed column."); 
					}

					if ( DefaultValue != DBNull.Value ) {
						throw new ArgumentException("Can not set AutoIncrement while" +
							" default value exists for this column.");
					}

					//If the DataType of this Column isn't an Int
					//Make it an int
					TypeCode typeCode = Type.GetTypeCode(DataType);
					if(typeCode != TypeCode.Int16 && 
						typeCode != TypeCode.Int32 && 
						typeCode != TypeCode.Int64)
					{
						DataType = typeof(Int32); 
					}

					if (_table != null)
						_table.Columns.UpdateAutoIncrement(this,true);
				}
				else
				{
					if (_table != null)
						_table.Columns.UpdateAutoIncrement(this,false);
				}
				_autoIncrement = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the starting value for an AutoIncrement column.")]
		[DefaultValue (0)]
		public long AutoIncrementSeed
		{
			get {
				return _autoIncrementSeed;
			}
			set {
				_autoIncrementSeed = value;
				_nextAutoIncrementValue = _autoIncrementSeed;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the increment used by an AutoIncrement column.")]
		[DefaultValue (1)]
		public long AutoIncrementStep
		{
			get {
				return _autoIncrementStep;
			}
			set {
				_autoIncrementStep = value;
			}
		}

		internal void UpdateAutoIncrementValue(long value64)
		{
			if (_autoIncrementStep > 0 ) {
				if (value64 >= _nextAutoIncrementValue) {
					_nextAutoIncrementValue = value64;
					AutoIncrementValue ();
				}
			}
			else if (value64 <= _nextAutoIncrementValue) {
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

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the default user-interface caption for this column.")]
		public string Caption 
		{
			get {
				if(_caption == null)
					return ColumnName;
				else
					return _caption;
			}
			set {
				if (value == null)
					value = String.Empty;
					
				_caption = value;
			}
		}
		[DataSysDescription ("Indicates how this column persists in XML: as an attribute, element, simple content node, or nothing.")]
		[DefaultValue (MappingType.Element)]
		public virtual MappingType ColumnMapping
		{
			get {
				return _columnMapping;
			}
			set {
				_columnMapping = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the name used to look up this column in the Columns collection of a DataTable.")]
		[RefreshProperties (RefreshProperties.All)]
		[DefaultValue ("")]
		public string ColumnName
		{
			get {
				return (_columnName == null ? String.Empty : _columnName);
			}
			set {
				//Both are checked after the column is part of the collection
				//TODO: Check Name duplicate
				//TODO: check Name != null
				_columnName = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the type of data stored in this column.")]
		[DefaultValue (typeof (string))]
		[RefreshProperties (RefreshProperties.All)]
		[TypeConverterAttribute (typeof (ColumnTypeConverter))] 
		public Type DataType
		{
			get {
				return DataContainer.Type;
			}
			set {

                                if ( value == null ) 
                                        return;

				if ( _dataContainer != null ) {
					if ( value == _dataContainer.Type ) 
						return;

					// check if data already exists can we change the datatype
					if ( _dataContainer.Capacity > 0 )
						throw new ArgumentException("The column already has data stored.");
				}

                                if (null != GetParentRelation () || null != GetChildRelation ())
                                        throw new InvalidConstraintException ("Cannot change datatype, " + 
                                                                              "when column is part of a relation");
                                
                                _dataContainer = AbstractDataContainer.CreateInstance(value, this);

				//Check AutoIncrement status, make compatible datatype
				if(AutoIncrement == true) {
					// we want to check that the datatype is supported?
					TypeCode typeCode = Type.GetTypeCode(value);
					
					if(typeCode != TypeCode.Int16 &&
					   typeCode != TypeCode.Int32 &&
					   typeCode != TypeCode.Int64) {
						AutoIncrement = false;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>When AutoIncrement is set to true, there can be no default value.</remarks>
		/// <exception cref="System.InvalidCastException"></exception>
		/// <exception cref="System.ArgumentException"></exception>
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the default column value used when adding new rows to the table.")]
		[TypeConverterAttribute (typeof (System.Data.DefaultValueTypeConverter))]
		public object DefaultValue
		{
			get {
				return _defaultValue;
			}

			set {
				if (AutoIncrement) {
					throw new ArgumentException("Can not set default value while" +
						" AutoIncrement is true on this column.");
				}

				object tmpObj;
				if (!this._defaultValue.Equals(value)) {		
					if (value == null) {
						tmpObj = DBNull.Value;
					}
					else {
						tmpObj = value;
					}

					if ((this.DataType != typeof (object))&& (tmpObj != DBNull.Value)) {
						try {
							//Casting to the new type
							tmpObj= Convert.ChangeType(tmpObj,this.DataType);
						}
						catch (InvalidCastException) {
							throw new InvalidCastException("Default Value type is not compatible with" + 
								" column type.");
						}
					}
					_defaultValue = tmpObj;
				}

				// store default value in the table if already belongs to
				if (Table != null && Table.DefaultValuesRowIndex != -1) {
					DataContainer[Table.DefaultValuesRowIndex] = _defaultValue;
				}
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the value that this column computes for each row based on other columns instead of taking user input.")]
		[DefaultValue ("")]
		[RefreshProperties (RefreshProperties.All)]
		public string Expression
		{
			get {
				return expression;
			}
			set {
				if (value == null)
					value = String.Empty;
					
		 		if (value != String.Empty) {
					Parser parser = new Parser ();
					compiledExpression = parser.Compile (value);
					ReadOnly = true;
   				}
				expression = value;  
			}
		}

		internal IExpression CompiledExpression {
			get { return compiledExpression; }
		}

		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds custom user information.")]
		public PropertyCollection ExtendedProperties
		{
			get {
				return _extendedProperties;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the maximum length of the value this column allows.")]
		[DefaultValue (-1)]
		public int MaxLength
		{
			get {
				//Default == -1 no max length
				return maxLength;
			}
			set {
				if (value >= 0 &&
					_columnMapping == MappingType.SimpleContent)
					throw new ArgumentException (String.Format ("Cannot set MaxLength property on '{0}' column which is mapped to SimpleContent.", ColumnName));
				//only applies to string columns
				maxLength = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the XML uri for elements stored in this  column.")]
		public string Namespace
		{
			get {
				return nameSpace;
			}
			set {
				if (value == null)
					value = String.Empty;
				nameSpace = value;
			}
		}

		//Need a good way to set the Ordinal when the column is added to a columnCollection.
		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the index of this column in the Columns collection.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Ordinal
		{
			get {
				//value is -1 if not part of a collection
				return _ordinal;
			}
		}

		internal void SetOrdinal(int ordinal)
		{
			_ordinal = ordinal;
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the prefix used for this DataColumn in the xml representation.")]
		[DefaultValue ("")]
		public string Prefix
		{
			get {
				return prefix;
			}
			set {
				if (value == null)
					value = String.Empty;
				prefix = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this column allows changes once a row has been added to the table.")]
		[DefaultValue (false)]
		public bool ReadOnly
		{
			get {
				return readOnly;
			}
			set {
				readOnly = value;
			}
		}

		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("Returns the DataTable to which this column belongs.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public DataTable Table
		{
			get {
				return _table;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this column should restrict its values in the rows of the table to be unique.")]
		[DefaultValue (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
                public bool Unique 
		{
			get {
				return unique;
			}
			set {
				//NOTE: In .NET 1.1 the Unique property
                                //is left unchanged when it is added
                                //to a UniqueConstraint

				if(unique != value)
				{
				unique = value;

					if( value )
					{
						if (Expression != null && Expression != String.Empty)
							throw new ArgumentException("Cannot change Unique property for the expression column.");
						if( _table != null )
						{
							UniqueConstraint uc = new UniqueConstraint(this);
							_table.Constraints.Add(uc);
						}
					}
					else
					{
						if( _table != null )
						{
							ConstraintCollection cc = _table.Constraints;
							//foreach (Constraint c in cc) 
							for (int i = 0; i < cc.Count; i++)
							{
								Constraint c = cc[i];
								if (c is UniqueConstraint)
								{
									DataColumn[] cols = ((UniqueConstraint)c).Columns;
									
									if (cols.Length == 1 && cols[0] == this)
									{
										if (!cc.CanRemove(c))
											throw new ArgumentException("Cannot remove unique constraint '" + c.ConstraintName + "'. Remove foreign key constraint first.");

										cc.Remove(c);
									}
									
								}
							}
						}
					}

				}
			}
		}

		internal AbstractDataContainer DataContainer {
			get {
				return _dataContainer;
			}
		}

		#endregion // Properties

		#region Methods
		
/* ??
		[MonoTODO]
		protected internal void CheckNotAllowNull() {
		}

		[MonoTODO]
		protected void CheckUnique() {
		}
*/

		/// <summary>
		///  Sets unique true whithout creating Constraint
		/// </summary>
		internal void SetUnique() 
		{
			unique = true;
		}

		[MonoTODO]
		internal void AssertCanAddToCollection()
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

		[MonoTODO]
		protected internal virtual void 
		OnPropertyChanging (PropertyChangedEventArgs pcevent) {
		}

		[MonoTODO]
		protected internal void RaisePropertyChanging(string name) {
		}

		/// <summary>
		/// Gets the Expression of the column, if one exists.
		/// </summary>
		/// <returns>The Expression value, if the property is set; 
		/// otherwise, the ColumnName property.</returns>
		public override string ToString()
		{
			if (expression != string.Empty)
				return ColumnName + " + " + expression;
			
			return ColumnName;
		}

		internal void SetTable(DataTable table) {
			if(_table!=null) { // serves as double check while adding to a table
                    throw new ArgumentException("The column already belongs to a different table");
            }
            _table = table;
            // this will get called by DataTable
            // and DataColumnCollection
            if(unique) {
                // if the DataColumn is marked as Unique and then
	            // added to a DataTable , then a UniqueConstraint
        	    // should be created
                UniqueConstraint uc = new UniqueConstraint(this);
                _table.Constraints.Add(uc);
            }

			// allocate space in the column data container 
			DataContainer.Capacity = _table.RecordCache.CurrentCapacity;
			
			int defaultValuesRowIndex = _table.DefaultValuesRowIndex;
			if ( defaultValuesRowIndex != -1) {
				// store default value in the table
				DataContainer[defaultValuesRowIndex] = _defaultValue;
				// Set all the values in data container to default
				// it's cheaper that raise event on each row.
				DataContainer.FillValues(_table.DefaultValuesRowIndex);
			}
		}

		
		// Returns true if all the same collumns are in columnSet and compareSet
		internal static bool AreColumnSetsTheSame(DataColumn[] columnSet, DataColumn[] compareSet)
		{
			if (null == columnSet && null == compareSet) return true;
			if (null == columnSet || null == compareSet) return false;

			if (columnSet.Length != compareSet.Length) return false;
			
			foreach (DataColumn col in columnSet)
			{
				bool matchFound = false;
				foreach (DataColumn compare in compareSet)
				{
					if (col == compare)
					{
						matchFound = true;					
					}
				}
				if (! matchFound) return false;
			}
			
			return true;
		}
		
		internal int CompareValues (int index1, int index2)
		{
			return DataContainer.CompareValues(index1, index2);
		}

                /// <summary>
                ///     Returns the data relation, which contains this column.
                ///     This searches in current table's parent relations.
                /// <summary>
                /// <returns>
                ///     DataRelation if found otherwise null.
                /// </returns>
                internal DataRelation GetParentRelation ()
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
                internal DataRelation GetChildRelation ()
                {
                        if (_table == null)
                                return null;
                        foreach (DataRelation rel in _table.ChildRelations)
                                if (rel.Contains (this))
                                        return rel;
                        return null;
                }
                

		#endregion // Methods

	}
}
