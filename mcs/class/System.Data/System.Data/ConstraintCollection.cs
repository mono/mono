//
// System.Data.ConstraintCollection.cs
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Daniel Morgan
//   
// (C) Ximian, Inc. 2002
// (C) 2002 Franklin Wise
// (C) 2002 Daniel Morgan
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data {
	[Serializable]
	internal delegate void DelegateValidateRemoveConstraint(ConstraintCollection sender, Constraint constraintToRemove, ref bool fail,ref string failReason);
	
	/// <summary>
	/// hold collection of constraints for data table
	/// </summary>
	[DefaultEvent ("CollectionChanged")]
	[Serializable]
	public class ConstraintCollection : InternalDataCollectionBase 
	{
		//private bool beginInit = false;
		
		public event CollectionChangeEventHandler CollectionChanged;
		internal event DelegateValidateRemoveConstraint ValidateRemoveConstraint;
		
		//Don't allow public instantiation
		//Will be instantianted from DataTable
		internal ConstraintCollection(){} 

		public virtual Constraint this[string name] {
			get {
				//If the name is not found we just return null
				int index = IndexOf(name); //case insensitive
				if (-1 == index) return null;
				return this[index];
			}
		}
		
		public virtual Constraint this[int index] {
			get {
				if (index < 0 || index >= List.Count)
					throw new IndexOutOfRangeException();
				return (Constraint)List[index];
			}
		}

		private void _handleBeforeConstraintNameChange(object sender, string newName)
		{
			//null or empty
			if (newName == null || newName == "") 
				throw new ArgumentException("ConstraintName cannot be set to null or empty " +
					" after it has been added to a ConstraintCollection.");

			if (_isDuplicateConstraintName(newName,(Constraint)sender))
				throw new DuplicateNameException("Constraint name already exists.");
		}

		private bool _isDuplicateConstraintName(string constraintName, Constraint excludeFromComparison) 
		{	
			string cmpr = constraintName.ToUpper();
			foreach (Constraint cst in List) 
			{
				//Case insensitive comparision
				if (  cmpr.CompareTo(cst.ConstraintName.ToUpper()) == 0  &&
					cst != excludeFromComparison) 
				{
					return true;
				}
			}

			return false;
		}
		
		//finds an open name slot of ConstraintXX
		//where XX is a number
		private string _createNewConstraintName() 
		{
			bool loopAgain = false;
			int index = 1;

			do
			{	
				loopAgain = false;
				foreach (Constraint cst in List) 
				{
					//Case insensitive
					if (cst.ConstraintName.ToUpper().CompareTo("CONSTRAINT" + 
						index.ToString()) == 0 ) 
					{
						loopAgain = true;
						index++;
					}
				}
			} while (loopAgain);

			return "Constraint" + index.ToString(); 	
			
		}
		
		
		// Overloaded Add method (5 of them)
		// to add Constraint object to the collection

		public void Add(Constraint constraint) 
		{		
			//not null
			if (null == constraint) throw new ArgumentNullException("Can not add null.");
			
			//check constraint membership 
			//can't already exist in this collection or any other
			if (this == constraint.ConstraintCollection) 
				throw new ArgumentException("Constraint already belongs to this collection.");
			if (null != constraint.ConstraintCollection) 
				throw new ArgumentException("Constraint already belongs to another collection.");
			
			//check for duplicate name
			if (_isDuplicateConstraintName(constraint.ConstraintName,null)  )
				throw new DuplicateNameException("Constraint name already exists.");
		
			//Allow constraint to run validation rules and setup 
			constraint.AddToConstraintCollectionSetup(this); //may throw if it can't setup

			//Run Constraint to check existing data in table
			constraint.AssertConstraint();

			//if name is null or empty give it a name
			if (constraint.ConstraintName == null || 
				constraint.ConstraintName == "" ) 
			{ 
				constraint.ConstraintName = _createNewConstraintName();
			}

			//Add event handler for ConstraintName change
			constraint.BeforeConstraintNameChange += new DelegateConstraintNameChange(
				_handleBeforeConstraintNameChange);
			
			constraint.ConstraintCollection = this;
			List.Add(constraint);

			OnCollectionChanged( new CollectionChangeEventArgs( CollectionChangeAction.Add, this) );
		}

	

		public virtual Constraint Add(string name, DataColumn column, bool primaryKey) 
		{

			UniqueConstraint uc = new UniqueConstraint(name, column, primaryKey);
			Add(uc);
			 
			return uc;
		}

		public virtual Constraint Add(string name, DataColumn primaryKeyColumn,
			DataColumn foreignKeyColumn) 
		{
			ForeignKeyConstraint fc = new ForeignKeyConstraint(name, primaryKeyColumn, 
					foreignKeyColumn);
			Add(fc);

			return fc;
		}

		public virtual Constraint Add(string name, DataColumn[] columns, bool primaryKey) 
		{
			UniqueConstraint uc = new UniqueConstraint(name, columns, primaryKey);
			Add(uc);

			return uc;
		}

		public virtual Constraint Add(string name, DataColumn[] primaryKeyColumns,
			DataColumn[] foreignKeyColumns) 
		{
			ForeignKeyConstraint fc = new ForeignKeyConstraint(name, primaryKeyColumns, 
					foreignKeyColumns);
			Add(fc);

			return fc;
		}

		[MonoTODO]
		public void AddRange(Constraint[] constraints) {

			throw new NotImplementedException ();
		}

		public bool CanRemove(Constraint constraint) 
		{

			//Rule A UniqueConstraint can't be removed if there is
			//a foreign key relationship to that column

			//not null 
			//LAMESPEC: MSFT implementation throws and exception here
			//spec says nothing about this
			if (null == constraint) throw new ArgumentNullException("Constraint can't be null.");
			
			//LAMESPEC: spec says return false (which makes sense) and throw exception for False case (?).
			//TODO: I may want to change how this is done
			//maybe put a CanRemove on the Constraint class
			//and have the Constraint fire this event

			//discover if there is a related ForeignKey
			string failReason ="";
			return _canRemoveConstraint(constraint, ref failReason);
			
		}

		[MonoTODO]
		public void Clear() 
		{
			
			//CanRemove? See Lamespec below.

			//the Constraints have a reference to us
			//and we listen to name change events 
			//we should remove these before clearing
			foreach (Constraint con in List)
			{
				con.ConstraintCollection = null;
				con.BeforeConstraintNameChange -= new DelegateConstraintNameChange(
				_handleBeforeConstraintNameChange);
			}

			//LAMESPEC: MSFT implementation allows this
			//even when a ForeignKeyConstraint exist for a UniqueConstraint
			//thus violating the CanRemove logic
			List.Clear(); //Will violate CanRemove rule
			OnCollectionChanged( new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this) );
		}

		public bool Contains(string name) 
		{
			return (-1 != IndexOf(name));
		}

		public int IndexOf(Constraint constraint) 
		{
			return List.IndexOf(constraint);
		}

		public virtual int IndexOf(string constraintName) 
		{
			//LAMESPEC: Spec doesn't say case insensitive
			//it should to be consistant with the other 
			//case insensitive comparisons in this class

			int index = 0;
			foreach (Constraint con in List)
			{
				if (constraintName.ToUpper().CompareTo( con.ConstraintName.ToUpper() ) == 0)
				{
					return index;
				}

				index++;
			}
			return -1; //not found
		}

		public void Remove(Constraint constraint) {
			//LAMESPEC: spec doesn't document the ArgumentException the
			//will be thrown if the CanRemove rule is violated
			
			//LAMESPEC: spec says an exception will be thrown
			//if the element is not in the collection. The implementation
			//doesn't throw an exception. ArrayList.Remove doesn't throw if the
			//element doesn't exist
			//ALSO the overloaded remove in the spec doesn't say it throws any exceptions

			//not null
			if (null == constraint) throw new ArgumentNullException();

			string failReason = "";
			if (! _canRemoveConstraint(constraint, ref failReason) )
			{
				if (failReason != null || failReason != "")	
					throw new ArgumentException(failReason);
				else
					throw new ArgumentException("Can't remove constraint.");		
			}
				
			constraint.RemoveFromConstraintCollectionCleanup(this);
			List.Remove(constraint);
			OnCollectionChanged( new CollectionChangeEventArgs(CollectionChangeAction.Remove,this));
		}

		public void Remove(string name) 
		{
			//if doesn't exist fail quietly
			int index = IndexOf(name);
			if (-1 == index) return;

			Remove(this[index]);
		}

		public void RemoveAt(int index) 
		{
			if (index < 0 || index + 1 > List.Count)
				throw new IndexOutOfRangeException("Index out of range, index = " 
						+ index.ToString() + ".");
	
			this[index].RemoveFromConstraintCollectionCleanup(this);
			List.RemoveAt(index);
			OnCollectionChanged( new CollectionChangeEventArgs(CollectionChangeAction.Remove,this));
		}

		protected override ArrayList List {
			get{
				return base.List;
			}
		}

		protected virtual void OnCollectionChanged( CollectionChangeEventArgs ccevent) 
		{
			if (null != CollectionChanged)
			{
				CollectionChanged(this, ccevent);
			}
		}

		private bool _canRemoveConstraint(Constraint constraint, ref string failReason )
		{
			bool cancel = false;
			string tmp = "";
			if (null != ValidateRemoveConstraint)
			{
				ValidateRemoveConstraint(this, constraint, ref cancel, ref tmp);
			}
			failReason = tmp;
			return !cancel;
		}
	}
}
