//
// Mono.Data.PostgreSqlClient.PgSqlClientPermissionAttribute.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace Mono.Data.PostgreSqlClient {

	[AttributeUsage(AttributeTargets.Assembly    | 
			AttributeTargets.Class 	     | 
			AttributeTargets.Struct      | 
			AttributeTargets.Constructor |
			AttributeTargets.Method)]
	[Serializable]
	public sealed class PgSqlClientPermissionAttribute :
		DBDataPermissionAttribute {

		[MonoTODO]
		public PgSqlClientPermissionAttribute(SecurityAction action) : 
			base(action)
		{
			// FIXME: do constructor
		}

		[MonoTODO]
		public override IPermission CreatePermission() {
			throw new NotImplementedException ();
		}

		//[MonoTODO]
		//~PgSqlClientPermissionAttribute() {
		//	// FIXME: destructor to release resources
		//}
	}

}
