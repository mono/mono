//
// OraclePermission.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors:
//	Tim Coleman <tim@timcoleman.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Licensed under the MIT/X11 License.
//

using System.Collections;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OracleClient {

	[Serializable]
	[MonoTODO ("Current MS implementation of Data Provider requires FullTrust")]
	public sealed class OraclePermission : DBDataPermission {

		public OraclePermission (PermissionState state)
			: base (state)
		{
		}

		// required for Copy method
		internal OraclePermission (DBDataPermission permission)
			: base (permission)
		{
		}

		// easier (and common) permission creation from attribute class
		internal OraclePermission (DBDataPermissionAttribute attribute)
			: base (attribute)
		{
		}

		public override IPermission Copy ()
		{
			return new OraclePermission (this);
		}
	}
}
