//
// System.Security.Permissions.PrincipalPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
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

using System;
using System.Collections;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class PrincipalPermission : IPermission, IUnrestrictedPermission, IBuiltInPermission {

		internal class PrincipalInfo {

			private string _name;
			private string _role;
			private bool _isAuthenticated;
			
			public PrincipalInfo (string name, string role, bool isAuthenticated) {
				_name = name;
				_role = role;
				_isAuthenticated = isAuthenticated;
			}

			public string Name {
				get { return _name; }
			}

			public string Role {
				get { return _role; }
			}

			public bool IsAuthenticated {
				get { return _isAuthenticated; }
			}
		}

		private ArrayList principals;

		// Constructors

		public PrincipalPermission (PermissionState state)
		{
			principals = new ArrayList ();
			switch (state) {
				case PermissionState.None:
					break;
				case PermissionState.Unrestricted:
					PrincipalInfo pi = new PrincipalInfo (null, null, true);
					principals.Add (pi);
					break;
				default:
					throw new ArgumentException ("unknown PermissionState");
			}
		}

		public PrincipalPermission (string name, string role) : this (name, role, true)
		{
		}

		public PrincipalPermission (string name, string role, bool isAuthenticated)
		{
			principals = new ArrayList ();
			PrincipalInfo pi = new PrincipalInfo (name, role, isAuthenticated);
			principals.Add (pi);
		}

		internal PrincipalPermission (ArrayList principals) 
		{
			this.principals = (ArrayList) principals.Clone ();
		}

		// Properties

		// Methods

		public IPermission Copy () 
		{
			return new PrincipalPermission (principals);
		}

		public void Demand ()
		{
			IPrincipal p = Thread.CurrentPrincipal;
			if (p == null)
				throw new SecurityException ("no Principal");

			if (principals.Count > 0) {
				// check restrictions
				bool demand = false;
				foreach (PrincipalInfo pi in principals) {
					// if a name is present then it must be equal
					// if a role is present then the identity must be a member of this role
					// if authentication is required then the identity must be authenticated
					if (((pi.Name == null) || (pi.Name == p.Identity.Name)) &&
						((pi.Role == null) || (p.IsInRole (pi.Role))) &&
						((pi.IsAuthenticated && p.Identity.IsAuthenticated) || (!pi.IsAuthenticated))) {
						demand = true;
						break;
					}
				}

				if (!demand)
					throw new SecurityException ("invalid Principal");
			}
		}

		public void FromXml (SecurityElement esd) 
		{
			if (esd == null)
				throw new ArgumentNullException ("esd");
			if (esd.Tag != "IPermission")
				throw new ArgumentException ("not IPermission");
			if (!(esd.Attributes ["class"] as string).StartsWith ("System.Security.Permissions.PrincipalPermission"))
				throw new ArgumentException ("not PrincipalPermission");
			if ((esd.Attributes ["version"] as string) != "1")
				throw new ArgumentException ("wrong version");

			principals.Clear ();
			// Children is null, not empty, when no child is present
			if (esd.Children != null) {
				foreach (SecurityElement se in esd.Children) {
					if (se.Tag != "Identity")
						throw new ArgumentException ("not IPermission/Identity");
					string name = (se.Attributes ["Name"] as string);
					string role = (se.Attributes ["Role"] as string);
					bool isAuthenticated = ((se.Attributes ["Authenticated"] as string) == "true");
					PrincipalInfo pi = new PrincipalInfo (name, role, isAuthenticated);
					principals.Add (pi);
				}
			}
		}

		public IPermission Intersect (IPermission target) 
		{
			if (target == null)
				return null;
			if (! (target is PrincipalPermission))
				throw new ArgumentException ("wrong type");

			PrincipalPermission o = (PrincipalPermission) target;
			if (IsUnrestricted ())
				return o.Copy ();
			if (o.IsUnrestricted ())
				return Copy ();

			PrincipalPermission intersect = new PrincipalPermission (PermissionState.None);
			foreach (PrincipalInfo pi in principals) {
				foreach (PrincipalInfo opi in o.principals) {
					if (pi.IsAuthenticated == opi.IsAuthenticated) {
						string name = null;
						if ((pi.Name == opi.Name) || (opi.Name == null))
							name = pi.Name;
						string role = null;
						if ((pi.Role == opi.Role) || (opi.Role == null))
							role = pi.Role;
						if ((name != null) || (role != null)) {
							PrincipalInfo ipi = new PrincipalInfo (name, role, pi.IsAuthenticated);
							intersect.principals.Add (ipi);
						}
					}
				}
			}

			return ((intersect.principals.Count > 0) ? intersect : null);
		}

		public bool IsSubsetOf (IPermission target) 
		{
			if (target == null)
				return false;

			if (! (target is PrincipalPermission))
				throw new ArgumentException ("wrong type");

			PrincipalPermission o = (PrincipalPermission) target;
			if (IsUnrestricted ())
				return o.IsUnrestricted ();
			else if (o.IsUnrestricted ())
				return true;

			// each must be a subset of the target
			foreach (PrincipalInfo pi in principals) {
				bool thisItem = false;
				foreach (PrincipalInfo opi in o.principals) {
					if (((pi.Name == opi.Name) || (opi.Name == null)) && 
						((pi.Role == opi.Role) || (opi.Role == null)) && 
						(pi.IsAuthenticated == opi.IsAuthenticated))
						thisItem = true;
				}
				if (!thisItem)
					return false;
			}

			return true;
		}

		public bool IsUnrestricted () 
		{
			foreach (PrincipalInfo pi in principals) {
				if ((pi.Name == null) && (pi.Role == null) && (pi.IsAuthenticated))
					return true;
			}
			return false;
		}

		public override string ToString () 
		{
			return ToXml ().ToString ();
		}

		public SecurityElement ToXml () 
		{
			SecurityElement se = new SecurityElement ("IPermission");
			Type type = this.GetType ();
			StringBuilder asmName = new StringBuilder (type.Assembly.ToString ());
			asmName.Replace ('\"', '\'');
			se.AddAttribute ("class", type.FullName + ", " + asmName);
			se.AddAttribute ("version", "1");
			foreach (PrincipalInfo pi in principals) {
				SecurityElement sec = new SecurityElement ("Identity");
				if (pi.Name != null)
					sec.AddAttribute ("Name", pi.Name);
				if (pi.Role != null)
					sec.AddAttribute ("Role", pi.Role);
				if (pi.IsAuthenticated)
					sec.AddAttribute ("Authenticated", "true");
				se.AddChild (sec);
			}
			return se;
		}

		public IPermission Union (IPermission target)
		{
			if (target == null)
				return Copy ();
			if (! (target is PrincipalPermission))
				throw new ArgumentException ("wrong type");

			PrincipalPermission o = (PrincipalPermission) target;
			if (IsUnrestricted () || o.IsUnrestricted ())
				return new PrincipalPermission (PermissionState.Unrestricted);

			PrincipalPermission union = new PrincipalPermission (principals);
			foreach (PrincipalInfo pi in o.principals)
				union.principals.Add (pi);

			return union;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 8;
		}
	}
}
