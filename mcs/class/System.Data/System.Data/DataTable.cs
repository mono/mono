//
// System.Data.DataTable.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Daniel Morgan <danmorg@sc.rr.com>
//   Rodrigo Moya <rodrigo@ximian.com>
//
// (C) Chris Podurgiel
// (C) Ximian, Inc 2002
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data
{
	/// <summary>
	/// Represents one table of in-memory data.
	/// </summary>
	[Serializable]
		public class DataTable : ISerializable
                //MarshalByValueComponent, IListSource, ISupportInitialize
	{
		
		private bool _caseSensitive;
		private DataColumnCollection _columnCollection;
		private ConstraintCollection _constraintCollection;
		// FIXME: temporarily commented

		internal DataSet dataSet;   

		// private DataView _defaultView;
		private string _displayExpression;
		private PropertyCollection _extendedProperties;
		private bool _hasErrors;
		private CultureInfo _locale;
		private int _minimumCapacity;
		private string _nameSpace;
		// FIXME: temporarily commented
		// private DataTableRelationCollection _childRelations; 
		// private DataTableRelationCollection _parentRelations;
		private string _prefix;
		private DataColumn[] _primaryKey;
		private DataRowCollection rows;
		private ISite _site;
		private string _tableName;
		private bool _containsListCollection;
		private string _encodedTableName;
		
		/// <summary>
		/// Initializes a new instance of the DataTable class with no arguments.
		/// </summary>
		
		public DataTable()
		{
			dataSet = null;
			// _defaultView = null; // FIXME: temporarily commented
			_columnCollection = new DataColumnCollection(this);
			_constraintCollection = new ConstraintCollection(); 
			_extendedProperties = null;
			_tableName = "";
			_nameSpace = null;
			_caseSensitive = false;
			_displayExpression = null;
			_primaryKey = null;
			rows = new DataRowCollection (this);
			
			// FIXME: temporaily commented DataTableRelationCollection
			// _childRelations = new DataTableRelationCollection();
			// _parentRelations = new DataTableRelationCollection();

			//_nextRowID = 1;
			//_elementColumnCount = 0;
			//_caseSensitiveAmbient = true;
			//_culture = null; // _locale??
			//_compareFlags = 25; // why 25??
			//_fNestedInDataset = true; // what?
			//_encodedTableName = null; //??
			//_xmlText = null; //??
			//_colUnique = null; //??
			//_textOnly = false; //??
			//repeatableElement = false; //??
			//zeroIntegers[]
			//zeroColumns[]
			//primaryIndex[]
			//delayedSetPrimaryKey = null; //??
		}

		/// <summary>
		/// Intitalizes a new instance of the DataTable class with the specified table name.
		/// </summary>
		
		public DataTable(string tableName) : this ()
		{
			_tableName = tableName;
		}

		/// <summary>
		/// Initializes a new instance of the DataTable class with the SerializationInfo and the StreamingContext.
		/// </summary>
		
		protected DataTable(SerializationInfo info, StreamingContext context)
			: this ()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <summary>
		/// Indicates whether string comparisons within the table are case-sensitive.
		/// </summary>
		
		public bool CaseSensitive 
		{
			get {
				return _caseSensitive;
			}
			set {
				_caseSensitive = value;
			}
		}


		/// <summary>
		/// Gets the collection of child relations for this DataTable.
		/// </summary>
		
		public DataRelationCollection ChildRelations
		{
			get {
				// FIXME: temporarily commented to compile
				// return (DataRelationCollection)_childRelations;
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Gets the collection of columns that belong to this table.
		/// </summary>
		
		public DataColumnCollection Columns
		{
			get {
				return _columnCollection;
			}
		}

		/// <summary>
		/// Gets the collection of constraints maintained by this table.
		/// </summary>
		
		public ConstraintCollection Constraints
		{
			get {
				return _constraintCollection;
			}
		}

		/// <summary>
		/// Gets the DataSet that this table belongs to.
		/// </summary>
		public DataSet DataSet {
			get { return dataSet; }
		}

		/*

		/// <summary>
		/// Gets a customized view of the table which may 
		/// include a filtered view, or a cursor position.
		/// </summary>
		
		public DataView DefaultView
		{
			get
			{
				return _defaultView;
			}
		}
		*/

		/// <summary>
		/// Gets or sets the expression that will return 
		/// a value used to represent this table in the user interface.
		/// </summary>
		
		public string DisplayExpression 
		{
			get {
				return _displayExpression;
			}
			set {
				_displayExpression = value;
			}
		}

		/// <summary>
		/// Gets the collection of customized user information.
		/// </summary>
		public PropertyCollection ExtendedProperties
		{
			get {
				return _extendedProperties;
			}
		}

		/// <summary>
		/// Gets a value indicating whether there are errors in 
		/// any of the rows in any of the tables of the DataSet to 
		/// which the table belongs.
		/// </summary>
		public bool HasErrors
		{
			get {
				return _hasErrors;
			}
		}

		/// <summary>
		/// Gets or sets the locale information used to 
		/// compare strings within the table.
		/// </summary>
		public CultureInfo Locale
		{
			get {
				return _locale;
			}
			set {
				_locale = value;
			}
		}

		/// <summary>
		/// Gets or sets the initial starting size for this table.
		/// </summary>
		public int MinimumCapacity
		{
			get {
				return _minimumCapacity;
			}
			set {
				_minimumCapacity = value;
			}
		}

		/// <summary>
		/// Gets or sets the namespace for the XML represenation 
		/// of the data stored in the DataTable.
		/// </summary>
		public string Namespace
		{
			get {
				return _nameSpace;
			}
			set {
				_nameSpace = value;
			}
		}

		/// <summary>
		/// Gets the collection of parent relations for 
		/// this DataTable.
		/// </summary>
		public DataRelationCollection ParentRelations
		{
			get {	
				// FIXME: temporarily commented to compile
				// return _parentRelations;
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Gets or sets the namespace for the XML represenation
		///  of the data stored in the DataTable.
		/// </summary>
		public string Prefix
		{
			get {
				return _prefix;
			}
			set {
				_prefix = value;
			}
		}

		/// <summary>
		/// Gets or sets an array of columns that function as 
		/// primary keys for the data table.
		/// </summary>
		public DataColumn[] PrimaryKey
		{
			get {
				return _primaryKey;
			}
			set {
				_primaryKey = value;
			}
		}

		/// <summary>
		/// Gets the collection of rows that belong to this table.
		/// </summary>
		
		public DataRowCollection Rows
		{
			get { return rows; }
		}

		/// <summary>
		/// Gets or sets an System.ComponentModel.ISite 
		/// for the DataTable.
		/// </summary>
		
		public virtual ISite Site
		{
			get {
				return _site;
			}
			set {
				_site = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the the DataTable.
		/// </summary>
		
		public string TableName
		{
			get {
				return _tableName;
			}
			set {
				_tableName = value;
			}
		}

		/* FIXME: implement IListSource
		public bool IListSource.ContainsListCollection
		{
			get {
				return _containsListCollection;
			}
		}
		*/

		/// <summary>
		/// Commits all the changes made to this table since the 
		/// last time AcceptChanges was called.
		/// </summary>
		
		public void AcceptChanges()
		{
		}

		/// <summary>
		/// Begins the initialization of a DataTable that is used 
		/// on a form or used by another component. The initialization
		/// occurs at runtime.
		/// </summary>
		
		public void BeginInit()
		{
		}

		/// <summary>
		/// Turns off notifications, index maintenance, and 
		/// constraints while loading data.
		/// </summary>
		
		public void BeginLoadData()
		{
		}

		/// <summary>
		/// Clears the DataTable of all data.
		/// </summary>
		
		public void Clear()
		{
			rows.Clear ();
		}

		/// <summary>
		/// Clones the structure of the DataTable, including
		///  all DataTable schemas and constraints.
		/// </summary>
		
		public virtual DataTable Clone()
		{
			return this;
		}

		/// <summary>
		/// Computes the given expression on the current rows that 
		/// pass the filter criteria.
		/// </summary>
		
		public object Compute(string expression, string filter)
		{
			object obj = "a";
			return obj;
		}

		/// <summary>
		/// Copies both the structure and data for this DataTable.
		/// </summary>
		
		public DataTable Copy()
		{
			return this;
		}

		/// <summary>
		/// Ends the initialization of a DataTable that is used 
		/// on a form or used by another component. The 
		/// initialization occurs at runtime.
		/// </summary>
		
		public void EndInit()
		{
		}

		/// <summary>
		/// Turns on notifications, index maintenance, and 
		/// constraints after loading data.
		/// </summary>
		
		public void EndLoadData()
		{
		}

		/// <summary>
		/// Gets a copy of the DataTable that contains all
		///  changes made to it since it was loaded or 
		///  AcceptChanges was last called.
		/// </summary>
		
		public DataTable GetChanges()
		{
			return this;
		}

		/// <summary>
		/// Gets a copy of the DataTable containing all 
		/// changes made to it since it was last loaded, or 
		/// since AcceptChanges was called, filtered by DataRowState.
		/// </summary>
		
		public DataTable GetChanges(DataRowState rowStates)
		{
			return this;
		}

		/// <summary>
		/// Gets an array of DataRow objects that contain errors.
		/// </summary>
		
		public DataRow[] GetErrors()
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// This member supports the .NET Framework infrastructure
		/// and is not intended to be used directly from your code.
		/// </summary>
		
		//protected virtual Type GetRowType()
		//{	
		//}

		/// <summary>
		/// This member supports the .NET Framework infrastructure 
		/// and is not intended to be used directly from your code.
		/// </summary>
		
		/* FIXME: implement IListSource
		public IList IListSource.GetList()
		{
			IList list = null;
			return list;
		}
		*/
		
		/// <summary>
		/// Copies a DataRow into a DataTable, preserving any 
		/// property settings, as well as original and current values.
		/// </summary>
		
		public void ImportRow(DataRow row)
		{
		}

		/// <summary>
		/// This member supports the .NET Framework infrastructure
		///  and is not intended to be used directly from your code.
		/// </summary>
		
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}

		/// <summary>
		/// Finds and updates a specific row. If no matching row
		///  is found, a new row is created using the given values.
		/// </summary>
		public DataRow LoadDataRow(object[] values, bool fAcceptChanges)
		{
			DataRow dataRow = null;
			return dataRow;
		}

		/// <summary>
		/// Creates a new DataRow with the same schema as the table.
		/// </summary>
		public DataRow NewRow()
		{
			return this.NewRowFromBuilder (new DataRowBuilder (this, 0, 0));
		}

		/// <summary>
		/// This member supports the .NET Framework infrastructure
		///  and is not intended to be used directly from your code.
		/// </summary>
		protected internal DataRow[] NewRowArray(int size)
		{
			DataRow[] dataRows = {null};
			return dataRows;
		}

		/// <summary>
		/// Creates a new row from an existing row.
		/// </summary>
		
		protected virtual DataRow NewRowFromBuilder(DataRowBuilder builder)
		{
			return new DataRow (builder);
		}
	

		/// <summary>
		/// Raises the ColumnChanged event.
		/// </summary>
		protected virtual void OnColumnChanged(DataColumnChangeEventArgs e)
		{
		}

		/// <summary>
		/// Raises the ColumnChanging event.
		/// </summary>
		
		protected virtual void OnColumnChanging(DataColumnChangeEventArgs e)
		{
		}

		/// <summary>
		/// Raises the PropertyChanging event.
		/// </summary>
		
		protected internal virtual void OnPropertyChanging(PropertyChangedEventArgs pcevent)
		{
		}

		/// <summary>
		/// Notifies the DataTable that a DataColumn is being removed.
		/// </summary>
		
		protected internal virtual void OnRemoveColumn(DataColumn column)
		{
		}

		/// <summary>
		/// Raises the RowChanged event.
		/// </summary>
		
		protected virtual void OnRowChanged(DataRowChangeEventArgs e)
		{
		}

		/// <summary>
		/// Raises the RowChanging event.
		/// </summary>
		
		protected virtual void OnRowChanging(DataRowChangeEventArgs e)
		{
		}

		/// <summary>
		/// Raises the RowDeleted event.
		/// </summary>
		
		protected virtual void OnRowDeleted(DataRowChangeEventArgs e)
		{
		}

		/// <summary>
		/// Raises the RowDeleting event.
		/// </summary>
		
		protected virtual void OnRowDeleting(DataRowChangeEventArgs e)
		{
		}

		/// <summary>
		/// Rolls back all changes that have been made to the 
		/// table since it was loaded, or the last time AcceptChanges
		///  was called.
		/// </summary>
		
		public void RejectChanges()
		{
		}

		/// <summary>
		/// Resets the DataTable to its original state.
		/// </summary>
		
		public virtual void Reset()
		{
		}

		/// <summary>
		/// Gets an array of all DataRow objects.
		/// </summary>
		
		public DataRow[] Select()
		{
			DataRow[] dataRows = {null};
			return dataRows;
		}

		/// <summary>
		/// Gets an array of all DataRow objects that match 
		/// the filter criteria in order of primary key (or 
		/// lacking one, order of addition.)
		/// </summary>
		
		public DataRow[] Select(string filterExpression)
		{
			DataRow[] dataRows = {null};
			return dataRows;
		}

		/// <summary>
		/// Gets an array of all DataRow objects that 
		/// match the filter criteria, in the the 
		/// specified sort order.
		/// </summary>
		
		public DataRow[] Select(string filterExpression, string sort)
		{
			DataRow[] dataRows = {null};
			return dataRows;
		}

		/// <summary>
		/// Gets an array of all DataRow objects that match
		/// the filter in the order of the sort, that match 
		/// the specified state.
		/// </summary>
		
		public DataRow[] Select(string filterExpression, string sort, DataViewRowState recordStates)
		{
			DataRow[] dataRows = {null};
			return dataRows;
		}

		/// <summary>
		/// Gets the TableName and DisplayExpression, if 
		/// there is one as a concatenated string.
		/// </summary>
		
		public override string ToString()
		{
			return "";
		}

		/* FIXME: temporarily commented - so we can get a
		 *        a simple forward read only result set
		/// <summary>
		/// Occurs when after a value has been changed for 
		/// the specified DataColumn in a DataRow.
		/// </summary>
		
		public event DataColumnChangeEventHandler ColumnChanged;

		/// <summary>
		/// Occurs when a value is being changed for the specified 
		/// DataColumn in a DataRow.
		/// </summary>
		
		public event DataColumnChangeEventHandler ColumnChanging;

		/// <summary>
		/// Occurs after a DataRow has been changed successfully.
		/// </summary>
		
		public event DataRowChangeEventHandler RowChanged;

		/// <summary>
		/// Occurs when a DataRow is changing.
		/// </summary>
		
		public event DataRowChangeEventHandler RowChanging;

		/// <summary>
		/// Occurs after a row in the table has been deleted.
		/// </summary>
		
		public event DataRowChangeEventHandler RowDeleted;

		/// <summary>
		/// Occurs before a row in the table is about to be deleted.
		/// </summary>
		
		public event DataRowChangeEventHandler RowDeleting;
		*/
	}
}
