//
// SynchronizedSecurityTokenCache.cs - Synchronized access to SecurityTokenCache
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;

namespace Microsoft.Web.Services.Security {

	public class SynchronizedSecurityTokenCache : SecurityTokenCache {

		public SynchronizedSecurityTokenCache (SecurityTokenCache wrapped) {}

		[MonoTODO]
		public override int Count { 
			get { return 0; }
		}

		[MonoTODO]
		public override bool IsSynchronized { 
			get { return false; }
		}

		[MonoTODO]
		public override SecurityToken this [string identifier] { 
			get { return null; }
		}

		[MonoTODO]
		public override object SyncRoot { 
			get { return null; }
		}

		[MonoTODO]
		public override void Add (SecurityToken token) {}

		[MonoTODO]
		public override void Clear () {}

		[MonoTODO]
		public override bool Contains (SecurityToken token) 
		{
			return false;
		}

		[MonoTODO]
		public override void CopyTo (Array array, int index) {}

		[MonoTODO]
		public override IEnumerator GetEnumerator () 
		{
			return null;
		}

		[MonoTODO]
		public override void Remove (SecurityToken token) {}
	}
}
