//
// Mono.Data.SybaseClient.SybasePermissionAttribute.cs
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
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method)]
	[Serializable]
	public sealed class SybasePermissionAttribute : DBDataPermissionAttribute 
	{
		[MonoTODO]
		public SybasePermissionAttribute(SecurityAction action) 
			: base(action)
		{
			// FIXME: do constructor
		}

		[MonoTODO]
		public override IPermission CreatePermission() 
		{
			throw new NotImplementedException ();
		}
	}
}
