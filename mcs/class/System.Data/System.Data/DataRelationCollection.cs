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

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data {
	/// <summary>
	/// Represents the collection of DataRelation objects for this DataSet.
	/// </summary>
	[Editor]
	[DefaultEvent ("CollectionChanged")]
	[Serializable]
	public abstract class DataRelationCollection : InternalDataCollectionBase
	{
		/// <summary>
		/// Summary description for DataTableRelationCollection.
		/// </summary>
		internal class DataSetRelationCollection : DataRelationCollection
		{
			private DataSet dataSet;
			
			/// <summary>
			/// Initializes a new instance of the DataSetRelationCollection class.
			/// </summary>
			internal DataSetRelationCollection (DataSet dataSet)
			{
				this.dataSet = dataSet;
			}

			/// <summary>
			/// Gets the DataRelation object specified by name.
			/// </summary>
			public override DataRelation this [string name]
			{
				get {
					foreach (DataRelation dataRelation in List)
						if (dataRelation.RelationName == name) return dataRelation;
					return null;
				}
			}

			/// <summary>
			/// Gets the DataRelation object at the specified index.
			/// </summary>
			public override DataRelation this [int index]
			{
				get {
					return List [index] as DataRelation;
				}
			}

			protected override DataSet GetDataSet()
			{
				return dataSet;
			}

			/// <summary>
			/// Performs verification on the table.
			/// </summary>
			/// <param name="relation">The relation to check.</param>
			protected override void AddCore (DataRelation relation)
			{
				base.AddCore (relation);
				if (relation.ChildTable.DataSet != this.dataSet || relation.ParentTable.DataSet != this.dataSet)
					throw new DataException ();
				List.Add (relation);
				relation.SetDataSet (dataSet);
				relation.ParentTable.ChildRelations.Add (relation);
				relation.ChildTable.ParentRelations.Add (relation);
				ForeignKeyConstraint foreignKeyConstraint = null;
				if (relation.createConstraints) {
					foreignKeyConstraint = new ForeignKeyConstraint (relation.ParentColumns, relation.ChildColumns);
					relation.ChildTable.Constraints.Add (foreignKeyConstraint);
				}
				UniqueConstraint uniqueConstraint = null;
				foreach (object o in List) {
					if (o is UniqueConstraint) {
						UniqueConstraint uc = (UniqueConstraint) o;
						if (uc.Columns.Length == relation.ParentColumns.Length) {
							bool allColumnsEqual = true;
							for (int columnCnt = 0; columnCnt < uc.Columns.Length; ++columnCnt) {
								if (uc.Columns[columnCnt] != relation.ParentColumns[columnCnt]) {
									allColumnsEqual = false;
									break;
								}
							}
							if (allColumnsEqual) {
								uniqueConstraint = uc;
								break;
							}
						}
					}
				}
				relation.SetParentKeyConstraint (uniqueConstraint);
				relation.SetChildKeyConstraint (foreignKeyConstraint);
			}

			public override void AddRange (DataRelation[] relations)
			{
				base.AddRange (relations);
			}

			public override void Clear ()
			{
				base.Clear ();
			}

			protected override void RemoveCore (DataRelation relation)
			{
				base.RemoveCore (relation);
				relation.SetDataSet (null);
				relation.ParentTable.ChildRelations.Remove (relation);
				relation.ChildTable.ParentRelations.Remove (relation);
				ForeignKeyConstraint foreignKeyConstraint = null;
				relation.SetParentKeyConstraint (null);
				relation.SetChildKeyConstraint (null);
			}

			protected override ArrayList List {
				get {
					return base.List;
				}
			}
		}

		/// <summary>
		/// Summary description for DataTableRelationCollection.
		/// </summary>
		internal class DataTableRelationCollection : DataRelationCollection
		{
			private DataTable dataTable;
			
			/// <summary>
			/// Initializes a new instance of the DataTableRelationCollection class.
			/// </summary>
			internal DataTableRelationCollection (DataTable dataTable)
			{
				this.dataTable = dataTable;
			}

			/// <summary>
			/// Gets the DataRelation object specified by name.
			/// </summary>
			public override DataRelation this [string name]
			{
				get {
					foreach (DataRelation dataRelation in List)
						if (dataRelation.RelationName == name) return dataRelation;
					return null;
				}
			}

			/// <summary>
			/// Gets the DataRelation object at the specified index.
			/// </summary>
			public override DataRelation this [int index]
			{
				get {
					return List [index] as DataRelation;
				}
			}

			protected override DataSet GetDataSet()
			{
				return dataTable.DataSet;
			}

			protected override void AddCore (DataRelation relation)
			{
				base.AddCore (relation);
			}

			protected override void RemoveCore (DataRelation relation)
			{
				base.RemoveCore (relation);
			}

			protected override ArrayList List {
				get {
					return base.List;
				}
			}
		}

		private int defaultNameIndex;
		private bool inTransition;
		
		/// <summary>
		/// Initializes a new instance of the DataRelationCollection class.
		/// </summary>
		protected DataRelationCollection () 
			: base ()
		{
			defaultNameIndex = 1;
			inTransition = false;
		}

		/// <summary>
		/// Gets the DataRelation object specified by name.
		/// </summary>
		public abstract DataRelation this[string name]{get;}

		/// <summary>
		/// Gets the DataRelation object at the specified index.
		/// </summary>
		public abstract DataRelation this[int index]{get;}

		
		#region Add Methods
		private string GetNextDefaultRelationName ()
		{
			string defRelationName = "Relation";
			for (int index = 1; Contains (defRelationName); ++index) {
				defRelationName = "Relation" + index;
			}
			return defRelationName;
		}

		/// <summary>
		/// Adds a DataRelation to the DataRelationCollection.
		/// </summary>
		/// <param name="relation">The DataRelation to add to the collection.</param>
		[MonoTODO]
		public void Add(DataRelation relation)
		{
			this.AddCore (relation);
			CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Add, this);
			List.Add(relation);
			OnCollectionChanged(e);
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
		public virtual DataRelation Add(DataColumn parentColumn, DataColumn childColumn)
		{	
			DataRelation dataRelation = new DataRelation(GetNextDefaultRelationName (), parentColumn, childColumn);
			Add(dataRelation);
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
		public virtual DataRelation Add(DataColumn[] parentColumns, DataColumn[] childColumns)
		{
			DataRelation dataRelation = new DataRelation(GetNextDefaultRelationName (), parentColumns, childColumns);
			Add(dataRelation);
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
		public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn)
		{
			//If no name was supplied, give it a default name.
			if (name == null || name == "") name = GetNextDefaultRelationName ();

			DataRelation dataRelation = new DataRelation(name, parentColumn, childColumn);
			Add(dataRelation);
			return dataRelation;
		}

		/// <summary>
		/// Creates a DataRelation with the specified name, and arrays of parent and child columns, and adds it to the collection.
		/// </summary>
		/// <param name="name">The name of the DataRelation to create.</param>
		/// <param name="parentColumns">An array of parent DataColumn objects.</param>
		/// <param name="childColumns">An array of child DataColumn objects.</param>
		/// <returns>The created DataRelation.</returns>
		public virtual DataRelation Add(string name, DataColumn[] parentColumns, DataColumn[] childColumns)
		{
			//If no name was supplied, give it a default name.
			if (name == null || name == "") name = GetNextDefaultRelationName ();

			DataRelation dataRelation = new DataRelation(name, parentColumns, childColumns);
			Add(dataRelation);
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
			if (name == null || name == "") name = GetNextDefaultRelationName ();

			DataRelation dataRelation = new DataRelation(name, parentColumn, childColumn, createConstraints);
			Add(dataRelation);
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
		public virtual DataRelation Add(string name, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints)
		{
			//If no name was supplied, give it a default name.
			if (name == null || name == "") name = GetNextDefaultRelationName ();

			DataRelation dataRelation = new DataRelation(name, parentColumns, childColumns, createConstraints);
			Add(dataRelation);
			return dataRelation;
		}
		#endregion
	
		/// <summary>
		/// Performs verification on the table.
		/// </summary>
		/// <param name="relation">The relation to check.</param>
		[MonoTODO]
		protected virtual void AddCore(DataRelation relation)
		{
			if (relation == null)
			{
				//TODO: Issue a good exception message.
				throw new ArgumentNullException();
			}
			else if(List.IndexOf(relation) != -1)
			{
				//TODO: Issue a good exception message.
				throw new ArgumentException();
			}
			else if(List.Contains(relation.RelationName))
			{
				//TODO: Issue a good exception message.
				throw new DuplicateNameException("A Relation named " + relation.RelationName + " already belongs to this DataSet.");
			}
		}

		/// <summary>
		/// Copies the elements of the specified DataRelation array to the end of the collection.
		/// </summary>
		/// <param name="relations">The array of DataRelation objects to add to the collection.</param>
		public virtual void AddRange(DataRelation[] relations)
		{
			foreach (DataRelation relation in relations) Add(relation);
		}

		[MonoTODO]
		public virtual bool CanRemove(DataRelation relation)
		{
			//TODO: Implement.
			return false;
		}

		public virtual void Clear()
		{
			List.Clear();
		}

		public virtual bool Contains(string name)
		{
			return IndexOf(name) != -1;
		}

		private CollectionChangeEventArgs CreateCollectionChangeEvent (CollectionChangeAction action)
		{
			return new CollectionChangeEventArgs (action, this);
		}

		protected abstract DataSet GetDataSet();

		public virtual int IndexOf(DataRelation relation)
		{
			return List.IndexOf(relation);
		}

		public virtual int IndexOf(string relationName)
		{
			return List.IndexOf(this[relationName]);
		}

		protected virtual void OnCollectionChanged (CollectionChangeEventArgs ccevent)
		{
			if (CollectionChanged != null)
				CollectionChanged (this, ccevent);
		}

		[MonoTODO]
		protected internal virtual void OnCollectionChanging (CollectionChangeEventArgs ccevent)
		{
			throw new NotImplementedException ();
		}

		public void Remove (DataRelation relation)
		{
			RemoveCore (relation);
			List.Remove (relation);
			OnCollectionChanged (CreateCollectionChangeEvent (CollectionChangeAction.Remove));
		}

		public void Remove (string name)
		{
			Remove ((DataRelation) List[IndexOf (name)]);
		}

		public void RemoveAt (int index)
		{
			List.RemoveAt (index);
		}

		[MonoTODO]
		protected virtual void RemoveCore(DataRelation relation)
		{
			// TODO: What have to be done?
		}

		[ResDescriptionAttribute ("Occurs whenever this collection's membership changes.")]
		public event CollectionChangeEventHandler CollectionChanged;
	}
}
