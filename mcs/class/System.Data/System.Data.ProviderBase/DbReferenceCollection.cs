//
// System.Data.ProviderBase.DbReferenceCollection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbReferenceCollection : IEnumerable
	{
		#region Constructors
	
		[MonoTODO]
		protected DbReferenceCollection ()
		{
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public void Add (object value)
		{
			throw new NotImplementedException ();
		}

		public abstract void Add (object value, int tag);

		[MonoTODO]
		protected void AddItem (object value, int tag)
		{
			Add (value, tag);
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Notify (int message, int tag, object connectionInternal)
		{
			throw new NotImplementedException ();
		}

		protected abstract bool NotifyItem (int message, object value, int tag, object connectionInternal);

		[MonoTODO]
		public void Purge ()
		{
			throw new NotImplementedException ();
		}

		public abstract void Remove (object value);

		[MonoTODO]
		protected void RemoveItem (object value)
		{
			Remove (value);
		}

		#endregion // Methods
	}
}

#endif
