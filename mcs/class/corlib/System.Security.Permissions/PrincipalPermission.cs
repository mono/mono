//
// System.Security.Permissions.PrincipalPermission.cs
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
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
using System.Security.Principal;
using System.Threading;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class PrincipalPermission : IPermission, IUnrestrictedPermission, IBuiltInPermission {

		private const int version = 1;

		internal class PrincipalInfo {

			private string _name;
			private string _role;
			private bool _isAuthenticated;
			
			public PrincipalInfo (string name, string role, bool isAuthenticated)
			{
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
			if (CodeAccessPermission.CheckPermissionState (state, true) == PermissionState.Unrestricted) {
				PrincipalInfo pi = new PrincipalInfo (null, null, true);
				principals.Add (pi);
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
			// General validation in CodeAccessPermission
			CodeAccessPermission.CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			principals.Clear ();
			// Children is null, not empty, when no child is present
			if (esd.Children != null) {
				foreach (SecurityElement se in esd.Children) {
					if (se.Tag != "Identity")
						throw new ArgumentException ("not IPermission/Identity");
					string name = se.Attribute ("Name");
					string role = se.Attribute ("Role");
					string auth = se.Attribute ("Authenticated");
					bool isAuthenticated = false;
					if (auth != null) {
						try {
							isAuthenticated = Boolean.Parse (auth);
						}
						catch {}
					}
					PrincipalInfo pi = new PrincipalInfo (name, role, isAuthenticated);
					principals.Add (pi);
				}
			}
		}

		public IPermission Intersect (IPermission target) 
		{
			PrincipalPermission pp = Cast (target);
			if (pp == null)
				return null;

			if (IsUnrestricted ())
				return pp.Copy ();
			if (pp.IsUnrestricted ())
				return Copy ();

			PrincipalPermission intersect = new PrincipalPermission (PermissionState.None);
			foreach (PrincipalInfo pi in principals) {
				foreach (PrincipalInfo opi in pp.principals) {
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
			PrincipalPermission pp = Cast (target);
			if (pp == null)
				return false;

			if (IsUnrestricted ())
				return pp.IsUnrestricted ();
			else if (pp.IsUnrestricted ())
				return true;

			// each must be a subset of the target
			foreach (PrincipalInfo pi in principals) {
				bool thisItem = false;
				foreach (PrincipalInfo opi in pp.principals) {
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
			se.AddAttribute ("class", type.FullName + ", " + type.Assembly.ToString ().Replace ('\"', '\''));
			se.AddAttribute ("version", version.ToString ());

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
			PrincipalPermission pp = Cast (target);
			if (pp == null)
				return Copy ();

			if (IsUnrestricted () || pp.IsUnrestricted ())
				return new PrincipalPermission (PermissionState.Unrestricted);

			PrincipalPermission union = new PrincipalPermission (principals);
			foreach (PrincipalInfo pi in pp.principals)
				union.principals.Add (pi);

			return union;
		}

#if NET_2_0
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;

			PrincipalPermission pp = (obj as PrincipalPermission);
			if (pp == null)
				return false;

			// same number of principals ?
			if (principals.Count != pp.principals.Count)
				return false;

			// then all principals in "this" should be in "pp"
			foreach (PrincipalInfo pi in principals) {
				bool thisItem = false;
				foreach (PrincipalInfo opi in pp.principals) {
					if (((pi.Name == opi.Name) || (opi.Name == null)) && 
						((pi.Role == opi.Role) || (opi.Role == null)) && 
						(pi.IsAuthenticated == opi.IsAuthenticated)) {
						thisItem = true;
						break;
					}
				}
				if (!thisItem)
					return false;
			}
			return true;
		}

		// according to documentation (fx 2.0 beta 1) we can have 
		// different hash code even if both a Equals
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
#endif

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.Principal;
		}

		// helpers

		private PrincipalPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			PrincipalPermission pp = (target as PrincipalPermission);
			if (pp == null) {
				CodeAccessPermission.ThrowInvalidPermission (target, typeof (PrincipalPermission));
			}

			return pp;
		}
	}
}
