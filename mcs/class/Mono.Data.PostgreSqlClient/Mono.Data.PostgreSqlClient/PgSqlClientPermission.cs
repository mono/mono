//
// Mono.Data.PostgreSqlClient.PgSqlClientPermission.cs
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

	public sealed class PgSqlClientPermission : DBDataPermission {
	
#if NET_1_2
		[Obsolete ("use PgSqlClientPermission(PermissionState.None)", true)]
#endif
		[MonoTODO]
		public PgSqlClientPermission() 
#if NET_1_2
			: base (PermissionState.None)
#endif
		{
		}

		[MonoTODO]
		public PgSqlClientPermission(PermissionState state) 
			: base (state)
		{
			// FIXME: do constructor
		}

#if NET_1_2
		[Obsolete ("use PgSqlClientPermission(PermissionState.None)", true)]
#endif
		[MonoTODO]
		public PgSqlClientPermission(PermissionState state, bool allowBlankPassword) 
#if NET_1_2
			: base (PermissionState.None)
#endif
		{ 
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Copy() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void FromXml(SecurityElement 
					securityElement) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Intersect(IPermission target)  {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSubsetOf(IPermission target) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override SecurityElement ToXml() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Union(IPermission target) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		~PgSqlClientPermission() {
			// FIXME: destructor to release resources
		}
	}
}
