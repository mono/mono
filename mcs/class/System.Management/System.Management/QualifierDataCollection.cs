//
// System.Management.QualifierDataCollection
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
	public class QualifierDataCollection : ICollection, IEnumerable
	{
		internal QualifierDataCollection ()
		{
		}

		[MonoTODO]
		public virtual void Add (string qualifierName, object qualifierValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Add (string qualifierName,
					 object qualifierValue,
					 bool isAmended,
					 bool propagatesToInstance,
					 bool propagatesToSubclass,
					 bool isOverridable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (QualifierData [] qualifierArray, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public QualifierDataEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Remove (string qualifierName)
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

		public virtual QualifierData this [string qualifierName] {
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

		public class QualifierDataEnumerator : IEnumerator
		{
			internal QualifierDataEnumerator ()
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

			public QualifierData Current {
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

