//
// OraclePermission.cs 
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
	[Serializable]
	public sealed class OraclePermission : CodeAccessPermission, IUnrestrictedPermission
	{
		#region Fields

		bool allowBlankPassword;
		PermissionState state;

		#endregion // Fields

		#region Constructors

		public OraclePermission (PermissionState state)
		{
			this.state = state;
		}

		#endregion // Constructors

		#region Properties

		public bool AllowBlankPassword {
			get { return allowBlankPassword; }
			set { allowBlankPassword = value; }
		}

		internal PermissionState State {
			get { return state; }
			set { state = value; }
		}

		#endregion // Properties

		#region Methods

		public override IPermission Copy ()
		{
			OraclePermission copy = (OraclePermission) Activator.CreateInstance (this.GetType ());
			copy.AllowBlankPassword = allowBlankPassword;
			copy.State = state;
			return copy;
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target)
		{
			if (target != null && !(target is OraclePermission))
				throw new ArgumentException ();
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target)
		{
			throw new NotImplementedException ();
		}

		public bool IsUnrestricted ()
		{
			return (State == PermissionState.Unrestricted);
		}

		[MonoTODO]
		public override SecurityElement ToXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			if (target != null && !(target is OraclePermission))
				throw new ArgumentException ();
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
