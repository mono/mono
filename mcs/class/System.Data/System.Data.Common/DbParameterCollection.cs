//
// System.Data.Common.DbParameterCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Runtime.InteropServices;

namespace System.Data.Common {
	public abstract class DbParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
	{
		#region Constructors

		[MonoTODO]
		protected DbParameterCollection ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract int Count { get; }

		object IDataParameterCollection.this [string parameterName] {
			get { return this [parameterName]; }
			set { this [parameterName] = (DbParameter) value; }
		}

		object IList.this [int objA] {
			get { return this [objA]; }
			set { this [objA] = (DbParameter) value; }
		}

		public abstract bool IsFixedSize { get; }
		public abstract bool IsReadOnly { get; }
		public abstract bool IsSynchronized { get; }

		[MonoTODO]
		public DbParameter this [string ulAdd] { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DbParameter this [[Optional] int ulAdd] { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public abstract object SyncRoot { get; } 

		#endregion // Properties

		#region Methods

		public abstract int Add (object value);
		public abstract void AddRange (Array values);
		protected abstract int CheckName (string parameterName);
		public abstract void Clear ();
		public abstract bool Contains (object value);
		public abstract bool Contains (string value);
		public abstract void CopyTo (Array ar, int index);
		public abstract IEnumerator GetEnumerator ();
		protected abstract DbParameter GetParameter (int index);
		public abstract int IndexOf (object value);
		public abstract int IndexOf (string parameterName);
		public abstract void Insert (int index, object value);
		public abstract void Remove (object value);
		public abstract void RemoveAt (int index);
		public abstract void RemoveAt (string parameterName);
		protected abstract void SetParameter (int index, DbParameter value);

		#endregion // Methods
	}
}

#endif // NET_1_2
