// 
// System.EnterpriseServices.SecurityCallers.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace System.EnterpriseServices {
	public sealed class SecurityCallers : IEnumerable {

		#region Constructors

		internal SecurityCallers ()
		{
		}

		internal SecurityCallers (ISecurityCallersColl collection)
		{
		}

		#endregion // Constructors

		#region Properties

		public int Count {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public SecurityIdentity this [int idx] {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
