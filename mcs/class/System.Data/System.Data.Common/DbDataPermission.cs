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
			allowBlankPassword = false;
			permissionState = PermissionState.None;
		}

		protected DBDataPermission (PermissionState state) {
			allowBlankPassword = false;
			permissionState = state;
		}

		public DBDataPermission (PermissionState state, bool abp) {
			allowBlankPassword = abp;
			permissionState = state;
		}

		public override IPermission Copy () {
			DbDataPermission copy = new DbDataPermission (
				permissionState, allowBlankPassword);

			return copy;
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) {
			throw new NotImplementedException ();
		}

		public bool IsUnrestricted () {
			if (permissionState == PermissionState.Unrestricted)
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
			get {
				return allowBlankPassword;
			}
			set {
				allowBlankPassword = value;
			}
		}
	}
}
