//
// System.Data.Odbc.OdbcPermissionAttribute
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Novell Inc 2004
//

using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Odbc
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
			| AttributeTargets.Struct | AttributeTargets.Constructor |
			AttributeTargets.Method)]
	[Serializable]
	public sealed class OdbcPermissionAttribute : DBDataPermissionAttribute
	{

		#region Constructors 

		[MonoTODO]
		public OdbcPermissionAttribute (SecurityAction action) 
			: base (action)
		{
		}

		#endregion

		#region Properties

		[MonoTODO]
		internal string Provider {
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
