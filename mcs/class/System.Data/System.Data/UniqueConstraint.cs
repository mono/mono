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

namespace System.Data {
	[Editor]
	[DefaultProperty ("ConstraintName")]
	[Serializable]
	public class UniqueConstraint : Constraint 
	{
		private bool _isPrimaryKey = false;
		private bool __isPrimaryKey = false;
		private DataTable _dataTable; //set by ctor except when unique case
		
		private DataColumn [] _dataColumns;

		//TODO:provide helpers for this case
		private string [] _dataColumnNames; //unique case
		private bool _dataColsNotValidated;


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
			 _dataColsNotValidated = true;
                                                                                                    
                        //keep list of names to resolve later
                        _dataColumnNames = columnNames;
                                                                                                    
                        base.ConstraintName = name;
                                                                                                    
                        _isPrimaryKey = isPrimaryKey;

		}

		//helper ctor
		private void _uniqueConstraint(string name, DataColumn column, bool isPrimaryKey) 
		{
			_dataColsNotValidated = false;
			//validate
			_validateColumn (column);

			//Set Constraint Name
			base.ConstraintName = name;

			__isPrimaryKey = isPrimaryKey;

			//keep reference 
			_dataColumns = new DataColumn [] {column};
			
			//Get table reference
			_dataTable = column.Table;

			
		}

		//helpter ctor	
		private void _uniqueConstraint(string name, DataColumn[] columns, bool isPrimaryKey) 
		{
			_dataColsNotValidated = false;
			
			//validate
			_validateColumns (columns, out _dataTable);

			//Set Constraint Name
			base.ConstraintName = name;

			//keep reference
			_dataColumns = columns;

			//PK?
			__isPrimaryKey = isPrimaryKey;

			
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

		/// <summary>
		///  If IsPrimaryKey is set to be true, this sets it true
		/// </summary>
		internal void UpdatePrimaryKey ()
		{
			_isPrimaryKey = __isPrimaryKey;
			// if unique constraint defined on single column
			// the column becomes unique
			if (_dataColumns.Length == 1){
				// use SetUnique - because updating Unique property causes loop
				_dataColumns[0].SetUnique();
			}
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
			
			UniqueConstraint uniqueConstraint;
			IEnumerator enumer = collection.GetEnumerator();
			while (enumer.MoveNext())
			{
				uniqueConstraint = enumer.Current as UniqueConstraint;
				if (uniqueConstraint != null)
				{
					if ( DataColumn.AreColumnSetsTheSame(uniqueConstraint.Columns, columns) )
					{
						return uniqueConstraint;
					}
				}
			}
			return null;
		}

		 internal bool DataColsNotValidated {
                        
			get { return (_dataColsNotValidated); 
			}
                }

		// Helper Special Ctor
                // Set the _dataTable property to the table to which this instance is bound when AddRange()
                // is called with the special constructor.
                // Validate whether the named columns exist in the _dataTable
                internal void PostAddRange( DataTable _setTable ) {
                
			_dataTable = _setTable;
                        DataColumn []cols = new DataColumn [_dataColumnNames.Length];
                        int i = 0;
                        foreach ( string _columnName in _dataColumnNames ){
                                 if ( _setTable.Columns.Contains (_columnName) ){
                                        cols [i] = _setTable.Columns [_columnName];
                                        i++;
                                        continue;
                                }
                                throw( new InvalidConstraintException ( "The named columns must exist in the table" ));
                        }
                        _dataColumns = cols;
                }

			
		#endregion //Helpers

		#region Properties

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the columns of this constraint.")]
		[ReadOnly (true)]
		public virtual DataColumn[] Columns {
			get { return _dataColumns; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates if this constraint is a primary key.")]
		public bool IsPrimaryKey {
			get { return _isPrimaryKey; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the table of this constraint.")]
		[ReadOnly (true)]
		public override DataTable Table {
			get { return _dataTable; }
		}

		#endregion // Properties

		#region Methods

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
		
		[MonoTODO]
		internal override void AddToConstraintCollectionSetup(
				ConstraintCollection collection)
		{
			//run Ctor rules again
			_validateColumns(_dataColumns);
			
			//make sure a unique constraint doesn't already exists for these columns
			UniqueConstraint uc = UniqueConstraint.GetUniqueConstraintForColumnSet(collection, this.Columns);	
			if (null != uc) throw new ArgumentException("Unique constraint already exists for these" +
					" columns. Existing ConstraintName is " + uc.ConstraintName);

			//Allow only one primary key
			if (this.IsPrimaryKey)
			{
				uc = GetPrimaryKeyConstraint(collection);
				if (null != uc) uc._isPrimaryKey = false;

			}
					
			//FIXME: ConstraintCollection calls AssertContraint() again rigth after calling
			//this method, so that it is executed twice. Need to investigate which
			// call to remove as that migth affect other parts of the classes.
			AssertConstraint();
		}
					
		
		internal override void RemoveFromConstraintCollectionCleanup( 
				ConstraintCollection collection)
		{
			Index index = this.Index;
			this.Index = null;
			// if a foreign key constraint references the same index - 
			// change the index be to not unique.
			// In this case we can not just drop the index
			ICollection fkCollection = collection.ForeignKeyConstraints;
			foreach (ForeignKeyConstraint fkc in fkCollection) {
				if (index == fkc.Index) {
					fkc.Index.SetUnique (false);
					// this is not referencing the index anymore
					return;
				}
			}
			
			// if we are here no one is using this index so we can remove it.
			// There is no need calling drop index here
			// since two unique constraints never references the same index
			// and we already check that there is no foreign key constraint referencing it.
			Table.RemoveIndex (index);
		}

		[MonoTODO]
		internal override void AssertConstraint()
		{			
			if (_dataTable == null) return; //???
			if (_dataColumns == null) return; //???		
			
			Index fromTableIndex = null;
			if (Index == null) {
				fromTableIndex = Table.GetIndexByColumns (Columns);
				if (fromTableIndex == null) {
					Index = new Index (ConstraintName, _dataTable, _dataColumns, true);	
				}
				else {
					fromTableIndex.SetUnique (true);
					Index = fromTableIndex;
				}
			}

			try {
				Table.InitializeIndex (Index);
			}
			catch (ConstraintException) {
#if NET_1_1
				throw;
#else
				Index = null;
				throw new ArgumentException (String.Format ("Column '{0}' contains non-unique values", this._dataColumns[0]));
#endif
			}
			
			// if there is no index with same columns - add the new index to the table.
			if (fromTableIndex == null)
				Table.AddIndex (Index);
		}

		[MonoTODO]
		internal override void AssertConstraint(DataRow row)
		{
			if (_dataTable == null) return; //???
			if (_dataColumns == null) return; //???
			
			if (Index == null) {
				Index = Table.GetIndexByColumns (Columns, true);
				if (Index == null) {
					Index = new Index (ConstraintName, _dataTable, _dataColumns, true);
					Table.AddIndex (Index);
				}
			}

			if (IsPrimaryKey) {
				object val;
				for (int i = 0; i < _dataColumns.Length; i++) {
					val = row[_dataColumns[i]];
					if (val == null || val == DBNull.Value)
						throw new NoNullAllowedException("Column '" + _dataColumns[i].ColumnName + "' does not allow nulls.");
				}
			}

			try {
				UpdateIndex (row);
			}
			catch (ConstraintException) {
				throw new ConstraintException(GetErrorMessage(row));
			}
		}

		private string GetErrorMessage(DataRow row)
		{
			int i;
			 
			System.Text.StringBuilder sb = new System.Text.StringBuilder(row[_dataColumns[0]].ToString());
			for (i = 1; i < _dataColumns.Length; i++)
				sb = sb.Append(", ").Append(row[_dataColumns[i].ColumnName]);
			string valStr = sb.ToString();
			sb = new System.Text.StringBuilder(_dataColumns[0].ColumnName);
			for (i = 1; i < _dataColumns.Length; i++)
				sb = sb.Append(", ").Append(_dataColumns[i].ColumnName);
			string colStr = sb.ToString();
			return "Column '" + colStr + "' is constrained to be unique.  Value '" + valStr + "' is already present.";
		}
		
                internal bool Contains (DataColumn c)
                {
                        foreach (DataColumn col in Columns)
                                if (c == col)
                                        return true;
                        return false;
                }
                
		
		#endregion // Methods

	}

	
}
