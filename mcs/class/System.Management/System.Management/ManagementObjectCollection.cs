//
// System.Management.ManagementObjectCollection
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
	public class ManagementObjectCollection : ICollection, IEnumerable, IDisposable
	{
		private ManagementObjectCollection ()
		{
		}

		[MonoTODO]
		public virtual void CopyTo (System.Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (ManagementBaseObject [] objectCollection, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ManagementObjectEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public virtual int Count {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual bool IsSynchronized {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual object SyncRoot {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public class ManagementObjectEnumerator : IEnumerator, IDisposable
		{
			internal ManagementObjectEnumerator ()
			{
			}

			[MonoTODO]
			public virtual void Dispose ()
			{
				throw new NotImplementedException ();
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

			public ManagementBaseObject Current {
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

