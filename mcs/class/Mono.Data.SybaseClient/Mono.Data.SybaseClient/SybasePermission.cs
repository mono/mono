//
// Mono.Data.SybaseClient.SybasePermission.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace Mono.Data.SybaseClient {
	public sealed class SybasePermission : DBDataPermission 
	{
#if NET_1_2
		[Obsolete ("Use SybasePermission(PermissionState.None)", true)]
#endif
		[MonoTODO]
		public SybasePermission () 
#if NET_1_2
			: base (PermissionState.None)
#endif
		{
			// FIXME: do constructor
		}

		[MonoTODO]
		public SybasePermission (PermissionState state) 
			: base (state)
		{
			// FIXME: do constructor
		}

#if NET_1_2
		[Obsolete ("Use SybasePermission(PermissionState.None)", true)]
#endif
		[MonoTODO]
		public SybasePermission (PermissionState state, bool allowBlankPassword) 
#if NET_1_2
			: base (PermissionState.None)
#endif
		{ 
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Copy () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target)  
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override SecurityElement ToXml () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Union (IPermission target) 
		{
			throw new NotImplementedException ();
		}
	}
}
