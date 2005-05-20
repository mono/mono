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
//   Sureshkumar T <tsureshkumar@novell.com>
//   Konstantin Triger <kostat@mainsoft.com>
//
// (C) Chris Podurgiel
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002-2003
// Copyright (C) Daniel Morgan, 2002-2003
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
using System.Data.Common;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using Mono.Data.SqlExpressions;

namespace System.Data {
	//[Designer]
	[ToolboxItem (false)]
	[DefaultEvent ("RowChanging")]
	[DefaultProperty ("TableName")]
	[DesignTimeVisible (false)]
	[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DataTableEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
	[Serializable]
	public class DataTable : MarshalByValueComponent, IListSource, ISupportInitialize, ISerializable 
	{
		#region Fields

		internal DataSet dataSet;   
		
		private bool _caseSensitive;
		private DataColumnCollection _columnCollection;
		private ConstraintCollection _constraintCollection;
		// never access it. Use DefaultView.
		private DataView _defaultView = null;

		private string _displayExpression;
		private PropertyCollection _extendedProperties;
		private bool _hasErrors;
		private CultureInfo _locale;
		private int _minimumCapacity;
		private string _nameSpace;
		private DataRelationCollection _childRelations; 
		private DataRelationCollection _parentRelations;
		private string _prefix;
		private UniqueConstraint _primaryKeyConstraint;
		private DataRowCollection _rows;
		private ISite _site;
		private string _tableName;
		private bool _containsListCollection;
		private string _encodedTableName;
		internal bool _duringDataLoad;
		internal bool _nullConstraintViolationDuringDataLoad;
		private bool dataSetPrevEnforceConstraints;
		private bool dataTablePrevEnforceConstraints;
		private bool enforceConstraints = true;
		private DataRowBuilder _rowBuilder;
		private ArrayList _indexes;
		private RecordCache _recordCache;
		private int _defaultValuesRowIndex = -1;
		protected internal bool fInitInProgress;

		// If CaseSensitive property is changed once it does not anymore follow owner DataSet's 
		// CaseSensitive property. So when you lost you virginity it's gone for ever
		private bool _virginCaseSensitive = true;
		
		private PropertyDescriptorCollection _propertyDescriptorsCache;
		static DataColumn[] _emptyColumnArray = new DataColumn[0];
		
		#endregion //Fields
		
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
			_primaryKeyConstraint = null;
			_site = null;
			_rows = new DataRowCollection (this);
			_indexes = new ArrayList();
			_recordCache = new RecordCache(this);
			
			//LAMESPEC: spec says 25 impl does 50
			_minimumCapacity = 50;
			
			_childRelations = new DataRelationCollection.DataTableRelationCollection (this);
			_parentRelations = new DataRelationCollection.DataTableRelationCollection (this);
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
			string schema = info.GetString ("XmlSchema");
			string data = info.GetString ("XmlDiffGram");
			
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema (new StringReader (schema));
			ds.Tables [0].CopyProperties (this);
			ds = new DataSet ();
			ds.Tables.Add (this);
			ds.ReadXml (new StringReader (data), XmlReadMode.DiffGram);
			ds.Tables.Remove (this);
/* keeping for a while. With the change above, we shouldn't have to consider 
 * DataTable mode in schema inference/read.
			XmlSchemaMapper mapper = new XmlSchemaMapper (this);
			XmlTextReader xtr = new XmlTextReader(new StringReader (schema));
			mapper.Read (xtr);
			
			XmlDiffLoader loader = new XmlDiffLoader (this);
			xtr = new XmlTextReader(new StringReader (data));
			loader.Load (xtr);
*/
		}

#if NET_2_0
		public DataTable (string tableName, string tbNamespace)
			: this (tableName)
		{
			_nameSpace = tbNamespace;
		}
#endif

		/// <summary>
		/// Indicates whether string comparisons within the table are case-sensitive.
		/// </summary>
		[DataSysDescription ("Indicates whether comparing strings within the table is case sensitive.")]	
		public bool CaseSensitive {
			get { 
				if (_virginCaseSensitive && dataSet != null)
					return dataSet.CaseSensitive; 
				else
					return _caseSensitive;
				}
			set {
				if (_childRelations.Count > 0 || _parentRelations.Count > 0) {
					throw new ArgumentException ("Cannot change CaseSensitive or Locale property. This change would lead to at least one DataRelation or Constraint to have different Locale or CaseSensitive settings between its related tables.");
				}
				_virginCaseSensitive = false;
				_caseSensitive = value; 
				ResetCaseSensitiveIndexes();
			}
		}
		
		internal ArrayList Indexes{
			get { return _indexes; }
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
			get {
				if (_defaultView == null) {
					lock(this){
						if (_defaultView == null){
							if (dataSet != null)
								_defaultView = dataSet.DefaultViewManager.CreateDataView(this);
							else
								_defaultView = new DataView(this);
						}
					}
				}
				return _defaultView;
			}
		}
		

		/// <summary>
		/// Gets or sets the expression that will return 
		/// a value used to represent this table in the user interface.
		/// </summary>
		[DataCategory ("Data")]
		[DataSysDescription ("The expression used to compute the data-bound value of this row.")]	
		[DefaultValue ("")]
		public string DisplayExpression {
			get { return _displayExpression == null ? "" : _displayExpression; }
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
				if (_childRelations.Count > 0 || _parentRelations.Count > 0) {
					throw new ArgumentException ("Cannot change CaseSensitive or Locale property. This change would lead to at least one DataRelation or Constraint to have different Locale or CaseSensitive settings between its related tables.");
				}
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
			get
			{
				if (_nameSpace != null)
				{
					return _nameSpace;
				}
				if (DataSet != null)
				{
					return DataSet.Namespace;
				}
				return String.Empty;
			}
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
			get { return _prefix == null ? "" : _prefix; }
			set {
				// Prefix cannot contain any special characters other than '_' and ':'
				for (int i = 0; i < value.Length; i++) {
					if (!(Char.IsLetterOrDigit (value [i])) && (value [i] != '_') && (value [i] != ':'))
						throw new DataException ("Prefix '" + value + "' is not valid, because it contains special characters.");
				}
				_prefix = value;
			}
		}

		/// <summary>
		/// Gets or sets an array of columns that function as 
		/// primary keys for the data table.
		/// </summary>
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the column(s) that represent the primary key for this table.")]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.PrimaryKeyEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		[TypeConverterAttribute ("System.Data.PrimaryKeyTypeConverter, System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
		public DataColumn[] PrimaryKey {
			get {
				if (_primaryKeyConstraint == null) { 
					return new DataColumn[] {};
				}
				return _primaryKeyConstraint.Columns;
			}
			set {
				UniqueConstraint oldPKConstraint = _primaryKeyConstraint;
				
				// first check if value is the same as current PK.
				if (oldPKConstraint != null && DataColumn.AreColumnSetsTheSame(value, oldPKConstraint.Columns))
					return;

				// remove PK Constraint
				if(oldPKConstraint != null) {
					_primaryKeyConstraint = null;
					Constraints.Remove(oldPKConstraint);
				}
				
				if (value != null) {
					//Does constraint exist for these columns
					UniqueConstraint uc = UniqueConstraint.GetUniqueConstraintForColumnSet(this.Constraints, (DataColumn[]) value);
				
					//if constraint doesn't exist for columns
					//create new unique primary key constraint
					if (null == uc) {
						foreach (DataColumn Col in (DataColumn[]) value) {
							if (Col.Table == null)
								break;

							if (Columns.IndexOf (Col) < 0)
								throw new ArgumentException ("PrimaryKey columns do not belong to this table.");
						}
						// create constraint with primary key indication set to false
						// to avoid recursion
						uc = new UniqueConstraint( (DataColumn[]) value, false);		
						Constraints.Add (uc);
					}

					//set the constraint as the new primary key
						UniqueConstraint.SetAsPrimaryKey(this.Constraints, uc);
					_primaryKeyConstraint = uc;
				}				
			}
		}

		internal UniqueConstraint PrimaryKeyConstraint {
			get{
				return _primaryKeyConstraint;
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
			get { return _tableName == null ? "" : _tableName; }
			set { _tableName = value; }
		}
		
		bool IListSource.ContainsListCollection {
			get {
				// the collection is a DataView
				return false;
			}
		}
		
		internal RecordCache RecordCache {
			get {
				return _recordCache;
			}
		}
		
		private DataRowBuilder RowBuilder
		{
			get
			{
				// initiate only one row builder.
				if (_rowBuilder == null)
					_rowBuilder = new DataRowBuilder (this, -1, 0);
				else			
					// new row get id -1.
					_rowBuilder._rowId = -1;

				return _rowBuilder;
			}
		}
		
		private bool EnforceConstraints {
			get { return enforceConstraints; }
			set {
				if (value != enforceConstraints) {
					if (value) {
						// reset indexes since they may be outdated
						ResetIndexes();

						bool violatesConstraints = false;

						//FIXME: use index for AllowDBNull
						for (int i = 0; i < Columns.Count; i++) {
							DataColumn column = Columns[i];
							if (!column.AllowDBNull) {
								for (int j = 0; j < Rows.Count; j++){
									if (Rows[j].IsNull(column)) {
										violatesConstraints = true;
										Rows[j].RowError = String.Format("Column '{0}' does not allow DBNull.Value.", column.ColumnName);
									}
								}
							}
						}

						if (violatesConstraints)
							Constraint.ThrowConstraintException();

						// assert all constraints
						foreach (Constraint constraint in Constraints) {
							constraint.AssertConstraint();
						}
					}
					enforceConstraints = value;
				}
			}
		}

		internal bool RowsExist(DataColumn[] columns, DataColumn[] relatedColumns,DataRow row)
		{
			int curIndex = row.IndexFromVersion(DataRowVersion.Default);
			int tmpRecord = RecordCache.NewRecord();

			try {
				for (int i = 0; i < relatedColumns.Length; i++) {
					// according to MSDN: the DataType value for both columns must be identical.
					columns[i].DataContainer.CopyValue(relatedColumns[i].DataContainer, curIndex, tmpRecord);
				}
				return RowsExist(columns, tmpRecord);
			}
			finally {
				RecordCache.DisposeRecord(tmpRecord);
			}
		}

		bool RowsExist(DataColumn[] columns, int index)
		{
			bool rowsExist = false;
			Index indx = this.FindIndex(columns);

			if (indx != null) { // lookup for a row in index			
				rowsExist = (indx.Find(index) != -1);
			} 
			else { 
				// we have to perform full-table scan
 				// check that there is a parent for this row.
				foreach (DataRow thisRow in this.Rows) {
					if (thisRow.RowState != DataRowState.Deleted) {
						bool match = true;
						// check if the values in the columns are equal
						int thisIndex = thisRow.IndexFromVersion(DataRowVersion.Current);
						foreach (DataColumn column in columns) {
							if (column.DataContainer.CompareValues(thisIndex, index) != 0) {
								match = false;
								break;
							}	
						}
						if (match) {// there is a row with columns values equals to those supplied.
							rowsExist = true;
							break;
						}
					}
				}				
			}
			return rowsExist;
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
		public virtual void BeginInit () 
		{
			fInitInProgress = true;
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
				this._nullConstraintViolationDuringDataLoad = false;
			
				if (this.dataSet != null)
				{
					//Saving old Enforce constraints state for later
					//use in the EndLoadData.
					this.dataSetPrevEnforceConstraints = this.dataSet.EnforceConstraints;
					this.dataSet.EnforceConstraints = false;
				}
				else {
					//if table does not belong to any data set use EnforceConstraints of the table
					this.EnforceConstraints = false;
				}
			}
			return;
		}

		/// <summary>
		/// Clears the DataTable of all data.
		/// </summary>
		public void Clear () {
                        // Foriegn key constraints are checked in _rows.Clear method
			_rows.Clear ();
#if NET_2_0
                        OnTableCleared (new DataTableClearEventArgs (this));
#endif // NET_2_0

		}

		/// <summary>
		/// Clones the structure of the DataTable, including
		///  all DataTable schemas and constraints.
		/// </summary>
		public virtual DataTable Clone () 
		{
			 // Use Activator so we can use non-public constructors.
			DataTable Copy = (DataTable) Activator.CreateInstance(GetType(), true);			
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
			// expression is an aggregate function
			// filter is an expression used to limit rows

			DataRow[] rows = Select(filter);
			
			if (rows == null || rows.Length == 0)
				return DBNull.Value;
			
			Parser parser = new Parser (rows);
			IExpression expr = parser.Compile (expression);
			object obj = expr.Eval (rows[0]);
			
			return obj;
		}

		/// <summary>
		/// Copies both the structure and data for this DataTable.
		/// </summary>
		public DataTable Copy () 
		{
			DataTable copy = Clone();

			copy._duringDataLoad = true;
			foreach (DataRow row in Rows) {
				DataRow newRow = copy.NewNotInitializedRow();
				copy.Rows.AddInternal(newRow);
				CopyRow(row,newRow);
			}
			copy._duringDataLoad = false;		
		
			// rebuild copy indexes after loading all rows
			copy.ResetIndexes();
			return copy;
		}

		internal void CopyRow(DataRow fromRow,DataRow toRow)
		{
			if (fromRow.HasErrors) {
				fromRow.CopyErrors(toRow);
			}

			if (fromRow.HasVersion(DataRowVersion.Original)) {
				toRow.Original = toRow.Table.RecordCache.CopyRecord(this,fromRow.Original,-1);
			}

			if (fromRow.HasVersion(DataRowVersion.Current)) {
				if (fromRow.Original != fromRow.Current) {
					toRow.Current = toRow.Table.RecordCache.CopyRecord(this,fromRow.Current,-1);
				}
				else {
					toRow.Current = toRow.Original;
				}
			}
		}

		private void CopyProperties (DataTable Copy) 
		{
			Copy.CaseSensitive = CaseSensitive;
			Copy._virginCaseSensitive = _virginCaseSensitive;

			// Copy.ChildRelations
			// Copy.Constraints
			// Copy.Container
			// Copy.DefaultView
			// Copy.DesignMode
			Copy.DisplayExpression = DisplayExpression;
			if(ExtendedProperties.Count > 0) {
				//  Cannot copy extended properties directly as the property does not have a set accessor
				Array tgtArray = Array.CreateInstance( typeof (object), ExtendedProperties.Count);
				ExtendedProperties.Keys.CopyTo (tgtArray, 0);
				for (int i=0; i < ExtendedProperties.Count; i++)
					Copy.ExtendedProperties.Add (tgtArray.GetValue (i), ExtendedProperties[tgtArray.GetValue (i)]);
			}
			Copy.Locale = Locale;
			Copy.MinimumCapacity = MinimumCapacity;
			Copy.Namespace = Namespace;
			// Copy.ParentRelations
			Copy.Prefix = Prefix;
			Copy.Site = Site;
			Copy.TableName = TableName;

			bool isEmpty = Copy.Columns.Count == 0;

			// Copy columns
			foreach (DataColumn column in Columns) {			
				// When cloning a table, the columns may be added in the default constructor.
				if (isEmpty || !Copy.Columns.Contains(column.ColumnName)) {
					Copy.Columns.Add (column.Clone());	
				}
			}

			CopyConstraints(Copy);
			// add primary key to the copy
			if (PrimaryKey.Length > 0) {
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
		public virtual void EndInit () 
		{
			fInitInProgress = false;
			// Add the constraints
			_constraintCollection.PostEndInit();
			Columns.PostEndInit();
		}

		/// <summary>
		/// Turns on notifications, index maintenance, and 
		/// constraints after loading data.
		/// </summary>
		public void EndLoadData() 
		{
			if (this._duringDataLoad) {
				if(this._nullConstraintViolationDuringDataLoad) {
					this._nullConstraintViolationDuringDataLoad = false;
					throw new ConstraintException ("Failed to enable constraints. One or more rows contain values violating non-null, unique, or foreign-key constraints.");
				}
				
				if (this.dataSet !=null) {
					//Getting back to previous EnforceConstraint state
					this.dataSet.InternalEnforceConstraints(this.dataSetPrevEnforceConstraints,true);
				}
				else {
					//Getting back to the table's previous EnforceConstraint state
					this.EnforceConstraints = true;
				}

				//Returning from loading mode, raising exceptions as usual
				this._duringDataLoad = false;
			}
		}

		/// <summary>
		/// Gets a copy of the DataTable that contains all
		///  changes made to it since it was loaded or 
		///  AcceptChanges was last called.
		/// </summary>
		public DataTable GetChanges() 
		{
			return GetChanges(DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
		}

		/// <summary>
		/// Gets a copy of the DataTable containing all 
		/// changes made to it since it was last loaded, or 
		/// since AcceptChanges was called, filtered by DataRowState.
		/// </summary>
		public DataTable GetChanges(DataRowState rowStates) 
		{
			DataTable copyTable = null;

			IEnumerator rowEnumerator = Rows.GetEnumerator();
			while (rowEnumerator.MoveNext()) {
				DataRow row = (DataRow)rowEnumerator.Current;
				// The spec says relationship constraints may cause Unchanged parent rows to be included but
				// MS .NET 1.1 does not include Unchanged rows even if their child rows are changed.
				if (row.IsRowChanged(rowStates)) {
					if (copyTable == null)
						copyTable = Clone();
					DataRow newRow = copyTable.NewNotInitializedRow();
					row.CopyValuesToRow(newRow);
					copyTable.Rows.AddInternal (newRow);
				}
			}
			 
			return copyTable;
		}

#if NET_2_0
		[MonoTODO]
		public DataTableReader GetDataReader ()
		{
			throw new NotImplementedException ();
		}
#endif

		/// <summary>
		/// Gets an array of DataRow objects that contain errors.
		/// </summary>
		public DataRow[] GetErrors () 
		{
			ArrayList errors = new ArrayList();
			for (int i = 0; i < _rows.Count; i++)
			{
				if (_rows[i].HasErrors)
					errors.Add(_rows[i]);
			}
			
			DataRow[] ret = NewRowArray(errors.Count);
			errors.CopyTo(ret, 0);
			return ret;
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
			IList list = (IList) DefaultView;
			return list;
		}
				
		/// <summary>
		/// Copies a DataRow into a DataTable, preserving any 
		/// property settings, as well as original and current values.
		/// </summary>
		public void ImportRow (DataRow row) 
		{
			DataRow newRow = NewNotInitializedRow();

			int original = -1;
			if (row.HasVersion(DataRowVersion.Original)) {
				original = row.IndexFromVersion(DataRowVersion.Original);
				newRow.Original = RecordCache.NewRecord();
				RecordCache.CopyRecord(row.Table,original,newRow.Original);
			}

			if (row.HasVersion(DataRowVersion.Current)) {
				int current = row.IndexFromVersion(DataRowVersion.Current);
				if (current == original)
					newRow.Current = newRow.Original;
				else {
					newRow.Current = RecordCache.NewRecord();
					RecordCache.CopyRecord(row.Table,current,newRow.Current);
				}
			}

			if (EnforceConstraints)
				// we have to check that the new row doesn't colide with existing row
				Rows.ValidateDataRowInternal(newRow);

			Rows.AddInternal(newRow);		
	
			if (row.HasErrors) {
				row.CopyErrors(newRow);
			}
		}

		internal int DefaultValuesRowIndex
		{
			get {
				return _defaultValuesRowIndex;
			}	
		}

		/// <summary>
		/// This member is only meant to support Mono's infrastructure 		
		/// </summary>
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) 
		{
			DataSet dset;
			if (dataSet != null)
				dset = dataSet;
			else {
				dset = new DataSet ("tmpDataSet");
				dset.Tables.Add (this);
			}
			
			StringWriter sw = new StringWriter ();
			XmlTextWriter tw = new XmlTextWriter (sw);
			tw.Formatting = Formatting.Indented;
			dset.WriteIndividualTableContent (tw, this, XmlWriteMode.DiffGram);
			tw.Close ();
			
			StringWriter sw2 = new StringWriter ();
			DataTableCollection tables = new DataTableCollection (dset);
			tables.Add (this);
			XmlSchemaWriter.WriteXmlSchema (dset, new XmlTextWriter (sw2), tables, null);
			sw2.Close ();
			
			info.AddValue ("XmlSchema", sw2.ToString(), typeof(string));
			info.AddValue ("XmlDiffGram", sw.ToString(), typeof(string));
		}

#if NET_2_0
                /// <summary>
                ///     Loads the table with the values from the reader
                /// </summary>
		public void Load (IDataReader reader)
		{
                        Load (reader, LoadOption.PreserveChanges);
		}

                /// <summary>
                ///     Loads the table with the values from the reader and the pattern
                ///     of the changes to the existing rows or the new rows are based on
                ///     the LoadOption passed.
                /// </summary>
		public void Load (IDataReader reader, LoadOption loadOption)
		{
                        bool prevEnforceConstr = this.EnforceConstraints;
                        try {
                                this.EnforceConstraints = false;
                                int [] mapping = DbDataAdapter.BuildSchema (reader, this, SchemaType.Mapped, 
                                                                            MissingSchemaAction.AddWithKey,
                                                                            MissingMappingAction.Passthrough, 
                                                                            new DataTableMappingCollection ());
                                DbDataAdapter.FillFromReader (this,
                                                              reader,
                                                              0, // start from
                                                              0, // all records
                                                              mapping,
                                                              loadOption);
                        } finally {
                                this.EnforceConstraints = prevEnforceConstr;
                        }
		}

                
#endif

		/// <summary>
		/// Finds and updates a specific row. If no matching row
		///  is found, a new row is created using the given values.
		/// </summary>
		public DataRow LoadDataRow (object[] values, bool fAcceptChanges) 
		{
			DataRow row = null;
			if (PrimaryKey.Length == 0) {
				row = Rows.Add (values);
				if (fAcceptChanges)
					row.AcceptChanges ();
			}
			else {
				int newRecord = CreateRecord(values);
				int existingRecord = _primaryKeyConstraint.Index.Find(newRecord);

				if (existingRecord < 0) {
					row = NewRowFromBuilder (RowBuilder);
					row.Proposed = newRecord;
					Rows.AddInternal(row);
				}
				else {
					row = RecordCache[existingRecord];
					row.BeginEdit();
					row.ImportRecord(newRecord);
					row.EndEdit();
					
				}
				
				if (fAcceptChanges)
					row.AcceptChanges ();
			}
				
			return row;
		}

		internal DataRow LoadDataRow(IDataRecord record, int[] mapping, int length, bool fAcceptChanges)
		{
			DataRow row = null;
				int tmpRecord = this.RecordCache.NewRecord();
				try {
				RecordCache.ReadIDataRecord(tmpRecord,record,mapping,length);
				if (PrimaryKey.Length != 0) {
					bool hasPrimaryValues = true;
					foreach(DataColumn col in PrimaryKey) {
						if(!(col.Ordinal < mapping.Length)) {
							hasPrimaryValues = false;
							break;
						}
					}
					
					if (hasPrimaryValues) {
						// find the row in the table.
						row = Rows.Find(tmpRecord);
					}
				}
					
				bool shouldUpdateIndex = false;
				if (row == null) {
					row = NewNotInitializedRow();
					row.ImportRecord(tmpRecord);
					Rows.AddInternal (row);
					shouldUpdateIndex = true;
				}
				else {
					// Proposed = tmpRecord
					row.ImportRecord(tmpRecord);
				}
				
				if (fAcceptChanges) {
					row.AcceptChanges();
				}
				
				if (shouldUpdateIndex || !fAcceptChanges) {
					// AcceptChanges not always updates indexes because it calls EndEdit
					foreach(Index index in Indexes) {
						index.Update(row,tmpRecord);
					}
				}

			}
			catch(Exception e) {
				this.RecordCache.DisposeRecord(tmpRecord);
				throw e;
			}				
			return row;
		}

#if NET_2_0
                /// <summary>
                ///     Loads the given values into an existing row if matches or creates
                ///     the new row popluated with the values.
                /// </summary>
                /// <remarks>
                ///     This method searches for the values using primary keys and it first
                ///     searches using the original values of the rows and if not found, it
                ///     searches using the current version of the row.
                /// </remarks>
		public DataRow LoadDataRow (object [] values, LoadOption loadOption)
		{
                        DataRow row  = null;
                        bool new_row = false;
                        
                        // Find Data DataRow
                        if (this.PrimaryKey.Length > 0) {
				int newRecord = CreateRecord(values);
				try {
					Index index = GetIndex(PrimaryKey,null,DataViewRowState.OriginalRows,null,false);
					int existingRecord = index.Find(newRecord);
					if (existingRecord >= 0)
						row = RecordCache[existingRecord];
					else {
						existingRecord = _primaryKeyConstraint.Index.Find(newRecord);
						if (existingRecord >= 0)
							row = RecordCache[existingRecord];
					}
				}
				finally {
					RecordCache.DisposeRecord(newRecord);
				}
                        }
                                
                        // If not found, add new row
                        if (row == null) {
                                row = this.NewRow ();
                                new_row = true;
                        }

                        bool deleted = row.RowState == DataRowState.Deleted;

                        if (deleted && loadOption == LoadOption.OverwriteChanges)
                                row.RejectChanges ();                        

                        row.Load (values, loadOption, new_row);

                        if (deleted && loadOption == LoadOption.Upsert) {
                                row = this.NewRow ();
                                row.Load (values, loadOption, new_row = true);
                        }

                        if (new_row) {
                                this.Rows.Add (row);
                                if (loadOption == LoadOption.OverwriteChanges ||
                                    loadOption == LoadOption.PreserveChanges) {
                                        row.AcceptChanges ();
                                }
                        }

                        return row;
		}

		[MonoTODO]
		public void Merge (DataTable table)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Merge (DataTable table, bool preserveChanges)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Merge (DataTable table, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			throw new NotImplementedException ();
		}
#endif

		/// <summary>
		/// Creates a new DataRow with the same schema as the table.
		/// </summary>
		public DataRow NewRow () 
		{
			EnsureDefaultValueRowIndex();

			DataRow newRow = NewRowFromBuilder (RowBuilder);

			newRow.Proposed = CreateRecord(null);
			return newRow;
		}

		internal int CreateRecord(object[] values) {
			int valCount = values != null ? values.Length : 0;
			if (valCount > Columns.Count)
				throw new ArgumentException("Input array is longer than the number of columns in this table.");

			int index = RecordCache.NewRecord();

			try {
				for (int i = 0; i < valCount; i++) {
					try {
                                                if (values [i] != null) {
                                                        Columns[i].DataContainer[index] = values [i];
                                                        continue;
                                                }
                                                
                                                DataColumn column = Columns [i];
                                                if (column.AutoIncrement)
                                                        column.DataContainer[index] = column.AutoIncrementValue ();
                                                else
                                                        column.DataContainer.CopyValue(DefaultValuesRowIndex, index);                                                        
					}
					catch(Exception e) {
						throw new ArgumentException(e.Message +
							String.Format("Couldn't store <{0}> in {1} Column.  Expected type is {2}.",
							values[i], Columns[i].ColumnName, Columns[i].DataType.Name), e);
					}
				}

				for(int i = valCount; i < Columns.Count; i++) {
					DataColumn column = Columns[i];
					if (column.AutoIncrement)
						column.DataContainer[index] = column.AutoIncrementValue ();
					else
						column.DataContainer.CopyValue(DefaultValuesRowIndex, index);
				}

				return index;
			}
			catch {
				RecordCache.DisposeRecord(index);
				throw;
			}
		}

		private void EnsureDefaultValueRowIndex()
		{
			// initialize default values row for the first time
			if ( _defaultValuesRowIndex == -1 ) {
				_defaultValuesRowIndex = RecordCache.NewRecord();
				foreach(DataColumn column in Columns) {
					column.DataContainer[_defaultValuesRowIndex] = column.DefaultValue;
				}
			}
		}

#if NET_2_0
		internal int CompareRecords(int x, int y) {
			for (int col = 0; col < Columns.Count; col++) {
				int res = Columns[col].DataContainer.CompareValues (x, y);
				if (res != 0)
					return res;
			}

			return 0;
		}
#endif

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
		
		internal DataRow NewNotInitializedRow()
		{
			EnsureDefaultValueRowIndex();

			return NewRowFromBuilder (RowBuilder);
		}

#if NET_2_0
		[MonoTODO]
		XmlReadMode ReadXml (Stream stream)
		{
			throw new NotImplementedException ();
		}

		public void ReadXmlSchema (Stream stream)
		{
			ReadXmlSchema (new XmlTextReader (stream));
		}

		public void ReadXmlSchema (TextReader reader)
		{
			ReadXmlSchema (new XmlTextReader (reader));
		}

		public void ReadXmlSchema (string fileName)
		{
			XmlTextReader reader = null;
			try {
				reader = new XmlTextReader (fileName);
			ReadXmlSchema (reader);
			} finally {
				if (reader != null)
			reader.Close ();
		}
		}

		public void ReadXmlSchema (XmlReader reader)
		{
			DataSet ds = new DataSet ();
			new XmlSchemaDataImporter (ds, reader).Process ();
			DataTable target = null;
			if (TableName == String.Empty) {
				if (ds.Tables.Count > 0)
					target = ds.Tables [0];
			}
			else {
				target = ds.Tables [TableName];
				if (target == null)
					throw new ArgumentException (String.Format ("DataTable '{0}' does not match to any DataTable in source.", TableName));
			}
			if (target != null)
				target.CopyProperties (this);
		}
#endif

		/// <summary>
		/// Rolls back all changes that have been made to the 
		/// table since it was loaded, or the last time AcceptChanges
		///  was called.
		/// </summary>
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
			if (filterExpression == null)
				filterExpression = String.Empty;

			DataColumn[] columns = _emptyColumnArray;
			ListSortDirection[] sorts = null;
			if (sort != null && !sort.Equals(String.Empty))
				columns = ParseSortString (this, sort, out sorts, false);

			IExpression filter = null;
			if (filterExpression != String.Empty) {
				Parser parser = new Parser ();
				filter = parser.Compile (filterExpression);
			}

			Index index = FindIndex(columns, sorts, recordStates, filter);
			if (index == null)
				index = new Index(new Key(this,columns,sorts,recordStates,filter));

			int[] records = index.GetAll();
			DataRow[] dataRows = NewRowArray(index.Size);
			for (int i = 0; i < dataRows.Length; i++)
				dataRows[i] = RecordCache[records[i]];

			return dataRows;
		}

		
		private void AddIndex (Index index)
		{
			if (_indexes == null) {
				_indexes = new ArrayList();
			}

			_indexes.Add (index);
		}

		/// <summary>
		/// Returns index corresponding to columns,sort,row state filter and unique values given.
		/// If such an index not exists, creates a new one.
		/// </summary>
		/// <param name="columns">Columns set of the index to look for.</param>
		/// <param name="sort">Columns sort order of the index to look for.</param>
		/// <param name="rowState">Rpw state filter of the index to look for.</param>
		/// <param name="unique">Uniqueness of the index to look for.</param>
		/// <param name="strict">Indicates whenever the index found should correspond in its uniquness to the value of unique parameter specified.</param>
		/// <param name="reset">Indicates whenever the already existing index should be forced to reset.</param>
		/// <returns></returns>
		internal Index GetIndex(DataColumn[] columns, ListSortDirection[] sort, DataViewRowState rowState, IExpression filter, bool reset)
		{
			Index index = FindIndex(columns,sort,rowState,filter);
			if (index == null ) {
				index = new Index(new Key(this,columns,sort,rowState,filter));

				AddIndex(index);
			}
			else if (reset) {
				// reset existing index only if asked for this
				index.Reset();
			}
			return index;
		}

		internal Index FindIndex(DataColumn[] columns)
		{
			return FindIndex(columns,null,DataViewRowState.None, null);
		}

		internal Index FindIndex(DataColumn[] columns, ListSortDirection[] sort, DataViewRowState rowState, IExpression filter)
		{
			if (Indexes != null) {
				foreach (Index index in Indexes) {
					if (index.Key.Equals(columns,sort,rowState, filter)) {
						return index;
					}
				}
			}
			return null;
		}

		internal void ResetIndexes()
		{
			foreach(Index index in Indexes) {
				index.Reset();
			}
		}

		internal void ResetCaseSensitiveIndexes()
		{
			foreach(Index index in Indexes) {
				bool containsStringcolumns = false;
				foreach(DataColumn column in index.Key.Columns) {
					if (column.DataType == typeof(string)) {
						containsStringcolumns = true;
						break;
					}
				}

				if (containsStringcolumns) {
					index.Reset();
				}
			}
		}

		internal void DropIndex(Index index)
		{
			if (index != null && index.RefCount == 0) {	
				_indexes.Remove(index);
			}
		}

		internal void DeleteRowFromIndexes (DataRow row)
		{
			if (_indexes != null) {
				foreach (Index indx in _indexes) {
					indx.Delete (row);
				}
			}
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

#if NET_2_0
		private XmlWriterSettings GetWriterSettings ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.Indent = true;
			return s;
		}

		public void WriteXml (Stream stream)
		{
			WriteXml (stream, XmlWriteMode.IgnoreSchema);
		}

		public void WriteXml (TextWriter writer)
		{
			WriteXml (writer, XmlWriteMode.IgnoreSchema);
		}

		public void WriteXml (XmlWriter writer)
		{
			WriteXml (writer, XmlWriteMode.IgnoreSchema);
		}

		public void WriteXml (string fileName)
		{
			WriteXml (fileName, XmlWriteMode.IgnoreSchema);
		}

		public void WriteXml (Stream stream, XmlWriteMode mode)
		{
			WriteXml (XmlWriter.Create (stream, GetWriterSettings ()), mode);
		}

		public void WriteXml (TextWriter writer, XmlWriteMode mode)
		{
			WriteXml (XmlWriter.Create (writer, GetWriterSettings ()), mode);
		}

		[MonoTODO]
		public void WriteXml (XmlWriter writer, XmlWriteMode mode)
		{
			throw new NotImplementedException ();
		}

		public void WriteXml (string fileName, XmlWriteMode mode)
		{
			XmlWriter xw = null;
			try {
				xw = XmlWriter.Create (fileName, GetWriterSettings ());
				WriteXml (xw, mode);
			} finally {
				if (xw != null)
					xw.Close ();
			}
		}

		public void WriteXmlSchema (Stream stream)
		{
			WriteXmlSchema (XmlWriter.Create (stream, GetWriterSettings ()));
		}

		public void WriteXmlSchema (TextWriter writer)
		{
			WriteXmlSchema (XmlWriter.Create (writer, GetWriterSettings ()));
		}

		public void WriteXmlSchema (XmlWriter writer)
		{
			DataSet ds = DataSet;
			DataSet tmp = null;
			try {
				if (ds == null) {
					tmp = ds = new DataSet ();
					ds.Tables.Add (this);
				}
				DataTableCollection col = new DataTableCollection (ds);
				col.Add (this);
				XmlSchemaWriter.WriteXmlSchema (ds, writer, col, null);
			} finally {
				if (tmp != null)
					ds.Tables.Remove (this);
			}
		}

		public void WriteXmlSchema (string fileName)
		{
			XmlWriter xw = null;
			try {
				xw = XmlWriter.Create (fileName, GetWriterSettings ());
				WriteXmlSchema (xw);
			} finally {
				if (xw != null)
					xw.Close ();
			}
		}
#endif
		
		#region Events 
		
		/// <summary>
		/// Raises the ColumnChanged event.
		/// </summary>
		protected virtual void OnColumnChanged (DataColumnChangeEventArgs e) {
			if (null != ColumnChanged) {
				ColumnChanged (this, e);
			}
		}

		internal void RaiseOnColumnChanged (DataColumnChangeEventArgs e) {
			OnColumnChanged(e);
		}

#if NET_2_0
                /// <summary>
		/// Raises TableCleared Event and delegates to subscribers
		/// </summary>
		protected virtual void OnTableCleared (DataTableClearEventArgs e) {
			if (TableCleared != null)
				TableCleared (this, e);
		}
#endif // NET_2_0

		/// <summary>
		/// Raises the ColumnChanging event.
		/// </summary>
		protected virtual void OnColumnChanging (DataColumnChangeEventArgs e) {
			if (null != ColumnChanging) {
				ColumnChanging (this, e);
			}
		}

		internal void RaiseOnColumnChanging (DataColumnChangeEventArgs e) {
			OnColumnChanging(e);
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

#if NET_2_0
		/// <summary>
		/// Occurs after the Clear method is called on the datatable.
		/// </summary>
		[DataCategory ("Data")]	
		[DataSysDescription ("Occurs when the rows in a table is cleared . Throw an exception to cancel the deletion.")]
		public event DataTableClearEventHandler TableCleared;
#endif // NET_2_0

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

		internal static DataColumn[] ParseSortString (DataTable table, string sort, out ListSortDirection[] sortDirections, bool rejectNoResult)
		{
			DataColumn[] sortColumns = _emptyColumnArray;
			sortDirections = null;
			
			ArrayList columns = null;
			ArrayList sorts = null;
		
			if (sort != null && !sort.Equals ("")) {
				columns = new ArrayList ();
				sorts = new ArrayList();
				string[] columnExpression = sort.Trim ().Split (new char[1] {','});
			
				for (int c = 0; c < columnExpression.Length; c++) {
					string[] columnSortInfo = columnExpression[c].Trim ().Split (new char[1] {' '});
				
					string columnName = columnSortInfo[0].Trim ();
					string sortOrder = "ASC";
					if (columnSortInfo.Length > 1) 
						sortOrder = columnSortInfo[1].Trim ().ToUpper (table.Locale);
					
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

					if (columnName.StartsWith("[") || columnName.EndsWith("]")) {
						if (columnName.StartsWith("[") && columnName.EndsWith("]"))
							columnName = columnName.Substring(1, columnName.Length - 2);
						else
							throw new ArgumentException(String.Format("{0} isn't a valid Sort string entry.", columnName));
					}

					DataColumn dc = table.Columns[columnName];
					if (dc == null){
						try {
							dc = table.Columns[Int32.Parse (columnName)];
					}
					catch (FormatException) {
							throw new IndexOutOfRangeException("Cannot find column " + columnName);
					}
					}

					columns.Add (dc);
					sorts.Add(sortDirection);
				}	
				sortColumns = (DataColumn[]) columns.ToArray (typeof (DataColumn));
				sortDirections = new ListSortDirection[sorts.Count];
				for (int i = 0; i < sortDirections.Length; i++)
					sortDirections[i] = (ListSortDirection)sorts[i];
			}		

			if (rejectNoResult) {
				if (sortColumns == null)
					throw new SystemException ("sort expression result is null");
				if (sortColumns.Length == 0)
					throw new SystemException("sort expression result is 0");
			}

			return sortColumns;
		}

		private void UpdatePropertyDescriptorsCache()
		{
			PropertyDescriptor[] descriptors = new PropertyDescriptor[Columns.Count + ChildRelations.Count];
			int index = 0;
			foreach(DataColumn col in Columns) {
				descriptors[index++] = new DataColumnPropertyDescriptor(col);
			}

			foreach(DataRelation rel in ChildRelations) {
				descriptors[index++] = new DataRelationPropertyDescriptor(rel);
			}

			_propertyDescriptorsCache = new PropertyDescriptorCollection(descriptors);
		}

		internal PropertyDescriptorCollection GetPropertyDescriptorCollection()
		{
			if (_propertyDescriptorsCache == null) {
				UpdatePropertyDescriptorsCache();
			}

			return _propertyDescriptorsCache;
		}

		internal void ResetPropertyDescriptorsCache() {
			_propertyDescriptorsCache = null;
		}
	}
}
