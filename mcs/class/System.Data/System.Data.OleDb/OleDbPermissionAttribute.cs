//
// System.Data.OleDb.OleDbPermissionAttribute
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
			| AttributeTargets.Struct | AttributeTargets.Constructor |
			AttributeTargets.Method)]
	[Serializable]
	public sealed class OleDbPermissionAttribute : DBDataPermissionAttribute
	{

		#region Constructors 

		[MonoTODO]
		OleDbPermissionAttribute (SecurityAction action) 
			: base (action)
		{
		}

		#endregion

		#region Properties

		[MonoTODO]
		public string Provider {
			[MonoTODO]
			get {
				throw new NotImplementedException (); 
			}
			[MonoTODO]
			set {
				throw new NotImplementedException (); 
			}
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override IPermission CreatePermission () 
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
