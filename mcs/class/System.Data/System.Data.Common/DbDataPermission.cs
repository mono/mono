//
// System.Data.Common.DbDataPermission.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002-2003
//

using System.Data;
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

#if NET_1_2
		[Obsolete ("use DBDataPermission (PermissionState.None)", true)]
#endif
		protected DBDataPermission () 
			: this (PermissionState.None, false)
		{
		}

#if NET_1_2
		[MonoTODO]
		protected DBDataPermission (DbConnectionString constr)
		{
		}
#endif

		protected DBDataPermission (PermissionState state) 
			: this (state, false)
		{
		}

#if NET_1_2
		[Obsolete ("use DBDataPermission (PermissionState.None)", true)]
#endif
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

#if NET_1_1
		[MonoTODO]
		public virtual void Add (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			throw new NotImplementedException ();
		}
#endif

#if NET_1_2
		[MonoTODO]
		protected void AddConnectionString (DbConnectionString constr)
		{
			throw new NotImplementedException ();
		}
#endif

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

#if NET_1_2
		[MonoTODO]
		protected void SetConnectionString (DbConnectionString constr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetRestriction (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			throw new NotImplementedException ();
		}
#endif

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
