//
// System.Data.DataTable.cs
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Daniel Morgan <danmorg@sc.rr.com>
//   Rodrigo Moya <rodrigo@ximian.com>
//   Tim Coleman (tim@timcoleman.com)
//   Ville Palo <vi64pa@koti.soon.fi>
//
// (C) Chris Podurgiel
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Daniel Morgan, 2002-2003
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {
	//[Designer]
	[ToolboxItem (false)]
	[DefaultEvent ("RowChanging")]
	[DefaultProperty ("TableName")]
	[DesignTimeVisible (false)]
	[Serializable]
	public class DataTable : MarshalByValueComponent, IListSource, ISupportInitialize, ISerializable 
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
		private DataRelationCollection _childRelations; 
		private DataRelationCollection _parentRelations;
		private string _prefix;
		private DataColumn[] _primaryKey;
		private DataRowCollection _rows;
		private ISite _site;
		private string _tableName;
		private bool _containsListCollection;
		private string _encodedTableName;
		internal bool _duringDataLoad;
		private bool dataSetPrevEnforceConstraints;


		
		// If CaseSensitive property is changed once it does not anymore follow owner DataSet's 
		// CaseSensitive property. So when you lost you virginity it's gone for ever
		private bool _virginCaseSensitive = true;

		/// <summary>
		/// Initializes a new instance of the DataTable class with no arguments.
		/// </summary>
		
		public DataTable () 
		{
			dataSet = null;
			_columnCollection = new DataColumnCollection(this);
			_constraintCollection = new ConstraintCollection(this); 
			_extendedProperties = new PropertyCollection();
			_tableName = "";
			_nameSpace = null;
			_caseSensitive = false;  	//default value
			_displayExpression = null;
			_primaryKey = null;
			_site = null;
			_rows = new DataRowCollection (this);
			
			//LAMESPEC: spec says 25 impl does 50
			_minimumCapacity = 50;
			
			_childRelations = new DataRelationCollection.DataTableRelationCollection (this);
			_parentRelations = new DataRelationCollection.DataTableRelationCollection (this);

		
			_defaultView = new DataView(this);
		}

		/// <summary>
		/// Intitalizes a new instance of the DataTable class with the specified table name.
		/// </summary>
		public DataTable (string tableName) : this () 
		{
			_tableName = tableName;
		}

		/// <summary>
		/// Initializes a new instance of the DataTable class with the SerializationInfo and the StreamingContext.
		/// </summary>
		[MonoTODO]
		protected DataTable (SerializationInfo info, StreamingContext context)
			: this () 
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <summary>
		/// Indicates whether string comparisons within the table are case-sensitive.
		/// </summary>
		[DataSysDescription ("Indicates whether comparing strings within the table is case sensitive.")]	
		public bool CaseSensitive {
			get { return _caseSensitive; }
			set { 
				_virginCaseSensitive = false;
				_caseSensitive = value; 
			}
		}

		internal bool VirginCaseSensitive {
			get { return _virginCaseSensitive; }
			set { _virginCaseSensitive = value; }
		}

		internal void ChangedDataColumn (DataRow dr, DataColumn dc, object pv) 
		{
			DataColumnChangeEventArgs e = new DataColumnChangeEventArgs (dr, dc, pv);
			OnColumnChanged(e);
		}

		internal void ChangingDataColumn (DataRow dr, DataColumn dc, object pv) 
		{
			DataColumnChangeEventArgs e = new DataColumnChangeEventArgs (dr, dc, pv);
			OnColumnChanging (e);
		}

		internal void DeletedDataRow (DataRow dr, DataRowAction action) 
		{
			DataRowChangeEventArgs e = new DataRowChangeEventArgs (dr, action);
			OnRowDeleted (e);
		}

		internal void DeletingDataRow (DataRow dr, DataRowAction action) 
		{
			DataRowChangeEventArgs e = new DataRowChangeEventArgs (dr, action);
			OnRowDeleting(e);
		}

		internal void ChangedDataRow (DataRow dr, DataRowAction action) 
		{
			DataRowChangeEventArgs e = new DataRowChangeEventArgs (dr, action);
			OnRowChanged (e);
		}

		internal void ChangingDataRow (DataRow dr, DataRowAction action) 
		{
			DataRowChangeEventArgs e = new DataRowChangeEventArgs (dr, action);
			OnRowChanging (e);
		}

		/// <summary>
		/// Gets the collection of child relations for this DataTable.
		/// </summary>
		[Browsable (false)]
		[DataSysDescription ("Returns the child relations for this table.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataRelationCollection ChildRelations {
			get {
				return _childRelations;
			}
		}

		/// <summary>
		/// Gets the collection of columns that belong to this table.
		/// </summary>
		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds the columns for this table.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataColumnCollection Columns {
			get { return _columnCollection; }
		}

		/// <summary>
		/// Gets the collection of constraints maintained by this table.
		/// </summary>
		[DataCategory ("Data")]	
		[DataSysDescription ("The collection that holds the constraints for this table.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public ConstraintCollection Constraints {
			get { return _constraintCollection; }
		}

		/// <summary>
		/// Gets the DataSet that this table belongs to.
		/// </summary>
		[Browsable (false)]
		[DataSysDescription ("Indicates the DataSet to which this table belongs.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataSet DataSet {
			get { return dataSet; }
		}

		

		/// <summary>
		/// Gets a customized view of the table which may 
		/// include a filtered view, or a cursor position.
		/// </summary>
		[MonoTODO]	
		[Browsable (false)]
		[DataSysDescription ("This is the default DataView for the table.")]
		public DataView DefaultView {
			get { return _defaultView; }
		}
		

		/// <summary>
		/// Gets or sets the expression that will return 
		/// a value used to represent this table in the user interface.
		/// </summary>
		[DataCategory ("Data")]
		[DataSysDescription ("The expression used to compute the data-bound value of this row.")]	
		[DefaultValue ("")]
		public string DisplayExpression {
			get { return "" + _displayExpression; }
			set { _displayExpression = value; }
		}

		/// <summary>
		/// Gets the collection of customized user information.
		/// </summary>
		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds custom user information.")]
		public PropertyCollection ExtendedProperties {
			get { return _extendedProperties; }
		}

		/// <summary>
		/// Gets a value indicating whether there are errors in 
		/// any of the_rows in any of the tables of the DataSet to 
		/// which the table belongs.
		/// </summary>
		[Browsable (false)]
		[DataSysDescription ("Returns whether the table has errors.")]
		public bool HasErrors {
			get { 
				// we can not use the _hasError flag because we do not know when to turn it off!
				for (int i = 0; i < _rows.Count; i++)
				{
					if (_rows[i].HasErrors)
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets or sets the locale information used to 
		/// compare strings within the table.
		/// </summary>
		[DataSysDescription ("Indicates a locale under which to compare strings within the table.")]
		public CultureInfo Locale {
			get { 
				// if the locale is null, we check for the DataSet locale
				// and if the DataSet is null we return the current culture.
				// this way if DataSet locale is changed, only if there is no locale for 
				// the DataTable it influece the Locale get;
				if (_locale != null)
					return _locale;
				if (DataSet != null)
					return DataSet.Locale;
				return CultureInfo.CurrentCulture;
			}
			set { 
				if (_locale == null || !_locale.Equals(value))
					_locale = value; 
			}
		}

		/// <summary>
		/// Gets or sets the initial starting size for this table.
		/// </summary>
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates an initial starting size for this table.")]
		[DefaultValue (50)]
		public int MinimumCapacity {
			get { return _minimumCapacity; }
			set { _minimumCapacity = value; }
		}

		/// <summary>
		/// Gets or sets the namespace for the XML represenation 
		/// of the data stored in the DataTable.
		/// </summary>
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the XML uri namespace for the elements contained in this table.")]
		public string Namespace {
			get { return "" + _nameSpace; }
			set { _nameSpace = value; }
		}

		/// <summary>
		/// Gets the collection of parent relations for 
		/// this DataTable.
		/// </summary>
		[Browsable (false)]
		[DataSysDescription ("Returns the parent relations for this table.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataRelationCollection ParentRelations {
			get {	
				return _parentRelations;
			}
		}

		/// <summary>
		/// Gets or sets the namespace for the XML represenation
		///  of the data stored in the DataTable.
		/// </summary>
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the Prefix of the namespace used for this table in XML representation.")]
		[DefaultValue ("")]
		public string Prefix {
			get { return "" + _prefix; }
			set { _prefix = value; }
		}

		/// <summary>
		/// Gets or sets an array of columns that function as 
		/// primary keys for the data table.
		/// </summary>
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the column(s) that represent the primary key for this table.")]
		public DataColumn[] PrimaryKey {
			get {
				UniqueConstraint uc = UniqueConstraint.GetPrimaryKeyConstraint( Constraints);
				if (null == uc) return new DataColumn[] {};
				return uc.Columns;
			}
			set {

				//YUK: msft removes a previous unique constraint if it is flagged as a pk  
				//when a new pk is set 
				
				//clear Primary Key if value == null
				if (null == value) {
					
					RemoveUniqueConstraints ();
					return;
				}

				//Does constraint exist for these columns
				UniqueConstraint uc = UniqueConstraint.GetUniqueConstraintForColumnSet(
					this.Constraints, (DataColumn[]) value);

				//if constraint doesn't exist for columns
				//create new unique primary key constraint
				if (null == uc) {

					RemoveUniqueConstraints ();						
					
					foreach (DataColumn Col in (DataColumn[]) value) {

						if (Col.Table == null)
							break;

						if (Columns.IndexOf (Col) < 0)
							throw new ArgumentException ("PrimaryKey columns do not belong to this table.");
					}


					uc = new UniqueConstraint( (DataColumn[]) value, true);
					
					Constraints.Add (uc);
				}
				else { //set existing constraint as the new primary key
					UniqueConstraint.SetAsPrimaryKey(this.Constraints, uc);
				}
				
			}
		}

		/// <summary>
		/// Gets the collection of_rows that belong to this table.
		/// </summary>
		[Browsable (false)]
		[DataSysDescription ("Indicates the collection that holds the rows of data for this table.")]	
		public DataRowCollection Rows {
			get { return _rows; }
		}

		/// <summary>
		/// Gets or sets an System.ComponentModel.ISite 
		/// for the DataTable.
		/// </summary>
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ISite Site {
			get { return _site; }
			set { _site = value; }
		}

		/// <summary>
		/// Gets or sets the name of the the DataTable.
		/// </summary>
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the name used to look up this table in the Tables collection of a DataSet.")]
		[DefaultValue ("")]	
		[RefreshProperties (RefreshProperties.All)]
		public string TableName {
			get { return "" + _tableName; }
			set { _tableName = value; }
		}
		
		bool IListSource.ContainsListCollection {
			get {
				// the collection is a DataView
				return false;
			}
		}

		/// <summary>
		/// Commits all the changes made to this table since the 
		/// last time AcceptChanges was called.
		/// </summary>
		public void AcceptChanges () 
		{
			//FIXME: Do we need to validate anything here or
			//try to catch any errors to deal with them?
			
			// we do not use foreach because if one of the rows is in Delete state
			// it will be romeved from Rows and we get an exception.
			DataRow myRow;
			for (int i = 0; i < Rows.Count; )
			{
				myRow = Rows[i];
				myRow.AcceptChanges();

				// if the row state is Detached it meens that it was removed from row list (Rows)
				// so we should not increase 'i'.
				if (myRow.RowState != DataRowState.Detached)
					i++;
			}
		}

		/// <summary>
		/// Begins the initialization of a DataTable that is used 
		/// on a form or used by another component. The initialization
		/// occurs at runtime.
		/// </summary>
		public void BeginInit () 
		{
		}

		/// <summary>
		/// Turns off notifications, index maintenance, and 
		/// constraints while loading data.
		/// </summary>
		[MonoTODO]
		public void BeginLoadData () 
		{
			if (!this._duringDataLoad)
			{
				//duringDataLoad is important to EndLoadData and
				//for not throwing unexpected exceptions.
				this._duringDataLoad = true;
			
				if (this.dataSet != null)
				{
					//Saving old Enforce constraints state for later
					//use in the EndLoadData.
					this.dataSetPrevEnforceConstraints = this.dataSet.EnforceConstraints;
					this.dataSet.EnforceConstraints = false;
				}
			}
			return;
		}

		/// <summary>
		/// Clears the DataTable of all data.
		/// </summary>
		public void Clear () {
			// TODO: thow an exception if any rows that 
			//       have enforced child relations 
			//       that would result in child rows being orphaned
			// now we check if any ForeignKeyConstraint is referncing 'table'.
			if (DataSet != null)
			{
				IEnumerator tableEnumerator = DataSet.Tables.GetEnumerator();
			
				// loop on all tables in dataset
				while (tableEnumerator.MoveNext())
				{
					IEnumerator constraintEnumerator = ((DataTable) tableEnumerator.Current).Constraints.GetEnumerator();
					// loop on all constrains in the current table
					while (constraintEnumerator.MoveNext())
					{
						Object o = constraintEnumerator.Current;
						// we only check ForeignKeyConstraint
						if (o is ForeignKeyConstraint)
						{
							ForeignKeyConstraint fc = (ForeignKeyConstraint) o;
							if(fc.RelatedTable == this && fc.Table.Rows.Count > 0)
								throw new InvalidConstraintException("Cannot clear table " + fc.RelatedTable + " because ForeignKeyConstraint " + fc.ConstraintName + " enforces constraints and there are child rows in " + fc.Table);
						}
					}
				}
			}
			
			// TODO: throw a NotSupportedException if the DataTable is part
			//       of a DataSet that is binded to an XmlDataDocument
			
			_rows.Clear ();
		}

		/// <summary>
		/// Clones the structure of the DataTable, including
		///  all DataTable schemas and constraints.
		/// </summary>
		[MonoTODO]
		public virtual DataTable Clone () 
		{
			DataTable Copy = new DataTable ();			
			
			CopyProperties (Copy);
			return Copy;
		}

		/// <summary>
		/// Computes the given expression on the current_rows that 
		/// pass the filter criteria.
		/// </summary>
		[MonoTODO]
		public object Compute (string expression, string filter) 
		{
			// TODO: implement this function based
			//       on Select (expression)
			//
			// expression is an aggregate function
			// filter is an expression used to limit rows

			object obj = null;

			// filter rows
			ExpressionElement Expression = new ExpressionMainElement (filter);
			
			ArrayList List = new ArrayList ();
			foreach (DataRow Row in Rows) {
				
				if (Expression.Test (Row))
					List.Add (Row);
			}
			
			DataRow[] rows = (DataRow [])List.ToArray (typeof (DataRow));

			// TODO: with the filtered rows, execute the aggregate function
			//       mentioned in expression

			return obj;
		}

		/// <summary>
		/// Copies both the structure and data for this DataTable.
		/// </summary>
		[MonoTODO]	
		public DataTable Copy () 
		{
			DataTable Copy = new DataTable ();
			CopyProperties (Copy);

			// we can not simply copy the row values (NewRow [C.ColumnName] = Row [C.ColumnName])
			// because if the row state is deleted we get an exception.
			Copy._duringDataLoad = true;
			foreach (DataRow Row in Rows) {
				DataRow NewRow = Copy.NewRow ();
				Copy.Rows.Add (NewRow);
				Row.CopyValuesToRow(NewRow);
			}
		       	Copy._duringDataLoad = false;		
			return Copy;
		}

		[MonoTODO]
		private void CopyProperties (DataTable Copy) 
		{
					
			Copy.CaseSensitive = CaseSensitive;
			Copy.VirginCaseSensitive = VirginCaseSensitive;

			// Copy.ChildRelations
			// Copy.Constraints
			// Copy.Container
			// Copy.DefaultView
			// Copy.DesignMode
			Copy.DisplayExpression = DisplayExpression;
			// Copy.ExtendedProperties
			Copy.Locale = Locale;
			Copy.MinimumCapacity = MinimumCapacity;
			Copy.Namespace = Namespace;
			// Copy.ParentRelations
			Copy.Prefix = Prefix;
			Copy.Site = Site;
			Copy.TableName = TableName;



			// Copy columns
			foreach (DataColumn Column in Columns) {
				
				Copy.Columns.Add (CopyColumn (Column));	
			}

			CopyConstraints(Copy);
			// add primary key to the copy
			if (PrimaryKey.Length > 0)
			{
				DataColumn[] pColumns = new DataColumn[PrimaryKey.Length];
				for (int i = 0; i < pColumns.Length; i++)
					pColumns[i] = Copy.Columns[PrimaryKey[i].ColumnName];

				Copy.PrimaryKey = pColumns;
			}
		}

		private void CopyConstraints(DataTable copy)
		{
			UniqueConstraint origUc;
			UniqueConstraint copyUc;
			for (int i = 0; i < this.Constraints.Count; i++)
			{
				if (this.Constraints[i] is UniqueConstraint)
				{
					origUc = (UniqueConstraint)this.Constraints[i];
					DataColumn[] columns = new DataColumn[origUc.Columns.Length];
					for (int j = 0; j < columns.Length; j++)
						columns[j] = copy.Columns[origUc.Columns[j].ColumnName];
					
					copyUc = new UniqueConstraint(origUc.ConstraintName, columns, origUc.IsPrimaryKey);
					copy.Constraints.Add(copyUc);
				}
			}
		}
		/// <summary>
		/// Ends the initialization of a DataTable that is used 
		/// on a form or used by another component. The 
		/// initialization occurs at runtime.
		/// </summary>
		[MonoTODO]
		public void EndInit () 
		{

		}

		/// <summary>
		/// Turns on notifications, index maintenance, and 
		/// constraints after loading data.
		/// </summary>
		[MonoTODO]
		public void EndLoadData() 
		{
			int i = 0;
			if (this._duringDataLoad) 
			{
				//Returning from loading mode, raising exceptions as usual
				this._duringDataLoad = false;
				
				if (this.dataSet !=null)
				{
					//Getting back to previous EnforceConstraint state
					this.dataSet.EnforceConstraints = this.dataSetPrevEnforceConstraints;
				}
				for (i=0 ; i<this.Rows.Count ; i++)
				{
					if (this.Rows[i]._nullConstraintViolation )
					{
						throw new ConstraintException ("Failed to enable constraints. One or more rows contain values violating non-null, unique, or foreign-key constraints.");
					}
						
				}

			}

		}

		/// <summary>
		/// Gets a copy of the DataTable that contains all
		///  changes made to it since it was loaded or 
		///  AcceptChanges was last called.
		/// </summary>
		[MonoTODO]
		public DataTable GetChanges() 
		{
			return GetChanges(DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
		}

		/// <summary>
		/// Gets a copy of the DataTable containing all 
		/// changes made to it since it was last loaded, or 
		/// since AcceptChanges was called, filtered by DataRowState.
		/// </summary>
		[MonoTODO]	
		public DataTable GetChanges(DataRowState rowStates) 
		{
			DataTable copyTable = null;

			IEnumerator rowEnumerator = Rows.GetEnumerator();
			while (rowEnumerator.MoveNext()) {
				DataRow row = (DataRow)rowEnumerator.Current;
				if (row.IsRowChanged(rowStates)) {
					if (copyTable == null)
						copyTable = Clone();
					DataRow newRow = copyTable.NewRow();
					copyTable.Rows.Add(newRow);
					row.CopyValuesToRow(newRow);
				}
			}
			 
			return copyTable;
		}

		/// <summary>
		/// Gets an array of DataRow objects that contain errors.
		/// </summary>
		[MonoTODO]
		public DataRow[] GetErrors () 
		{
			ArrayList errors = new ArrayList();
			for (int i = 0; i < _rows.Count; i++)
			{
				if (_rows[i].HasErrors)
					errors.Add(_rows[i]);
			}
			
			return (DataRow[]) errors.ToArray(typeof(DataRow));
		}
	
		/// <summary>
		/// This member is only meant to support Mono's infrastructure 
		/// </summary>
		protected virtual DataTable CreateInstance () 
		{
			return Activator.CreateInstance (this.GetType (), true) as DataTable;
		}

		/// <summary>
		/// This member is only meant to support Mono's infrastructure 
		/// </summary>
		protected virtual Type GetRowType () 
		{
			return typeof (DataRow);
		}

		/// <summary>
		/// This member is only meant to support Mono's infrastructure 
		/// 
		/// Used for Data Binding between System.Web.UI. controls 
		/// like a DataGrid
		/// or
		/// System.Windows.Forms controls like a DataGrid
		/// </summary>
		IList IListSource.GetList () 
		{
			IList list = (IList) _defaultView;
			return list;
		}
				
		/// <summary>
		/// Copies a DataRow into a DataTable, preserving any 
		/// property settings, as well as original and current values.
		/// </summary>
		[MonoTODO]
		public void ImportRow (DataRow row) 
		{
			DataRow newRow = NewRow();
			Rows.Add(newRow);
			row.CopyValuesToRow(newRow);
			
		}

		/// <summary>
		/// This member is only meant to support Mono's infrastructure 		
		/// </summary>
		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) 
		{
		}

		/// <summary>
		/// Finds and updates a specific row. If no matching row
		///  is found, a new row is created using the given values.
		/// </summary>
		[MonoTODO]
		public DataRow LoadDataRow (object[] values, bool fAcceptChanges) 
		{
			DataRow row = null;
			if (PrimaryKey.Length == 0) {
				row = Rows.Add (values);
				if (fAcceptChanges)
					row.AcceptChanges ();
			}
			else {
				bool hasPrimaryValues = true;
				// initiate an array that has the values of the primary keys.
				object[] keyValues = new object[PrimaryKey.Length];
				for (int i = 0; i < keyValues.Length && hasPrimaryValues; i++)
				{
					if(PrimaryKey[i].Ordinal < values.Length)
						keyValues[i] = values[PrimaryKey[i].Ordinal];
					else
						hasPrimaryValues = false;
				}
				
				if (hasPrimaryValues){
					// find the row in the table.
					row = Rows.Find(keyValues);
				}
				
				if (row == null)
					row = Rows.Add (values);
				else
					row.ItemArray = values;
				
				if (fAcceptChanges)
					row.AcceptChanges ();
			}
				
			return row;
		}

		/// <summary>
		/// Creates a new DataRow with the same schema as the table.
		/// </summary>
		public DataRow NewRow () 
		{
			return this.NewRowFromBuilder (new DataRowBuilder (this, 0, 0));
		}

		/// <summary>
		/// This member supports the .NET Framework infrastructure
		///  and is not intended to be used directly from your code.
		/// </summary>
		protected internal DataRow[] NewRowArray (int size) 
		{
			return (DataRow[]) Array.CreateInstance (GetRowType (), size);
		}

		/// <summary>
		/// Creates a new row from an existing row.
		/// </summary>
		protected virtual DataRow NewRowFromBuilder (DataRowBuilder builder) 
		{
			return new DataRow (builder);
		}
	

		/// <summary>
		/// Rolls back all changes that have been made to the 
		/// table since it was loaded, or the last time AcceptChanges
		///  was called.
		/// </summary>
		[MonoTODO]
		public void RejectChanges () 
		{	
			for (int i = _rows.Count - 1; i >= 0; i--) {
				DataRow row = _rows [i];
				if (row.RowState != DataRowState.Unchanged)
					_rows [i].RejectChanges ();
			}
		}

		/// <summary>
		/// Resets the DataTable to its original state.
		/// </summary>		
		[MonoTODO]
		public virtual void Reset () 
		{
			Clear();
			while (ParentRelations.Count > 0)
			{
				if (dataSet.Relations.Contains(ParentRelations[ParentRelations.Count - 1].RelationName))
					dataSet.Relations.Remove(ParentRelations[ParentRelations.Count - 1]);
			}

			while (ChildRelations.Count > 0)
			{
				if (dataSet.Relations.Contains(ChildRelations[ChildRelations.Count - 1].RelationName))
					dataSet.Relations.Remove(ChildRelations[ChildRelations.Count - 1]);
			}
			Constraints.Clear();
			Columns.Clear();
		}

		/// <summary>
		/// Gets an array of all DataRow objects.
		/// </summary>
		public DataRow[] Select () 
		{
			return Select(String.Empty, String.Empty, DataViewRowState.CurrentRows);
		}

		/// <summary>
		/// Gets an array of all DataRow objects that match 
		/// the filter criteria in order of primary key (or 
		/// lacking one, order of addition.)
		/// </summary>
		public DataRow[] Select (string filterExpression) 
		{
			return Select(filterExpression, String.Empty, DataViewRowState.CurrentRows);
		}

		/// <summary>
		/// Gets an array of all DataRow objects that 
		/// match the filter criteria, in the the 
		/// specified sort order.
		/// </summary>
		public DataRow[] Select (string filterExpression, string sort) 
		{
			return Select(filterExpression, sort, DataViewRowState.CurrentRows);
		}

		/// <summary>
		/// Gets an array of all DataRow objects that match
		/// the filter in the order of the sort, that match 
		/// the specified state.
		/// </summary>
		[MonoTODO]
		public DataRow[] Select(string filterExpression, string sort, DataViewRowState recordStates) 
		{
			DataRow[] dataRows = null;
			if (filterExpression == null)
				filterExpression = String.Empty;

			ExpressionElement Expression = null;
			if (filterExpression != null && filterExpression.Length != 0)
				Expression = new ExpressionMainElement(filterExpression);

			ArrayList List = new ArrayList();
			int recordStateFilter = GetRowStateFilter(recordStates);
			foreach (DataRow Row in Rows) 
			{
				if (((int)Row.RowState & recordStateFilter) != 0)
				{
					if (Expression == null || Expression.Test(Row))
						List.Add(Row);
				}
			}

			dataRows = (DataRow[])List.ToArray(typeof(DataRow));
			

			if (sort != null && !sort.Equals(String.Empty)) 
			{
				SortableColumn[] sortableColumns = null;

				sortableColumns = ParseTheSortString (sort);
				if (sortableColumns == null)
					throw new Exception ("sort expression result is null");
				if (sortableColumns.Length == 0)
					throw new Exception("sort expression result is 0");

				RowSorter rowSorter = new RowSorter (dataRows, sortableColumns);
				dataRows = rowSorter.SortRows ();

				sortableColumns = null;
				rowSorter = null;
			}

			
			return dataRows;
		}

		private static int GetRowStateFilter(DataViewRowState recordStates)
		{
			int flag = 0;

			if ((recordStates & DataViewRowState.Added) != 0)
				flag |= (int)DataRowState.Added;
			if ((recordStates & DataViewRowState.Deleted) != 0)
				flag |= (int)DataRowState.Deleted;
			if ((recordStates & DataViewRowState.ModifiedCurrent) != 0)
				flag |= (int)DataRowState.Modified;
			if ((recordStates & DataViewRowState.ModifiedOriginal) != 0)
				flag |= (int)DataRowState.Modified;
			if ((recordStates & DataViewRowState.Unchanged) != 0)
				flag |= (int)DataRowState.Unchanged;

			return flag;
		}

		/// <summary>
		/// Gets the TableName and DisplayExpression, if 
		/// there is one as a concatenated string.
		/// </summary>
		public override string ToString() 
		{
			//LAMESPEC: spec says concat the two. impl puts a 
			//plus sign infront of DisplayExpression
			string retVal = TableName;
			if(DisplayExpression != null && DisplayExpression != "")
				retVal += " + " + DisplayExpression;
			return retVal;
		}

		
		#region Events /////////////////
		
		/// <summary>
		/// Raises the ColumnChanged event.
		/// </summary>
		protected virtual void OnColumnChanged (DataColumnChangeEventArgs e) {
			if (null != ColumnChanged) {
				ColumnChanged (this, e);
			}
		}

		/// <summary>
		/// Raises the ColumnChanging event.
		/// </summary>
		protected virtual void OnColumnChanging (DataColumnChangeEventArgs e) {
			if (null != ColumnChanging) {
				ColumnChanging (this, e);
			}
		}

		/// <summary>
		/// Raises the PropertyChanging event.
		/// </summary>
		[MonoTODO]
		protected internal virtual void OnPropertyChanging (PropertyChangedEventArgs pcevent) {
			//	if (null != PropertyChanging)
			//	{
			//		PropertyChanging (this, e);
			//	}
		}

		/// <summary>
		/// Notifies the DataTable that a DataColumn is being removed.
		/// </summary>
		[MonoTODO]
		protected internal virtual void OnRemoveColumn (DataColumn column) {
			//	if (null != RemoveColumn)
			//	{
			//		RemoveColumn(this, e);
			//	}
		}

		/// <summary>
		/// Raises the RowChanged event.
		/// </summary>
		protected virtual void OnRowChanged (DataRowChangeEventArgs e) {
			if (null != RowChanged) {
				RowChanged(this, e);
			}
		}


		/// <summary>
		/// Raises the RowChanging event.
		/// </summary>
		protected virtual void OnRowChanging (DataRowChangeEventArgs e) {
			if (null != RowChanging) {
				RowChanging(this, e);
			}
		}

		/// <summary>
		/// Raises the RowDeleted event.
		/// </summary>
		protected virtual void OnRowDeleted (DataRowChangeEventArgs e) {
			if (null != RowDeleted) {
				RowDeleted(this, e);
			}
		}

		/// <summary>
		/// Raises the RowDeleting event.
		/// </summary>
		protected virtual void OnRowDeleting (DataRowChangeEventArgs e) {
			if (null != RowDeleting) {
				RowDeleting(this, e);
			}
		}

		[MonoTODO]
		private DataColumn CopyColumn (DataColumn Column) {
			DataColumn Copy = new DataColumn ();

			// Copy all the properties of column
			Copy.AllowDBNull = Column.AllowDBNull;
			Copy.AutoIncrement = Column.AutoIncrement;
			Copy.AutoIncrementSeed = Column.AutoIncrementSeed;
			Copy.AutoIncrementStep = Column.AutoIncrementStep;
			Copy.Caption = Column.Caption;
			Copy.ColumnMapping = Column.ColumnMapping;
			Copy.ColumnName = Column.ColumnName;
			//Copy.Container
			Copy.DataType = Column.DataType;
			Copy.DefaultValue = Column.DefaultValue;			
			Copy.Expression = Column.Expression;
			//Copy.ExtendedProperties
			Copy.MaxLength = Column.MaxLength;
			Copy.Namespace = Column.Namespace;
			Copy.Prefix = Column.Prefix;
			Copy.ReadOnly = Column.ReadOnly;
			//Copy.Site
			//we do not copy the unique value - it will be copyied when copying the constraints.
			//Copy.Unique = Column.Unique;
			
			return Copy;
		}			

		/// <summary>
		/// Occurs when after a value has been changed for 
		/// the specified DataColumn in a DataRow.
		/// </summary>
		[DataCategory ("Data")]	
		[DataSysDescription ("Occurs when a value has been changed for this column.")]
		public event DataColumnChangeEventHandler ColumnChanged;

		/// <summary>
		/// Occurs when a value is being changed for the specified 
		/// DataColumn in a DataRow.
		/// </summary>
		[DataCategory ("Data")]
		[DataSysDescription ("Occurs when a value has been submitted for this column. The user can modify the proposed value and should throw an exception to cancel the edit.")]
		public event DataColumnChangeEventHandler ColumnChanging;

		/// <summary>
		/// Occurs after a DataRow has been changed successfully.
		/// </summary>
		[DataCategory ("Data")]	
		[DataSysDescription ("Occurs after a row in the table has been successfully edited.")]
		public event DataRowChangeEventHandler RowChanged;

		/// <summary>
		/// Occurs when a DataRow is changing.
		/// </summary>
		[DataCategory ("Data")]	
		[DataSysDescription ("Occurs when the row is being changed so that the event handler can modify or cancel the change. The user can modify values in the row and should throw an  exception to cancel the edit.")]
		public event DataRowChangeEventHandler RowChanging;

		/// <summary>
		/// Occurs after a row in the table has been deleted.
		/// </summary>
		[DataCategory ("Data")]	
		[DataSysDescription ("Occurs after a row in the table has been successfully deleted.")] 
		public event DataRowChangeEventHandler RowDeleted;

		/// <summary>
		/// Occurs before a row in the table is about to be deleted.
		/// </summary>
		[DataCategory ("Data")]	
		[DataSysDescription ("Occurs when a row in the table marked for deletion. Throw an exception to cancel the deletion.")]
		public event DataRowChangeEventHandler RowDeleting;
		
		#endregion // Events

		/// <summary>
		///  Removes all UniqueConstraints
		/// </summary>
		private void RemoveUniqueConstraints () 
		{
			foreach (Constraint Cons in Constraints) {
				
				if (Cons is UniqueConstraint) {
					Constraints.Remove (Cons);
					break;
				}
			}
			
			UniqueConstraint.SetAsPrimaryKey(this.Constraints, null);
		}

		// to parse the sort string for DataTable:Select(expression,sort)
		// into sortable columns (think ORDER BY, 
		// such as, "customer ASC, price DESC" )
		private SortableColumn[] ParseTheSortString (string sort) 
		{
			SortableColumn[] sortColumns = null;
			ArrayList columns = null;
		
			if (sort != null && !sort.Equals ("")) {
				columns = new ArrayList ();
				string[] columnExpression = sort.Trim ().Split (new char[1] {','});
			
				for (int c = 0; c < columnExpression.Length; c++) {
					string[] columnSortInfo = columnExpression[c].Trim ().Split (new char[1] {' '});
				
					string columnName = columnSortInfo[0].Trim ();
					string sortOrder = "ASC";
					if (columnSortInfo.Length > 1) 
						sortOrder = columnSortInfo[1].Trim ().ToUpper ();
					
					ListSortDirection sortDirection = ListSortDirection.Ascending;
					switch (sortOrder) {
					case "ASC":
						sortDirection = ListSortDirection.Ascending;
						break;
					case "DESC":
						sortDirection = ListSortDirection.Descending;
						break;
					default:
						throw new IndexOutOfRangeException ("Could not find column: " + columnExpression[c]);
					}
					Int32 ord = 0;
					try {
						ord = Int32.Parse (columnName);
					}
					catch (FormatException) {
						ord = -1;
					}
					DataColumn dc = null;
					if (ord == -1)				
						dc = _columnCollection[columnName];
					else
						dc = _columnCollection[ord];
					SortableColumn sortCol = new SortableColumn (dc,sortDirection);
					columns.Add (sortCol);
				}	
				sortColumns = (SortableColumn[]) columns.ToArray (typeof (SortableColumn));
			}		
			return sortColumns;
		}
	
		private class SortableColumn 
		{
			private DataColumn col;
			private ListSortDirection dir;

			internal SortableColumn (DataColumn column, 
						ListSortDirection direction) 
			{
				col = column;
				dir = direction;
			}

			public DataColumn Column {
				get {
					return col;
				}
			}

			public ListSortDirection SortDirection {
				get {
					return dir;
				}
			}
		}

		private class RowSorter : IComparer 
		{
			private SortableColumn[] sortColumns;
			private DataRow[] rowsToSort;
			
			internal RowSorter(DataRow[] unsortedRows, 
					SortableColumn[] sortColumns) 
			{
				this.sortColumns = sortColumns;
				this.rowsToSort = unsortedRows;
			}

			public SortableColumn[] SortColumns {
				get {
					return sortColumns;
				}
			}
			
			public DataRow[] SortRows () 
			{
				Array.Sort (rowsToSort, this);
				return rowsToSort;
			}

			int IComparer.Compare (object x, object y) 
			{
				if(x == null)
					throw new Exception ("Object to compare is null: x");
				if(y == null)
					throw new Exception ("Object to compare is null: y");
				if(!(x is DataRow))
					throw new Exception ("Object to compare is not DataRow: x is " + x.GetType().ToString());
				if(!(y is DataRow))
					throw new Exception ("Object to compare is not DataRow: y is " + x.GetType().ToString());

				DataRow rowx = (DataRow) x;
				DataRow rowy = (DataRow) y;

				for(int i = 0; i < sortColumns.Length; i++) {
					SortableColumn sortColumn = sortColumns[i];
					DataColumn dc = sortColumn.Column;

					IComparable objx = (IComparable) rowx[dc];
					object objy = rowy[dc];

					int result = CompareObjects (objx, objy);
					if (result != 0) {
						if (sortColumn.SortDirection == ListSortDirection.Ascending) {
							return result;
						}
						else {
							return -result;
						}
					}
				}
				return 0;
			}

			private int CompareObjects (object a, object b) 
			{
				if (a == b)
					return 0;
				else if (a == null)
					return -1;
				else if (a == DBNull.Value)
					return -1;
				else if (b == null)
					return 1;
				else if (b == DBNull.Value)
					return 1;

				if((a is string) && (b is string)) {
					a = ((string) a).ToUpper ();
					b = ((string) b).ToUpper ();			
				}

				if (a is IComparable)
					return ((a as IComparable).CompareTo (b));
				else if (b is IComparable)
					return -((b as IComparable).CompareTo (a));

				throw new ArgumentException ("Neither a nor b IComparable");
			}
		}
	}
}
