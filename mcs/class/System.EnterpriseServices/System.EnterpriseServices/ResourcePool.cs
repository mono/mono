// 
// System.EnterpriseServices.ResourcePool.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	public sealed class ResourcePool {

		#region Fields

		ResourcePool.TransactionEndDelegate cb;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public ResourcePool (ResourcePool.TransactionEndDelegate cb)
		{
			this.cb = cb;
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public object GetResource ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool PutResource (object resource)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

		#region Delegates

		public delegate void TransactionEndDelegate (object resource);

		#endregion
	}
}
