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
#if NET_2_0
using System.Collections.Generic;
#endif
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using Mono.Data.SqlExpressions;

namespace System.Data {
	//[Designer]
	[ToolboxItem (false)]
	[DefaultEvent ("RowChanging")]
	[DefaultProperty ("TableName")]
	[DesignTimeVisible (false)]
	[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DataTableEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
	[Serializable]
	public partial class DataTable : MarshalByValueComponent, IListSource, ISupportInitialize, ISerializable {
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

		// Regex to parse the Sort string.
		static Regex SortRegex = new Regex ( @"^((\[(?<ColName>.+)\])|(?<ColName>\S+))([ ]+(?<Order>ASC|DESC))?$",
							RegexOptions.IgnoreCase|RegexOptions.ExplicitCapture);


		DataColumn [] _latestPrimaryKeyCols;
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
			_caseSensitive = false;	 	//default value
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
		public DataTable (string tableName)
			: this ()
		{
			_tableName = tableName;
		}

		/// <summary>
		/// Initializes a new instance of the DataTable class with the SerializationInfo and the StreamingContext.
		/// </summary>
		protected DataTable (SerializationInfo info, StreamingContext context)
			: this ()
		{
#if NET_2_0
			SerializationInfoEnumerator e = info.GetEnumerator ();
			SerializationFormat serializationFormat = SerializationFormat.Xml;

			while (e.MoveNext()) {
				if (e.ObjectType == typeof(System.Data.SerializationFormat)) {
					serializationFormat = (SerializationFormat) e.Value;
					break;
				}
			}
			if (serializationFormat == SerializationFormat.Xml) {
#endif
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
#if NET_2_0
			} else /*if (Tables.RemotingFormat == SerializationFormat.Binary)*/ {
				BinaryDeserializeTable (info);
			}
#endif
		}

		/// <summary>
		/// Indicates whether string comparisons within the table are case-sensitive.
		/// </summary>
#if !NET_2_0
		[DataSysDescription ("Indicates whether comparing strings within the table is case sensitive.")]
#endif
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

		internal ArrayList Indexes {
			get { return _indexes; }
		}

		internal void ChangedDataColumn (DataRow dr, DataColumn dc, object pv)
		{
			DataColumnChangeEventArgs e = new DataColumnChangeEventArgs (dr, dc, pv);
			OnColumnChanged (e);
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
			OnRowDeleting (e);
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
#if !NET_2_0
		[DataSysDescription ("Returns the child relations for this table.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataRelationCollection ChildRelations {
			get { return _childRelations; }
		}

		/// <summary>
		/// Gets the collection of columns that belong to this table.
		/// </summary>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("The collection that holds the columns for this table.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataColumnCollection Columns {
			get { return _columnCollection; }
		}

		/// <summary>
		/// Gets the collection of constraints maintained by this table.
		/// </summary>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("The collection that holds the constraints for this table.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public ConstraintCollection Constraints {
			get { return _constraintCollection; }
#if NET_2_0
			internal set { _constraintCollection = value; }
#endif
		}

		/// <summary>
		/// Gets the DataSet that this table belongs to.
		/// </summary>
		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("Indicates the DataSet to which this table belongs.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataSet DataSet {
			get { return dataSet; }
		}

		/// <summary>
		/// Gets a customized view of the table which may
		/// include a filtered view, or a cursor position.
		/// </summary>
		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("This is the default DataView for the table.")]
#endif
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
#if !NET_2_0
		[DataSysDescription ("The expression used to compute the data-bound value of this row.")]
#endif
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
#if !NET_2_0
		[DataSysDescription ("The collection that holds custom user information.")]
#endif
		public PropertyCollection ExtendedProperties {
			get { return _extendedProperties; }
		}

		/// <summary>
		/// Gets a value indicating whether there are errors in
		/// any of the_rows in any of the tables of the DataSet to
		/// which the table belongs.
		/// </summary>
		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("Returns whether the table has errors.")]
#endif
		public bool HasErrors {
			get {
				// we can not use the _hasError flag because we do not know when to turn it off!
				for (int i = 0; i < _rows.Count; i++) {
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
#if !NET_2_0
		[DataSysDescription ("Indicates a locale under which to compare strings within the table.")]
#endif
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

		internal bool LocaleSpecified {
			get { return _locale != null; }
		}

		/// <summary>
		/// Gets or sets the initial starting size for this table.
		/// </summary>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates an initial starting size for this table.")]
#endif
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
#if !NET_2_0
		[DataSysDescription ("Indicates the XML uri namespace for the elements contained in this table.")]
#endif
		public string Namespace {
			get {
				if (_nameSpace != null)
					return _nameSpace;
				if (DataSet != null)
					return DataSet.Namespace;
				return String.Empty;
			}
			set { _nameSpace = value; }
		}

		/// <summary>
		/// Gets the collection of parent relations for
		/// this DataTable.
		/// </summary>
		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("Returns the parent relations for this table.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataRelationCollection ParentRelations {
			get { return _parentRelations; }
		}

		/// <summary>
		/// Gets or sets the namespace for the XML represenation
		///  of the data stored in the DataTable.
		/// </summary>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the Prefix of the namespace used for this table in XML representation.")]
#endif
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
#if !NET_2_0
		[DataSysDescription ("Indicates the column(s) that represent the primary key for this table.")]
#endif
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.PrimaryKeyEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		[TypeConverterAttribute ("System.Data.PrimaryKeyTypeConverter, " + Consts.AssemblySystem_Data)]
		public DataColumn[] PrimaryKey {
			get {
				if (_primaryKeyConstraint == null)
					return new DataColumn[] {};
				return _primaryKeyConstraint.Columns;
			}
			set {
				if (value == null || value.Length == 0) {
					if (_primaryKeyConstraint != null) {
						_primaryKeyConstraint.SetIsPrimaryKey (false);
						Constraints.Remove(_primaryKeyConstraint);
						_primaryKeyConstraint = null;
					}
					return;
				}

				if (InitInProgress) {
					_latestPrimaryKeyCols = value;
					return;
				}

				// first check if value is the same as current PK.
				if (_primaryKeyConstraint != null &&
				    DataColumn.AreColumnSetsTheSame (value, _primaryKeyConstraint.Columns))
					return;

				//Does constraint exist for these columns
				UniqueConstraint uc = UniqueConstraint.GetUniqueConstraintForColumnSet (this.Constraints, (DataColumn[]) value);

				//if constraint doesn't exist for columns
				//create new unique primary key constraint
				if (null == uc) {
					foreach (DataColumn Col in (DataColumn []) value) {
						if (Col.Table == null)
							break;

						if (Columns.IndexOf (Col) < 0)
							throw new ArgumentException ("PrimaryKey columns do not belong to this table.");
					}
					// create constraint with primary key indication set to false
					// to avoid recursion
					uc = new UniqueConstraint ((DataColumn []) value, false);
					Constraints.Add (uc);
				}

				//Remove the existing primary key
				if (_primaryKeyConstraint != null) {
					_primaryKeyConstraint.SetIsPrimaryKey (false);
					Constraints.Remove (_primaryKeyConstraint);
					_primaryKeyConstraint = null;
				}

				//set the constraint as the new primary key
				UniqueConstraint.SetAsPrimaryKey (Constraints, uc);
				_primaryKeyConstraint = uc;

				for (int j = 0; j < uc.Columns.Length; ++j)
					uc.Columns [j].AllowDBNull = false;
			}
		}

		internal UniqueConstraint PrimaryKeyConstraint {
			get { return _primaryKeyConstraint; }
		}

		/// <summary>
		/// Gets the collection of_rows that belong to this table.
		/// </summary>
		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("Indicates the collection that holds the rows of data for this table.")]
#endif
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
#if !NET_2_0
		[DataSysDescription ("Indicates the name used to look up this table in the Tables collection of a DataSet.")]
#endif
		[DefaultValue ("")]
		[RefreshProperties (RefreshProperties.All)]
		public string TableName {
			get { return _tableName == null ? "" : _tableName; }
			set { _tableName = value; }
		}

		bool IListSource.ContainsListCollection {
			// the collection is a DataView
			get { return false; }
		}

		internal RecordCache RecordCache {
			get { return _recordCache; }
		}

		private DataRowBuilder RowBuilder {
			get {
				// initiate only one row builder.
				if (_rowBuilder == null)
					_rowBuilder = new DataRowBuilder (this, -1, 0);
				else
					// new row get id -1.
					_rowBuilder._rowId = -1;

				return _rowBuilder;
			}
		}

		internal bool EnforceConstraints {
			get { return enforceConstraints; }
			set {
				if (value == enforceConstraints)
					return;

				if (value) {
					// reset indexes since they may be outdated
					ResetIndexes();

					// assert all constraints
					foreach (Constraint constraint in Constraints)
						constraint.AssertConstraint ();

					AssertNotNullConstraints ();

					if (HasErrors)
						Constraint.ThrowConstraintException ();
				}
				enforceConstraints = value;
			}
		}

		internal void AssertNotNullConstraints ()
		{
			if (_duringDataLoad && !_nullConstraintViolationDuringDataLoad)
				return;

			bool seen = false;
			for (int i = 0; i < Columns.Count; i++) {
				DataColumn column = Columns [i];
				if (column.AllowDBNull)
					continue;
				for (int j = 0; j < Rows.Count; j++) {
					if (Rows [j].HasVersion (DataRowVersion.Default) && Rows[j].IsNull (column)) {
						seen = true;
						string errMsg = String.Format ("Column '{0}' does not allow DBNull.Value.",
									       column.ColumnName);
						Rows [j].SetColumnError (i, errMsg);
						Rows [j].RowError = errMsg;
					}
				}
			}
			_nullConstraintViolationDuringDataLoad = seen;
		}

		internal bool RowsExist (DataColumn [] columns, DataColumn [] relatedColumns, DataRow row)
		{
			int curIndex = row.IndexFromVersion (DataRowVersion.Default);
			int tmpRecord = RecordCache.NewRecord ();

			try {
				for (int i = 0; i < relatedColumns.Length; i++)
					// according to MSDN: the DataType value for both columns must be identical.
					columns [i].DataContainer.CopyValue (relatedColumns [i].DataContainer, curIndex, tmpRecord);
				return RowsExist (columns, tmpRecord);
			} finally {
				RecordCache.DisposeRecord (tmpRecord);
			}
		}

		bool RowsExist (DataColumn [] columns, int index)
		{
			Index indx = this.FindIndex (columns);

			if (indx != null)
				return indx.Find (index) != -1;

			// we have to perform full-table scan
			// check that there is a parent for this row.
			foreach (DataRow thisRow in this.Rows) {
				if (thisRow.RowState == DataRowState.Deleted)
					continue;
				// check if the values in the columns are equal
				int thisIndex = thisRow.IndexFromVersion (
					thisRow.RowState == DataRowState.Modified ? DataRowVersion.Original : DataRowVersion.Current);
				bool match = true;
				foreach (DataColumn column in columns) {
					if (column.DataContainer.CompareValues (thisIndex, index) != 0) {
						match = false;
						break;
					}
				}
				if (match)
					return true;
			}
			return false;
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
			for (int i = 0; i < Rows.Count; ) {
				myRow = Rows [i];
				myRow.AcceptChanges ();

				// if the row state is Detached it meens that it was removed from row list (Rows)
				// so we should not increase 'i'.
				if (myRow.RowState != DataRowState.Detached)
					i++;
			}
			_rows.OnListChanged (this, new ListChangedEventArgs (ListChangedType.Reset, -1, -1));
		}

		/// <summary>
		/// Begins the initialization of a DataTable that is used
		/// on a form or used by another component. The initialization
		/// occurs at runtime.
		/// </summary>
		public
#if NET_2_0
		virtual
#endif
		void BeginInit ()
		{
			InitInProgress = true;
#if NET_2_0
			tableInitialized = false;
#endif
		}

		/// <summary>
		/// Turns off notifications, index maintenance, and
		/// constraints while loading data.
		/// </summary>
		public void BeginLoadData ()
		{
			if (this._duringDataLoad)
				return;

			//duringDataLoad is important to EndLoadData and
			//for not throwing unexpected exceptions.
			this._duringDataLoad = true;
			this._nullConstraintViolationDuringDataLoad = false;

			if (this.dataSet != null) {
				//Saving old Enforce constraints state for later
				//use in the EndLoadData.
				this.dataSetPrevEnforceConstraints = this.dataSet.EnforceConstraints;
				this.dataSet.EnforceConstraints = false;
			} else {
				//if table does not belong to any data set use EnforceConstraints of the table
				this.EnforceConstraints = false;
			}
			return;
		}

		/// <summary>
		/// Clears the DataTable of all data.
		/// </summary>
		public void Clear ()
		{
			// Foriegn key constraints are checked in _rows.Clear method
			_rows.Clear ();
		}

		/// <summary>
		/// Clones the structure of the DataTable, including
		///  all DataTable schemas and constraints.
		/// </summary>
		public virtual DataTable Clone ()
		{
			 // Use Activator so we can use non-public constructors.
			DataTable Copy = (DataTable) Activator.CreateInstance (GetType (), true);
			CopyProperties (Copy);
			return Copy;
		}

		/// <summary>
		/// Computes the given expression on the current_rows that
		/// pass the filter criteria.
		/// </summary>
		public object Compute (string expression, string filter)
		{
			// expression is an aggregate function
			// filter is an expression used to limit rows

			DataRow [] rows = Select (filter);

			if (rows == null || rows.Length == 0)
				return DBNull.Value;

			Parser parser = new Parser (rows);
			IExpression expr = parser.Compile (expression);
			object obj = expr.Eval (rows [0]);

			return obj;
		}

		/// <summary>
		/// Copies both the structure and data for this DataTable.
		/// </summary>
		public DataTable Copy ()
		{
			DataTable copy = Clone ();

			copy._duringDataLoad = true;
			foreach (DataRow row in Rows) {
				DataRow newRow = copy.NewNotInitializedRow ();
				copy.Rows.AddInternal (newRow);
				CopyRow (row, newRow);
			}
			copy._duringDataLoad = false;

			// rebuild copy indexes after loading all rows
			copy.ResetIndexes ();
			return copy;
		}

		internal void CopyRow (DataRow fromRow, DataRow toRow)
		{
			if (fromRow.HasErrors)
				fromRow.CopyErrors (toRow);

			if (fromRow.HasVersion (DataRowVersion.Original))
				toRow.Original = toRow.Table.RecordCache.CopyRecord (this, fromRow.Original, -1);

			if (fromRow.HasVersion (DataRowVersion.Current)) {
				if (fromRow.Original != fromRow.Current)
					toRow.Current = toRow.Table.RecordCache.CopyRecord (this, fromRow.Current, -1);
				else
					toRow.Current = toRow.Original;
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
			if (ExtendedProperties.Count > 0) {
				//  Cannot copy extended properties directly as the property does not have a set accessor
				Array tgtArray = Array.CreateInstance (typeof (object), ExtendedProperties.Count);
				ExtendedProperties.Keys.CopyTo (tgtArray, 0);
				for (int i=0; i < ExtendedProperties.Count; i++)
					Copy.ExtendedProperties.Add (tgtArray.GetValue (i), ExtendedProperties[tgtArray.GetValue (i)]);
			}
			Copy._locale = _locale;
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
				if (isEmpty || !Copy.Columns.Contains (column.ColumnName))
					Copy.Columns.Add (column.Clone ());
			}

			CopyConstraints (Copy);
			// add primary key to the copy
			if (PrimaryKey.Length > 0) {
				DataColumn[] pColumns = new DataColumn[PrimaryKey.Length];
				for (int i = 0; i < pColumns.Length; i++)
					pColumns[i] = Copy.Columns[PrimaryKey[i].ColumnName];

				Copy.PrimaryKey = pColumns;
			}
		}

		private void CopyConstraints (DataTable copy)
		{
			UniqueConstraint origUc;
			UniqueConstraint copyUc;
			for (int i = 0; i < this.Constraints.Count; i++) {
				if (this.Constraints[i] is UniqueConstraint) {
					// typed ds can already contain the constraints
					if (copy.Constraints.Contains (this.Constraints [i].ConstraintName))
						continue;

					origUc = (UniqueConstraint) this.Constraints [i];
					DataColumn [] columns = new DataColumn [origUc.Columns.Length];
					for (int j = 0; j < columns.Length; j++)
						columns[j] = copy.Columns [origUc.Columns [j].ColumnName];

					copyUc = new UniqueConstraint (origUc.ConstraintName, columns, origUc.IsPrimaryKey);
					copy.Constraints.Add (copyUc);
				}
			}
		}
		/// <summary>
		/// Ends the initialization of a DataTable that is used
		/// on a form or used by another component. The
		/// initialization occurs at runtime.
		/// </summary>
		public
#if NET_2_0
		virtual
#endif
		void EndInit ()
		{
			InitInProgress = false;
			DataTableInitialized ();
			FinishInit ();
		}

		// defined in NET_2_0 profile
		partial void DataTableInitialized ();

		internal bool InitInProgress {
			get { return fInitInProgress; }
			set { fInitInProgress = value; }
		}

		internal void FinishInit ()
		{
			UniqueConstraint oldPK = _primaryKeyConstraint;

			// Columns shud be added 'before' the constraints
			Columns.PostAddRange ();

			// Add the constraints
			_constraintCollection.PostAddRange ();

			// ms.net behavior : If a PrimaryKey (UniqueConstraint) is added thru AddRange,
			// then it takes precedence over an direct assignment of PrimaryKey
			if (_primaryKeyConstraint == oldPK)
				PrimaryKey = _latestPrimaryKeyCols;
		}

		/// <summary>
		/// Turns on notifications, index maintenance, and
		/// constraints after loading data.
		/// </summary>
		public void EndLoadData ()
		{
			if (this._duringDataLoad) {
				//Getting back to previous EnforceConstraint state
				if (this.dataSet != null)
					this.dataSet.InternalEnforceConstraints (this.dataSetPrevEnforceConstraints, true);
				else
					this.EnforceConstraints = true;

				this._duringDataLoad = false;
			}
		}

		/// <summary>
		/// Gets a copy of the DataTable that contains all
		///  changes made to it since it was loaded or
		///  AcceptChanges was last called.
		/// </summary>
		public DataTable GetChanges ()
		{
			return GetChanges (DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
		}

		/// <summary>
		/// Gets a copy of the DataTable containing all
		/// changes made to it since it was last loaded, or
		/// since AcceptChanges was called, filtered by DataRowState.
		/// </summary>
		public DataTable GetChanges (DataRowState rowStates)
		{
			DataTable copyTable = null;

			foreach (DataRow row in Rows) {
				// The spec says relationship constraints may cause Unchanged parent rows to be included but
				// MS .NET 1.1 does not include Unchanged rows even if their child rows are changed.
				if (!row.IsRowChanged (rowStates))
					continue;
				if (copyTable == null)
					copyTable = Clone ();
				DataRow newRow = copyTable.NewNotInitializedRow ();
				// Don't check for ReadOnly, when cloning data to new uninitialized row.
				row.CopyValuesToRow (newRow, false);
#if NET_2_0
				newRow.XmlRowID = row.XmlRowID;
#endif
				copyTable.Rows.AddInternal (newRow);
			}

			return copyTable;
		}

		/// <summary>
		/// Gets an array of DataRow objects that contain errors.
		/// </summary>
		public DataRow [] GetErrors ()
		{
			ArrayList errors = new ArrayList();
			for (int i = 0; i < _rows.Count; i++) {
				if (_rows[i].HasErrors)
					errors.Add (_rows[i]);
			}

			DataRow[] ret = NewRowArray (errors.Count);
			errors.CopyTo (ret, 0);
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
			if (row.RowState == DataRowState.Detached)
				return;

			DataRow newRow = NewNotInitializedRow ();

			int original = -1;
			if (row.HasVersion (DataRowVersion.Original)) {
				original = row.IndexFromVersion (DataRowVersion.Original);
				newRow.Original = RecordCache.NewRecord ();
				RecordCache.CopyRecord (row.Table, original, newRow.Original);
			}

			if (row.HasVersion (DataRowVersion.Current)) {
				int current = row.IndexFromVersion (DataRowVersion.Current);
				if (current == original) {
					newRow.Current = newRow.Original;
				} else {
					newRow.Current = RecordCache.NewRecord ();
					RecordCache.CopyRecord (row.Table, current, newRow.Current);
				}
			}

			//Import the row only if RowState is not detached
			//Validation for Deleted Rows happens during Accept/RejectChanges
			if (row.RowState != DataRowState.Deleted)
				newRow.Validate ();
			else
				AddRowToIndexes (newRow);
			Rows.AddInternal(newRow);

			if (row.HasErrors)
				row.CopyErrors (newRow);
		}

		internal int DefaultValuesRowIndex {
			get { return _defaultValuesRowIndex; }
		}

		/// <summary>
		/// This member is only meant to support Mono's infrastructure
		/// </summary>
#if NET_2_0
		public virtual
#endif
		void
#if !NET_2_0
		ISerializable.
#endif
		GetObjectData (SerializationInfo info, StreamingContext context)
		{
#if NET_2_0
			if (RemotingFormat == SerializationFormat.Xml) {
#endif
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
#if NET_2_0
			} else /*if (RemotingFormat == SerializationFormat.Binary)*/ {
				BinarySerializeProperty (info);
				if (dataSet == null) {
					for (int i = 0; i < Columns.Count; i++) {
						info.AddValue ("DataTable.DataColumn_" + i + ".Expression",
							       Columns[i].Expression);
					}
					BinarySerialize (info, "DataTable_0.");
				}
			}
#endif
		}

		/// <summary>
		/// Finds and updates a specific row. If no matching row
		///  is found, a new row is created using the given values.
		/// </summary>
		public DataRow LoadDataRow (object [] values, bool fAcceptChanges)
		{
			DataRow row = null;
			if (PrimaryKey.Length == 0) {
				row = Rows.Add (values);
			} else {
				EnsureDefaultValueRowIndex ();
				int newRecord = CreateRecord (values);
				int existingRecord = _primaryKeyConstraint.Index.Find (newRecord);

				if (existingRecord < 0) {
					row = NewRowFromBuilder (RowBuilder);
					row.Proposed = newRecord;
					Rows.AddInternal(row);
					if (!_duringDataLoad)
						AddRowToIndexes (row);
				} else {
					row = RecordCache [existingRecord];
					row.BeginEdit ();
					row.ImportRecord (newRecord);
					row.EndEdit ();
				}
			}

			if (fAcceptChanges)
				row.AcceptChanges ();

			return row;
		}

		internal DataRow LoadDataRow (IDataRecord record, int[] mapping, int length, bool fAcceptChanges)
		{
			DataRow row = null;
			int tmpRecord = this.RecordCache.NewRecord ();
			try {
				RecordCache.ReadIDataRecord (tmpRecord,record,mapping,length);
				if (PrimaryKey.Length != 0) {
					bool hasPrimaryValues = true;
					foreach(DataColumn col in PrimaryKey) {
						if(!(col.Ordinal < mapping.Length)) {
							hasPrimaryValues = false;
							break;
						}
					}

					if (hasPrimaryValues) {
						int existingRecord = _primaryKeyConstraint.Index.Find (tmpRecord);
						if (existingRecord != -1)
							row  = RecordCache [existingRecord];
					}
				}

				if (row == null) {
					row = NewNotInitializedRow ();
					row.Proposed = tmpRecord;
					Rows.AddInternal (row);
				} else {
					row.BeginEdit ();
					row.ImportRecord (tmpRecord);
					row.EndEdit ();
				}

				if (fAcceptChanges)
					row.AcceptChanges ();

			} catch {
				this.RecordCache.DisposeRecord (tmpRecord);
				throw;
			}
			return row;
		}

		/// <summary>
		/// Creates a new DataRow with the same schema as the table.
		/// </summary>
		public DataRow NewRow ()
		{
			EnsureDefaultValueRowIndex();

			DataRow newRow = NewRowFromBuilder (RowBuilder);
			newRow.Proposed = CreateRecord (null);
			NewRowAdded (newRow);
			return newRow;
		}

		// defined in the NET_2_0 profile
		partial void NewRowAdded (DataRow dr);

		internal int CreateRecord (object [] values)
		{
			int valCount = values != null ? values.Length : 0;
			if (valCount > Columns.Count)
				throw new ArgumentException ("Input array is longer than the number of columns in this table.");

			int index = RecordCache.NewRecord ();

			try {
				for (int i = 0; i < valCount; i++) {
					object value = values[i];
					if (value == null)
						Columns [i].SetDefaultValue (index);
					else
						Columns [i][index] = values [i];
				}

				for(int i = valCount; i < Columns.Count; i++)
					Columns [i].SetDefaultValue (index);

				return index;
			} catch {
				RecordCache.DisposeRecord (index);
				throw;
			}
		}

		private void EnsureDefaultValueRowIndex ()
		{
			// initialize default values row for the first time
			if (_defaultValuesRowIndex == -1) {
				_defaultValuesRowIndex = RecordCache.NewRecord();
				for (int i = 0; i < Columns.Count; ++i) {
					DataColumn column = Columns [i];
					column.DataContainer [_defaultValuesRowIndex] = column.DefaultValue;
				}
			}
		}

		/// <summary>
		/// This member supports the .NET Framework infrastructure
		///  and is not intended to be used directly from your code.
		/// </summary>
		DataRow [] empty_rows;
		protected internal DataRow [] NewRowArray (int size)
		{
			if (size == 0 && empty_rows != null)
				return empty_rows;
			Type t = GetRowType ();
			/* Avoid reflection if possible */
			DataRow [] rows = t == typeof (DataRow) ? new DataRow [size] : (DataRow []) Array.CreateInstance (t, size);
			if (size == 0)
				empty_rows = rows;
			return rows;
		}

		/// <summary>
		/// Creates a new row from an existing row.
		/// </summary>
		protected virtual DataRow NewRowFromBuilder (DataRowBuilder builder)
		{
			return new DataRow (builder);
		}

		internal DataRow NewNotInitializedRow ()
		{
			EnsureDefaultValueRowIndex ();

			return NewRowFromBuilder (RowBuilder);
		}

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
			Clear ();
			while (ParentRelations.Count > 0) {
				if (dataSet.Relations.Contains (ParentRelations [ParentRelations.Count - 1].RelationName))
					dataSet.Relations.Remove (ParentRelations [ParentRelations.Count - 1]);
			}

			while (ChildRelations.Count > 0) {
				if (dataSet.Relations.Contains (ChildRelations [ChildRelations.Count - 1].RelationName))
					dataSet.Relations.Remove (ChildRelations [ChildRelations.Count - 1]);
			}
			Constraints.Clear ();
			Columns.Clear ();
		}

		/// <summary>
		/// Gets an array of all DataRow objects.
		/// </summary>
		public DataRow[] Select ()
		{
			return Select (String.Empty, String.Empty, DataViewRowState.CurrentRows);
		}

		/// <summary>
		/// Gets an array of all DataRow objects that match
		/// the filter criteria in order of primary key (or
		/// lacking one, order of addition.)
		/// </summary>
		public DataRow[] Select (string filterExpression)
		{
			return Select (filterExpression, String.Empty, DataViewRowState.CurrentRows);
		}

		/// <summary>
		/// Gets an array of all DataRow objects that
		/// match the filter criteria, in the the
		/// specified sort order.
		/// </summary>
		public DataRow[] Select (string filterExpression, string sort)
		{
			return Select (filterExpression, sort, DataViewRowState.CurrentRows);
		}

		/// <summary>
		/// Gets an array of all DataRow objects that match
		/// the filter in the order of the sort, that match
		/// the specified state.
		/// </summary>
		public DataRow [] Select (string filterExpression, string sort, DataViewRowState recordStates)
		{
			if (filterExpression == null)
				filterExpression = String.Empty;

			IExpression filter = null;
			if (filterExpression != String.Empty) {
				Parser parser = new Parser ();
				filter = parser.Compile (filterExpression);
			}

			DataColumn [] columns = _emptyColumnArray;
			ListSortDirection [] sorts = null;

			if (sort != null && !sort.Equals(String.Empty))
				columns = ParseSortString (this, sort, out sorts, false);

			if (Rows.Count == 0)
				return NewRowArray (0);

			//if sort order is not given, sort it in Ascending order of the
			//columns involved in the filter
			if (columns.Length == 0 && filter != null) {
				ArrayList list = new ArrayList ();
				for (int i = 0; i < Columns.Count; ++i) {
					if (!filter.DependsOn (Columns [i]))
						continue;
					list.Add (Columns [i]);
				}
				columns = (DataColumn []) list.ToArray (typeof (DataColumn));
			}

			bool addIndex = true;
			if (filterExpression != String.Empty)
				addIndex = false;
			Index index = GetIndex (columns, sorts, recordStates, filter, false, addIndex);

			int [] records = index.GetAll ();
			DataRow [] dataRows = NewRowArray (index.Size);
			for (int i = 0; i < dataRows.Length; i++)
				dataRows [i] = RecordCache [records [i]];

			return dataRows;
		}

		private void AddIndex (Index index)
		{
			if (_indexes == null)
				_indexes = new ArrayList();
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
		internal Index GetIndex (DataColumn[] columns, ListSortDirection[] sort, DataViewRowState rowState, IExpression filter, bool reset)
		{
			return GetIndex (columns, sort, rowState, filter, reset, true);
		}

		internal Index GetIndex (DataColumn[] columns, ListSortDirection[] sort,
					 DataViewRowState rowState, IExpression filter,
					 bool reset, bool addIndex)
		{
			Index index = FindIndex(columns, sort, rowState, filter);
			if (index == null) {
				index = new Index(new Key (this, columns, sort, rowState, filter));

				if (addIndex)
					AddIndex (index);
			} else if (reset) {
				// reset existing index only if asked for this
				index.Reset ();
			}
			return index;
		}

		internal Index FindIndex (DataColumn[] columns)
		{
			return FindIndex (columns, null, DataViewRowState.None, null);
		}

		internal Index FindIndex (DataColumn[] columns, ListSortDirection[] sort, DataViewRowState rowState, IExpression filter)
		{
			if (Indexes != null) {
				foreach (Index index in Indexes) {
					if (index.Key.Equals (columns,sort,rowState, filter))
						return index;
				}
			}
			return null;
		}

		internal void ResetIndexes ()
		{
			foreach(Index index in Indexes)
				index.Reset ();
		}

		internal void ResetCaseSensitiveIndexes ()
		{
			foreach (Index index in Indexes) {
				bool containsStringcolumns = false;
				foreach(DataColumn column in index.Key.Columns) {
					if (column.DataType == typeof(string)) {
						containsStringcolumns = true;
						break;
					}
				}

				if (!containsStringcolumns && index.Key.HasFilter) {
					foreach (DataColumn column in Columns) {
						if ((column.DataType == DbTypes.TypeOfString) && (index.Key.DependsOn (column))) {
							containsStringcolumns = true;
							break;
						}
					}
				}

				if (containsStringcolumns)
					index.Reset ();
			}
		}

		internal void DropIndex (Index index)
		{
			if (index != null && index.RefCount == 0) {
				_indexes.Remove (index);
			}
		}

		internal void DropReferencedIndexes (DataColumn column)
		{
			if (_indexes != null)
				for (int i = _indexes.Count - 1; i >= 0; i--) {
					Index indx = (Index)_indexes [i];
					if (indx.Key.DependsOn (column))
						_indexes.Remove (indx);
				}
		}

		internal void AddRowToIndexes (DataRow row)
		{
			if (_indexes != null) {
				for (int i = 0; i < _indexes.Count; ++i)
					((Index)_indexes [i]).Add (row);
			}
		}

		internal void DeleteRowFromIndexes (DataRow row)
		{
			if (_indexes != null) {
				foreach (Index indx in _indexes)
					indx.Delete (row);
			}
		}

		/// <summary>
		/// Gets the TableName and DisplayExpression, if
		/// there is one as a concatenated string.
		/// </summary>
		public override string ToString ()
		{
			//LAMESPEC: spec says concat the two. impl puts a
			//plus sign infront of DisplayExpression
			string retVal = TableName;
			if(DisplayExpression != null && DisplayExpression != "")
				retVal += " + " + DisplayExpression;
			return retVal;
		}

		#region Events

		/// <summary>
		/// Raises the ColumnChanged event.
		/// </summary>
		protected virtual void OnColumnChanged (DataColumnChangeEventArgs e)
		{
			if (null != ColumnChanged)
				ColumnChanged (this, e);
		}

		internal void RaiseOnColumnChanged (DataColumnChangeEventArgs e)
		{
			OnColumnChanged (e);
		}

		/// <summary>
		/// Raises the ColumnChanging event.
		/// </summary>
		protected virtual void OnColumnChanging (DataColumnChangeEventArgs e)
		{
			if (null != ColumnChanging)
				ColumnChanging (this, e);
		}

		internal void RaiseOnColumnChanging (DataColumnChangeEventArgs e)
		{
			OnColumnChanging(e);
		}

		/// <summary>
		/// Raises the PropertyChanging event.
		/// </summary>
		[MonoTODO]
		protected internal virtual void OnPropertyChanging (PropertyChangedEventArgs pcevent)
		{
			//if (null != PropertyChanging)
			//{
			//	PropertyChanging (this, pcevent);
			//}
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Notifies the DataTable that a DataColumn is being removed.
		/// </summary>
		protected internal virtual void OnRemoveColumn (DataColumn column)
		{
			DropReferencedIndexes (column);
		}

		/// <summary>
		/// Raises the RowChanged event.
		/// </summary>
		protected virtual void OnRowChanged (DataRowChangeEventArgs e)
		{
			if (null != RowChanged)
				RowChanged (this, e);
		}


		/// <summary>
		/// Raises the RowChanging event.
		/// </summary>
		protected virtual void OnRowChanging (DataRowChangeEventArgs e)
		{
			if (null != RowChanging)
				RowChanging (this, e);
		}

		/// <summary>
		/// Raises the RowDeleted event.
		/// </summary>
		protected virtual void OnRowDeleted (DataRowChangeEventArgs e)
		{
			if (null != RowDeleted)
				RowDeleted (this, e);
		}

		/// <summary>
		/// Raises the RowDeleting event.
		/// </summary>
		protected virtual void OnRowDeleting (DataRowChangeEventArgs e)
		{
			if (null != RowDeleting)
				RowDeleting (this, e);
		}

		/// <summary>
		/// Occurs when after a value has been changed for
		/// the specified DataColumn in a DataRow.
		/// </summary>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Occurs when a value has been changed for this column.")]
#endif
		public event DataColumnChangeEventHandler ColumnChanged;

		/// <summary>
		/// Occurs when a value is being changed for the specified
		/// DataColumn in a DataRow.
		/// </summary>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Occurs when a value has been submitted for this column.  The user can modify the proposed value and should throw an exception to cancel the edit.")]
#endif
		public event DataColumnChangeEventHandler ColumnChanging;

		/// <summary>
		/// Occurs after a DataRow has been changed successfully.
		/// </summary>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Occurs after a row in the table has been successfully edited.")]
#endif
		public event DataRowChangeEventHandler RowChanged;

		/// <summary>
		/// Occurs when a DataRow is changing.
		/// </summary>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Occurs when the row is being changed so that the event handler can modify or cancel the change. The user can modify values in the row and should throw an  exception to cancel the edit.")]
#endif
		public event DataRowChangeEventHandler RowChanging;

		/// <summary>
		/// Occurs after a row in the table has been deleted.
		/// </summary>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Occurs after a row in the table has been successfully deleted.")]
#endif
		public event DataRowChangeEventHandler RowDeleted;

		/// <summary>
		/// Occurs before a row in the table is about to be deleted.
		/// </summary>
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Occurs when a row in the table marked for deletion.  Throw an exception to cancel the deletion.")]
#endif
		public event DataRowChangeEventHandler RowDeleting;

		#endregion // Events

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
					string rawColumnName = columnExpression[c].Trim ();

					Match match = SortRegex.Match (rawColumnName);
					Group g = match.Groups["ColName"] ;
					if (!g.Success)
						throw new IndexOutOfRangeException ("Could not find column: " + rawColumnName);

					string columnName = g.Value;
					DataColumn dc = table.Columns[columnName];
					if (dc == null){
						try {
							dc = table.Columns[Int32.Parse (columnName)];
						} catch (FormatException) {
							throw new IndexOutOfRangeException("Cannot find column " + columnName);
						}
					}
					columns.Add (dc);

					g = match.Groups["Order"];
					if (!g.Success || String.Compare (g.Value, "ASC", true, CultureInfo.InvariantCulture) == 0)
						sorts.Add(ListSortDirection.Ascending);
					else
						sorts.Add (ListSortDirection.Descending);
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

		private void UpdatePropertyDescriptorsCache ()
		{
			PropertyDescriptor[] descriptors = new PropertyDescriptor[Columns.Count + ChildRelations.Count];
			int index = 0;
			foreach (DataColumn col in Columns)
				descriptors [index++] = new DataColumnPropertyDescriptor (col);

			foreach (DataRelation rel in ChildRelations)
				descriptors [index++] = new DataRelationPropertyDescriptor (rel);

			_propertyDescriptorsCache = new PropertyDescriptorCollection (descriptors);
		}

		internal PropertyDescriptorCollection GetPropertyDescriptorCollection()
		{
			if (_propertyDescriptorsCache == null)
				UpdatePropertyDescriptorsCache ();
			return _propertyDescriptorsCache;
		}

		internal void ResetPropertyDescriptorsCache ()
		{
			_propertyDescriptorsCache = null;
		}

		internal void SetRowsID()
		{
			int dataRowID = 0;
			foreach (DataRow row in Rows) {
				row.XmlRowID = dataRowID;
				dataRowID++;
			}
		}
	}

#if NET_2_0
	[XmlSchemaProvider ("GetDataTableSchema")]
	partial class DataTable : IXmlSerializable {
		[MonoNotSupported ("")]
		XmlSchema IXmlSerializable.GetSchema ()
		{
			return GetSchema ();
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			ReadXml_internal (reader, true);
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			DataSet dset = dataSet;
			bool isPartOfDataSet = true;

			if (dataSet == null) {
				dset = new DataSet ();
				dset.Tables.Add (this);
				isPartOfDataSet = false;
			}

			XmlSchemaWriter.WriteXmlSchema (writer, new DataTable [] { this },
											null, TableName, dset.DataSetName, LocaleSpecified ? Locale : dset.LocaleSpecified ? dset.Locale : null);
			dset.WriteIndividualTableContent (writer, this, XmlWriteMode.DiffGram);
			writer.Flush ();

			if (!isPartOfDataSet)
				dataSet.Tables.Remove(this);
		}

		[MonoTODO]
		protected virtual XmlSchema GetSchema ()
		{
			throw new NotImplementedException ();
		}

		public static XmlSchemaComplexType GetDataTableSchema (XmlSchemaSet schemaSet)
		{
			return new XmlSchemaComplexType ();
		}

		public XmlReadMode ReadXml (Stream stream)
		{
			return ReadXml (new XmlTextReader(stream, null));
		}

		public XmlReadMode ReadXml (string fileName)
		{
			XmlReader reader = new XmlTextReader (fileName);
			try {
				return ReadXml (reader);
			} finally {
				reader.Close();
			}
		}

		public XmlReadMode ReadXml (TextReader reader)
		{
			return ReadXml (new XmlTextReader (reader));
		}

		public XmlReadMode ReadXml (XmlReader reader)
		{
			return ReadXml_internal (reader, false);
		}

		public XmlReadMode ReadXml_internal (XmlReader reader, bool serializable)
		{
			// The documentation from MS for this method is rather
			// poor.  The following cases have been observed
			// during testing:
			//
			//     Reading a table from XML may create a DataSet to
			//     store child tables.
			//
			//     If the table has at least one column present,
			//     we do not require the schema to be present in
			//     the xml.  If the table has no columns, neither
			//     regular data nor diffgrams will be read, but
			//     will throw an error indicating that schema
			//     will not be inferred.
			//
			//     We will likely need to take advantage of the
			//     msdata:MainDataTable attribute added to the
			//     schema info to load into the appropriate
			//     locations.
			bool isPartOfDataSet = true;
			bool isTableNameBlank = false;
			XmlReadMode mode = XmlReadMode.ReadSchema;
			DataSet dataSet = null;
			DataSet ds = new DataSet ();

			reader.MoveToContent ();
			if (Columns.Count > 0 && reader.LocalName != "diffgram" || serializable)
				mode = ds.ReadXml (reader);
			else if (Columns.Count > 0 && reader.LocalName == "diffgram") {
				  try {
					if (TableName == String.Empty)
						isTableNameBlank = true;
					if (DataSet == null) {
						isPartOfDataSet = false;
						ds.Tables.Add (this);
						mode = ds.ReadXml (reader);
					} else
						mode = DataSet.ReadXml (reader);
				  } catch (DataException) {
					mode = XmlReadMode.DiffGram;
					if (isTableNameBlank)
						TableName = String.Empty;
				  } finally {
					if (!isPartOfDataSet)
						ds.Tables.Remove (this);
				  }
				  return mode;
			} else {
				mode = ds.ReadXml (reader, XmlReadMode.ReadSchema);
			}
			if (mode == XmlReadMode.InferSchema)
				mode = XmlReadMode.IgnoreSchema;
			if (DataSet == null) {
				  isPartOfDataSet = false;
				  dataSet = new DataSet ();
				  if (TableName == String.Empty)
					isTableNameBlank = true;
				  dataSet.Tables.Add (this);
			}

			DenyXmlResolving (this, ds, mode, isTableNameBlank, isPartOfDataSet);
			if (Columns.Count > 0 && TableName != ds.Tables [0].TableName) {
				if (isPartOfDataSet == false)
					dataSet.Tables.Remove (this);

				if (isTableNameBlank && isPartOfDataSet == false)
					TableName = String.Empty;
				return mode;
			}
			else {
				TableName = ds.Tables [0].TableName;
			}

			if (!isPartOfDataSet) {
				if (Columns.Count > 0) {
					dataSet.Merge (ds, true, MissingSchemaAction.Ignore);
				} else {
					dataSet.Merge (ds, true, MissingSchemaAction.AddWithKey);
				}
				if (ChildRelations.Count == 0) {
					dataSet.Tables.Remove (this);
				} else {
					dataSet.DataSetName = ds.DataSetName;
				}
			} else {
				if (Columns.Count > 0) {
					DataSet.Merge (ds, true, MissingSchemaAction.Ignore);
				} else {
					DataSet.Merge (ds, true, MissingSchemaAction.AddWithKey);
				}
			}
			return mode;
		}

		private void DenyXmlResolving (DataTable table, DataSet ds, XmlReadMode mode, bool isTableNameBlank, bool isPartOfDataSet)
		{
			if (ds.Tables.Count == 0 && table.Columns.Count == 0)
				throw new InvalidOperationException ("DataTable does not support schema inference from XML");

			if (table.Columns.Count == 0 && ds.Tables [0].TableName != table.TableName && isTableNameBlank == false)
				throw new ArgumentException (String.Format ("DataTable '{0}' does not match to any DataTable in source",
									    table.TableName));

			if (table.Columns.Count > 0 && ds.Tables [0].TableName != table.TableName &&
			    isTableNameBlank == false && mode == XmlReadMode.ReadSchema &&
			    isPartOfDataSet == false)
				throw new ArgumentException (String.Format ("DataTable '{0}' does not match to any DataTable in source",
									    table.TableName));

			if (isPartOfDataSet == true && table.Columns.Count > 0 &&
			    mode == XmlReadMode.ReadSchema && table.TableName != ds.Tables [0].TableName)
				throw new ArgumentException (String.Format ("DataTable '{0}' does not match to any DataTable in source",
									    table.TableName));
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
			if (this.Columns.Count > 0)
				return;

			DataSet ds = new DataSet ();
			new XmlSchemaDataImporter (ds, reader, false).Process ();
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

		[MonoNotSupported ("")]
		protected virtual void ReadXmlSerializable (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		private XmlWriterSettings GetWriterSettings ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.Indent = true;
			s.OmitXmlDeclaration = true;
			return s;
		}

		public void WriteXml (Stream stream)
		{
			WriteXml (stream, XmlWriteMode.IgnoreSchema, false);
		}

		public void WriteXml (TextWriter writer)
		{
			WriteXml (writer, XmlWriteMode.IgnoreSchema, false);
		}

		public void WriteXml (XmlWriter writer)
		{
			WriteXml (writer, XmlWriteMode.IgnoreSchema, false);
		}

		public void WriteXml (string fileName)
		{
			WriteXml (fileName, XmlWriteMode.IgnoreSchema, false);
		}

		public void WriteXml (Stream stream, XmlWriteMode mode)
		{
			WriteXml (stream, mode, false);
		}

		public void WriteXml (TextWriter writer, XmlWriteMode mode)
		{
			WriteXml (writer, mode, false);
		}

		public void WriteXml (XmlWriter writer, XmlWriteMode mode)
		{
			WriteXml (writer, mode, false);
		}

		public void WriteXml (string fileName, XmlWriteMode mode)
		{
			WriteXml (fileName, mode, false);
		}

		public void WriteXml (Stream stream, bool writeHierarchy)
		{
			WriteXml (stream, XmlWriteMode.IgnoreSchema, writeHierarchy);
		}

		public void WriteXml (string fileName, bool writeHierarchy)
		{
			WriteXml (fileName, XmlWriteMode.IgnoreSchema, writeHierarchy);
		}

		public void WriteXml (TextWriter writer, bool writeHierarchy)
		{
			WriteXml (writer, XmlWriteMode.IgnoreSchema, writeHierarchy);
		}

		public void WriteXml (XmlWriter writer, bool writeHierarchy)
		{
			WriteXml (writer, XmlWriteMode.IgnoreSchema, writeHierarchy);
		}

		public void WriteXml (Stream stream, XmlWriteMode mode, bool writeHierarchy)
		{
			WriteXml (XmlWriter.Create (stream, GetWriterSettings ()), mode, writeHierarchy);
		}

		public void WriteXml (string fileName, XmlWriteMode mode, bool writeHierarchy)
		{
			XmlWriter xw = null;
			try {
				xw = XmlWriter.Create (fileName, GetWriterSettings ());
				WriteXml (xw, mode, writeHierarchy);
			} finally {
				if (xw != null)
					xw.Close ();
			}
		}

		public void WriteXml (TextWriter writer, XmlWriteMode mode, bool writeHierarchy)
		{
			WriteXml (XmlWriter.Create (writer, GetWriterSettings ()), mode, writeHierarchy);
		}

		public void WriteXml (XmlWriter writer, XmlWriteMode mode, bool writeHierarchy)
		{
			// If we're in mode XmlWriteMode.WriteSchema, we need to output an extra
			// msdata:MainDataTable attribute that wouldn't normally be part of the
			// DataSet WriteXml output.
			//
			// For the writeHierarchy == true case, we write what would be output by
			// a DataSet write, but we limit ourselves to our table and its descendants.
			//
			// For the writeHierarchy == false case, we write what would be output by
			// a DataSet write, but we limit ourselves to this table.
			//
			// If the table is not in a DataSet, we follow the following behaviour:
			//   For WriteSchema cases, we do a write as if there is a wrapper
			//   dataset called NewDataSet.
			//   For IgnoreSchema or DiffGram cases, we do a write as if there
			//   is a wrapper dataset called DocumentElement.

			// Generate a list of tables to write.
			List <DataTable> tables = new List <DataTable> ();
			if (writeHierarchy == false)
				tables.Add (this);
			else
				FindAllChildren (tables, this);

			// If we're in a DataSet, generate a list of relations to write.
			List <DataRelation> relations = new List <DataRelation> ();
			if (DataSet != null) {
				foreach (DataRelation relation in DataSet.Relations) {
					if (tables.Contains (relation.ParentTable) &&
					   tables.Contains (relation.ChildTable))
						relations.Add (relation);
				}
			}

			// Add the msdata:MainDataTable info if we're writing schema data.
			string mainDataTable = null;
			if (mode == XmlWriteMode.WriteSchema)
				mainDataTable = this.TableName;

			// Figure out the DataSet name.
			string dataSetName = null;
			if (DataSet != null)
				dataSetName = DataSet.DataSetName;
			else if (DataSet == null && mode == XmlWriteMode.WriteSchema)
				dataSetName = "NewDataSet";
			else
				dataSetName = "DocumentElement";

			XmlTableWriter.WriteTables(writer, mode, tables, relations, mainDataTable, dataSetName);
		}

		private void FindAllChildren(List<DataTable> list, DataTable root)
		{
			if (!list.Contains(root))
			{
				list.Add(root);
				foreach (DataRelation relation in root.ChildRelations)
				{
					FindAllChildren(list, relation.ChildTable);
				}
			}
		}

		public void WriteXmlSchema (Stream stream)
		{
			if (TableName == "") {
				throw new InvalidOperationException ("Cannot serialize the DataTable. DataTable name is not set.");
			}
			XmlWriterSettings s = GetWriterSettings ();
			s.OmitXmlDeclaration = false;
			WriteXmlSchema (XmlWriter.Create (stream, s));
		}

		public void WriteXmlSchema (TextWriter writer)
		{
			if (TableName == "") {
				throw new InvalidOperationException ("Cannot serialize the DataTable. DataTable name is not set.");
			}
			XmlWriterSettings s = GetWriterSettings ();
			s.OmitXmlDeclaration = false;
			WriteXmlSchema (XmlWriter.Create (writer, s));
		}

		public void WriteXmlSchema (XmlWriter writer)
		{
			WriteXmlSchema (writer, false);
/*
			if (TableName == "") {
				throw new InvalidOperationException ("Cannot serialize the DataTable. DataTable name is not set.");
			}
			DataSet ds = DataSet;
			DataSet tmp = null;
			try {
				if (ds == null) {
					tmp = ds = new DataSet ();
					ds.Tables.Add (this);
				}
				writer.WriteStartDocument ();
				DataTableCollection col = new DataTableCollection (ds);
				col.Add (this);
				DataTable [] tables = new DataTable [col.Count];
				for (int i = 0; i < col.Count; i++) tables[i] = col[i];
				string tableName;
				if (ds.Namespace == "")
					tableName = TableName;
				else
					tableName = ds.Namespace + "_x003A_" + TableName;
				XmlSchemaWriter.WriteXmlSchema (writer, tables, null, tableName, ds.DataSetName);
			} finally {
				if (tmp != null)
					ds.Tables.Remove (this);
			}
*/
		}

		public void WriteXmlSchema (string fileName)
		{
			if (fileName == "") {
				throw new ArgumentException ("Empty path name is not legal.");
			}
			if (TableName == "") {
				throw new InvalidOperationException ("Cannot serialize the DataTable. DataTable name is not set.");
			}
			XmlTextWriter writer = null;
			try {
				XmlWriterSettings s = GetWriterSettings ();
				s.OmitXmlDeclaration = false;
				writer = new XmlTextWriter (fileName, null);
				WriteXmlSchema (writer);
			} finally {
				if (writer != null) {
					writer.Close ();
				}
			}
		}

		public void WriteXmlSchema (Stream stream, bool writeHierarchy)
		{
			if (TableName == "") {
				throw new InvalidOperationException ("Cannot serialize the DataTable. DataTable name is not set.");
			}
			XmlWriterSettings s = GetWriterSettings ();
			s.OmitXmlDeclaration = false;
			WriteXmlSchema (XmlWriter.Create (stream, s), writeHierarchy);
		}

		public void WriteXmlSchema (TextWriter writer, bool writeHierarchy)
		{
			if (TableName == "") {
				throw new InvalidOperationException ("Cannot serialize the DataTable. DataTable name is not set.");
			}
			XmlWriterSettings s = GetWriterSettings ();
			s.OmitXmlDeclaration = false;
			WriteXmlSchema (XmlWriter.Create (writer, s), writeHierarchy);
		}

		public void WriteXmlSchema (XmlWriter writer, bool writeHierarchy)
		{
			if (TableName == "") {
				throw new InvalidOperationException ("Cannot serialize the DataTable. DataTable name is not set.");
			}
//			if (writeHierarchy == false) {
//				WriteXmlSchema (writer);
//			}
//			else {
				DataSet ds = DataSet;
				DataSet tmp = null;
				try {
					if (ds == null) {
						tmp = ds = new DataSet ();
						ds.Tables.Add (this);
					}
					writer.WriteStartDocument ();
					//XmlSchemaWriter.WriteXmlSchema (ds, writer);
					//DataTable [] tables = new DataTable [ds.Tables.Count];
					DataTable [] tables = null;
					//DataRelation [] relations =  new DataRelation [ds.Relations.Count];
					//for (int i = 0; i < ds.Relations.Count; i++) {
					//	relations[i] = ds.Relations[i];
					//}
					DataRelation [] relations = null;
					if (writeHierarchy && ChildRelations.Count > 0) {
						relations = new DataRelation [ChildRelations.Count];
						for (int i = 0; i < ChildRelations.Count; i++) {
							relations [i] = ChildRelations [i];
						}
						tables = new DataTable [ds.Tables.Count];
						for (int i = 0; i < ds.Tables.Count; i++) tables [i] = ds.Tables [i];
					} else {
						tables = new DataTable [1];
						tables [0] = this;
					}

					string tableName;
					if (ds.Namespace == "")
						tableName = TableName;
					else
						tableName = ds.Namespace + "_x003A_" + TableName;
					XmlSchemaWriter.WriteXmlSchema (writer, tables, relations, tableName, ds.DataSetName, LocaleSpecified ? Locale : ds.LocaleSpecified ? ds.Locale : null);
				} finally {
					if (tmp != null)
						ds.Tables.Remove (this);
				}
//			}
		}

		public void WriteXmlSchema (string fileName, bool writeHierarchy)
		{
			if (fileName == "") {
				throw new ArgumentException ("Empty path name is not legal.");
			}
			if (TableName == "") {
				throw new InvalidOperationException ("Cannot serialize the DataTable. DataTable name is not set.");
			}
			XmlTextWriter writer = null;
			try {
				XmlWriterSettings s = GetWriterSettings ();
				s.OmitXmlDeclaration = false;
				writer = new XmlTextWriter (fileName, null);
				WriteXmlSchema (writer, writeHierarchy);
			} finally {
				if (writer != null) {
					writer.Close ();
				}
			}
		}
	}

	partial class DataTable : ISupportInitializeNotification {
		private bool tableInitialized = true;

		[Browsable (false)]
		public bool IsInitialized {
			get { return tableInitialized; }
		}

		public event EventHandler Initialized;

		private void OnTableInitialized (EventArgs e)
		{
			if (null != Initialized)
				Initialized (this, e);
		}

		partial void DataTableInitialized ()
		{
			tableInitialized = true;
			OnTableInitialized (new EventArgs ());
		}
	}

	partial class DataTable {
		public DataTable (string tableName, string tbNamespace)
			: this (tableName)
		{
			_nameSpace = tbNamespace;
		}

		SerializationFormat remotingFormat = SerializationFormat.Xml;
		[DefaultValue (SerializationFormat.Xml)]
		public SerializationFormat RemotingFormat {
			get {
				if (dataSet != null)
					remotingFormat = dataSet.RemotingFormat;
				return remotingFormat;
			}
			set {
				if (dataSet != null)
					throw new ArgumentException ("Cannot have different remoting format property value for DataSet and DataTable");
				remotingFormat = value;
			}
		}

		internal void DeserializeConstraints (ArrayList arrayList)
		{
			bool fKeyNavigate = false;
			for (int i = 0; i < arrayList.Count; i++) {
				ArrayList tmpArrayList = arrayList [i] as ArrayList;
				if (tmpArrayList == null)
					continue;
				if ((string) tmpArrayList [0] == "F") {
					int [] constraintsArray = tmpArrayList [2] as int [];

					if (constraintsArray == null)
						continue;
					ArrayList parentColumns = new ArrayList ();
					DataTable dt = dataSet.Tables [constraintsArray [0]];
					for (int j = 0; j < constraintsArray.Length - 1; j++) {
						parentColumns.Add (dt.Columns [constraintsArray [j + 1]]);
					}

					constraintsArray = tmpArrayList [3] as int [];

					if (constraintsArray == null)
						continue;
					ArrayList childColumns  = new ArrayList ();
					dt = dataSet.Tables [constraintsArray [0]];
					for (int j = 0; j < constraintsArray.Length - 1; j++) {
						childColumns.Add (dt.Columns [constraintsArray [j + 1]]);
					}

					ForeignKeyConstraint fKeyConstraint = new
					  ForeignKeyConstraint ((string) tmpArrayList [1],
								(DataColumn []) parentColumns.ToArray (typeof (DataColumn)),
								(DataColumn []) childColumns.ToArray (typeof (DataColumn)));
					Array ruleArray = (Array) tmpArrayList [4];
					fKeyConstraint.AcceptRejectRule = (AcceptRejectRule) ruleArray.GetValue (0);
					fKeyConstraint.UpdateRule = (Rule) ruleArray.GetValue (1);
					fKeyConstraint.DeleteRule = (Rule) ruleArray.GetValue (2);
					// FIXME: refactor this deserialization code out to ForeighKeyConstraint
					fKeyConstraint.SetExtendedProperties ((PropertyCollection) tmpArrayList [5]);
					Constraints.Add (fKeyConstraint);
					fKeyNavigate = true;
				} else if (fKeyNavigate == false &&
					   (string) tmpArrayList [0] == "U") {
					UniqueConstraint unique = null;
					ArrayList uniqueDataColumns = new ArrayList ();
					int [] constraintsArray = tmpArrayList [2] as int [];

					if (constraintsArray == null)
						continue;

					for (int j = 0; j < constraintsArray.Length; j++) {
						uniqueDataColumns.Add (Columns [constraintsArray [j]]);
					}
					unique = new UniqueConstraint ((string) tmpArrayList[1],
								       (DataColumn[]) uniqueDataColumns.
								       ToArray (typeof (DataColumn)),
								       (bool) tmpArrayList [3]);
					/*
					  If UniqueConstraint already exist, don't add them
					*/
					if (Constraints.IndexOf (unique) != -1 ||
					    Constraints.IndexOf ((string) tmpArrayList[1]) != -1) {
						continue;
					}
					// FIXME: refactor this deserialization code out to UniqueConstraint
					unique.SetExtendedProperties ((PropertyCollection) tmpArrayList [4]);
					Constraints.Add (unique);
				} else {
					fKeyNavigate = false;
				}
			}
		}

		DataRowState GetCurrentRowState (BitArray rowStateBitArray, int i)
		{
			DataRowState currentRowState;
			if (rowStateBitArray[i] == false &&
			    rowStateBitArray[i+1] == false &&
			    rowStateBitArray[i+2] == true)
				currentRowState = DataRowState.Detached;
			else if (rowStateBitArray[i] == false &&
				 rowStateBitArray[i+1] == false &&
				 rowStateBitArray[i+2] == false)
				currentRowState = DataRowState.Unchanged;
			else if (rowStateBitArray[i] == false &&
				 rowStateBitArray[i+1] == true &&
				 rowStateBitArray[i+2] == false)
				currentRowState = DataRowState.Added;
			else if (rowStateBitArray[i] == true &&
				 rowStateBitArray[i+1] == true &&
				 rowStateBitArray[i+2] == false)
				currentRowState = DataRowState.Deleted;
			else
				currentRowState = DataRowState.Modified;
			return currentRowState;
		}

		internal void DeserializeRecords (ArrayList arrayList, ArrayList nullBits, BitArray rowStateBitArray)
		{
			BitArray  nullBit = null;
			if (arrayList == null || arrayList.Count < 1)
				return;
			int len = ((Array) arrayList [0]).Length;
			object [] tmpArray = new object [arrayList.Count];
			int k = 0;
			DataRowState currentRowState;
			for (int l = 0; l < len; l++) { // Columns
				currentRowState = GetCurrentRowState (rowStateBitArray, k *3);
				for (int j = 0; j < arrayList.Count; j++) { // Rows
					Array array = (Array) arrayList [j];
					nullBit = (BitArray) nullBits [j];
					if (nullBit [l] == false) {
						tmpArray [j] = array.GetValue (l);
					} else {
						tmpArray [j] = null;
					}
				}
				LoadDataRow (tmpArray, false);
				if (currentRowState == DataRowState.Modified) {
					Rows[k].AcceptChanges ();
					l++;
					for (int j = 0; j < arrayList.Count; j++) {
						Array array = (Array) arrayList [j];
						nullBit = (BitArray) nullBits[j];
						if (nullBit [l] == false) {
							Rows [k][j] = array.GetValue (l);
						} else {
							Rows [k][j] = null;
						}
					}
				} else if (currentRowState == DataRowState.Unchanged) {
					Rows[k].AcceptChanges ();
				} else if (currentRowState == DataRowState.Deleted) {
					Rows[k].AcceptChanges ();
					Rows[k].Delete ();
				}
				k++;
			}
		}

		void BinaryDeserializeTable (SerializationInfo info)
		{
			ArrayList arrayList = null;

			TableName = info.GetString ("DataTable.TableName");
			Namespace = info.GetString ("DataTable.Namespace");
			Prefix = info.GetString ("DataTable.Prefix");
			CaseSensitive = info.GetBoolean ("DataTable.CaseSensitive");
			/*
			  FIXME: Private variable available in SerializationInfo
			  this.caseSensitiveAmbientCaseSensitive = info.GetBoolean("DataTable.caseSensitiveAmbientCaseSensitive");
			  this.NestedInDataSet = info.GetBoolean("DataTable.NestedInDataSet");
			  this.RepeatableElement = info.GetBoolean("DataTable.RepeatableElement");
			  this.RemotingVersion = (System.Version) info.GetValue("DataTable.RemotingVersion",
			  typeof(System.Version));
			*/
			Locale = new CultureInfo (info.GetInt32 ("DataTable.LocaleLCID"));
			_extendedProperties = (PropertyCollection) info.GetValue ("DataTable.ExtendedProperties",
										 typeof (PropertyCollection));
			MinimumCapacity = info.GetInt32 ("DataTable.MinimumCapacity");
			int columnsCount = info.GetInt32 ("DataTable.Columns.Count");

			for (int i = 0; i < columnsCount; i++) {
				Columns.Add ();
				string prefix = "DataTable.DataColumn_" + i + ".";
				Columns[i].ColumnName = info.GetString (prefix + "ColumnName");
				Columns[i].Namespace = info.GetString (prefix + "Namespace");
				Columns[i].Caption = info.GetString (prefix + "Caption");
				Columns[i].Prefix = info.GetString (prefix + "Prefix");
				Columns[i].DataType = (Type) info.GetValue (prefix + "DataType",
									    typeof (Type));
				Columns[i].DefaultValue = info.GetValue (prefix + "DefaultValue",
										  typeof (Object));
				Columns[i].AllowDBNull = info.GetBoolean (prefix + "AllowDBNull");
				Columns[i].AutoIncrement = info.GetBoolean (prefix + "AutoIncrement");
				Columns[i].AutoIncrementStep = info.GetInt64 (prefix + "AutoIncrementStep");
				Columns[i].AutoIncrementSeed = info.GetInt64(prefix + "AutoIncrementSeed");
				Columns[i].ReadOnly = info.GetBoolean (prefix + "ReadOnly");
				Columns[i].MaxLength = info.GetInt32 (prefix + "MaxLength");
				/*
				  FIXME: Private variable available in SerializationInfo
				  this.Columns[i].SimpleType = info.GetString("DataTable.DataColumn_" +
				  i + ".SimpleType");
				  this.Columns[i].AutoIncrementCurrent = info.GetInt64("DataTable.DataColumn_" +
				  i + ".AutoIncrementCurrent");
				  this.Columns[i].XmlDataType = info.GetString("DataTable.DataColumn_" +
				  i + ".XmlDataType");
				*/
				Columns[i].ExtendedProperties = (PropertyCollection) info.GetValue (prefix + "ExtendedProperties",
												    typeof (PropertyCollection));
				if (Columns[i].DataType == typeof (DataSetDateTime)) {
					Columns[i].DateTimeMode = (DataSetDateTime) info.GetValue (prefix + "DateTimeMode",
												   typeof (DataSetDateTime));
				}
				Columns[i].ColumnMapping = (MappingType) info.GetValue (prefix + "ColumnMapping",
											typeof (MappingType));
				try {
					Columns[i].Expression = info.GetString (prefix + "Expression");

					prefix = "DataTable_0.";

					arrayList = (ArrayList) info.GetValue (prefix + "Constraints",
									       typeof (ArrayList));
					if (Constraints == null)
						Constraints = new ConstraintCollection (this);
					DeserializeConstraints (arrayList);
				} catch (SerializationException) {
				}
			}
			try {
				String prefix = "DataTable_0.";
				ArrayList nullBits = (ArrayList) info.GetValue (prefix + "NullBits",
										typeof (ArrayList));
				arrayList = (ArrayList) info.GetValue (prefix + "Records",
								       typeof (ArrayList));
				BitArray rowStateBitArray = (BitArray) info.GetValue (prefix + "RowStates",
										      typeof (BitArray));
				Hashtable htRowErrors = (Hashtable) info.GetValue (prefix + "RowErrors",
										  typeof (Hashtable));
				DeserializeRecords (arrayList, nullBits, rowStateBitArray);
			} catch (SerializationException) {
			}
		}

		internal void BinarySerializeProperty (SerializationInfo info)
		{
			Version vr = new Version (2, 0);
			info.AddValue ("DataTable.RemotingVersion", vr);
			info.AddValue ("DataTable.RemotingFormat", RemotingFormat);
			info.AddValue ("DataTable.TableName", TableName);
			info.AddValue ("DataTable.Namespace", Namespace);
			info.AddValue ("DataTable.Prefix", Prefix);
			info.AddValue ("DataTable.CaseSensitive", CaseSensitive);
			/*
			  FIXME: Required by MS.NET
			  caseSensitiveAmbient, NestedInDataSet, RepeatableElement
			*/
			info.AddValue ("DataTable.caseSensitiveAmbient", true);
			info.AddValue ("DataTable.NestedInDataSet", true);
			info.AddValue ("DataTable.RepeatableElement", false);
			info.AddValue ("DataTable.LocaleLCID", Locale.LCID);
			info.AddValue ("DataTable.MinimumCapacity", MinimumCapacity);
			info.AddValue ("DataTable.Columns.Count", Columns.Count);
			info.AddValue ("DataTable.ExtendedProperties", _extendedProperties);
			for (int i = 0; i < Columns.Count; i++) {
				info.AddValue ("DataTable.DataColumn_" + i + ".ColumnName",
					       Columns[i].ColumnName);
				info.AddValue ("DataTable.DataColumn_" + i + ".Namespace",
					       Columns[i].Namespace);
				info.AddValue ("DataTable.DataColumn_" + i + ".Caption",
					       Columns[i].Caption);
				info.AddValue ("DataTable.DataColumn_" + i + ".Prefix",
					       Columns[i].Prefix);
				info.AddValue ("DataTable.DataColumn_" + i + ".DataType",
					       Columns[i].DataType, typeof (Type));
				info.AddValue ("DataTable.DataColumn_" + i + ".DefaultValue",
					       Columns[i].DefaultValue, typeof (DBNull));
				info.AddValue ("DataTable.DataColumn_" + i + ".AllowDBNull",
					       Columns[i].AllowDBNull);
				info.AddValue ("DataTable.DataColumn_" + i + ".AutoIncrement",
					       Columns[i].AutoIncrement);
				info.AddValue ("DataTable.DataColumn_" + i + ".AutoIncrementStep",
					       Columns[i].AutoIncrementStep);
				info.AddValue ("DataTable.DataColumn_" + i + ".AutoIncrementSeed",
					       Columns[i].AutoIncrementSeed);
				info.AddValue ("DataTable.DataColumn_" + i + ".ReadOnly",
					       Columns[i].ReadOnly);
				info.AddValue ("DataTable.DataColumn_" + i + ".MaxLength",
					       Columns[i].MaxLength);
				info.AddValue ("DataTable.DataColumn_" + i + ".ExtendedProperties",
					       Columns[i].ExtendedProperties);
				info.AddValue ("DataTable.DataColumn_" + i + ".DateTimeMode",
					       Columns[i].DateTimeMode);
				info.AddValue ("DataTable.DataColumn_" + i + ".ColumnMapping",
					       Columns[i].ColumnMapping, typeof (MappingType));
				/*
				  FIXME: Required by MS.NET
				  SimpleType, AutoIncrementCurrent, XmlDataType
				*/
				info.AddValue ("DataTable.DataColumn_" + i + ".SimpleType",
					       null, typeof (string));
				info.AddValue ("DataTable.DataColumn_" + i + ".AutoIncrementCurrent",
					       Columns[i].AutoIncrementValue());
				info.AddValue ("DataTable.DataColumn_" + i + ".XmlDataType",
					       null, typeof (string));
			}
			/*
			  FIXME: Required by MS.NET
			  TypeName
			*/
			info.AddValue ("DataTable.TypeName", null, typeof (string));
		}

		internal void SerializeConstraints (SerializationInfo info, string prefix)
		{
			ArrayList constraintArrayList = new ArrayList ();
			for (int j = 0; j < Constraints.Count; j++) {
				ArrayList constraintList = new ArrayList ();
				if (Constraints[j] is UniqueConstraint) {
					constraintList.Add ("U");
					UniqueConstraint unique = (UniqueConstraint) Constraints [j];
					constraintList.Add (unique.ConstraintName);
					DataColumn [] columns = unique.Columns;
					int [] tmpArray = new int [columns.Length];
					for (int k = 0; k < columns.Length; k++)
						tmpArray [k] = unique.Table.Columns.IndexOf (unique.Columns [k]);
					constraintList.Add (tmpArray);
					constraintList.Add (unique.IsPrimaryKey);
					constraintList.Add (unique.ExtendedProperties);
				} else if (Constraints[j] is ForeignKeyConstraint) {
					constraintList.Add ("F");
					ForeignKeyConstraint fKey = (ForeignKeyConstraint) Constraints [j];
					constraintList.Add (fKey.ConstraintName);

					int [] tmpArray = new int [fKey.RelatedColumns.Length + 1];
					tmpArray [0] = DataSet.Tables.IndexOf (fKey.RelatedTable);
					for (int i = 0; i < fKey.Columns.Length; i++) {
						tmpArray [i + 1] = fKey.RelatedColumns [i].Ordinal;
					}
					constraintList.Add (tmpArray);

					tmpArray = new int [fKey.Columns.Length + 1];
					tmpArray [0] = DataSet.Tables.IndexOf (fKey.Table);
					for (int i = 0; i < fKey.Columns.Length; i++) {
						tmpArray [i + 1] = fKey.Columns [i].Ordinal;
					}
					constraintList.Add (tmpArray);

					tmpArray = new int [3];
					tmpArray [0] = (int) fKey.AcceptRejectRule;
					tmpArray [1] = (int) fKey.UpdateRule;
					tmpArray [2] = (int) fKey.DeleteRule;
					constraintList.Add (tmpArray);

					constraintList.Add (fKey.ExtendedProperties);
				}
				else
					continue;
				constraintArrayList.Add (constraintList);
			}
			info.AddValue (prefix, constraintArrayList, typeof (ArrayList));
		}

		internal void BinarySerialize (SerializationInfo info, string prefix)
		{
			int columnsCount = Columns.Count;
			int rowsCount = Rows.Count;
			int recordsCount = Rows.Count;
			ArrayList recordList = new ArrayList ();
			ArrayList bitArrayList = new ArrayList ();
			BitArray rowStateBitArray = new BitArray (rowsCount * 3);
			for (int k = 0; k < Rows.Count; k++) {
				if (Rows[k].RowState == DataRowState.Modified)
					recordsCount++;
			}
			SerializeConstraints (info, prefix + "Constraints");
			for (int j = 0; j < columnsCount; j++) {
				if (rowsCount == 0)
					continue;
				BitArray nullBits = new BitArray (rowsCount);
				DataColumn column = Columns [j];
				Array recordArray = Array.CreateInstance (column.DataType, recordsCount);
				for (int k = 0, l = 0; k < Rows.Count; k++, l++) {
					DataRowVersion version;
					DataRow dr = Rows[k];
					if (dr.RowState == DataRowState.Modified) {
						version = DataRowVersion.Default;
						nullBits.Length = nullBits.Length + 1;
						if (dr.IsNull (column, version) == false) {
							nullBits [l] = false;
							recordArray.SetValue (dr [j, version], l);
						} else {
							nullBits [l] = true;
						}
						l++;
						version = DataRowVersion.Current;
					} else if (dr.RowState == DataRowState.Deleted) {
						version = DataRowVersion.Original;
					} else {
						version = DataRowVersion.Default;
					}
					if (dr.IsNull (column, version) == false) {
						nullBits [l] = false;
						recordArray.SetValue (dr [j, version], l);
					} else {
						nullBits [l] = true;
					}
				}
				recordList.Add (recordArray);
				bitArrayList.Add (nullBits);
			}
			for (int j = 0; j < Rows.Count; j++) {
				int l = j * 3;
				DataRowState rowState = Rows [j].RowState;
				if (rowState == DataRowState.Detached) {
					rowStateBitArray [l] = false;
					rowStateBitArray [l + 1] = false;
					rowStateBitArray [l + 2] = true;
				} else if (rowState == DataRowState.Unchanged) {
					rowStateBitArray [l] = false;
					rowStateBitArray [l + 1] = false;
					rowStateBitArray [l + 2] = false;
				} else if (rowState == DataRowState.Added) {
					rowStateBitArray [l] = false;
					rowStateBitArray [l + 1] = true;
					rowStateBitArray [l + 2] = false;
				} else if (rowState == DataRowState.Deleted) {
					rowStateBitArray [l] = true;
					rowStateBitArray [l + 1] = true;
					rowStateBitArray [l + 2] = false;
				} else {
					rowStateBitArray [l] = true;
					rowStateBitArray [l + 1] = false;
					rowStateBitArray [l + 2] = false;
				}
			}
			info.AddValue (prefix + "Rows.Count", Rows.Count);
			info.AddValue (prefix + "Records.Count", recordsCount);
			info.AddValue (prefix + "Records", recordList, typeof (ArrayList));
			info.AddValue (prefix + "NullBits", bitArrayList, typeof (ArrayList));
			info.AddValue (prefix + "RowStates",
				       rowStateBitArray, typeof (BitArray));
			// FIXME: To get row errors
			Hashtable htRowErrors = new Hashtable ();
			info.AddValue (prefix + "RowErrors",
				       htRowErrors, typeof (Hashtable));
			// FIXME: To get column errors
			Hashtable htColumnErrors = new Hashtable ();
			info.AddValue (prefix + "ColumnErrors",
				       htColumnErrors, typeof (Hashtable));
		}

		public DataTableReader CreateDataReader ()
		{
			return new DataTableReader (this);
		}

		/// <summary>
		///     Loads the table with the values from the reader
		/// </summary>
		public void Load (IDataReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("Value cannot be null. Parameter name: reader");
			Load (reader, LoadOption.PreserveChanges);
		}

		/// <summary>
		///     Loads the table with the values from the reader and the pattern
		///     of the changes to the existing rows or the new rows are based on
		///     the LoadOption passed.
		/// </summary>
		public void Load (IDataReader reader, LoadOption loadOption)
		{
			if (reader == null)
				throw new ArgumentNullException ("Value cannot be null. Parameter name: reader");
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

		public virtual void Load (IDataReader reader, LoadOption loadOption, FillErrorEventHandler errorHandler)
		{
			if (reader == null)
				throw new ArgumentNullException ("Value cannot be null. Parameter name: reader");

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
							      loadOption,
							      errorHandler);
			} finally {
				this.EnforceConstraints = prevEnforceConstr;
			}
		}

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
			DataRow row = null;

			// Find Data DataRow
			if (this.PrimaryKey.Length > 0) {
				object [] keys = new object [PrimaryKey.Length];
				for (int i = 0; i < PrimaryKey.Length; i++)
					keys [i] = values [PrimaryKey [i].Ordinal];

				row = Rows.Find (keys, DataViewRowState.OriginalRows);
				if (row == null)
					row = Rows.Find (keys);
			}

			// If not found, add new row
			if (row == null ||
			    (row.RowState == DataRowState.Deleted && loadOption == LoadOption.Upsert)) {
				row = NewNotInitializedRow ();
				row.ImportRecord (CreateRecord (values));

				row.Validate (); // this adds to index ;-)

				if (loadOption == LoadOption.OverwriteChanges ||
				    loadOption == LoadOption.PreserveChanges)
					Rows.AddInternal (row, DataRowAction.ChangeCurrentAndOriginal);
				else
					Rows.AddInternal(row);
				return row;
			}

			row.Load (values, loadOption);

			return row;
		}

		public void Merge (DataTable table)
		{
			Merge (table, false, MissingSchemaAction.Add);
		}

		public void Merge (DataTable table, bool preserveChanges)
		{
			Merge (table, preserveChanges, MissingSchemaAction.Add);
		}

		public void Merge (DataTable table, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			MergeManager.Merge (this, table, preserveChanges, missingSchemaAction);
		}

		internal int CompareRecords (int x, int y)
		{
			for (int col = 0; col < Columns.Count; col++) {
				int res = Columns[col].DataContainer.CompareValues (x, y);
				if (res != 0)
					return res;
			}

			return 0;
		}

		/// <summary>
		/// Occurs after the Clear method is called on the datatable.
		/// </summary>
		[DataCategory ("Data")]
		public event DataTableClearEventHandler TableCleared;

		[DataCategory ("Data")]
		public event DataTableClearEventHandler TableClearing;

		public event DataTableNewRowEventHandler TableNewRow;

		/// <summary>
		/// Raises TableCleared Event and delegates to subscribers
		/// </summary>
		protected virtual void OnTableCleared (DataTableClearEventArgs e)
		{
			if (TableCleared != null)
				TableCleared (this, e);
		}

		internal void DataTableCleared ()
		{
			OnTableCleared (new DataTableClearEventArgs (this));
		}

		protected virtual void OnTableClearing (DataTableClearEventArgs e)
		{
			if (TableClearing != null)
				TableClearing (this, e);
		}

		internal void DataTableClearing ()
		{
			OnTableClearing (new DataTableClearEventArgs (this));
		}

		protected virtual void OnTableNewRow (DataTableNewRowEventArgs e)
		{
			if (null != TableNewRow)
				TableNewRow (this, e);
		}

		partial void NewRowAdded (DataRow dr)
		{
			OnTableNewRow (new DataTableNewRowEventArgs (dr));
		}
	}
#endif
}
