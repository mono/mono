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

		protected DBDataPermissionAttribute (SecurityAction action) {
			securityAction = action;
			allowBlankPassword = false;
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
