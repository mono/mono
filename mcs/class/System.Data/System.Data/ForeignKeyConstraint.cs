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

namespace System.Data
{

	[Serializable]
	public class ForeignKeyConstraint : Constraint {

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

		public ForeignKeyConstraint(string constraintName,
			DataColumn parentColumn, DataColumn childColumn) 
		{
			if (null == parentColumn || null == childColumn) {
				throw new ArgumentNullException("Neither parentColumn or" +
					" childColumn can be null.");
			}

			_foreignKeyConstraint(constraintName, new DataColumn[] {parentColumn},
					new DataColumn[] {childColumn});
		}

		public ForeignKeyConstraint(string constraintName,
			DataColumn[] parentColumns, DataColumn[] childColumns) 
		{
			_foreignKeyConstraint(constraintName, parentColumns, childColumns);
		}
		
		//special case
		[MonoTODO]
		public ForeignKeyConstraint(string constraintName,
			string parentTableName,	string[] parentColumnNames,
			string[] childColumnNames, 
			AcceptRejectRule acceptRejectRule, Rule deleteRule,
			Rule updateRule) {
		}


		private void _foreignKeyConstraint(string constraintName, DataColumn[] parentColumns,
				DataColumn[] childColumns)
		{

			//Validate 
			_validateCtor(parentColumns, childColumns);

			//Set Constraint Name
			base.ConstraintName = constraintName;	

			//Keep reference to columns
			_parentColumns = parentColumns;
			_childColumns = childColumns;
		}

		[MonoTODO]
		private void _validateCtor(DataColumn[] parentColumns, DataColumn[] childColumns)
		{
			//not null
			if (null == parentColumns || null == childColumns) 
				throw new ArgumentNullException();
			
			if (parentColumns.Length < 1 || childColumns.Length < 1)
				throw new ArgumentException("Neither ParentColumns or ChildColumns can't be" +
						" zero length.");
				
			//DataTypes must match
			//column sets must all be from the same table

			DataTable ptable = parentColumns[0].Table;
			DataTable ctable = childColumns[0].Table;
		
			//not null check
			//TODO: columns must belong to a table else ArgumentException
			
			
			foreach (DataColumn pc in parentColumns)
			{
				
				foreach (DataColumn cc in childColumns)
				{

				}	
			}

			//Tables must be from same datasets
			if (ptable.DataSet != ctable.DataSet)
			{
				throw new InvalidOperationException("Tables must belong to the same dataset.");
			}
			
		}
		
		#endregion // Constructors

		#region Helpers
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
		

		#region Properites

		public virtual AcceptRejectRule AcceptRejectRule {
			get {
				return _acceptRejectRule;
			}
			
			set {
				_acceptRejectRule = value;
			}
		}

		public virtual Rule DeleteRule {
			get {
				return _deleteRule;
			}
			
			set {
				_deleteRule = value;
			}
		}

		public virtual Rule UpdateRule {
			get {
				return _updateRule;
			}
			
			set {
				_updateRule = value;
			}
		}
		
		public virtual DataColumn[] Columns {
			get {
				return _childColumns;
			}
		}

		public virtual DataColumn[] RelatedColumns {
			get {	
				return _parentColumns;
			}
		}

		public virtual DataTable RelatedTable {
			get {
				if (_parentColumns != null)
					if (_parentColumns.Length > 0)
						return _parentColumns[0].Table;

				return null;
			}
		}

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

		[MonoTODO]
		public override int GetHashCode() {
			throw new NotImplementedException ();
		}

		protected internal override void AddToConstraintCollectionSetup(
				ConstraintCollection collection)
		{

			//TODO:Should run Ctor rules again
			
			_ensureUniqueConstraintExists(collection, _parentColumns);
			
		}
					
	
		[MonoTODO]
		protected internal override void RemoveFromConstraintCollectionCleanup( 
				ConstraintCollection collection)
		{
			return; //no rules yet		
		}
		
		[MonoTODO]
		internal override void AssertConstraint()
		{
			//Constraint only works if both tables are part of the same dataset
			
			//if DataSet ...
			
			//check for orphaned children
			//check for...
			
		}
		
		#endregion // Methods
	}

}
