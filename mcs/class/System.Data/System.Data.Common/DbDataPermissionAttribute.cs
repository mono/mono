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
		[MonoTODO]
		protected DBDataPermissionAttribute(SecurityAction action) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool AllowBlankPassword {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
