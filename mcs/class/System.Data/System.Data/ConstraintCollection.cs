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
	internal delegate void DelegateValidateRemoveConstraint (ConstraintCollection sender, Constraint constraintToRemove, ref bool fail,ref string failReason);

	/// <summary>
	/// hold collection of constraints for data table
	/// </summary>
	[DefaultEvent ("CollectionChanged")]
	[Editor ("Microsoft.VSDesigner.Data.Design.ConstraintsCollectionEditor, " + Consts.AssemblyMicrosoft_VSDesigner,
		 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	public partial class ConstraintCollection : InternalDataCollectionBase {
		public event CollectionChangeEventHandler CollectionChanged;
		private DataTable table;

		// Keep reference to most recent constraints passed to AddRange()
		// so that they can be added when EndInit() is called.
		private Constraint [] _mostRecentConstraints;

		//Don't allow public instantiation
		//Will be instantianted from DataTable
		internal ConstraintCollection(DataTable table)
		{
			this.table = table;
		}

		internal DataTable Table {
			get { return this.table; }
		}

		public
#if !NET_2_0
		virtual
#endif
		Constraint this [string name] {
			get {
				int index = IndexOf (name);
				return -1 == index ? null : (Constraint) List [index];
			}
		}

		public
#if !NET_2_0
		virtual
#endif
		Constraint this [int index] {
			get {
				if (index < 0 || index >= List.Count)
					throw new IndexOutOfRangeException ();
				return (Constraint) List [index];
			}
		}

		private void _handleBeforeConstraintNameChange (object sender, string newName)
		{
			if (newName == null || newName == "")
				throw new ArgumentException (
					"ConstraintName cannot be set to null or empty after adding it to a ConstraintCollection.");

			if (_isDuplicateConstraintName (newName, (Constraint) sender))
				throw new DuplicateNameException ("Constraint name already exists.");
		}

		private bool _isDuplicateConstraintName (string constraintName, Constraint excludeFromComparison)
		{
			foreach (Constraint cst in List) {
				if (cst == excludeFromComparison)
					continue;
				if (String.Compare (constraintName, cst.ConstraintName, false, Table.Locale) == 0)
					return true;
			}

			return false;
		}

		//finds an open name slot of ConstraintXX
		//where XX is a number
		private string _createNewConstraintName ()
		{
			// FIXME: Do constraint id's need to be reused?  This loop is horrendously slow.
			for (int i = 1; ; ++i) {
				string new_name = "Constraint" + i;
				if (IndexOf (new_name) == -1)
					return new_name;
			}
		}


		// Overloaded Add method (5 of them)
		// to add Constraint object to the collection

		public void Add (Constraint constraint)
		{
			//not null
			if (null == constraint)
				throw new ArgumentNullException ("Can not add null.");

			if (constraint.InitInProgress)
				throw new ArgumentException ("Hmm .. Failed to Add to collection");

			//check constraint membership
			//can't already exist in this collection or any other
			if (this == constraint.ConstraintCollection)
				throw new ArgumentException ("Constraint already belongs to this collection.");
			if (null != constraint.ConstraintCollection)
				throw new ArgumentException ("Constraint already belongs to another collection.");

			//check if a constraint already exists for the datacolums
			foreach (Constraint c in this) {
				if (c.Equals (constraint))
					throw new DataException (
						"Constraint matches contraint named '" + c.ConstraintName + "' already in collection");
			}

			//check for duplicate name
			if (_isDuplicateConstraintName (constraint.ConstraintName, null))
				throw new DuplicateNameException ("Constraint name already exists.");

			//Allow constraint to run validation rules and setup
			constraint.AddToConstraintCollectionSetup (this); //may throw if it can't setup

			//if name is null or empty give it a name
			if (constraint.ConstraintName == null || constraint.ConstraintName == "")
				constraint.ConstraintName = _createNewConstraintName ();

			//Add event handler for ConstraintName change
			constraint.BeforeConstraintNameChange += new DelegateConstraintNameChange (_handleBeforeConstraintNameChange);

			constraint.ConstraintCollection = this;
			List.Add (constraint);

			if (constraint is UniqueConstraint && ((UniqueConstraint) constraint).IsPrimaryKey)
				table.PrimaryKey = ((UniqueConstraint) constraint).Columns;

			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, this));
		}

		public
#if !NET_2_0
		virtual
#endif
		Constraint Add (string name, DataColumn column, bool primaryKey)
		{
			UniqueConstraint uc = new UniqueConstraint (name, column, primaryKey);
			Add (uc);
			return uc;
		}

		public
#if !NET_2_0
		virtual
#endif
		Constraint Add (string name, DataColumn primaryKeyColumn, DataColumn foreignKeyColumn)
		{
			ForeignKeyConstraint fc = new ForeignKeyConstraint (name, primaryKeyColumn, foreignKeyColumn);
			Add (fc);
			return fc;
		}

		public
#if !NET_2_0
		virtual
#endif
		Constraint Add (string name, DataColumn[] columns, bool primaryKey)
		{
			UniqueConstraint uc = new UniqueConstraint (name, columns, primaryKey);
			Add (uc);
			return uc;
		}

		public
#if !NET_2_0
		virtual
#endif
		Constraint Add (string name, DataColumn[] primaryKeyColumns, DataColumn[] foreignKeyColumns)
		{
			ForeignKeyConstraint fc = new ForeignKeyConstraint (name, primaryKeyColumns, foreignKeyColumns);
			Add (fc);
			return fc;
		}

		public void AddRange (Constraint[] constraints)
		{
			//When AddRange() occurs after BeginInit,
			//it does not add any elements to the collection until EndInit is called.
			if (Table.InitInProgress) {
				// Keep reference so that they can be added when EndInit() is called.
				_mostRecentConstraints = constraints;
				return;
			}

			if (constraints == null)
				return;

			for (int i = 0; i < constraints.Length; ++i) {
				if (constraints [i] != null)
					Add (constraints [i]);
			}
		}

		// Helper AddRange() - Call this function when EndInit is called
		// keeps track of the Constraints most recently added and adds them
		// to the collection
		internal void PostAddRange ()
		{
			if (_mostRecentConstraints == null)
				return;

			// Check whether the constraint is Initialized
			// If not, initialize before adding to collection
			for (int i = 0; i < _mostRecentConstraints.Length; i++) {
				Constraint c = _mostRecentConstraints [i];
				if (c == null)
					continue;
				if (c.InitInProgress)
					c.FinishInit (Table);
				Add (c);
			}
			_mostRecentConstraints = null;
		}

		public bool CanRemove (Constraint constraint)
		{
			return constraint.CanRemoveFromCollection (this, false);
		}

		public void Clear ()
		{
			// Clear should also remove PrimaryKey
			Table.PrimaryKey = null;

			//CanRemove? See Lamespec below.
			//the Constraints have a reference to us
			//and we listen to name change events
			//we should remove these before clearing
			foreach (Constraint con in List) {
				con.ConstraintCollection = null;
				con.BeforeConstraintNameChange -= new DelegateConstraintNameChange (_handleBeforeConstraintNameChange);
			}

			//LAMESPEC: MSFT implementation allows this
			//even when a ForeignKeyConstraint exist for a UniqueConstraint
			//thus violating the CanRemove logic
			//CanRemove will throws Exception incase of the above
			List.Clear (); //Will violate CanRemove rule
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, this));
		}

		public bool Contains (string name)
		{
			return -1 != IndexOf (name);
		}

		public int IndexOf (Constraint constraint)
		{
			int index = 0;
			foreach (Constraint c in this) {
				if (c == constraint)
					return index;
				index++;
			}
			return -1;
		}

		public
#if !NET_2_0
		virtual
#endif
		int IndexOf (string constraintName)
		{
			//LAMESPEC: Spec doesn't say case insensitive
			//it should to be consistant with the other
			//case insensitive comparisons in this class

			int index = 0;
			foreach (Constraint con in List) {
				if (String.Compare (constraintName, con.ConstraintName, !Table.CaseSensitive, Table.Locale) == 0)
					return index;
				index++;
			}
			return -1; //not found
		}

		public void Remove (Constraint constraint)
		{
			//LAMESPEC: spec doesn't document the ArgumentException the
			//will be thrown if the CanRemove rule is violated

			//LAMESPEC: spec says an exception will be thrown
			//if the element is not in the collection. The implementation
			//doesn't throw an exception. ArrayList.Remove doesn't throw if the
			//element doesn't exist
			//ALSO the overloaded remove in the spec doesn't say it throws any exceptions

			//not null
			if (null == constraint)
				throw new ArgumentNullException();

			if (!constraint.CanRemoveFromCollection (this, true))
				return;

			constraint.RemoveFromConstraintCollectionCleanup (this);
			constraint.ConstraintCollection = null;
			List.Remove (constraint);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, this));
		}

		public void Remove (string name)
		{
			int index = IndexOf (name);
			if (-1 == index)
				throw new ArgumentException ("Constraint '" + name + "' does not belong to this DataTable.");

			Remove (this [index]);
		}

		public void RemoveAt(int index)
		{
			Remove (this [index]);
		}

		protected override ArrayList List {
			get { return base.List; }
		}


#if !NET_2_0
		protected virtual
#else
		internal
#endif
		void OnCollectionChanged (CollectionChangeEventArgs ccevent)
		{
			if (null != CollectionChanged)
				CollectionChanged(this, ccevent);
		}
	}

#if NET_2_0
	sealed partial class ConstraintCollection {
		public void CopyTo (Constraint [] array, int index)
		{
			base.CopyTo (array, index);
		}
	}
#else
	[Serializable]
	partial class ConstraintCollection {
	}
#endif
}
