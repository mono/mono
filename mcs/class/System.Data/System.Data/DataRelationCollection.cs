//
// System.Data.DataRelationCollection.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Represents the collection of DataRelation objects for this DataSet.
	/// </summary>
	[Serializable]
	public abstract class DataRelationCollection : InternalDataCollectionBase
	{
		private bool inTransition;
		private int defaultNameIndex;		
		
		/// <summary>
		/// Initializes a new instance of the DataRelationCollection class.
		/// </summary>
		internal DataRelationCollection()
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
		public void Add(DataRelation relation)
		{
			if(relation != null)
			{
				//CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Add, this);
				relations.Add(relation);
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

			if(parentColumn.Table.DataSet != childColumn.Table.DataSet)
			{
				throw new InvalidConstraintException("my ex");
			}
			
			DataRelation dataRelation = new DataRelation("Relation" + defaultNameIndex.ToString(), parentColumn, childColumn);
			//CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Add, this);
			relations.Add(dataRelation);
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
		public virtual DataRelation Add(DataColumn[] parentColumns, DataColumn[] childColumns)
		{
			DataRelation dataRelation = new DataRelation("Relation" + defaultNameIndex.ToString(), parentColumns, childColumns);
			relations.Add(dataRelation);
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
		public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn)
		{
			//If no name was supplied, give it a default name.
			if ((name == null) || (name == ""))
			{
				name = "Relation" + defaultNameIndex.ToString();
				defaultNameIndex++;
			}

			DataRelation dataRelation = new DataRelation(name, parentColumn, childColumn);
			relations.Add(dataRelation);
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
			if ((name == null) || (name == ""))
			{
				name = "Relation" + defaultNameIndex.ToString();
				defaultNameIndex++;
			}

			DataRelation dataRelation = new DataRelation(name, parentColumns, childColumns);
			relations.Add(dataRelation);
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
			if ((name == null) || (name == ""))
			{
				name = "Relation" + defaultNameIndex.ToString();
				defaultNameIndex++;
			}

			DataRelation dataRelation = new DataRelation(name, parentColumn, childColumn, createConstraints);
			relations.Add(dataRelation);
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
			if ((name == null) || (name == ""))
			{
				name = "Relation" + defaultNameIndex.ToString();
				defaultNameIndex++;
			}

			DataRelation dataRelation = new DataRelation(name, parentColumns, childColumns, createConstraints);
			AddCore(dataRelation);
			relations.Add(dataRelation);
			return dataRelation;
		}
		#endregion
	
		/// <summary>
		/// Performs verification on the table.
		/// </summary>
		/// <param name="relation">The relation to check.</param>
		protected virtual void AddCore(DataRelation relation)
		{
			if (relation == null)
			{
				//TODO: Issue a good exception message.
				throw new ArgumentNullException();
			}
			else if(relations.IndexOf(relation) != -1)
			{
				//TODO: Issue a good exception message.
				throw new ArgumentException();
			}
			else if(relations.Contains(relation.RelationName))
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
			//TODO: Implement.

			DataSet dataSet = GetDataSet();

			foreach(DataRelation dataRelation in relations)
			{
                
			}

		}

		public virtual bool CanRemove(DataRelation relation)
		{
			//TODO: Implement.
			return false;
		}

		public virtual void Clear()
		{
		}

		public virtual bool Contains(string name)
		{
			return false;
		}

		protected abstract DataSet GetDataSet();

		public virtual int IndexOf(DataRelation relation)
		{
			return relations.IndexOf(relation);
		}

		public virtual int IndexOf(string relationName)
		{
			return relations.IndexOf(this[relationName]);
		}

		protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent)
		{
		}

		protected internal virtual void OnCollectionChanging(CollectionChangeEventArgs ccevent)
		{
		}

		public void Remove(DataRelation relation)
		{
		}

		public void Remove(string name)
		{
		}

		public void RemoveAt(int index)
		{
		}

		protected virtual void RemoveCore(DataRelation relation)
		{
		}

		public event CollectionChangeEventHandler CollectionChanged;



	}
}
