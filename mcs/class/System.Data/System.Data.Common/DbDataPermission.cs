//
// System.Data.Common.DbDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.Common
{
	/// <summary>
	/// Provides the capability for a .NET data provider to ensure that a user has a security level adequate for accessing data.
	/// </summary>
	public abstract class DBDataPermission : CodeAccessPermission,
		IUnrestrictedPermission
	{
		[MonoTODO]
		protected DBDataPermission() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected DBDataPermission(PermissionState) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DBDataPermission(PermissionState, bool) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected DBDataPermission(PermissionState) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DBDataPermission(PermissionState, bool) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Copy() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void FromXml(SecurityElement securityElement) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Intersect(IPermission target) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSubsetOf(IPermission target) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsUnrestricted() {
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
		public bool AllowBlankPassword {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
