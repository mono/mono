//
// System.Data.ObjectSpaces.Schema.ExtendedPropertyCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Xml;

namespace System.Data.ObjectSpaces.Schema {
	public class ExtendedPropertyCollection : CollectionBase
	{
		#region Fields

		ArrayList qnames;

		#endregion

		#region Constructors

		public ExtendedPropertyCollection ()
		{
			qnames = new ArrayList();
		}

		public ExtendedPropertyCollection (ExtendedProperty[] value)
			: this ()
		{
			AddRange (value);
		}

		public ExtendedPropertyCollection (ExtendedPropertyCollection value)
			: this ()
		{
			AddRange (value);
		}

		#endregion // Constructors

		#region Properties

		public ExtendedProperty this [int obj] {
			get { return (ExtendedProperty) List [obj]; }
			set { 
				List [obj] = value;
				qnames [obj] = value.QualifiedName;
			}
		}

		[MonoTODO]
		public ExtendedProperty this [XmlQualifiedName type] {
			get { return (ExtendedProperty) List [qnames.IndexOf (type)]; }
			set { List [qnames.IndexOf (type)] = value; }
		}

		#endregion // Properties

		#region Methods

		public void Add (ExtendedProperty value)
		{
			Insert (Count, value);
		}

		public void AddRange (ExtendedProperty[] value)
		{
			foreach (ExtendedProperty p in value)
				Add (p);
		}

		public void AddRange (ExtendedPropertyCollection value)
		{
			foreach (ExtendedProperty p in value)
				Add (p);
		}

		public bool Contains (ExtendedProperty value)
		{
			return List.Contains (value);
		}

		public void CopyTo (ExtendedProperty[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (ExtendedProperty value)
		{
			return List.IndexOf (value);
		}

		[MonoTODO]
		public void Insert (int index, ExtendedProperty value)
		{
			List.Insert (index, value);
			qnames [index] = value.QualifiedName;
		}

		[MonoTODO]
		protected override void OnInsert (int index, object value)
		{
		}

		[MonoTODO]
		protected override void OnRemove (int index, object value)
		{
		}

		[MonoTODO]
		public void Remove (ExtendedProperty value)
		{
			int index = IndexOf (value);
			List.Remove (value);
			qnames.RemoveAt (index);
		}

		#endregion // Methods
	}
}

#endif // NET_1_2
