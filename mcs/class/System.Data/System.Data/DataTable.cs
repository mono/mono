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
		
		#endregion //Fields
		
		private delegate void PostEndInit();

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
			_indexes = new ArrayList();
			_recordCache = new RecordCache(this);
			
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
			}
		}

		internal bool VirginCaseSensitive {
			get { return _virginCaseSensitive; }
			set { _virginCaseSensitive = value; }
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
			get {
				if (_nameSpace != null)
					return _nameSpace;
				else if (dataSet != null)
					return dataSet.Namespace;
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
				UniqueConstraint uc = UniqueConstraint.GetPrimaryKeyConstraint( Constraints);
				if (null == uc) return new DataColumn[] {};
				return uc.Columns;
			}
			set {
				UniqueConstraint oldPKConstraint = UniqueConstraint.GetPrimaryKeyConstraint( Constraints);
				
				// first check if value is the same as current PK.
				if (oldPKConstraint != null && DataColumn.AreColumnSetsTheSame(value, oldPKConstraint.Columns))
					return;

				// remove PK Constraint
				if(oldPKConstraint != null) {
					Constraints.Remove(oldPKConstraint);
				}
				
				if (value != null) {
					//Does constraint exist for these columns
					UniqueConstraint uc = UniqueConstraint.GetUniqueConstraintForColumnSet(
						this.Constraints, (DataColumn[]) value);
				
					//if constraint doesn't exist for columns
					//create new unique primary key constraint
					if (null == uc) {
						foreach (DataColumn Col in (DataColumn[]) value) {

							if (Col.Table == null)
								break;

							if (Columns.IndexOf (Col) < 0)
								throw new ArgumentException ("PrimaryKey columns do not belong to this table.");
						}


						uc = new UniqueConstraint( (DataColumn[]) value, true);
					
						Constraints.Add (uc);
					}
					else { 
						//set existing constraint as the new primary key
						UniqueConstraint.SetAsPrimaryKey(this.Constraints, uc);
					}

					// if we really supplied some columns fo primary key - 
					// rebuild indexes fo all foreign key constraints
					if(value.Length > 0) {
						foreach(ForeignKeyConstraint constraint in this.Constraints.ForeignKeyConstraints){
							constraint.AssertConstraint();
						}
					}
				}
				else {
					// if primary key is null now - drop all the indexes for foreign key constraints
					foreach(ForeignKeyConstraint constraint in this.Constraints.ForeignKeyConstraints){
						Index index = constraint.Index;
						constraint.Index = null;
						this.DropIndex(index);
					}
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
		
		private bool EnforceConstraints {
			get { return enforceConstraints; }
			set {
				if (value != enforceConstraints) {
					if (value) {
						// first assert all unique constraints
						foreach (UniqueConstraint uc in this.Constraints.UniqueConstraints)
						uc.AssertConstraint ();
						// then assert all foreign keys
						foreach (ForeignKeyConstraint fk in this.Constraints.ForeignKeyConstraints)
							fk.AssertConstraint ();
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

				return RowsExist(columns, tmpRecord, relatedColumns.Length);
			}
			finally {
				RecordCache.DisposeRecord(tmpRecord);
			}
		}

		bool RowsExist(DataColumn[] columns, int index, int length)
		{
			bool rowsExist = false;
			Index indx = this.GetIndexByColumns (columns);

			if (indx != null) { // lookup for a row in index			
				rowsExist = (indx.FindSimple (index, length, false) != null);
			} 
			
			if(indx == null || rowsExist == false) { 
				// no index or rowExist= false, we have to perform full-table scan
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
					this.dataTablePrevEnforceConstraints = this.EnforceConstraints;
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
		}

		/// <summary>
		/// Clones the structure of the DataTable, including
		///  all DataTable schemas and constraints.
		/// </summary>
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
			// expression is an aggregate function
			// filter is an expression used to limit rows

			DataRow[] rows = Select(filter);
			
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
				copy.Rows.Add(newRow);
				CopyRow(row,newRow);
			}
			copy._duringDataLoad = false;		
			return copy;
		}

		internal void CopyRow(DataRow fromRow,DataRow toRow)
		{
			fromRow.CopyState(toRow);

			if (fromRow.HasVersion(DataRowVersion.Original)) {
				toRow._original = toRow.Table.RecordCache.CopyRecord(this,fromRow._original,-1);
			}

			if (fromRow.HasVersion(DataRowVersion.Current)) {
				toRow._current = toRow.Table.RecordCache.CopyRecord(this,fromRow._current,-1);
			}
		}

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
		public virtual void EndInit () 
		{
			fInitInProgress = false;
			// Add the constraints
			PostEndInit _postEndInit = new PostEndInit (_constraintCollection.PostEndInit);
			_postEndInit();
		}

		/// <summary>
		/// Turns on notifications, index maintenance, and 
		/// constraints after loading data.
		/// </summary>
		public void EndLoadData() 
		{
			int i = 0;
			if (this._duringDataLoad) 
			{
				
				if (this.dataSet !=null)
				{
					//Getting back to previous EnforceConstraint state
					this.dataSet.EnforceConstraints = this.dataSetPrevEnforceConstraints;
				}
				else {
					//Getting back to the table's previous EnforceConstraint state
					this.EnforceConstraints = this.dataTablePrevEnforceConstraints;
				}

				if(this._nullConstraintViolationDuringDataLoad) {
					this._nullConstraintViolationDuringDataLoad = false;
					throw new ConstraintException ("Failed to enable constraints. One or more rows contain values violating non-null, unique, or foreign-key constraints.");
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
					DataRow newRow = copyTable.NewRow();
					row.CopyValuesToRow(newRow);
					copyTable.Rows.Add (newRow);
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
		public void ImportRow (DataRow row) 
		{
			DataRow newRow = NewRow();
			Rows.Add(newRow);
			row.CopyValuesToRow(newRow);
			
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
			dset.WriteIndividualTableContent (tw, this, XmlWriteMode.DiffGram);
			tw.Close ();
			
			StringWriter sw2 = new StringWriter ();
			DataTableCollection tables = new DataTableCollection (dset);
			tables.Add (this);
			XmlSchema schema = dset.BuildSchema (tables, null);
			schema.Write (sw2);
			sw2.Close ();
			
			info.AddValue ("XmlSchema", sw2.ToString(), typeof(string));
			info.AddValue ("XmlDiffGram", sw.ToString(), typeof(string));
		}

#if NET_2_0
		[MonoTODO]
		public void Load (IDataReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Load (IDataReader reader, LoadOption loadOption)
		{
			throw new NotImplementedException ();
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

		internal DataRow LoadDataRow(IDataRecord record, int[] mapping, bool fAcceptChanges)
		{
			DataRow row = null;
			if (PrimaryKey.Length == 0) {
				row = NewRow();
				row.SetValuesFromDataRecord(record, mapping);
				Rows.Add (row);

				if (fAcceptChanges) {
					row.AcceptChanges();
				}
			}
			else {
				bool hasPrimaryValues = true;
				int tmpRecord = this.RecordCache.NewRecord();
				try {
					for (int i = 0; i < PrimaryKey.Length && hasPrimaryValues; i++) {
						DataColumn primaryKeyColumn = PrimaryKey[i];
						int ordinal = primaryKeyColumn.Ordinal;
						if(ordinal < mapping.Length) {
							primaryKeyColumn.DataContainer.SetItemFromDataRecord(tmpRecord,record,mapping[ordinal]);
						}
						else {
							hasPrimaryValues = false;
						}
					}
					
					if (hasPrimaryValues) {
						// find the row in the table.
						row = Rows.Find(tmpRecord,PrimaryKey.Length);
					}
				}
				finally {
					this.RecordCache.DisposeRecord(tmpRecord);
				}

				if (row == null) {
					row = NewRow();
					row.SetValuesFromDataRecord(record, mapping);
					Rows.Add (row);
				}
				else {
					row.SetValuesFromDataRecord(record, mapping);
				}
				
				if (fAcceptChanges) {
					row.AcceptChanges();
				}
			}				
			return row;
		}

#if NET_2_0
		[MonoTODO]
		public DataRow LoadDataRow (object[] values, LoadOption loadOption)
		{
			throw new NotImplementedException ();
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
			// initiate only one row builder.
			if (_rowBuilder == null)
				_rowBuilder = new DataRowBuilder (this, 0, 0);
			
			// new row get id -1.
			_rowBuilder._rowId = -1;

			// initialize default values row for the first time
			if ( _defaultValuesRowIndex == -1 ) {
				_defaultValuesRowIndex = RecordCache.NewRecord();
				foreach(DataColumn column in Columns) {
					column.DataContainer[_defaultValuesRowIndex] = column.DefaultValue;
				}
			}

			return this.NewRowFromBuilder (_rowBuilder);
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
		
		internal DataRow NewNotInitializedRow()
		{
			return new DataRow(this,-1);
		}

#if NET_2_0
		[MonoTODO]
		XmlReadMode ReadXml (Stream stream)
		{
			throw new NotImplementedException ();
		}

		public void ReadXmlSchema (Stream stream)
		{
			XmlSchemaMapper mapper = new XmlSchemaMapper (this);
			mapper.Read (new XmlTextReader(stream));
		}

		public void ReadXmlSchema (TextReader reader)
		{
			XmlSchemaMapper mapper = new XmlSchemaMapper (this);
			mapper.Read (new XmlTextReader(reader));
		}

		public void ReadXmlSchema (string fileName)
		{
			StreamReader reader = new StreamReader (fileName);
			ReadXmlSchema (reader);
			reader.Close ();
		}

		public void ReadXmlSchema (XmlReader reader)
		{
			XmlSchemaMapper mapper = new XmlSchemaMapper (this);
			mapper.Read (reader);
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

			IExpression filter = null;
			if (filterExpression != String.Empty) {
				Parser parser = new Parser ();
				filter = parser.Compile (filterExpression);
			}

			ArrayList rowList = new ArrayList();
			int recordStateFilter = GetRowStateFilter(recordStates);
			foreach (DataRow row in Rows) {
				if (((int)row.RowState & recordStateFilter) != 0) {
					if (filter != null && !(bool)filter.Eval (row))
						continue;
					rowList.Add (row);
				}
			}

			DataRow[] dataRows = (DataRow[])rowList.ToArray(typeof(DataRow));

			if (sort != null && !sort.Equals(String.Empty)) 
			{
				SortableColumn[] sortableColumns = null;

				sortableColumns = ParseTheSortString (sort);
				if (sortableColumns == null)
					throw new Exception ("sort expression result is null");
				if (sortableColumns.Length == 0)
					throw new Exception("sort expression result is 0");

				RowSorter rowSorter = new RowSorter (this, dataRows, sortableColumns);
				dataRows = rowSorter.SortRows ();

				sortableColumns = null;
				rowSorter = null;
			}

			
			return dataRows;
		}

		internal void AddIndex (Index index)
		{
			if (_indexes == null)
				_indexes = new ArrayList();

			_indexes.Add (index);
		}

		internal void RemoveIndex (Index indx)
		{
			_indexes.Remove (indx);
		}

		internal Index GetIndexByColumns (DataColumn[] columns)
		{
			return GetIndexByColumns(columns,false,false);
		}

//		internal Index GetIndexByColumnsExtended(DataColumn[] columns)
//		{
//			DataColumn[] pkColumns = this.PrimaryKey;
//			if((pkColumns != null) && (pkColumns.Length > 0)) {
//				DataColumn[] cols = new DataColumn[columns.Length + pkColumns.Length];					
//				Array.Copy(columns,0,cols,0,columns.Length);
//				Array.Copy(pkColumns,0,cols,columns.Length,pkColumns.Length);
//
//				return _getIndexByColumns(cols,false,false);
//			} else {
//				return null;
//			}
//		}

		internal Index GetIndexByColumns (DataColumn[] columns, bool unique)
		{
			return GetIndexByColumns(columns,unique,true);
		}

//		internal Index GetIndexByColumnsExtended(DataColumn[] columns, bool unique)
//		{
//			DataColumn[] pkColumns = this.PrimaryKey;
//			if((pkColumns != null) && (pkColumns.Length > 0)) {
//				DataColumn[] cols = new DataColumn[columns.Length + pkColumns.Length];					
//				Array.Copy(columns,0,cols,0,columns.Length);
//				Array.Copy(pkColumns,0,cols,columns.Length,pkColumns.Length);
//
//				return _getIndexByColumns(cols,unique,true);
//			} else  {
//				return null;
//			}
//		}

		internal Index GetIndexByColumns(DataColumn[] columns, bool unique, bool useUnique)
		{
			if (_indexes != null) {
				foreach (Index indx in _indexes) {
					bool found = false;
					if ((!useUnique) || ((useUnique)&& (indx.IsUnique))) {
						found = DataColumn.AreColumnSetsTheSame (indx.Columns, columns);
					}
					if (found)
						return indx;
				}
			}

			return null;
		}

		internal void DeleteRowFromIndexes (DataRow row)
		{
			if (_indexes != null) {
				foreach (Index indx in _indexes) {
					indx.Delete (row);
				}
			}
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

#if NET_2_0
		[MonoTODO]
		public void WriteXml (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXml (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXml (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXml (string fileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXml (Stream stream, XmlWriteMode mode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXml (TextWriter writer, XmlWriteMode mode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXml (XmlWriter writer, XmlWriteMode mode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXml (string fileName, XmlWriteMode mode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXmlSchema (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXmlSchema (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXmlSchema (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteXmlSchema (string fileName)
		{
			throw new NotImplementedException ();
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
						sortOrder = columnSortInfo[1].Trim ().ToUpper (Locale);
					
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
			private DataTable table;
			private SortableColumn[] sortColumns;
			private DataRow[] rowsToSort;
			
			internal RowSorter(DataTable table,
					DataRow[] unsortedRows, 
					SortableColumn[] sortColumns) 
			{
				this.table = table;
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

					int result = dc.CompareValues(
						rowx.IndexFromVersion(DataRowVersion.Default),
						rowy.IndexFromVersion(DataRowVersion.Default));

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
		}

		/// <summary>
		/// Creates new index for a table
		/// </summary>
		internal Index CreateIndex(string name, DataColumn[] columns, bool unique)
		{
			// first check whenever index exists on the columns
			Index idx = this.GetIndexByColumns(columns);
			if(idx != null) {
			// if index on this columns already exists - return it
				return idx;
			}

			// create new index
			Index newIndex = new Index(name,this,columns,unique);

			//InitializeIndex (newIndex);			

			// add new index to table indexes
			this.AddIndex(newIndex);
			return newIndex;
		}

//		/// <summary>
//		/// Creates new extended index for a table
//		/// </summary>
//		internal Index CreateIndexExtended(string name, DataColumn[] columns, bool unique)
//		{
//			// first check whenever extended index exists on the columns
//			Index idx = this.GetIndexByColumnsExtended(columns);
//			if(idx != null) {
//				// if extended index on this columns already exists - return it
//				return idx;
//			}
//
//			DataColumn[] pkColumns = this.PrimaryKey;
//			if((pkColumns != null) && (pkColumns.Length > 0)) {
//				DataColumn[] cols = new DataColumn[columns.Length + pkColumns.Length];					
//				Array.Copy(columns,0,cols,0,columns.Length);
//				Array.Copy(pkColumns,0,cols,columns.Length,pkColumns.Length);
//				return this.CreateIndex(name, cols, unique);
//			} 
//			else {
//				throw new InvalidOperationException("Can not create extended index if the primary key is null or primary key does not contains any row");
//			}
//		}

//		/// <summary>
//		/// Drops extended index if it is not referenced anymore
//		/// by any of table constraints
//		/// </summary>
//		internal void DropIndexExtended(DataColumn[] columns)
//		{
//			// first check whenever extended index exists on the columns
//			Index index = this.GetIndexByColumnsExtended(columns);
//			if(index == null) {
//				// if no extended index on this columns exists - do nothing
//				return;
//			}
//			this.DropIndex(index);
//		}

		/// <summary>
		/// Drops index specified by columns if it is not referenced anymore
		/// by any of table constraints
		/// </summary>
		internal void DropIndex(DataColumn[] columns)
		{
			// first check whenever index exists for the columns
			Index index = this.GetIndexByColumns(columns);
			if(index == null) {
			// if no index on this columns already exists - do nothing
				return;
			}
			this.DropIndex(index);
		}

		internal void DropIndex(Index index)
		{
			// loop through table constraints and checks 
			foreach(Constraint constraint in Constraints) {
				// if we found another reference to the index we do not remove the index.
				if (index == constraint.Index)
					return; 
			}
			
			this.RemoveIndex(index);
		}

		internal void InitializeIndex (Index indx)
		{
			DataRow[] rows = new DataRow[this.Rows.Count];
			this.Rows.CopyTo (rows, 0);
			indx.Root = null;
			// fill index with table rows
			foreach(DataRow row in this.Rows) {
				if(row.RowState != DataRowState.Deleted) {
					indx.Insert(new Node(row), DataRowVersion.Default);
				}
			}
		}
	}
}
