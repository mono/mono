//
// OraclePermissionAttribute.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OracleClient {
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor)]
	[Serializable]
	public sealed class OraclePermissionAttribute : CodeAccessSecurityAttribute
	{
		#region Fields

		bool allowBlankPassword;

		#endregion // Fields

		#region Constructors

		public OraclePermissionAttribute (SecurityAction action) 
			: base (action)
		{
		}

		#endregion // Constructors

		#region Properties

		public bool AllowBlankPassword {
			get { return allowBlankPassword; }
			set { allowBlankPassword = value; }
		}

		#endregion // Properties

		#region Methods

		public override IPermission CreatePermission ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
