//
// System.Data.ObjectSpaces.ObjectSources.cs 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003-2004
//

#if NET_1_2

using System.Collections;

namespace System.Data.ObjectSpaces {
        public class ObjectSources : IDataSources, IEnumerable
        {
		Hashtable table;

		#region Constructors

		public ObjectSources ()
		{
			table = new Hashtable ();
		}

		#endregion // Constructors

		#region Properties 

		public int Count {
			get { return table.Count; }
		}

		public object this [string name] {
			get { return table [name]; }
		}

		#endregion // Properties
                
		#region Methods

		public void Add (string name, IDbConnection connection)
		{
			table.Add (name, connection);
		}

		public void Add (string name, IDbTransaction transaction)
		{
			table.Add (name, transaction);
		}

		public void Clear ()
		{
			table.Clear ();
		}

		public bool Contains (string name)
		{
			return table.Contains (name);
		}

		public void Remove (string name)
		{
			table.Remove (name);
		}

		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			return table.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion // Methods
        }
}

#endif
