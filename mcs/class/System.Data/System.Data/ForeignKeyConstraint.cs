//
// System.Data.ForeignKeyConstraint.cs
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) 2002 Franklin Wise
// (C) 2002 Daniel Morgan
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Data {
	[DefaultProperty ("ConstraintName")]
	[Serializable]
	public class ForeignKeyConstraint : Constraint 
	{
		private UniqueConstraint _parentUniqueConstraint;
		private DataColumn [] _parentColumns;
		private DataColumn [] _childColumns;
		private Rule _deleteRule;
		private Rule _updateRule;
		private AcceptRejectRule _acceptRejectRule;
		
		#region Constructors

		public ForeignKeyConstraint(DataColumn parentColumn, DataColumn childColumn) 
		{
			if (null == parentColumn || null == childColumn) {
				throw new ArgumentNullException("Neither parentColumn or" +
					" childColumn can be null.");
			}

			_foreignKeyConstraint(null, new DataColumn[] {parentColumn},
					new DataColumn[] {childColumn});
		}

		public ForeignKeyConstraint(DataColumn[] parentColumns, DataColumn[] childColumns) 
		{
			_foreignKeyConstraint(null, parentColumns, childColumns);
		}

		public ForeignKeyConstraint(string constraintName, DataColumn parentColumn, DataColumn childColumn) 
		{
			if (null == parentColumn || null == childColumn) {
				throw new ArgumentNullException("Neither parentColumn or" +
					" childColumn can be null.");
			}

			_foreignKeyConstraint(constraintName, new DataColumn[] {parentColumn},
					new DataColumn[] {childColumn});
		}

		public ForeignKeyConstraint(string constraintName, DataColumn[] parentColumns, DataColumn[] childColumns) 
		{
			_foreignKeyConstraint(constraintName, parentColumns, childColumns);
		}
		
		//special case
		[MonoTODO]
		[Browsable (false)]
		public ForeignKeyConstraint(string constraintName, string parentTableName, string[] parentColumnNames, string[] childColumnNames, AcceptRejectRule acceptRejectRule, Rule deleteRule, Rule updateRule) 
		{
		}

		private void _foreignKeyConstraint(string constraintName, DataColumn[] parentColumns,
				DataColumn[] childColumns)
		{

			//Validate 
			_validateColumns(parentColumns, childColumns);

			//Set Constraint Name
			base.ConstraintName = constraintName;	

			//Keep reference to columns
			_parentColumns = parentColumns;
			_childColumns = childColumns;
		}

		#endregion // Constructors

		#region Helpers

		private void _validateColumns(DataColumn[] parentColumns, DataColumn[] childColumns)
		{
			//not null
			if (null == parentColumns || null == childColumns) 
				throw new ArgumentNullException();
			
			//at least one element in each array
			if (parentColumns.Length < 1 || childColumns.Length < 1)
				throw new ArgumentException("Neither ParentColumns or ChildColumns can't be" +
						" zero length.");
				
			//same size arrays
			if (parentColumns.Length != childColumns.Length)
				throw new ArgumentException("Parent columns and child columns must be the same length.");
			

			DataTable ptable = parentColumns[0].Table;
			DataTable ctable = childColumns[0].Table;
	
			
			foreach (DataColumn pc in parentColumns)
			{
				//not null check
				if (null == pc.Table) 
				{
					throw new ArgumentException("All columns must belong to a table." + 
							" ColumnName: " + pc.ColumnName + " does not belong to a table.");
				}
				
				//All columns must belong to the same table
				if (ptable != pc.Table)
					throw new InvalidConstraintException("Parent columns must all belong to the same table.");
				
				foreach (DataColumn cc in childColumns)
				{
					//not null
					if (null == pc.Table) 
					{
						throw new ArgumentException("All columns must belong to a table." + 
							" ColumnName: " + pc.ColumnName + " does not belong to a table.");
					}
			
					//All columns must belong to the same table.
					if (ctable != cc.Table)
						throw new InvalidConstraintException("Child columns must all belong to the same table.");
						
						
					//Can't be the same column
					if (pc == cc)
						throw new InvalidOperationException("Parent and child columns can't be the same column.");

					if (! pc.DataType.Equals(cc.DataType))
					{
						//LAMESPEC: spec says throw InvalidConstraintException
						//		implementation throws InvalidOperationException
						throw new InvalidOperationException("Parent column is not type compatible with it's child"
								+ " column.");
					}
				}	
			}
			
			
			//Same dataset.  If both are null it's ok
			if (ptable.DataSet != ctable.DataSet)
			{
				//LAMESPEC: spec says InvalidConstraintExceptoin
				//	impl does InvalidOperationException
				throw new InvalidOperationException("Parent column and child column must belong to" + 
						" tables that belong to the same DataSet.");
						
			}

			
		}
		


		private void _validateRemoveParentConstraint(ConstraintCollection sender, 
				Constraint constraint, ref bool cancel, ref string failReason)
		{
			//if we hold a reference to the parent then cancel it
			if (constraint == _parentUniqueConstraint) 
			{
				cancel = true;
				failReason = "Cannot remove UniqueConstraint because the"
					+ " ForeignKeyConstraint " + this.ConstraintName + " exists.";
			}
		}
		
		//Checks to see if a related unique constraint exists
		//if it doesn't then a unique constraint is created.
		//if a unique constraint can't be created an exception will be thrown
		private void _ensureUniqueConstraintExists(ConstraintCollection collection,
				DataColumn [] parentColumns)
		{
			//not null
			if (null == parentColumns) throw new ArgumentNullException(
					"ParentColumns can't be null");

			UniqueConstraint uc = null;

			//see if unique constraint already exists
			//if not create unique constraint
			uc = UniqueConstraint.GetUniqueConstraintForColumnSet(collection, parentColumns);

			if (null == uc)	uc = new UniqueConstraint(parentColumns, false); //could throw
			
			//keep reference
			_parentUniqueConstraint = uc;

			//if this unique constraint is attempted to be removed before us
			//we can fail the validation
			collection.ValidateRemoveConstraint += new DelegateValidateRemoveConstraint(
					_validateRemoveParentConstraint);
		}
		
		
		#endregion //Helpers
		
		#region Properties

		[DataCategory ("Data")]
		[DataSysDescription ("For accept and reject changes, indicates what kind of cascading should take place across this relation.")]
		[DefaultValue (AcceptRejectRule.None)]
		public virtual AcceptRejectRule AcceptRejectRule {
			get { return _acceptRejectRule; }
			set { _acceptRejectRule = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the child columns of this constraint.")]
		[ReadOnly (true)]
		public virtual DataColumn[] Columns {
			get { return _childColumns; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("For deletions, indicates what kind of cascading should take place across this relation.")]
		[DefaultValue (Rule.Cascade)]
		public virtual Rule DeleteRule {
			get { return _deleteRule; }
			set { _deleteRule = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("For updates, indicates what kind of cascading should take place across this relation.")]
		[DefaultValue (Rule.Cascade)]
		public virtual Rule UpdateRule {
			get { return _updateRule; }
			set { _updateRule = value; }
		}

		[DataCategory ("Data")]	
		[DataSysDescription ("Indicates the parent columns of this constraint.")]
		[ReadOnly (true)]
		public virtual DataColumn[] RelatedColumns {
			get { return _parentColumns; }
		}

		[DataCategory ("Data")]	
		[DataSysDescription ("Indicates the child table of this constraint.")]
		[ReadOnly (true)]
		public virtual DataTable RelatedTable {
			get {
				if (_parentColumns != null)
					if (_parentColumns.Length > 0)
						return _parentColumns[0].Table;

				return null;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the table of this constraint.")]
		[ReadOnly (true)]
		public override DataTable Table {
			get {
				if (_childColumns != null)
					if (_childColumns.Length > 0)
						return _childColumns[0].Table;

				return null;
			}
		}

		#endregion // Properties

		#region Methods

		public override bool Equals(object key) 
		{
			ForeignKeyConstraint fkc = key as ForeignKeyConstraint;
			if (null == fkc) return false;

			//if the fk constrains the same columns then they are equal
			if (! DataColumn.AreColumnSetsTheSame( this.RelatedColumns, fkc.RelatedColumns))
				return false;
			if (! DataColumn.AreColumnSetsTheSame( this.Columns, fkc.Columns) )
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			//initialize hash1 and hash2 with default hashes
			//any two DIFFERENT numbers will do here
			int hash1 = 32, hash2 = 88;
			int i;

			//derive the hash code from the columns that way
			//Equals and GetHashCode return Equal objects to be the
			//same

			//Get the first parent column hash
			if (this.Columns.Length > 0)
				hash1 ^= this.Columns[0].GetHashCode();
			
			//get the rest of the parent column hashes if there any
			for (i = 1; i < this.Columns.Length; i++)
			{
				hash1 ^= this.Columns[1].GetHashCode();
				
			}
			
			//Get the child column hash
			if (this.RelatedColumns.Length > 0)
				hash2 ^= this.Columns[0].GetHashCode();
			
			for (i = 1; i < this.RelatedColumns.Length; i++)
			{
				hash2 ^= this.RelatedColumns[1].GetHashCode();
			}

			//combine the two hashes
			return hash1 ^ hash2;
		}

		internal override void AddToConstraintCollectionSetup(
				ConstraintCollection collection)
		{

			//run Ctor rules again
			_validateColumns(_parentColumns, _childColumns);
			
			//we must have a unique constraint on the parent
			_ensureUniqueConstraintExists(collection, _parentColumns);
			
			//Make sure we can create this thing
			AssertConstraint(); //TODO:if this fails and we created a unique constraint
						//we should probably roll it back
			
		}
					
	
		[MonoTODO]
		internal override void RemoveFromConstraintCollectionCleanup( 
				ConstraintCollection collection)
		{
			return; //no rules yet		
		}
		
		[MonoTODO]
		internal override void AssertConstraint()
		{
			//Constraint only works if both tables are part of the same dataset
			
			//if DataSet ...
			if (Table == null || RelatedTable == null) return; //TODO: Do we want this

			if (Table.DataSet == null || RelatedTable.DataSet == null) return; //	
				
			//TODO:
			//check for orphaned children
			//check for...
			
		}
		
		[MonoTODO]
		internal override void AssertConstraint(DataRow row)
		{
			//Implement: this should be used to validate ForeignKeys constraints 
			//when modifiying the DataRow values of a DataTable.
		}
		
		#endregion // Methods
	}

}
