//
// System.Data.DataTable.cs
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
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
	public class DataTable : MarshalByValueComponent, IListSource,
		ISupportInitialize, ISerializable	
	{
		internal DataSet dataSet;   
		
		private bool _caseSensitive;
		private DataColumnCollection _columnCollection;
		private ConstraintCollection _constraintCollection;
		private DataView _defaultView;

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
		private DataRowCollection _rows;
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
			_columnCollection = new DataColumnCollection(this);
			_constraintCollection = new ConstraintCollection(); 
			_extendedProperties = new PropertyCollection();
			_tableName = "";
			_nameSpace = null;
			_caseSensitive = false;  	//default value
			_displayExpression = null;
			_primaryKey = null;
			_site = null;
			_rows = new DataRowCollection (this);
			_locale = CultureInfo.CurrentCulture;

			//LAMESPEC: spec says 25 impl does 50
			_minimumCapacity = 50;
			
			// FIXME: temporaily commented DataTableRelationCollection
			// _childRelations = new DataTableRelationCollection();
			// _parentRelations = new DataTableRelationCollection();

		
			_defaultView = new DataView(this);
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
		
		[MonoTODO]
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
		[MonoTODO]	
		public DataRelationCollection ChildRelations
		{
			get {
				// FIXME: temporarily commented to compile
				// return (DataRelationCollection)_childRelations;
				
				//We are going to have to Inherit a class from 
				//DataRelationCollection because DRC is abstract
				
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

		

		/// <summary>
		/// Gets a customized view of the table which may 
		/// include a filtered view, or a cursor position.
		/// </summary>
		[MonoTODO]	
		public DataView DefaultView
		{
			get
			{
				return _defaultView;
			}
		}
		

		/// <summary>
		/// Gets or sets the expression that will return 
		/// a value used to represent this table in the user interface.
		/// </summary>
		
		public string DisplayExpression 
		{
			get {
				return "" + _displayExpression;
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
		/// any of the_rows in any of the tables of the DataSet to 
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
				return "" + _nameSpace;
			}
			set {
				_nameSpace = value;
			}
		}

		/// <summary>
		/// Gets the collection of parent relations for 
		/// this DataTable.
		/// </summary>
		[MonoTODO]
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
				return "" + _prefix;
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
				UniqueConstraint uc = UniqueConstraint.GetPrimaryKeyConstraint( Constraints);
				if (null == uc) return new DataColumn[] {};
				return uc.Columns;
			}
			set {

				//YUK: msft removes a previous unique constraint if it is flagged as a pk  
				//when a new pk is set 

				//clear Primary Key if value == null
				if (null == value)
				{
					UniqueConstraint.SetAsPrimaryKey(this.Constraints, null);
					return;
				}
			

				//Does constraint exist for these columns
				UniqueConstraint uc = UniqueConstraint.GetUniqueConstraintForColumnSet(
						this.Constraints, (DataColumn[]) value);

				//if constraint doesn't exist for columns
				//create new unique primary key constraint
				if (null == uc)
				{
					uc = new UniqueConstraint( (DataColumn[]) value, true);
				}
				else //set existing constraint as the new primary key
				{
					UniqueConstraint.SetAsPrimaryKey(this.Constraints, uc);
				}
				
			}
		}

		/// <summary>
		/// Gets the collection of_rows that belong to this table.
		/// </summary>
		
		public DataRowCollection Rows
		{
			get { return _rows; }
		}

		/// <summary>
		/// Gets or sets an System.ComponentModel.ISite 
		/// for the DataTable.
		/// </summary>
		
		public override ISite Site
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
				return "" + _tableName;
			}
			set {
				_tableName = value;
			}
		}
		
		bool IListSource.ContainsListCollection
		{
			get {
				// the collection is a DataView
				return false;
			}
		}

		/// <summary>
		/// Commits all the changes made to this table since the 
		/// last time AcceptChanges was called.
		/// </summary>
		
		public void AcceptChanges()
		{

			//FIXME: Do we need to validate anything here or
			//try to catch any errors to deal with them?

			foreach(DataRow myRow in _rows)
			{
				myRow.AcceptChanges();
			}

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
			_rows.Clear ();
		}

		/// <summary>
		/// Clones the structure of the DataTable, including
		///  all DataTable schemas and constraints.
		/// </summary>
		
		[MonoTODO]
		public virtual DataTable Clone()
		{
			//FIXME:
			return this; //Don't know if this is correct
		}

		/// <summary>
		/// Computes the given expression on the current_rows that 
		/// pass the filter criteria.
		/// </summary>
		
		[MonoTODO]
		public object Compute(string expression, string filter)
		{
			//FIXME: //Do a real compute
			object obj = "a";
			return obj;
		}

		/// <summary>
		/// Copies both the structure and data for this DataTable.
		/// </summary>
		[MonoTODO]	
		public DataTable Copy()
		{
			//FIXME: Do a real copy
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
		[MonoTODO]
		public DataTable GetChanges()
		{
			//TODO:
			return this;
		}

		/// <summary>
		/// Gets a copy of the DataTable containing all 
		/// changes made to it since it was last loaded, or 
		/// since AcceptChanges was called, filtered by DataRowState.
		/// </summary>
		[MonoTODO]	
		public DataTable GetChanges(DataRowState rowStates)
		{
			//TODO:
			return this;
		}

		/// <summary>
		/// Gets an array of DataRow objects that contain errors.
		/// </summary>
		
		[MonoTODO]
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
		/// 
		/// Used for Data Binding between System.Web.UI. controls 
		/// like a DataGrid
		/// or
		/// System.Windows.Forms controls like a DataGrid
		/// </summary>
		IList IListSource.GetList()
		{
			IList list = (IList) _defaultView;
			return list;
		}
				
		/// <summary>
		/// Copies a DataRow into a DataTable, preserving any 
		/// property settings, as well as original and current values.
		/// </summary>
		[MonoTODO]
		public void ImportRow(DataRow row)
		{
		}

		/// <summary>
		/// This member supports the .NET Framework infrastructure
		///  and is not intended to be used directly from your code.
		/// </summary>
		
		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}

		/// <summary>
		/// Finds and updates a specific row. If no matching row
		///  is found, a new row is created using the given values.
		/// </summary>
		[MonoTODO]
		public DataRow LoadDataRow(object[] values, bool fAcceptChanges)
		{
			DataRow row = null;
			if (PrimaryKey.Length == 0) {
				row = Rows.Add (values);
				if (fAcceptChanges)
					row.AcceptChanges ();
			}
			else
				throw new NotImplementedException ();
			return row;
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
		[MonoTODO]
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
		/// Rolls back all changes that have been made to the 
		/// table since it was loaded, or the last time AcceptChanges
		///  was called.
		/// </summary>
		
		[MonoTODO]
		public void RejectChanges()
		{
		}

		/// <summary>
		/// Resets the DataTable to its original state.
		/// </summary>
		
		[MonoTODO]
		public virtual void Reset()
		{
		}

		/// <summary>
		/// Gets an array of all DataRow objects.
		/// </summary>
		
		[MonoTODO]
		public DataRow[] Select()
		{
			//FIXME:
			DataRow[] dataRows = {null};
			return dataRows;
		}

		/// <summary>
		/// Gets an array of all DataRow objects that match 
		/// the filter criteria in order of primary key (or 
		/// lacking one, order of addition.)
		/// </summary>
		
		[MonoTODO]
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
		[MonoTODO]
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
		[MonoTODO]
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
			//LAMESPEC: spec says concat the two. impl puts a 
			//plus sign infront of DisplayExpression
			return TableName + " " + DisplayExpression;
		}

		
		#region Events /////////////////
		
		/// <summary>
		/// Raises the ColumnChanged event.
		/// </summary>
		protected virtual void OnColumnChanged(DataColumnChangeEventArgs e)
		{
			if (null != ColumnChanged)
			{
				ColumnChanged(this, e);
			}
		}

		/// <summary>
		/// Raises the ColumnChanging event.
		/// </summary>
		protected virtual void OnColumnChanging(DataColumnChangeEventArgs e)
		{
			if (null != ColumnChanging)
			{
				ColumnChanging(this, e);
			}
		}

		/// <summary>
		/// Raises the PropertyChanging event.
		/// </summary>
		[MonoTODO]
		protected internal virtual void OnPropertyChanging(PropertyChangedEventArgs pcevent)
		{
//			if (null != PropertyChanging)
//			{
//				PropertyChanging(this, e);
//			}
		}

		/// <summary>
		/// Notifies the DataTable that a DataColumn is being removed.
		/// </summary>
		[MonoTODO]
		protected internal virtual void OnRemoveColumn(DataColumn column)
		{
//			if (null != RemoveColumn)
//			{
//				RemoveColumn(this, e);
//			}
		}

		/// <summary>
		/// Raises the RowChanged event.
		/// </summary>
		
		protected virtual void OnRowChanged(DataRowChangeEventArgs e)
		{
			if (null != RowChanged)
			{
				RowChanged(this, e);
			}
		}

		/// <summary>
		/// Raises the RowChanging event.
		/// </summary>
		
		protected virtual void OnRowChanging(DataRowChangeEventArgs e)
		{
			if (null != RowChanging)
			{
				RowChanging(this, e);
			}
		}

		/// <summary>
		/// Raises the RowDeleted event.
		/// </summary>
		protected virtual void OnRowDeleted(DataRowChangeEventArgs e)
		{
			if (null != RowDeleted)
			{
				RowDeleted(this, e);
			}
		}

		/// <summary>
		/// Raises the RowDeleting event.
		/// </summary>
		protected virtual void OnRowDeleting(DataRowChangeEventArgs e)
		{
			if (null != RowDeleting)
			{
				RowDeleting(this, e);
			}
		}

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
		
		#endregion //Events
	}

}
