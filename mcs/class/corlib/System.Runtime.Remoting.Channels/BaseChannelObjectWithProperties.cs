//
// System.Runtime.Remoting.Channels.BaseChannelObjectWithProperties.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
// 	   Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{

	public abstract class BaseChannelObjectWithProperties :
		IDictionary, ICollection, IEnumerable
	{
		Hashtable table;
		
		public BaseChannelObjectWithProperties ()
		{
			table = new Hashtable ();
		}

	        public virtual int Count
		{
			get { return table.Count; }
		}

		public virtual bool IsFixedSize
		{
			get { return true; }
		}
		
		public virtual bool IsReadOnly
		{
			get { return false; }
		}

		public virtual bool IsSynchronized
		{
			get { return false; }
		}

		//
		// This is explicitly not implemented.
		//
		public virtual object this [object key]
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public virtual ICollection Keys
		{
			get { return table.Keys; }
		}

		public virtual IDictionary Properties
		{
			get { return this as IDictionary; }
		}

		public virtual object SyncRoot
		{
			get { return this; }
		}

		public virtual ICollection Values
		{
			get { return table.Values; }
		}

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
			return table.Contains (key);
		}

		public virtual void CopyTo (Array array, int index)
	        {
			// .NET says this method must not implemented
			throw new NotSupportedException ();
		}
		
		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			return table.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return table.GetEnumerator ();
		}
		
		public virtual void Remove (object key)
		{
			// .NET says this method must not implemented
			throw new NotSupportedException ();
		}
	}
}
