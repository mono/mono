// 
// System.EnterpriseServices.SharedPropertyGroupManager.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[ComVisible (false)]
	public sealed class SharedPropertyGroupManager : IEnumerable {

		#region Constructors

		public SharedPropertyGroupManager ()
		{
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public SharedPropertyGroup CreatePropertyGroup (string name, ref PropertyLockMode dwIsoMode, ref PropertyReleaseMode dwRelMode, out bool fExist)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SharedPropertyGroup Group (string name)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
