//
// System.Data.DataColumn.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Chris Podurgiel
// (C) Ximian, Inc 2002
//


using System;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Summary description for DataColumn.
	/// </summary>
	public class DataColumn : MarshalByValueComponent
	{		
		#region Fields

		private bool allowDBNull = true;
		private bool autoIncrement = false;
		private long autoIncrementSeed = 0;
		private long autoIncrementStep = 1;
		private string caption = null;
		private MappingType columnMapping = MappingType.Element;
		private string columnName = null;
		private Type dataType = null;
		private object defaultValue = null;
		private string expression = null;
		private PropertyCollection extendedProperties = null;
		private int maxLength = -1;
		private string nameSpace = null;
		private int ordinal = -1;
		private string prefix = null;
		private bool readOnly = false;
		private DataTable _table = null;
		private bool unique = false;

		#endregion // Fields

		#region Constructors

		public DataColumn()
		{
		}

		public DataColumn(string columnName): this()
		{
			ColumnName = columnName;
		}

		public DataColumn(string columnName, Type dataType): this(columnName)
		{
			if(dataType == null) {
				throw new ArgumentNullException();
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
		
		public bool AllowDBNull
		{
			get {
				return allowDBNull;
			}
			set {
				allowDBNull = value;
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
		public bool AutoIncrement
		{
			get {
				return autoIncrement;
			}
			set {
				autoIncrement = value;
				if(autoIncrement == true)
				{
					if(Expression != null)
					{
						throw new Exception();
					}
					if(Type.GetTypeCode(dataType) != TypeCode.Int16 && 
					   Type.GetTypeCode(dataType) != TypeCode.Int32 && 
					   Type.GetTypeCode(dataType) != TypeCode.Int64)
					{
						Int32 dtInt = new Int32();
						dataType = dtInt.GetType();
					}
				}
			}
		}

		public long AutoIncrementSeed
		{
			get {
				return autoIncrementSeed;
			}
			set {
				autoIncrementSeed = value;
			}
		}

		public long AutoIncrementStep
		{
			get {
				return autoIncrementStep;
			}
			set {
				autoIncrementStep = value;
			}
		}

		public string Caption 
		{
			get {
				if(caption == null)
					return columnName;
				else
					return caption;
			}
			set {
				caption = value;
			}
		}

		public virtual MappingType ColumnMapping
		{
			get {
				return columnMapping;
			}
			set {
				columnMapping = value;
			}
		}

		public string ColumnName
		{
			get {
				return columnName;
			}
			set {
				columnName = value;
			}
		}

		public Type DataType
		{
			get {
				return dataType;
			}
			set {
				if(AutoIncrement == true && 
				   Type.GetTypeCode(value) != TypeCode.Int32)
				{
					throw new Exception();
				}
				dataType = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>When AutoIncrement is set to true, there can be no default value.</remarks>
		public object DefaultValue
		{
			get {
				return defaultValue;
			}
			set {
				defaultValue = value;
			}
		}

		public string Expression
		{
			get {
				return expression;
			}
			set {
				expression = value;
			}
		}

		public PropertyCollection ExtendedProperties
		{
			get {
				return extendedProperties;
			}
		}

		public int MaxLength
		{
			get {
				return maxLength;
			}
			set {
				maxLength = value;
			}
		}

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
		public int Ordinal
		{
			get {
				return ordinal;
			}
		}

		public string Prefix
		{
			get {
				return prefix;
			}
			set {
				prefix = value;
			}
		}

		public bool ReadOnly
		{
			get {
				return readOnly;
			}
			set {
				readOnly = value;
			}
		}
	
		public DataTable Table
		{
			get {
				return _table;
			}
		}

		public bool Unique
		{
			get {
				return unique;
			}
			set {
				unique = value;
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected internal void CheckNotAllowNull() {
		}

		[MonoTODO]
		protected void CheckUnique() {
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
			
			return columnName;
		}

		[MonoTODO]
		internal void SetTable(DataTable table) {
			_table = table; 
			// FIXME: this will get called by DataTable 
			// and DataColumnCollection
		}

		#endregion // Methods

	}
}
