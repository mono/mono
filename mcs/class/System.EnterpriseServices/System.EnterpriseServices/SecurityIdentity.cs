// 
// System.EnterpriseServices.SecurityIdentity.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace System.EnterpriseServices {
	public sealed class SecurityIdentity {

		#region Constructors

		[MonoTODO]
		internal SecurityIdentity ()
		{
		}

		[MonoTODO]
		internal SecurityIdentity (ISecurityIdentityColl collection)
		{
		}

		#endregion // Constructors

		#region Properties

		public string AccountName {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public AuthenticationOption AuthenticationLevel {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public int AuthenticationService {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public ImpersonationLevelOption ImpersonationLevel {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}
