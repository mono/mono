//
// System.Runtime.Remoting.Channels.BaseChannelObjectWithProperties.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels {

	public abstract class BaseChannelObjectWithProperties :
		IDictionary, ICollection, IEnumerable
	{
		[MonoTODO]
		public BaseChannelObjectWithProperties ()
		{
		}

	        public virtual int Count
		{
			get { throw new NotImplementedException (); }
		}

		public virtual bool IsFixedSize
		{
			get { throw new NotImplementedException (); }
		}
		
		public virtual bool IsReadOnly
		{
			get { throw new NotImplementedException (); }
		}

		public virtual bool IsSynchronized
		{
			get { throw new NotImplementedException (); }
		}

		public virtual object this[object key]
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public virtual ICollection Keys
		{
			get { throw new NotImplementedException (); }
		}

		public virtual IDictionary Properties
		{
			get { throw new NotImplementedException (); }
		}

		public virtual object SyncRoot
		{
			get { throw new NotImplementedException (); }
		}

		public virtual ICollection Values
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual void Add (object key, object value)
		{
			// .NET says this method must not implemented
			throw new NotSupportedException ();
		}

		public virtual void Clear ()
		{
			// .NET says this method must not implemented
			throw new NotSupportedException ();
		}

		public virtual bool Contains (object key)
		{
			throw new NotImplementedException ();
		}

		public virtual void CopyTo (Array array, int index)
	        {
			// .NET says this method must not implemented
			throw new NotSupportedException ();
		}
		
		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		public virtual void Remove (object key)
		{
			// .NET says this method must not implemented
			throw new NotSupportedException ();
		}
	}
}
