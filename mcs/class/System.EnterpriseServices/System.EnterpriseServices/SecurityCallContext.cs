// 
// System.EnterpriseServices.SecurityCallContext.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	public sealed class SecurityCallContext {

		#region Fields

		#endregion // Fields

		#region Constructors

		internal SecurityCallContext ()
		{
		}

		internal SecurityCallContext (ISecurityCallContext context)
		{
		}

		#endregion // Constructors

		#region Properties

		public SecurityCallers Callers {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static SecurityCallContext CurrentCall {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public SecurityIdentity DirectCaller {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public bool IsSecurityEnabled {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public int MinAuthenticationLevel {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public int NumCallers {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public SecurityIdentity OriginalCaller {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public bool IsCallerInRole (string role)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsUserInRole (string user, string role)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
