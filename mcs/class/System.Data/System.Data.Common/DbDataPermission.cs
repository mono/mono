//
// System.Data.Common.DbDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

using System.Security.Permissions;

namespace System.Data.Common
{
	/// <summary>
	/// Provides the capability for a .NET data provider to ensure that a user has a security level adequate for accessing data.
	/// </summary>
	public abstract class DBDataPermission : CodeAccessPermission,
		IUnrestrictedPermission
	{
		private bool allowBlankPassword;
		private PermissionState permissionState;

		protected DBDataPermission () {
			this.allowBlankPassword = false;
			this.permissionState = None;
		}

		protected DBDataPermission (PermissionState state) {
			this.allowBlankPassword = false;
			this.permissionState = state;
		}

		public DBDataPermission (PermissionState state, bool abp) {
			this.allowBlankPassword = abp;
			this.permissionState = state;
		}

		public override IPermission Copy () {
			DbDataPermission copy = new DbDataPermission (
				this.permissionState, this.allowBlankPassword);

			return copy;
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement) {
			throw new NotImplementedException ();
		}

		public override IPermission Intersect (IPermission target) {
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) {
			throw new NotImplementedException ();
		}

		public bool IsUnrestricted () {
			if (this.permissionState == Unrestricted)
				return true;
			return false;
		}

		[MonoTODO]
		public override SecurityElement ToXml () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Union (IPermission target) {
			throw new NotImplementedException ();
		}
		
		public bool AllowBlankPassword {
			get { return this.allowBlankPassword; }
			set { this.allowBlankPassword = value; }
		}
	}
}
