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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

#if NET_2_0
		[Obsolete ("use DBDataPermission (PermissionState.None)", true)]
#endif
		protected DBDataPermission () 
#if NET_2_0
			: this (PermissionState.None)
#else
			: this (PermissionState.None, false)
#endif
		{
		}

		[MonoTODO]
		protected DBDataPermission (DBDataPermission permission)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected DBDataPermission (DBDataPermissionAttribute permissionAttribute)
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		protected DBDataPermission (DbConnectionString constr)
		{
			throw new NotImplementedException ();
		}
#endif

		protected DBDataPermission (PermissionState state) 
			: this (state, false, false)
		{
		}

#if NET_2_0
		[Obsolete ("use DBDataPermission (PermissionState.None)", true)]
#endif
		public DBDataPermission (PermissionState state, bool allowBlankPassword)
			: this (state, allowBlankPassword, true)
		{
		}

		internal DBDataPermission (PermissionState state, bool allowBlankPassword, bool dummyArg)
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

#if NET_2_0
		[MonoTODO]
		protected void AddConnectionString (DbConnectionString constr)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		protected void Clear ()
		{
			throw new NotImplementedException ();
		}

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

#if NET_2_0
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
