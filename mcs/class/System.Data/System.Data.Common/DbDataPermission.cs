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
				_connections = (Hashtable) permission._connections.Clone ();
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
				if (permissionAttribute.ConnectionString.Length > 0) {
					Add (permissionAttribute.ConnectionString, permissionAttribute.KeyRestrictions, 
						permissionAttribute.KeyRestrictionBehavior);
				}
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
			_connections [connectionString] = new object [2] { restrictions, behavior };
		}
#elif NET_1_1
		public virtual void Add (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			state = PermissionState.None;
			_connections [connectionString] = new object [2] { restrictions, behavior };
		}
#endif

		protected void Clear ()
		{
			_connections.Clear ();
		}

		public override IPermission Copy () 
		{
			DBDataPermission dbdp = CreateInstance ();
			dbdp.allowBlankPassword = this.allowBlankPassword;
			dbdp._connections = (Hashtable) this._connections.Clone ();
			return dbdp;
		}

		protected virtual DBDataPermission CreateInstance ()
		{
			return (DBDataPermission) Activator.CreateInstance (this.GetType (), new object [1] { PermissionState.None });
		}

		public override void FromXml (SecurityElement securityElement) 
		{
			PermissionHelper.CheckSecurityElement (securityElement, "securityElement", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			state = (PermissionHelper.IsUnrestricted (securityElement) ? 
				PermissionState.Unrestricted : PermissionState.None);

			allowBlankPassword = false;
			string blank = securityElement.Attribute ("AllowBlankPassword");
			if (blank != null) {
#if NET_2_0
				// avoid possible exceptions with Fx 2.0
				if (!Boolean.TryParse (blank, out allowBlankPassword))
					allowBlankPassword = false;
#else
				try {
					allowBlankPassword = Boolean.Parse (blank);
				}
				catch {
					allowBlankPassword = false;
				}
#endif
			}

			if (securityElement.Children != null) {
				foreach (SecurityElement child in securityElement.Children) {
					string connect = child.Attribute ("ConnectionString");
					string restricts = child.Attribute ("KeyRestrictions");
					KeyRestrictionBehavior behavior = (KeyRestrictionBehavior) Enum.Parse (
						typeof (KeyRestrictionBehavior), child.Attribute ("KeyRestrictionBehavior"));

					if ((connect != null) && (connect.Length > 0))
						Add (connect, restricts, behavior);
				}
			}
		}

		[MonoTODO ("restrictions not completely implemented - nor documented")]
		public override IPermission Intersect (IPermission target) 
		{
			DBDataPermission dbdp = Cast (target);
			if (dbdp == null)
				return null;
			if (IsUnrestricted ()) {
				if (dbdp.IsUnrestricted ()) {
					DBDataPermission u = CreateInstance ();
					u.state = PermissionState.Unrestricted;
					return u;
				}
				return dbdp.Copy ();
			}
			if (dbdp.IsUnrestricted ())
				return Copy ();
			if (IsEmpty () || dbdp.IsEmpty ())
				return null;

			DBDataPermission p = CreateInstance ();
			p.allowBlankPassword = (allowBlankPassword && dbdp.allowBlankPassword);
			foreach (DictionaryEntry de in _connections) {
				object o = dbdp._connections [de.Key];
				if (o != null)
					p._connections.Add (de.Key, de.Value);
			}
			return (p._connections.Count > 0) ? p : null;
		}

		[MonoTODO ("restrictions not completely implemented - nor documented")]
		public override bool IsSubsetOf (IPermission target) 
		{
			DBDataPermission dbdp = Cast (target);
			if (dbdp == null)
				return IsEmpty ();
			if (dbdp.IsUnrestricted ())
				return true;
			if (IsUnrestricted ())
				return dbdp.IsUnrestricted ();

			if (allowBlankPassword && !dbdp.allowBlankPassword)
				return false;
			if (_connections.Count > dbdp._connections.Count)
				return false;

			foreach (DictionaryEntry de in _connections) {
				object o = dbdp._connections [de.Key];
				if (o == null)
					return false;
				// FIXME: this is a subset of what is required
				// it seems that we must process both the connect string
				// and the restrictions - but this has other effects :-/
			}
			return true;
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

		[MonoTODO ("restrictions not completely implemented - nor documented")]
		public override IPermission Union (IPermission target) 
		{
			DBDataPermission dbdp = Cast (target);
			if (dbdp == null)
				return Copy ();
			if (IsEmpty () && dbdp.IsEmpty ())
				return null;

			DBDataPermission p = CreateInstance ();
			if (IsUnrestricted () || dbdp.IsUnrestricted ()) {
				p.state = PermissionState.Unrestricted;
			}
			else {
				p.allowBlankPassword = (allowBlankPassword || dbdp.allowBlankPassword);
				p._connections = new Hashtable (_connections.Count + dbdp._connections.Count);
				foreach (DictionaryEntry de in _connections)
					p._connections.Add (de.Key, de.Value);
				// don't duplicate
				foreach (DictionaryEntry de in dbdp._connections)
					p._connections [de.Key] = de.Value;
			}
			return p;
		}

		// helpers

		private bool IsEmpty ()
		{
			return ((state != PermissionState.Unrestricted) && (_connections.Count == 0));
		}

		private DBDataPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			DBDataPermission dbdp = (target as DBDataPermission);
			if (dbdp == null) {
				PermissionHelper.ThrowInvalidPermission (target, this.GetType ());
			}

			return dbdp;
		}

		#endregion // Methods
	}
}
