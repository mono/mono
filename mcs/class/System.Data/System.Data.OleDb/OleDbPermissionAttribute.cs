//
// System.Data.OleDb.OleDbPermissionAttribute
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
//

using System.Data;
using System.Data.Common;
using System.Security;

namespace System.Data.OleDb
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
			| AttributeTargets.Struct | AttributeTargets.Constructor |
			AttributeTargets.Method)]
	[Serializable]
	public sealed class OleDbPermissionAttribute : DBDataPermissionAttribute
	{
		[MonoTODO]
		[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
				| AttributeTargets.Struct | AttributeTargets.Constructor |
				AttributeTargets.Method)]
		[Serializable]
		public override IPermission CreatePermission () {
			throw new NotImplementedException ();
		}
	}
}
