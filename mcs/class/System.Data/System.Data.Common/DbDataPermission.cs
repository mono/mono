//
// System.Data.Common.DbDataPermission.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002
//

using System.Security;
using System.Security.Permissions;

namespace System.Data.Common {
	[Serializable]
	public abstract class DBDataPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		#region Fields

		bool allowBlankPassword;
		PermissionState state;

		#endregion // Fields

		#region Constructors

		protected DBDataPermission () 
			: this (PermissionState.None, false)
		{
		}

		protected DBDataPermission (PermissionState state) 
			: this (state, false)
		{
		}

		public DBDataPermission (PermissionState state, bool allowBlankPassword) 
		{
			this.state = state;
			this.allowBlankPassword = allowBlankPassword;
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
			DBDataPermission copy = CreateInstance ();
			copy.AllowBlankPassword = this.allowBlankPassword;
			copy.State = this.state;
			return copy;
		}

		protected virtual DBDataPermission CreateInstance ()
		{
			return (DBDataPermission) Activator.CreateInstance (this.GetType ());
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) 
		{
			throw new NotImplementedException ();
		}

		public bool IsUnrestricted () 
		{
			return (state == PermissionState.Unrestricted);
		}

		[MonoTODO]
		public override SecurityElement ToXml () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Union (IPermission target) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
