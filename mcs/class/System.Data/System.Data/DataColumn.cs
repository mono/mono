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
// (C) Daniel Morgan 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;
using System.Reflection;

namespace System.Data {
	internal delegate void DelegateColumnValueChange(DataColumn column, DataRow row, object proposedValue);
	
	/// <summary>
	/// Summary description for DataColumn.
	/// </summary>

	[DefaultMember ("Item")]
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
		private bool _autoIncrement = false;
		private long _autoIncrementSeed = 0;
		private long _autoIncrementStep = 1;
		private string _caption = null;
		private MappingType _columnMapping = MappingType.Element;
		private string _columnName = null;
		private Type _dataType = null;
		private object _defaultValue = null;
		private string expression = null;
		private PropertyCollection _extendedProperties = new PropertyCollection ();
		private int maxLength = -1; //-1 represents no length limit
		private string nameSpace = null;
		private int _ordinal = -1; //-1 represents not part of a collection
		private string prefix = null;
		private bool readOnly = false;
		private DataTable _table = null;
		private bool unique = false;

		#endregion // Fields

		#region Constructors

		public DataColumn()
		{
		}

		//TODO: Ctor init vars directly
		public DataColumn(string columnName): this()
		{
			ColumnName = columnName;
		}

		public DataColumn(string columnName, Type dataType): this(columnName)
		{
			if(dataType == null) {
				throw new ArgumentNullException("dataType can't be null.");
			}
			
			DataType = dataType;

		}

		public DataColumn( string columnName, Type dataType, 
			string expr): this(columnName, dataType)
		{
			Expression = expr;
		}

		public DataColumn(string columnName, Type dataType, 
			string expr, MappingType type): this(columnName, dataType, expr)
		{
			ColumnMapping = type;
		}
		#endregion

		#region Properties

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
					if(Expression != null)
					{
						throw new ArgumentException("Can't Auto Increment a computed column."); 
					}

					//If the DataType of this Column isn't an Int
					//Make it an int
					if(Type.GetTypeCode(_dataType) != TypeCode.Int16 && 
					   Type.GetTypeCode(_dataType) != TypeCode.Int32 && 
					   Type.GetTypeCode(_dataType) != TypeCode.Int64)
					{
						_dataType = typeof(Int32); 
					}
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
		public string ColumnName
		{
			get {
				return "" + _columnName;
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
		public Type DataType
		{
			get {
				return _dataType;
			}
			set {
				//TODO: check if data already exists can we change the datatype

				//TODO: we want to check that the datatype is supported?
				
				//Check AutoIncrement status, make compatible datatype
				//TODO: Check for other int values i.e. Int16 etc
				if(AutoIncrement == true && 
				   Type.GetTypeCode(value) != TypeCode.Int32)
				{
					throw new Exception(); //TODO: correction exception type
				}
				_dataType = value;
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
		public object DefaultValue
		{
			get {
				return _defaultValue;
			}
			set {
				
				//If autoIncrement == true throw
				if (AutoIncrement) 
				{
					throw new ArgumentException("Can not set default value while" +
							" AutoIncrement is true on this column.");
				}
					
				//Will throw invalid cast exception
				//if value is not the correct type
				//FIXME: some types can be casted
				if (value.GetType() != _dataType)
				{
					throw new InvalidCastException("Default Value type is not compatible with" + 
							" column type.");
				}
					
				_defaultValue = value;
			}
		}

		[MonoTODO]
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
				//TODO: validation of the expression
				expression = value;  //Check?
			}
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

		[MonoTODO]
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
				//if Table == null then the UniqueConstraint is
				//created on addition to the collection
				
				//FIXME?: need to check if value is the same
				//because when calling "new UniqueConstraint"
				//the new object tries to set "column.Unique = True"
				//which creates an infinite loop.
				if(unique != value)
				{
				unique = value;

					if( value )
					{
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
							//FIXME: Add code to remove constraint from DataTable
							throw new NotImplementedException ();
						}
					}

				}
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

		[MonoTODO]
		internal void AssertCanAddToCollection()
		{
			//Check if Default Value is set and AutoInc is set
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
		[MonoTODO]
		public override string ToString()
		{
			if (expression != null)
				return expression;
			
			return ColumnName;
		}

		[MonoTODO]
		internal void SetTable(DataTable table) {
			_table = table; 
			// this will get called by DataTable 
			// and DataColumnCollection
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
		
		#endregion // Methods

	}
}
