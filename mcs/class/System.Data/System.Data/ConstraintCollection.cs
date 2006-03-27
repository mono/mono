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

namespace System.Data {
	[Editor]
	[Serializable]
	internal delegate void DelegateValidateRemoveConstraint(ConstraintCollection sender, Constraint constraintToRemove, ref bool fail,ref string failReason);
	
	/// <summary>
	/// hold collection of constraints for data table
	/// </summary>
	[DefaultEvent ("CollectionChanged")]
	[EditorAttribute("Microsoft.VSDesigner.Data.Design.ConstraintsCollectionEditor, "+Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+Consts.AssemblySystem_Drawing )]
	[Serializable]
	public class ConstraintCollection : InternalDataCollectionBase 
	{
		//private bool beginInit = false;
		
		public event CollectionChangeEventHandler CollectionChanged;
		private DataTable table;
		
		// Call this to set the "table" property of the UniqueConstraint class
                // intialized with UniqueConstraint( string, string[], bool );
                // And also validate that the named columns exist in the "table"
                private delegate void PostAddRange( DataTable table );
		
		// Keep reference to most recent constraints passed to AddRange()
                // so that they can be added when EndInit() is called.
                private Constraint [] _mostRecentConstraints;

		//Don't allow public instantiation
		//Will be instantianted from DataTable
		internal ConstraintCollection(DataTable table){
			this.table = table;
		} 

		internal DataTable Table{
			get{
				return this.table;
			}
		}

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
			foreach (Constraint cst in List) {
				if (String.Compare (constraintName, cst.ConstraintName, false, Table.Locale) == 0  && cst != excludeFromComparison) 
					return true;
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
					if (String.Compare (cst.ConstraintName,
						"Constraint" + index,
						!Table.CaseSensitive,
						Table.Locale)
						== 0)
					{
						loopAgain = true;
						index++;
						break;
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
	
			// Check whether Constraint is UniqueConstraint and initailized with the special
            // constructor - UniqueConstraint( string, string[], bool );
            // If yes, It must be added via AddRange() only
            // Environment.StackTrace can help us 
			// FIXME: Is a different mechanism to do this?
            if (constraint is UniqueConstraint){
                if ((constraint as UniqueConstraint).DataColsNotValidated == true){
                    if ( Environment.StackTrace.IndexOf( "AddRange" ) == -1 ){
                        throw new ArgumentException(" Some DataColumns are invalid - They may not belong to the table associated with this Constraint Collection" );
                    }
                }
            }

			if (constraint is ForeignKeyConstraint){
                if ((constraint as ForeignKeyConstraint).DataColsNotValidated == true){
                    if ( Environment.StackTrace.IndexOf( "AddRange" ) == -1 ){
                        throw new ArgumentException(" Some DataColumns are invalid - They may not belong to the table associated with this Constraint Collection" );
                    }
                }
            }

			//Allow constraint to run validation rules and setup 
			constraint.AddToConstraintCollectionSetup(this); //may throw if it can't setup			

			//Run Constraint to check existing data in table
			// this is redundant, since AddToConstraintCollectionSetup 
			// calls AssertConstraint right before this call
			//constraint.AssertConstraint();

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

			if (constraint is UniqueConstraint && ((UniqueConstraint)constraint).IsPrimaryKey) { 
				table.PrimaryKey = ((UniqueConstraint)constraint).Columns;
			}

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

		public void AddRange(Constraint[] constraints) {

			//When AddRange() occurs after BeginInit,
            //it does not add any elements to the collection until EndInit is called.
			if (this.table.fInitInProgress) {
				// Keep reference so that they can be added when EndInit() is called.
                    _mostRecentConstraints = constraints;
                    return;
            }

			if ( (constraints == null) || (constraints.Length == 0))
					return;

            // Check whether the constraint is UniqueConstraint
            // And whether it was initialized with the special ctor
            // i.e UniqueConstraint( string, string[], bool );
            for (int i = 0; i < constraints.Length; i++){
                if (constraints[i] is UniqueConstraint){
                    if (( constraints[i] as UniqueConstraint).DataColsNotValidated == true){
                            PostAddRange _postAddRange= new PostAddRange ((constraints[i] as UniqueConstraint).PostAddRange);
                            // UniqueConstraint.PostAddRange() validates whether all named
                            // columns exist in the table associated with this instance of
                            // ConstraintCollection.
                            _postAddRange (this.table);                                                                                    
                    }
                }
				else if (constraints [i] is ForeignKeyConstraint){
                        if (( constraints [i] as ForeignKeyConstraint).DataColsNotValidated == true){
                            (constraints [i] as ForeignKeyConstraint).postAddRange (this.table);
                        }
					}
                }
                        
                foreach (Constraint constraint in constraints)
                        Add (constraint);

		}

		// Helper AddRange() - Call this function when EndInit is called
        internal void PostEndInit()
        {
			Constraint[] constraints = _mostRecentConstraints;
			_mostRecentConstraints = null;
			AddRange (constraints);
        }


		public bool CanRemove(Constraint constraint) 
		{
			return constraint.CanRemoveFromCollection(this, false);
		}

		public void Clear() 
		{	
			// Clear should also remove PrimaryKey
			Table.PrimaryKey = null;
			
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
			//CanRemove will throws Exception incase of the above
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
				if (String.Compare (constraintName, con.ConstraintName, !Table.CaseSensitive, Table.Locale) == 0)
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

			if (!constraint.CanRemoveFromCollection(this, true))
				return;
				
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
			Remove(this[index]);
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
	}
}
