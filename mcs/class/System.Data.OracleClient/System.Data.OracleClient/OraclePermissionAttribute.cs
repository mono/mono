//
// OraclePermissionAttribute.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors:
//	Tim Coleman <tim@timcoleman.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Licensed under the MIT/X11 License.
//

using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OracleClient {

	[Serializable]
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | 
			 AttributeTargets.Struct | AttributeTargets.Constructor | 
			 AttributeTargets.Method, AllowMultiple=true,
			 Inherited=false)]
#if NET_2_0
	public sealed class OraclePermissionAttribute : DBDataPermissionAttribute {
#else
	public sealed class OraclePermissionAttribute : CodeAccessSecurityAttribute {

		#region Fields

		bool allowBlankPassword;

		#endregion // Fields

		#region Properties

		public bool AllowBlankPassword {
			get { return allowBlankPassword; }
			set { allowBlankPassword = value; }
		}

		#endregion // Properties
#endif

		#region Constructors

		public OraclePermissionAttribute (SecurityAction action) 
			: base (action)
		{
		}

		#endregion // Constructors

		#region Methods

		public override IPermission CreatePermission ()
		{
			return new OraclePermission (this);
		}

		#endregion // Methods
	}
}
