//
// System.Data.Common.DbDataPermissionAttribute.cs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Security.Permissions;

namespace System.Data.Common {
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method)]
	[Serializable]
	public abstract class DBDataPermissionAttribute : CodeAccessSecurityAttribute
	{
		#region Fields

		SecurityAction securityAction;
		bool allowBlankPassword;

		#endregion // Fields

		#region Constructors

		protected DBDataPermissionAttribute (SecurityAction action) 
			: base (action) 
		{
			securityAction = action;
			allowBlankPassword = false;
		}

		#endregion // Constructors

		#region Properties

		public bool AllowBlankPassword {
			get { return allowBlankPassword; }
			set { allowBlankPassword = value; }
		}

		#endregion // Properties
	}
}
