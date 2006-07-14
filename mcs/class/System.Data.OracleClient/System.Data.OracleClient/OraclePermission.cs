//
// OraclePermission.cs 
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

using System.Collections;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OracleClient {

#if NET_2_0
	[Serializable]
	[MonoTODO ("Current MS implementation of Data Provider requires FullTrust")]
	public sealed class OraclePermission : DBDataPermission {

		public OraclePermission (PermissionState state)
			: base (state)
		{
		}

		// required for Copy method
		internal OraclePermission (DBDataPermission permission)
			: base (permission)
		{
		}

		// easier (and common) permission creation from attribute class
		internal OraclePermission (DBDataPermissionAttribute attribute)
			: base (attribute)
		{
		}

		public override IPermission Copy ()
		{
			return new OraclePermission (this);
		}
	}
#else
	[Serializable]
	[MonoTODO ("Current MS implementation of Data Provider requires FullTrust")]
	public sealed class OraclePermission : CodeAccessPermission, IUnrestrictedPermission {

		#region Fields

		bool allowBlankPassword;
		PermissionState state;

		#endregion // Fields

		#region Constructors

		public OraclePermission (PermissionState state)
		{
			this.state = state;
		}

		// easier (and common) permission creation from attribute class
		internal OraclePermission (OraclePermissionAttribute attribute)
		{
			if (attribute.Unrestricted) {
				state = PermissionState.Unrestricted;
			}
			else {
				state = PermissionState.None;
				allowBlankPassword = attribute.AllowBlankPassword;
			}
		}

		#endregion // Constructors

		#region Properties

		public bool AllowBlankPassword {
			get { return allowBlankPassword; }
			set { allowBlankPassword = value; }
		}

		#endregion // Properties

		#region Methods

		public override IPermission Copy ()
		{
			OraclePermission copy = (OraclePermission) Activator.CreateInstance (this.GetType ());
			copy.AllowBlankPassword = allowBlankPassword;
			copy.state = state;
			return copy;
		}

		// Note: No exception are thrown here to help the security runtime performance

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement)
		{
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target)
		{
			return null;
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target)
		{
			return false;
		}

		public bool IsUnrestricted ()
		{
			return (state == PermissionState.Unrestricted);
		}

		[MonoTODO]
		public override SecurityElement ToXml ()
		{
			return new SecurityElement ("IPermission");
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			return null;
		}

		#endregion // Methods
	}
#endif
}
