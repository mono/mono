//
// System.Data.ObjectSpaces.ObjectSources.cs 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;

namespace System.Data.ObjectSpaces {
        public class ObjectSources : IDataSources, IEnumerable
        {
		#region Constructors

		public ObjectSources ()
		{
		}

		#endregion // Constructors

		#region Properties 

		[MonoTODO]
		public int Count {
			get { throw new NotImplementedException (); }
		}

		public object this [string name] {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
                
		#region Methods

		[MonoTODO]
		public void Add (string name, IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (string name, IDbTransaction transaction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif
