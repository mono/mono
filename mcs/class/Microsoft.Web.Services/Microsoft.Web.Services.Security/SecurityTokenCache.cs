//
// SecurityTokenCache.cs - SecurityToken Cache
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;

namespace Microsoft.Web.Services.Security {

	public class SecurityTokenCache : ICollection {

		[MonoTODO]
		public static SecurityTokenCache GlobalCache { 
			get { return null; }
		}

		[MonoTODO]
		public static SecurityTokenCache Synchronized (SecurityTokenCache cache) 
		{
			return null;
		}


		[MonoTODO]
		public SecurityTokenCache () {}

		[MonoTODO]
		public virtual int Count {
			get { return 0; }
		}

		[MonoTODO]
		public virtual bool IsSynchronized { 
			get { return false; }
		}

		[MonoTODO]
		public virtual SecurityToken this [string identifier] { 
			get { return null; }
		}

		[MonoTODO]
		public virtual object SyncRoot { 
			get { return null; }
		}

		[MonoTODO]
		public event EventHandler Changed;


		[MonoTODO]
		protected virtual void Add (string identifier, SecurityToken token) {}
		
		[MonoTODO]
		protected void CacheChanged () {}
		
		[MonoTODO]
		protected virtual bool Contains (string identifier) 
		{
			return false;
		}

		[MonoTODO]
		protected virtual void Remove (string identifier) {}

		[MonoTODO]
		public virtual void Add (SecurityToken token) {}

		[MonoTODO]
		public void AddRange (ICollection collection) {}

		[MonoTODO]
		public virtual void Clear () {}

		[MonoTODO]
		public virtual bool Contains (SecurityToken token) 
		{
			return false;
		}

		[MonoTODO]
		public virtual void CopyTo (Array array, int index) {}

		[MonoTODO]
		public virtual IEnumerator GetEnumerator ()
		{
			return null;
		}

		[MonoTODO]
		public virtual void Remove (SecurityToken token) {}
	}
}
