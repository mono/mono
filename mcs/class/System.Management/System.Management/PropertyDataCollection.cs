//
// System.Management.PropertyDataCollection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;

namespace System.Management
{
	public class PropertyDataCollection : ICollection, IEnumerable
	{
		internal PropertyDataCollection ()
		{
		}

		[MonoTODO]
		public virtual void Add (string propertyName, object propertyValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (string propertyName, CimType propertyType, bool isArray)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (string propertyName, object propertyValue, CimType propertyType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (PropertyData [] propertyArray, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PropertyDataEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Remove (string propertyName)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int Count {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual bool IsSynchronized {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual PropertyData this [string propertyName] {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual object SyncRoot {
			get {
				throw new NotImplementedException ();
			}
		}

		public class PropertyDataEnumerator : IEnumerator
		{
			internal PropertyDataEnumerator ()
			{
			}

			[MonoTODO]
			public virtual bool MoveNext ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public virtual void Reset ()
			{
				throw new NotImplementedException ();
			}

			public PropertyData Current {
				[MonoTODO]
				get {
					throw new NotImplementedException ();
				}
			}

			object IEnumerator.Current {
				[MonoTODO]
				get {
					throw new NotImplementedException ();
				}
			}
		}
	}
}

