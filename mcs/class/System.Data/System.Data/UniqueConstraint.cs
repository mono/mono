//
// System.Data.UniqueConstraint.cs
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman (tim@timcoleman.com)
//   
// (C) 2002 Franklin Wise
// (C) 2002 Daniel Morgan
// Copyright (C) Tim Coleman, 2002

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
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Data.Common;

namespace System.Data {
	[Editor ("Microsoft.VSDesigner.Data.Design.UniqueConstraintEditor, " + Consts.AssemblyMicrosoft_VSDesigner,
		 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	[DefaultProperty ("ConstraintName")]
#if !NET_2_0
	[Serializable]
#endif
	public class UniqueConstraint : Constraint 
	{
		private bool _isPrimaryKey = false;
		private bool _belongsToCollection = false;
		private DataTable _dataTable; //set by ctor except when unique case
		
		//FIXME: create a class which will wrap this collection
		private DataColumn [] _dataColumns;

		//TODO:provide helpers for this case
		private string [] _dataColumnNames; //unique case
		private ForeignKeyConstraint _childConstraint = null;

		#region Constructors

		public UniqueConstraint (DataColumn column) 
		{
			_uniqueConstraint ("", column, false);
		}

		public UniqueConstraint (DataColumn[] columns) 
		{
			_uniqueConstraint ("", columns, false);
		}

		public UniqueConstraint (DataColumn column, bool isPrimaryKey) 
		{
			_uniqueConstraint ("", column, isPrimaryKey);
		}

		public UniqueConstraint (DataColumn[] columns, bool isPrimaryKey) 
		{
			_uniqueConstraint ("", columns, isPrimaryKey);
		}

		public UniqueConstraint (string name, DataColumn column) 
		{
			_uniqueConstraint (name, column, false);
		}

		public UniqueConstraint (string name, DataColumn[] columns) 
		{
			_uniqueConstraint (name, columns, false);
		}

		public UniqueConstraint (string name, DataColumn column, bool isPrimaryKey) 
		{
			_uniqueConstraint (name, column, isPrimaryKey);
		}

		public UniqueConstraint (string name, DataColumn[] columns, bool isPrimaryKey) 
		{
			_uniqueConstraint (name, columns, isPrimaryKey);
		}

		//Special case.  Can only be added to the Collection with AddRange
		[Browsable (false)]
		public UniqueConstraint (string name, string[] columnNames, bool isPrimaryKey) 
		{
			InitInProgress = true;
                   
			_dataColumnNames = new string [columnNames.Length];
			for (int i = 0; i < columnNames.Length; i++)
				_dataColumnNames[i] = columnNames[i];
                       
			base.ConstraintName = name;
			_isPrimaryKey = isPrimaryKey;
		}

		//helper ctor
		private void _uniqueConstraint(string name, DataColumn column, bool isPrimaryKey) 
		{
			//validate
			_validateColumn (column);

			//Set Constraint Name
			base.ConstraintName = name;

			_isPrimaryKey = isPrimaryKey;

			//keep reference 
			_dataColumns = new DataColumn [] {column};
			
			//Get table reference
			_dataTable = column.Table;			
		}

		//helpter ctor	
		private void _uniqueConstraint(string name, DataColumn[] columns, bool isPrimaryKey) 
		{
			//validate
			_validateColumns (columns, out _dataTable);

			//Set Constraint Name
			base.ConstraintName = name;

			//copy the columns - Do not keep reference #672113
			//_dataColumns = columns;
			Columns = columns;

			//PK?
			_isPrimaryKey = isPrimaryKey;			
		}
		
		#endregion // Constructors

		#region Helpers
		
		private void _validateColumns(DataColumn [] columns)
		{
			DataTable table;
			_validateColumns(columns, out table);
		}
		
		//Validates a collection of columns with the ctor rules
		private void _validateColumns(DataColumn [] columns, out DataTable table) {
			table = null;

			//not null
			if (null == columns) throw new ArgumentNullException();
			
			//check that there is at least one column
			//LAMESPEC: not in spec
			if (columns.Length < 1)
				throw new InvalidConstraintException("Must be at least one column.");
			
			DataTable compareTable = columns[0].Table;
			//foreach
			foreach (DataColumn col in columns){
				
				//check individual column rules
				_validateColumn (col);
				
				
				//check that columns are all from the same table??
				//LAMESPEC: not in spec
				if (compareTable != col.Table)
					throw new InvalidConstraintException("Columns must be from the same table.");
				
			}

			table = compareTable;
		}
		
		//validates a column with the ctor rules
		private void _validateColumn(DataColumn column) {
	
			//not null
			if (null == column)  // FIXME: This is little weird, but here it goes...
				throw new NullReferenceException("Object reference not set to an instance of an object.");

			
			//column must belong to a table
			//LAMESPEC: not in spec
			if (null == column.Table)
				throw new ArgumentException ("Column must belong to a table.");			
		}

		internal static void SetAsPrimaryKey(ConstraintCollection collection, UniqueConstraint newPrimaryKey)
		{
			//not null
			if (null == collection) throw new ArgumentNullException("ConstraintCollection can't be null.");
			
			//make sure newPrimaryKey belongs to the collection parm unless it is null
			if (  collection.IndexOf(newPrimaryKey) < 0 && (null != newPrimaryKey) ) 
				throw new ArgumentException("newPrimaryKey must belong to collection.");
			
			//Get existing pk
			UniqueConstraint uc = GetPrimaryKeyConstraint(collection);
			
			//clear existing
			if (null != uc) uc._isPrimaryKey = false;

			//set new key
			if (null != newPrimaryKey) newPrimaryKey._isPrimaryKey = true;
			
			
		}

		internal static UniqueConstraint GetPrimaryKeyConstraint(ConstraintCollection collection)
		{
			if (null == collection) throw new ArgumentNullException("Collection can't be null.");

			UniqueConstraint uc;
			IEnumerator enumer = collection.GetEnumerator();
			while (enumer.MoveNext())
			{
				uc = enumer.Current as UniqueConstraint;
				if (null == uc) continue;
				
				if (uc.IsPrimaryKey) return uc;	
			}

			//if we got here there was no pk
			return null;
			
		}

		internal static UniqueConstraint GetUniqueConstraintForColumnSet(ConstraintCollection collection,
				DataColumn[] columns)
		{
			if (null == collection) throw new ArgumentNullException("Collection can't be null.");
			if (null == columns ) return null;
			
			foreach(Constraint constraint in collection) {
				if (constraint is UniqueConstraint) {
					UniqueConstraint uc = constraint as UniqueConstraint;
					if ( DataColumn.AreColumnSetsTheSame(uc.Columns, columns) ) {
						return uc;
					}
				}
			}
			return null;
		}

		internal ForeignKeyConstraint ChildConstraint {
			get { return _childConstraint; }
			set { _childConstraint = value; }
		}

		// Helper Special Ctor
		// Set the _dataTable property to the table to which this instance is bound when AddRange()
		// is called with the special constructor.
		// Validate whether the named columns exist in the _dataTable
		internal override void FinishInit (DataTable _setTable) 
		{                
			_dataTable = _setTable;
			if (_isPrimaryKey == true && _setTable.PrimaryKey.Length != 0)
				throw new ArgumentException ("Cannot add primary key constraint since primary key" +
						"is already set for the table");

			DataColumn[] cols = new DataColumn [_dataColumnNames.Length];
			int i = 0;

			foreach (string _columnName in _dataColumnNames) {
				if (_setTable.Columns.Contains (_columnName)) {
					cols [i] = _setTable.Columns [_columnName];
					i++;
					continue;
				}
				throw(new InvalidConstraintException ("The named columns must exist in the table"));
			}
			_dataColumns = cols;
			_validateColumns (cols);
			InitInProgress = false;
		}


		#endregion //Helpers

		#region Properties

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the columns of this constraint.")]
#endif
		[ReadOnly (true)]
		public virtual DataColumn[] Columns {
			get { return _dataColumns; }
			internal set {
			       _dataColumns = new DataColumn [value.Length];
			       for (int i = 0; i < value.Length; i++)
			               _dataColumns[i] = value[i];                             
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates if this constraint is a primary key.")]
#endif
		public bool IsPrimaryKey {
			get { 
				if (Table == null || (!_belongsToCollection)) {
					return false;
				}
				return _isPrimaryKey; 
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the table of this constraint.")]
#endif
		[ReadOnly (true)]
		public override DataTable Table {
			get { return _dataTable; }
		}

		#endregion // Properties

		#region Methods

		internal void SetIsPrimaryKey (bool value)
		{
			_isPrimaryKey = value;
		}

		public override bool Equals(object key2) {

			UniqueConstraint cst = key2 as UniqueConstraint;
			if (null == cst) return false;

			//according to spec if the cols are equal
			//then two UniqueConstraints are equal
			return DataColumn.AreColumnSetsTheSame(cst.Columns, this.Columns);	

		}

		public override int GetHashCode() 
		{
			//initialize hash with default value 
			int hash = 42;
			int i;

			//derive the hash code from the columns that way
			//Equals and GetHashCode return Equal objects to be the
			//same

			//Get the first column hash
			if (this.Columns.Length > 0)
				hash ^= this.Columns[0].GetHashCode();
			
			//get the rest of the column hashes if there any
			for (i = 1; i < this.Columns.Length; i++)
			{
				hash ^= this.Columns[1].GetHashCode();
				
			}
			
			return hash ;
		}
		
		internal override void AddToConstraintCollectionSetup(
				ConstraintCollection collection)
		{
			for (int i = 0; i < Columns.Length; i++)
				if (Columns[i].Table != collection.Table)
					throw new ArgumentException("These columns don't point to this table.");
			//run Ctor rules again
			_validateColumns(_dataColumns);
			
			//make sure a unique constraint doesn't already exists for these columns
			UniqueConstraint uc = UniqueConstraint.GetUniqueConstraintForColumnSet(collection, this.Columns);	
			if (null != uc) throw new ArgumentException("Unique constraint already exists for these" +
					" columns. Existing ConstraintName is " + uc.ConstraintName);

			//Allow only one primary key
			if (this.IsPrimaryKey) {
				uc = GetPrimaryKeyConstraint(collection);
				if (null != uc) uc._isPrimaryKey = false;
			}

			// if constraint is based on one column only
			// this column becomes unique
			if (_dataColumns.Length == 1) {
				_dataColumns[0].SetUnique();
			}
					
			if (IsConstraintViolated())
				throw new ArgumentException("These columns don't currently have unique values.");

			_belongsToCollection = true;
		}
					
		
		internal override void RemoveFromConstraintCollectionCleanup( 
				ConstraintCollection collection)
		{
			if (Columns.Length == 1)
				Columns [0].Unique = false;

			_belongsToCollection = false;
			Index = null;
		}

		internal override bool IsConstraintViolated()
		{	
			if (Index == null) {
				Index = Table.GetIndex(Columns,null,DataViewRowState.None,null,false);
			}

			if (Index.HasDuplicates) {
				int[] dups = Index.Duplicates;
				for (int i = 0; i < dups.Length; i++){
					DataRow row = Table.RecordCache[dups[i]];
					ArrayList columns = new ArrayList();
					ArrayList values = new ArrayList();
					foreach (DataColumn col in Columns){
						columns.Add(col.ColumnName);
						values.Add(row[col].ToString());
					}

					string columnNames = String.Join(", ", (string[])columns.ToArray(typeof(string)));
					string columnValues = String.Join(", ", (string[])values.ToArray(typeof(string)));

					row.RowError = String.Format("Column '{0}' is constrained to be unique.  Value '{1}' is already present.", columnNames, columnValues);
					for (int j=0; j < Columns.Length; ++j)
						row.SetColumnError (Columns [j], row.RowError);
				}
				return true;
			}
			return false;
		}

		internal override void AssertConstraint(DataRow row)
		{	
			if (IsPrimaryKey && row.HasVersion(DataRowVersion.Default)) {
				for (int i = 0; i < Columns.Length; i++) {
					if (row.IsNull(Columns[i])) {
						throw new NoNullAllowedException("Column '" + Columns[i].ColumnName + "' does not allow nulls.");
					}
				}
			}
			
			if (Index == null) {
				Index = Table.GetIndex(Columns,null,DataViewRowState.None,null,false);
			}

			if (Index.HasDuplicates) {
				throw new ConstraintException(GetErrorMessage(row));
			}
		}

		internal override bool IsColumnContained(DataColumn column)
		{
			for (int i = 0; i < _dataColumns.Length; i++)
				if (column == _dataColumns[i])
					return true;

			return false;
		}

		internal override bool CanRemoveFromCollection(ConstraintCollection col, bool shouldThrow){
			if (IsPrimaryKey) {
				if (shouldThrow)
					throw new ArgumentException("Cannot remove unique constraint since it's the primary key of a table.");

				return false;
			}
			
			if (Table.DataSet == null)
				return true;

			if (ChildConstraint != null) {
				if (!shouldThrow)
					return false;
				throw new ArgumentException (String.Format (
								"Cannot remove unique constraint '{0}'." +
								"Remove foreign key constraint '{1}' first.",
								ConstraintName,ChildConstraint.ConstraintName));
			}
			return true;
		}

		private string GetErrorMessage(DataRow row)
		{
			int i;
			 
			System.Text.StringBuilder sb = new System.Text.StringBuilder(row[_dataColumns[0]].ToString());
			for (i = 1; i < _dataColumns.Length; i++) {
				sb = sb.Append(", ").Append(row[_dataColumns[i].ColumnName]);
			}
			string valStr = sb.ToString();
			sb = new System.Text.StringBuilder(_dataColumns[0].ColumnName);
			for (i = 1; i < _dataColumns.Length; i++) {
				sb = sb.Append(", ").Append(_dataColumns[i].ColumnName);
			}
			string colStr = sb.ToString();
			return "Column '" + colStr + "' is constrained to be unique.  Value '" + valStr + "' is already present.";
		}
			               
		
		#endregion // Methods

	}

	
}
