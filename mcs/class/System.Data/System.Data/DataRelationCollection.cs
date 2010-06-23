//
// System.Data.DataRelationCollection.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman (tim@timcoleman.com)
//   Alan Tam Siu Lung <Tam@SiuLung.com>
//
// (C) Chris Podurgiel
// (C) 2002 Daniel Morgan
// Copyright (C) Tim Coleman, 2002
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

using System.Runtime.Serialization;

namespace System.Data {
	/// <summary>
	/// Represents the collection of DataRelation objects for this DataSet.
	/// </summary>
	[Editor ("Microsoft.VSDesigner.Data.Design.DataRelationCollectionEditor, " + Consts.AssemblyMicrosoft_VSDesigner,
		 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	[DefaultEvent ("CollectionChanged")]
	[DefaultProperty ("Table")]
	public abstract partial class DataRelationCollection : InternalDataCollectionBase {
		/// <summary>
		/// Summary description for DataTableRelationCollection.
		/// </summary>
		internal partial class DataSetRelationCollection : DataRelationCollection {
			private DataSet dataSet;
			DataRelation [] mostRecentRelations;

			/// <summary>
			/// Initializes a new instance of the DataSetRelationCollection class.
			/// </summary>
			internal DataSetRelationCollection (DataSet dataSet)
			{
				this.dataSet = dataSet;
			}

			protected override DataSet GetDataSet ()
			{
				return dataSet;
			}

			/// <summary>
			/// Performs verification on the table.
			/// </summary>
			/// <param name="relation">The relation to check.</param>
			protected override void AddCore (DataRelation relation)
			{
				if (relation.ChildTable.DataSet != dataSet || relation.ParentTable.DataSet != dataSet)
				   throw new DataException ();

				base.AddCore (relation);
				relation.ParentTable.ChildRelations.Add (relation);
				relation.ChildTable.ParentRelations.Add (relation);
				relation.SetDataSet (dataSet);
				relation.UpdateConstraints ();
			}

			protected override void RemoveCore (DataRelation relation)
			{
				base.RemoveCore (relation);
				relation.SetDataSet (null);
				relation.ParentTable.ChildRelations.Remove (relation);
				relation.ChildTable.ParentRelations.Remove (relation);
				relation.SetParentKeyConstraint (null);
				relation.SetChildKeyConstraint (null);
			}

			public override void AddRange (DataRelation [] relations)
			{
				if (relations == null)
					return;

				if (dataSet != null && dataSet.InitInProgress){
					mostRecentRelations = relations;
					return;
				}

				foreach (DataRelation rel in relations){
					if (rel == null)
						continue;
					Add (rel);
				}
			}

			internal override void PostAddRange ()
			{
				if (mostRecentRelations == null)
					return;

				foreach (DataRelation rel in mostRecentRelations){
					if (rel == null)
						continue;
					if (rel.InitInProgress)
						rel.FinishInit (dataSet);
					Add (rel);
				}
				mostRecentRelations = null;
			}

			protected override ArrayList List {
				get { return base.List; }
			}

			public override DataRelation this [string name] {
				get {
					int index = IndexOf (name, true);
					return index < 0 ? null : (DataRelation) List [index];
				}
			}

			/// <summary>
			/// Gets the DataRelation object at the specified index.
			/// </summary>
			public override DataRelation this [int index] {
				get {
					if (index < 0 || index >= List.Count)
						throw new IndexOutOfRangeException (String.Format ("Cannot find relation {0}.", index));

					return (DataRelation) List [index];
				}
			}
		}

		/// <summary>
		/// Summary description for DataTableRelationCollection.
		/// </summary>
		internal class DataTableRelationCollection : DataRelationCollection {
			private DataTable dataTable;

			/// <summary>
			/// Initializes a new instance of the DataTableRelationCollection class.
			/// </summary>
			internal DataTableRelationCollection (DataTable dataTable)
			{
				this.dataTable = dataTable;
			}

			protected override DataSet GetDataSet ()
			{
				return dataTable.DataSet;
			}

			public override DataRelation this [string name] {
				get {
					int index = IndexOf (name, true);
					return index < 0 ? null : (DataRelation) List [index];
				}
			}

			/// <summary>
			/// Gets the DataRelation object at the specified index.
			/// </summary>
			public override DataRelation this [int index] {
				get {
					if (index < 0 || index >= List.Count)
						throw new IndexOutOfRangeException (String.Format ("Cannot find relation {0}.", index));

					return (DataRelation) List [index];
				}
			}

			/// <summary>
			/// Performs verification on the table.
			/// </summary>
			/// <param name="relation">The relation to check.</param>
			protected override void AddCore (DataRelation relation)
			{
				if (dataTable.ParentRelations == this && relation.ChildTable != dataTable)
					throw new ArgumentException ("Cannot add a relation to this table's " +
								     "ParentRelations where this table is not" +
								     " the Child table.");

				if (dataTable.ChildRelations == this && relation.ParentTable != dataTable)
					throw new ArgumentException("Cannot add a relation to this table's " +
								    "ChildRelations where this table is not" +
								    " the Parent table.");

				dataTable.DataSet.Relations.Add (relation);
				base.AddCore (relation);
			}

			protected override void RemoveCore (DataRelation relation)
			{
				relation.DataSet.Relations.Remove(relation);
				base.RemoveCore (relation);
			}

			protected override ArrayList List {
				get { return base.List; }
			}
		}

		private DataRelation inTransition;
		int index;


		/// <summary>
		/// Initializes a new instance of the DataRelationCollection class.
		/// </summary>
		protected DataRelationCollection ()
		{
			inTransition = null;
		}

		/// <summary>
		/// Gets the DataRelation object specified by name.
		/// </summary>
		public abstract DataRelation this [string name] {
			get;
		}

		/// <summary>
		/// Gets the DataRelation object at the specified index.
		/// </summary>
		public abstract DataRelation this [int index] {
			get;
		}


		#region Add Methods
		private string GetNextDefaultRelationName ()
		{
			int index = 1;
			string defRelationName = "Relation" + index;
			for (; Contains (defRelationName); ++index)
				defRelationName = "Relation" + index;
			return defRelationName;
		}

		/// <summary>
		/// Adds a DataRelation to the DataRelationCollection.
		/// </summary>
		/// <param name="relation">The DataRelation to add to the collection.</param>
		public void Add (DataRelation relation)
		{
			// To prevent endless recursion
			if (inTransition == relation)
				return;

			inTransition = relation;

			try {
				CollectionChangeEventArgs e = new CollectionChangeEventArgs (CollectionChangeAction.Add, relation);
				OnCollectionChanging (e);

				this.AddCore (relation);
				if (relation.RelationName == string.Empty)
					relation.RelationName = GenerateRelationName ();

				relation.ParentTable.ResetPropertyDescriptorsCache ();
				relation.ChildTable.ResetPropertyDescriptorsCache ();

				e = new CollectionChangeEventArgs (CollectionChangeAction.Add, relation);
				OnCollectionChanged (e);
			} finally {
				inTransition = null;
			}
		}

		private string GenerateRelationName ()
		{
			index++;
			return "Relation" + index;
		}

		/// <summary>
		/// Creates a relation given the parameters and adds it to the collection. The name is defaulted.
		/// An ArgumentException is generated if this relation already belongs to this collection or belongs to another collection.
		/// An InvalidConstraintException is generated if the relation can't be created based on the parameters.
		/// The CollectionChanged event is fired if it succeeds.
		/// </summary>
		/// <param name="parentColumn">parent column of relation.</param>
		/// <param name="childColumn">child column of relation.</param>
		/// <returns>The created DataRelation.</returns>
		public virtual DataRelation Add (DataColumn parentColumn, DataColumn childColumn)
		{
			DataRelation dataRelation = new DataRelation (GetNextDefaultRelationName (), parentColumn, childColumn);
			Add (dataRelation);
			return dataRelation;
		}

		/// <summary>
		/// Creates a relation given the parameters and adds it to the collection. The name is defaulted.
		/// An ArgumentException is generated if this relation already belongs to this collection or belongs to another collection.
		/// An InvalidConstraintException is generated if the relation can't be created based on the parameters.
		/// The CollectionChanged event is raised if it succeeds.
		/// </summary>
		/// <param name="parentColumns">An array of parent DataColumn objects.</param>
		/// <param name="childColumns">An array of child DataColumn objects.</param>
		/// <returns>The created DataRelation.</returns>
		public virtual DataRelation Add (DataColumn [] parentColumns, DataColumn [] childColumns)
		{
			DataRelation dataRelation = new DataRelation (GetNextDefaultRelationName (), parentColumns, childColumns);
			Add (dataRelation);
			return dataRelation;
		}

		/// <summary>
		/// Creates a relation given the parameters and adds it to the collection.
		/// An ArgumentException is generated if this relation already belongs to this collection or belongs to another collection.
		/// A DuplicateNameException is generated if this collection already has a relation with the same name (case insensitive).
		/// An InvalidConstraintException is generated if the relation can't be created based on the parameters.
		/// The CollectionChanged event is raised if it succeeds.
		/// </summary>
		/// <param name="name">The name of the relation.</param>
		/// <param name="parentColumn">parent column of relation.</param>
		/// <returns>The created DataRelation.</returns>
		/// <returns></returns>
		public virtual DataRelation Add (string name, DataColumn parentColumn, DataColumn childColumn)
		{
			//If no name was supplied, give it a default name.
			if (name == null || name == "")
				name = GetNextDefaultRelationName ();

			DataRelation dataRelation = new DataRelation (name, parentColumn, childColumn);
			Add (dataRelation);
			return dataRelation;
		}

		/// <summary>
		/// Creates a DataRelation with the specified name, and arrays of parent and child columns, and adds it to the collection.
		/// </summary>
		/// <param name="name">The name of the DataRelation to create.</param>
		/// <param name="parentColumns">An array of parent DataColumn objects.</param>
		/// <param name="childColumns">An array of child DataColumn objects.</param>
		/// <returns>The created DataRelation.</returns>
		public virtual DataRelation Add (string name, DataColumn [] parentColumns, DataColumn [] childColumns)
		{
			//If no name was supplied, give it a default name.
			if (name == null || name == "")
				name = GetNextDefaultRelationName ();

			DataRelation dataRelation = new DataRelation (name, parentColumns, childColumns);
			Add (dataRelation);
			return dataRelation;
		}

		/// <summary>
		/// Creates a relation given the parameters and adds it to the collection.
		/// An ArgumentException is generated if this relation already belongs to this collection or belongs to another collection.
		/// A DuplicateNameException is generated if this collection already has a relation with the same name (case insensitive).
		/// An InvalidConstraintException is generated if the relation can't be created based on the parameters.
		/// The CollectionChanged event is raised if it succeeds.
		/// </summary>
		/// <param name="name">The name of the relation.</param>
		/// <param name="parentColumn">parent column of relation.</param>
		/// <param name="childColumn">child column of relation.</param>
		/// <param name="createConstraints">true to create constraints; otherwise false. (default is true)</param>
		/// <returns>The created DataRelation.</returns>
		public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn, bool createConstraints)
		{
			//If no name was supplied, give it a default name.
			if (name == null || name == "")
				name = GetNextDefaultRelationName ();

			DataRelation dataRelation = new DataRelation (name, parentColumn, childColumn, createConstraints);
			Add (dataRelation);
			return dataRelation;
		}

		/// <summary>
		/// Creates a DataRelation with the specified name, arrays of parent and child columns,
		/// and value specifying whether to create a constraint, and adds it to the collection.
		/// </summary>
		/// <param name="name">The name of the DataRelation to create.</param>
		/// <param name="parentColumns">An array of parent DataColumn objects.</param>
		/// <param name="childColumns">An array of child DataColumn objects.</param>
		/// <param name="createConstraints">true to create a constraint; otherwise false.</param>
		/// <returns>The created DataRelation.</returns>
		public virtual DataRelation Add (string name, DataColumn [] parentColumns, DataColumn [] childColumns, bool createConstraints)
		{
			//If no name was supplied, give it a default name.
			if (name == null || name == "")
				name = GetNextDefaultRelationName ();

			DataRelation dataRelation = new DataRelation (name, parentColumns, childColumns, createConstraints);
			Add (dataRelation);
			return dataRelation;
		}
		#endregion

		/// <summary>
		/// Adds to the list
		/// </summary>
		/// <param name="relation">The relation to check.</param>
		protected virtual void AddCore (DataRelation relation)
		{
			if (relation == null)
				//TODO: Issue a good exception message.
				throw new ArgumentNullException();

			if(List.IndexOf (relation) != -1)
				//TODO: Issue a good exception message.
				throw new ArgumentException();

			// check if the collection has a relation with the same name.
			int tmp = IndexOf (relation.RelationName);
			// if we found a relation with same name we have to check
			// that it is the same case.
			// indexof can return a table with different case letters.
			if (tmp != -1 && relation.RelationName == this [tmp].RelationName)
				throw new DuplicateNameException("A DataRelation named '" + relation.RelationName + "' already belongs to this DataSet.");

			// check whether the relation exists between the columns already
			foreach (DataRelation rel in this) {
				// compare child columns
				bool differs = false;
				foreach (DataColumn current in relation.ChildColumns) {
					bool exists = false;
					foreach (DataColumn col in rel.ChildColumns) {
						if (col == current) {
							exists = true;
							break;
						}
					}
					if (!exists) {
						differs = true;
						break;
					}
				}

				if (! differs) {
					// compare parent columns
					differs = false;
					foreach (DataColumn current in relation.ParentColumns) {
						bool exists = false;
						foreach (DataColumn col in rel.ParentColumns) {
							if (col == current) {
								exists = true;
								break;
							}
						}
						if (!exists) {
							differs = true;
							break;
						}
					}

					if (! differs)
						throw new ArgumentException ("A relation already exists for these child columns");
				}
			}

			// Add to collection
			List.Add (relation);
		}

		/// <summary>
		/// Copies the elements of the specified DataRelation array to the end of the collection.
		/// </summary>
		/// <param name="relations">The array of DataRelation objects to add to the collection.</param>
		public virtual void AddRange (DataRelation[] relations)
		{
			if (relations == null)
				return;

			foreach (DataRelation relation in relations)
				Add (relation);
		}

		internal virtual void PostAddRange ()
		{
		}

		public virtual bool CanRemove (DataRelation relation)
		{
			if (relation == null || !GetDataSet ().Equals (relation.DataSet))
				return false;

			// check if the relation doesnot belong to this collection
			int tmp = IndexOf (relation.RelationName);
			return tmp != -1 && relation.RelationName == this [tmp].RelationName;
		}

		public virtual void Clear ()
		{
			for (int i = 0; i < Count; i++)
				Remove (this [i]);

			List.Clear ();
		}

		public virtual bool Contains (string name)
		{
			DataSet tmpDataSet = GetDataSet ();
			if (tmpDataSet != null) {
				DataRelation tmpRelation = tmpDataSet.Relations [name];
				if (tmpRelation != null)
					return true;
			}
			return (-1 != IndexOf (name, false));
		}

		private CollectionChangeEventArgs CreateCollectionChangeEvent (CollectionChangeAction action)
		{
			return new CollectionChangeEventArgs (action, this);
		}

		protected abstract DataSet GetDataSet ();

		public virtual int IndexOf (DataRelation relation)
		{
			return List.IndexOf (relation);
		}

		public virtual int IndexOf (string relationName)
		{
			return IndexOf (relationName, false);
		}

		private int IndexOf (string name, bool error)
		{
			int count = 0, match = -1;
			for (int i = 0; i < List.Count; i++) {
				String name2 = ((DataRelation) List[i]).RelationName;
				if (String.Compare (name, name2, true) == 0) {
					if (String.Compare (name, name2, false) == 0)
						return i;
					match = i;
					count++;
				}
			}
			if (count == 1)
				return match;
			if (count > 1 && error)
				throw new ArgumentException ("There is no match for the name in the same case and there are multiple matches in different case.");
			return -1;
		}

		protected virtual void OnCollectionChanged (CollectionChangeEventArgs ccevent)
		{
			if (CollectionChanged != null)
				CollectionChanged (this, ccevent);
		}

		protected virtual void OnCollectionChanging (CollectionChangeEventArgs ccevent)
		{
			// LAME Spec: No associated events and it doesn't update CollectionChanged
			// event too as specified in MSDN
			// throw new NotImplementedException ();
		}

		public void Remove (DataRelation relation)
		{
			// To prevent endless recursion
			if (inTransition == relation)
				return;

			inTransition = relation;

			if (relation == null)
				return;

			try {
				// check if the list doesnot contains this relation.
				if (!(List.Contains (relation)))
					throw new ArgumentException ("Relation doesnot belong to this Collection.");

				CollectionChangeEventArgs e = new CollectionChangeEventArgs (CollectionChangeAction.Remove, relation);
				OnCollectionChanging (e);

				RemoveCore (relation);
				string name = "Relation" + index;
				if (relation.RelationName == name)
					index--;

				e = new CollectionChangeEventArgs (CollectionChangeAction.Remove, relation);
				OnCollectionChanged (e);
			} finally {
				inTransition = null;
			}
		}

		public void Remove (string name)
		{
			DataRelation relation = this [name];
			if (relation == null)
				throw new ArgumentException ("Relation doesnot belong to this Collection.");
			Remove (relation);
		}

		public void RemoveAt (int index)
		{
			DataRelation relation = this [index];
			if (relation == null)
				throw new IndexOutOfRangeException (String.Format ("Cannot find relation {0}", index));
			Remove (relation);
		}

		protected virtual void RemoveCore (DataRelation relation)
		{
			// Remove from collection
			List.Remove (relation);
		}

		#region Events

		[ResDescriptionAttribute ("Occurs whenever this collection's membership changes.")]
		public event CollectionChangeEventHandler CollectionChanged;

		#endregion
	}

#if !NET_2_0
	[Serializable]
	partial class DataRelationCollection {
	}
#else
	partial class DataRelationCollection {
		public void CopyTo (DataRelation [] array, int index)
		{
			CopyTo ((Array) array, index);
		}

		internal void BinarySerialize (SerializationInfo si)
		{
			ArrayList l = new ArrayList ();
			for (int j = 0; j < Count; j++) {
				DataRelation dr = (DataRelation) List [j];
				ArrayList tmp = new ArrayList ();
				tmp.Add (dr.RelationName);

				// FIXME: Handle multi-column relations
				int [] rep = new int [2];
				DataTable dt = dr.ParentTable;
				rep [0] = dt.DataSet.Tables.IndexOf (dt);
				rep [1] = dt.Columns.IndexOf (dr.ParentColumns [0]);
				tmp.Add (rep);
				rep = new int [2];
				dt = dr.ChildTable;
				rep [0] = dt.DataSet.Tables.IndexOf (dt);
				rep [1] = dt.Columns.IndexOf (dr.ChildColumns [0]);
				tmp.Add (rep);
				tmp.Add (false); // FIXME
				tmp.Add (null); // FIXME
				l.Add (tmp);
			}
			si.AddValue ("DataSet.Relations", l, typeof (ArrayList));
		}
	}
#endif
}
