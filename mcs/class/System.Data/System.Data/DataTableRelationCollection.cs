//
// System.Data.DataTableRelationCollection.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data.Common;

namespace System.Data
{
	/// <summary>
	/// Summary description for DataTableRelationCollection.
	/// </summary>
	internal class DataTableRelationCollection : DataRelationCollection
	{
		/// <summary>
		/// Initializes a new instance of the DataRelationCollection class.
		/// </summary>
		[MonoTODO]
		internal DataTableRelationCollection():base()
		{
			// TODO: need to the constructor
		}

		/// <summary>
		/// Gets the DataRelation object specified by name.
		/// </summary>
		[MonoTODO]
		public override DataRelation this[string name]
		{
			get
			{
				foreach (DataRelation dataRelation in list)
				{
					if (dataRelation.RelationName == name)
					{
						return dataRelation;
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Gets the DataRelation object at the specified index.
		/// </summary>
		[MonoTODO]
		public override DataRelation this[int index]
		{
			get
			{
				return (DataRelation)list[index];
			}
		}

		/// <summary>
		/// Copies the elements of the specified DataRelation array to the end of the collection.
		/// </summary>
		/// <param name="relations">The array of DataRelation objects to add to the collection.</param>
		[MonoTODO]
		public override void AddRange(DataRelation[] relations)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool CanRemove(DataRelation relation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Clear()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Contains(string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override DataSet GetDataSet()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override int IndexOf(DataRelation relation)
		{
			return list.IndexOf(relation);
		}

		[MonoTODO]
		public override int IndexOf(string relationName)
		{
			return list.IndexOf(this[relationName]);
		}

		[MonoTODO]
		protected override void OnCollectionChanged(CollectionChangeEventArgs ccevent)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void OnCollectionChanging(CollectionChangeEventArgs ccevent)
		{
			throw new NotImplementedException ();
		}
		
	}
}
