//
// System.Data.UniqueConstraint.cs
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Daniel Morgan <danmorg@sc.rr.com>
//   
// (C) 2002 Franklin Wise
// (C) 2002 Daniel Morgan

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Data
{
	public class UniqueConstraint : Constraint 
	{
		private bool _isPrimaryKey = false;
		private DataTable _dataTable; //set by ctor except when unique case
		
		private DataColumn [] _dataColumns;

		//TODO:provide helpers for this case
		private string [] _dataColumnNames; //unique case
		

		#region Constructors

		public UniqueConstraint(DataColumn column) {

			_uniqueConstraint ("", column, false);
		}

		public UniqueConstraint(DataColumn[] columns) {
			
			_uniqueConstraint ("", columns, false);
		}

		public UniqueConstraint(DataColumn column,
			bool isPrimaryKey) {

			_uniqueConstraint ("", column, isPrimaryKey);
		}

		public UniqueConstraint(DataColumn[] columns, bool isPrimaryKey) {

			_uniqueConstraint ("", columns, isPrimaryKey);
		}

		public UniqueConstraint(string name, DataColumn column) {

			_uniqueConstraint (name, column, false);
		}

		public UniqueConstraint(string name, DataColumn[] columns) {

			_uniqueConstraint (name, columns, false);
		}

		public UniqueConstraint(string name, DataColumn column,
			bool isPrimaryKey) {

			_uniqueConstraint (name, column, isPrimaryKey);
		}

		public UniqueConstraint(string name,
			DataColumn[] columns, bool isPrimaryKey) {

			_uniqueConstraint (name, columns, isPrimaryKey);
		}

		//Special case.  Can only be added to the Collection with AddRange
		[MonoTODO]
		public UniqueConstraint(string name,
			string[] columnNames, bool isPrimaryKey) {

			throw new NotImplementedException(); //need to finish related logic

			base.ConstraintName = name;
			
			//set unique
			//must set unique when added to the collection

			//keep list of names to resolve later
			_dataColumnNames = columnNames;

			_isPrimaryKey = isPrimaryKey;
		}

		//helper ctor
		private void _uniqueConstraint(string name, 
				DataColumn column, bool isPrimaryKey) {

			//validate
			_validateColumn (column);

			//Set Constraint Name
			base.ConstraintName = name;

			//set unique
			column.Unique = true;

			//keep reference 
			_dataColumns = new DataColumn [] {column};
			
			//PK?
			_isPrimaryKey = isPrimaryKey;

			//Get table reference
			_dataTable = column.Table;
		}

		//helpter ctor	
		public void _uniqueConstraint(string name,
			DataColumn[] columns, bool isPrimaryKey) {

			//validate
			_validateColumns (columns, out _dataTable);

			//Set Constraint Name
			base.ConstraintName = name;

			//set unique
			_setColumnsUnique (columns);

			//keep reference
			_dataColumns = columns;

			//PK?
			_isPrimaryKey = isPrimaryKey;

		}
		
		#endregion // Constructors

		#region Helpers
		private void _setColumnsUnique(DataColumn [] columns) {
			if (null == columns) return;
			foreach (DataColumn col in columns) {
				col.Unique = true;
			}
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

		internal static UniqueConstraint GetUniqueConstraintForColumnSet(ConstraintCollection collection,
				DataColumn[] columns)
		{
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

		public virtual DataColumn[] Columns {
			get {
				return _dataColumns;
			}
		}

		public bool IsPrimaryKey {
			get {
				return _isPrimaryKey;
			}
		}

		public override DataTable Table {
			get {
				return _dataTable;
			}
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

		[MonoTODO]
		public override int GetHashCode() {
			throw new NotImplementedException ();
		}
		
	
		internal protected override void AddToConstraintCollectionSetup(
				ConstraintCollection collection)
		{
			//TODO:Should run Ctor rules again
			
			//make sure a unique constraint doesn't already exists for these columns
			UniqueConstraint uc = UniqueConstraint.GetUniqueConstraintForColumnSet(collection, this.Columns);	
			if (null != uc) throw new ArgumentException("Unique constraint already exists for these" +
					" columns.");
					
		}
					
		
		internal protected override void RemoveFromConstraintCollectionCleanup( 
				ConstraintCollection collection)
		{
		}

		[MonoTODO]
		internal override void AssertConstraint()
		{
			//Unique?
		}
		#endregion // Methods
	}
}
