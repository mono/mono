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
		
		[MonoTODO]
		public PgSqlClientPermission() 
			: this (PermissionState.None)
		{
		}

		[MonoTODO]
		public PgSqlClientPermission(PermissionState state) {
			// FIXME: do constructor
		}

		[MonoTODO]
		public PgSqlClientPermission(PermissionState state,
			bool allowBlankPassword) { 
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
