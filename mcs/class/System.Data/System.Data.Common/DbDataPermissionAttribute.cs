//
// System.Data.Common.DbDataPermissionAttribute.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.Common
{
	/// <summary>
	/// Associates a security action with a custom security attribute.
	/// </summary>
	public abstract class DBDataPermissionAttribute : CodeAccessSecurityAttribute
	{
		private SecurityAction securityAction;
		private bool allowBlankPassword;

		protected DBDataPermissionAttribute(SecurityAction action) {
			this.securityAction = action;
			this.allowBlankPassword = false;
		}

		public bool AllowBlankPassword {
			get {
				return this.allowBlankPassword;
			}
			set {
				this.allowBlankPassword = value;
			}
		}
	}
}
