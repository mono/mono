//
// System.Data.Common.DbDataPermission.cs
//
// Authors:
//	Rodrigo Moya (rodrigo@ximian.com)
//	Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002-2003
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

using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Common {

	[Serializable]
	public abstract class DBDataPermission : CodeAccessPermission, IUnrestrictedPermission {

		#region Fields

		private const int version = 1;

		private bool allowBlankPassword;
		private PermissionState state;
		private Hashtable _connections;

		#endregion // Fields

		#region Constructors

#if NET_2_0
		[Obsolete ("use DBDataPermission (PermissionState.None)", true)]
#endif
		protected DBDataPermission () 
			: this (PermissionState.None)
		{
		}

		protected DBDataPermission (DBDataPermission permission)
		{
			if (permission == null)
				throw new ArgumentNullException ("permission");

			state = permission.state;
			if (state != PermissionState.Unrestricted) {
				allowBlankPassword = permission.allowBlankPassword;
				_connections = (Hashtable) _connections.Clone ();
			}
		}

		protected DBDataPermission (DBDataPermissionAttribute permissionAttribute)
		{
			if (permissionAttribute == null)
				throw new ArgumentNullException ("permissionAttribute");

			_connections = new Hashtable ();
			if (permissionAttribute.Unrestricted) {
				state = PermissionState.Unrestricted;
			}
			else {
				state = PermissionState.None;
				allowBlankPassword = permissionAttribute.AllowBlankPassword;
				Add (permissionAttribute.ConnectionString, permissionAttribute.KeyRestrictions, 
					permissionAttribute.KeyRestrictionBehavior);
			}
		}

#if NET_2_0
		[MonoTODO]
		protected DBDataPermission (DbConnectionOptions connectionOptions)
			: this (PermissionState.None)
		{
			// ignore null (i.e. no ArgumentNullException)
			if (connectionOptions != null) {
				throw new NotImplementedException ();
			}
		}
#endif

		protected DBDataPermission (PermissionState state) 
		{
			this.state = PermissionHelper.CheckPermissionState (state, true);
			_connections = new Hashtable ();
		}

#if NET_2_0
		[Obsolete ("use DBDataPermission (PermissionState.None)", true)]
#endif
		public DBDataPermission (PermissionState state, bool allowBlankPassword)
			: this (state)
		{
			this.allowBlankPassword = allowBlankPassword;
		}

		#endregion // Constructors

		#region Properties

		public bool AllowBlankPassword {
			get { return allowBlankPassword; }
			set { allowBlankPassword = value; }
		}
		
		#endregion // Properties

		#region Methods

#if NET_2_0
		public virtual void Add (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			state = PermissionState.None;
			AddConnectionString (connectionString, restrictions, behavior, null, false);
		}

		[MonoTODO ("synonyms and useFirstKeyValue aren't supported")]
		protected virtual void AddConnectionString (string connectionString, string restrictions, 
			KeyRestrictionBehavior behavior, Hashtable synonyms, bool useFirstKeyValue)
		{
			_connections [connectionString] = new object [2] { restrictions, connectionString };
		}
#elif NET_1_1
		public virtual void Add (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			state = PermissionState.None;
			_connections [connectionString] = new object [2] { restrictions, connectionString };
		}
#endif

		protected void Clear ()
		{
			_connections.Clear ();
		}

		public override IPermission Copy () 
		{
			// call protected constructor
			return (IPermission) Activator.CreateInstance (this.GetType (), new object [1] { this });
		}

		protected virtual DBDataPermission CreateInstance ()
		{
			return (DBDataPermission) Activator.CreateInstance (this.GetType ());
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement) 
		{
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
		[MonoTODO ("DO NOT IMPLEMENT - will be removed")]
		[Obsolete ("DO NOT IMPLEMENT - will be removed")]
		protected void SetConnectionString (DbConnectionOptions constr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("DO NOT IMPLEMENT - will be removed")]
		[Obsolete ("DO NOT IMPLEMENT - will be removed")]
		public virtual void SetRestriction (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			throw new NotImplementedException ();
		}
#endif

		public override SecurityElement ToXml ()
		{
			SecurityElement se = PermissionHelper.Element (this.GetType (), version);
			if (IsUnrestricted ()) {
				se.AddAttribute ("Unrestricted", "true");
			}
			else {
				// attribute is present for both True and False
				se.AddAttribute ("AllowBlankPassword", allowBlankPassword.ToString ());
				foreach (DictionaryEntry de in _connections) {
					SecurityElement child = new SecurityElement ("add");
					child.AddAttribute ("ConnectionString", (string) de.Key);
					object[] restrictionsInfo = (object[]) de.Value;
					child.AddAttribute ("KeyRestrictions", (string) restrictionsInfo [0]);
					KeyRestrictionBehavior krb = (KeyRestrictionBehavior) restrictionsInfo [1];
					child.AddAttribute ("KeyRestrictionBehavior", krb.ToString ());
					se.AddChild (child);
				}
			}
			return se;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
