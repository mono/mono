//
// System.Data.DataRelationCollection.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman (tim@timcoleman.com)
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
	[DefaultEvent ("CollectionChanged")]
	[Serializable]
	public abstract class DataRelationCollection : InternalDataCollectionBase
	{
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
		/// <summary>
		/// Adds a DataRelation to the DataRelationCollection.
		/// </summary>
		/// <param name="relation">The DataRelation to add to the collection.</param>
		[MonoTODO]
		public void Add(DataRelation relation)
		{
			if(List != null)
			{
				//CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Add, this);
				List.Add(relation);
				//OnCollectionChanged(e);
			}
			return;
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
		[MonoTODO]
		public virtual DataRelation Add(DataColumn parentColumn, DataColumn childColumn)
		{	
			
			if(parentColumn == null)
			{
				throw new ArgumentNullException("parentColumn");
			}
			else if( childColumn == null)
			{
				throw new ArgumentNullException("childColumn");
			}

			// FIXME: temporarily commented so we can compile
			/*
			if(parentColumn.Table.DataSet != childColumn.Table.DataSet)
			{
				throw new InvalidConstraintException("my ex");
			}
			*/
			
			DataRelation dataRelation = new DataRelation("Relation" + defaultNameIndex.ToString(), parentColumn, childColumn);
			//CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Add, this);
			List.Add(dataRelation);
			//OnCollectionChanged(e);
			defaultNameIndex++;
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
		[MonoTODO]
		public virtual DataRelation Add(DataColumn[] parentColumns, DataColumn[] childColumns)
		{
			DataRelation dataRelation = new DataRelation("Relation" + defaultNameIndex.ToString(), parentColumns, childColumns);
			List.Add(dataRelation);
			defaultNameIndex++;
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
		[MonoTODO]
		public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn)
		{
			//If no name was supplied, give it a default name.
			if ((name == null) || (name == ""))
			{
				name = "Relation" + defaultNameIndex.ToString();
				defaultNameIndex++;
			}

			DataRelation dataRelation = new DataRelation(name, parentColumn, childColumn);
			List.Add(dataRelation);
			return dataRelation;
		}

		/// <summary>
		/// Creates a DataRelation with the specified name, and arrays of parent and child columns, and adds it to the collection.
		/// </summary>
		/// <param name="name">The name of the DataRelation to create.</param>
		/// <param name="parentColumns">An array of parent DataColumn objects.</param>
		/// <param name="childColumns">An array of child DataColumn objects.</param>
		/// <returns>The created DataRelation.</returns>
		[MonoTODO]
		public virtual DataRelation Add(string name, DataColumn[] parentColumns, DataColumn[] childColumns)
		{
			//If no name was supplied, give it a default name.
			if ((name == null) || (name == ""))
			{
				name = "Relation" + defaultNameIndex.ToString();
				defaultNameIndex++;
			}

			DataRelation dataRelation = new DataRelation(name, parentColumns, childColumns);
			List.Add(dataRelation);
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
		[MonoTODO]
		public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn, bool createConstraints)
		{
			//If no name was supplied, give it a default name.
			if ((name == null) || (name == ""))
			{
				name = "Relation" + defaultNameIndex.ToString();
				defaultNameIndex++;
			}

			DataRelation dataRelation = new DataRelation(name, parentColumn, childColumn, createConstraints);
			List.Add(dataRelation);
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
		[MonoTODO]
		public virtual DataRelation Add(string name, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints)
		{
			//If no name was supplied, give it a default name.
			if ((name == null) || (name == ""))
			{
				name = "Relation" + defaultNameIndex.ToString();
				defaultNameIndex++;
			}

			DataRelation dataRelation = new DataRelation(name, parentColumns, childColumns, createConstraints);
			AddCore(dataRelation);
			List.Add(dataRelation);
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
		[MonoTODO]
		public virtual void AddRange(DataRelation[] relations)
		{
			//TODO: Implement

			DataSet dataSet = GetDataSet();

			throw new NotImplementedException ();

			/*
			foreach(DataRelation dataRelation in relations)
			{
				
			}
			*/

		}

		[MonoTODO]
		public virtual bool CanRemove(DataRelation relation)
		{
			//TODO: Implement.
			return false;
		}

		public virtual void Clear()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool Contains(string name)
		{
			return false;
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
			throw new NotImplementedException ();
		}

		[ResDescriptionAttribute ("Occurs whenever this collection's membership changes.")]
		public event CollectionChangeEventHandler CollectionChanged;
	}
}
