//
// System.Data.SqlClient.SqlClientPermissionAttribute.cs
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

namespace System.Data.SqlClient {

	[AttributeUsage(AttributeTargets.Assembly    | 
			AttributeTargets.Class 	     | 
			AttributeTargets.Struct      | 
			AttributeTargets.Constructor |
			AttributeTargets.Method)]
	[Serializable]
	public sealed class SqlClientPermissionAttribute :
		DBDataPermissionAttribute {

		[MonoTODO]
		[AttributeUsage(AttributeTargets.Assembly    | 
			 AttributeTargets.Class       | 
			 AttributeTargets.Struct      | 
			 AttributeTargets.Constructor |
			 AttributeTargets.Method)]
		[Serializable]
		public SqlClientPermissionAttribute(SecurityAction action) {
			// FIXME: do constructor
		}

		[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
			 | AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
		[Serializable]
		public override IPermission CreatePermission() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		~SqlClientPermissionAttribute() {
			// FIXME: destructor to release resources
		}
	}

}
