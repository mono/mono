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
		private DataTable _dataTable; //set by ctor except when unique case
		
		private DataColumn [] _dataColumns;

		//TODO:provide helpers for this case
		private string [] _dataColumnNames; //unique case
		

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
		[MonoTODO]
		[Browsable (false)]
		public UniqueConstraint (string name, string[] columnNames, bool isPrimaryKey) 
		{
			throw new NotImplementedException(); //need to finish related logic
			/*
			base.ConstraintName = name;
			
			//set unique
			//must set unique when added to the collection

			//keep list of names to resolve later
			_dataColumnNames = columnNames;

			_isPrimaryKey = isPrimaryKey;
			*/
		}

		//helper ctor
		private void _uniqueConstraint(string name, DataColumn column, bool isPrimaryKey) 
		{
			//validate
			_validateColumn (column);

			//Set Constraint Name
			base.ConstraintName = name;

			//keep reference 
			_dataColumns = new DataColumn [] {column};
			
			//PK?
			_isPrimaryKey = isPrimaryKey;

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

			//keep reference
			_dataColumns = columns;

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
			if (null == column) throw new ArgumentNullException();
			
			//column must belong to a table
			//LAMESPEC: not in spec
			if (null == column.Table) 
				throw new ArgumentException("Column " + column.ColumnName + " must belong to a table.");
			
		}

		internal static void SetAsPrimaryKey(ConstraintCollection collection, UniqueConstraint newPrimaryKey)
		{
			//not null
			if (null == collection) throw new ArgumentNullException("ConstraintCollection can't be null.");
			
			//make sure newPrimaryKey belongs to the collection parm unless it is null
			if (  collection.IndexOf(newPrimaryKey) < 1 && (null != newPrimaryKey) ) 
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
		[ReadOnly (true)]
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
		}

		[MonoTODO]
		internal override void AssertConstraint()
		{
			
			if (_dataTable == null) return; //???
			if (_dataColumns == null) return; //???

			
			//Unique?	
			DataTable tbl = _dataTable;

			//TODO: Investigate other ways of speeding up the validation work below.
			//FIXME: This only works when only one DataColumn has been specified.

			//validate no duplicates exists.
			//Only validate when there are at least 2 rows
			//so that a duplicate migth exist.
			if(tbl.Rows.Count > 1) {
				//get copy of rows collection first so that we do not modify the
				//original.
				DataRow[] rows = new DataRow [tbl.Rows.Count];
				tbl.Rows.CopyTo (rows, 0);
				ArrayList clonedDataList = new ArrayList (rows);

				ArrayList newDataList = new ArrayList();

				//copy to array list only the column we are interested in.
				foreach (DataRow row in clonedDataList)
					newDataList.Add (row[this._dataColumns[0]]);
				
				//sort ArrayList and check adjacent values for duplicates.
				newDataList.Sort ();

				for (int i = 0 ; i < newDataList.Count - 1 ; i++) 
					if (newDataList[i].Equals (newDataList[i+1])) 
						throw new InvalidConstraintException (String.Format ("Column '{0}' contains non-unique values", this._dataColumns[0]));
			}


		}

		[MonoTODO]
		internal override void AssertConstraint(DataRow row)
		{

			if (_dataTable == null) return; //???
			if (_dataColumns == null) return; //???


			//Unique?
			DataTable tbl = _dataTable;

			foreach(DataRow compareRow in tbl.Rows)
			{
				//skip if it is the same row to be validated
				if(!row.Equals(compareRow))
				{
					if(compareRow.HasVersion (DataRowVersion.Original))
					{
						//FIXME: should we compare to compareRow[DataRowVersion.Current]?
						//FIXME: We need to compare to all columns the constraint is set to.
						if(row[_dataColumns[0], DataRowVersion.Proposed].Equals( compareRow[_dataColumns[0], DataRowVersion.Current]))
						{
							string ExceptionMessage;
							ExceptionMessage = "Column '" + _dataColumns[0].ColumnName + "' is constrained to be unique.";
							ExceptionMessage += " Value '" + row[_dataColumns[0], DataRowVersion.Proposed].ToString();
							ExceptionMessage += "' is already present.";

							throw new ConstraintException (ExceptionMessage);
						}

					}

		}

			}

		}

		#endregion // Methods
	}
}
