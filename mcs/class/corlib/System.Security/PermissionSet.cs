//
// System.Security.PermissionSet.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Nick Drochak
// Portions (C) 2003, 2004 Motus Technologies Inc. (http://www.motus.com)
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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.Security {

	[Serializable]
	public class PermissionSet: ISecurityEncodable, ICollection, IEnumerable, IStackWalk, IDeserializationCallback {

		private static string tagName = "PermissionSet";
		private const int version = 1;

		private PermissionState state;
		private ArrayList list;
		private int _hashcode;
		private PolicyLevel _policyLevel;

		// constructors

		// for PolicyLevel (to avoid validation duplication)
		internal PermissionSet () 
		{
			list = new ArrayList ();
		}

		public PermissionSet (PermissionState state) : this ()
		{
			if (!Enum.IsDefined (typeof (PermissionState), state))
				throw new System.ArgumentException ("state");
			this.state = state;
		}

		public PermissionSet (PermissionSet permSet) : this ()
		{
			// LAMESPEC: This would be handled by the compiler.  No way permSet is not a PermissionSet.
			//if (!(permSet is PermissionSet))
			//	throw new System.ArgumentException(); // permSet is not an instance of System.Security.PermissionSet.
			if (permSet == null)
				state = PermissionState.Unrestricted;
			else {
				state = permSet.state;
				foreach (IPermission p in permSet.list)
					list.Add (p);
			}
		}

		// methods

		public virtual IPermission AddPermission (IPermission perm)
		{
			if (perm == null)
				return null;

			// we don't add to an unrestricted permission set unless...
			if (state == PermissionState.Unrestricted) {
				// we're adding identity permission as they don't support unrestricted
				if (perm is IUnrestrictedPermission) {
					// we return the union of the permission with unrestricted
					// which results in a permission of the same type initialized 
					// with PermissionState.Unrestricted
					object[] args = new object [1] { PermissionState.Unrestricted };
					return (IPermission) Activator.CreateInstance (perm.GetType (), args);
				}
			}

			// we can't add two permissions of the same type in a set
			// so we remove an existing one, union with it and add it back
			IPermission existing = RemovePermission (perm.GetType ());
			if (existing != null) {
				perm = perm.Union (existing);
			}

			list.Add (perm);
			return perm;
		}

		[MonoTODO()]
		public virtual void Assert ()
		{
		}

		internal void Clear () 
		{
			list.Clear ();
		}

		public virtual PermissionSet Copy ()
		{
			return new PermissionSet (this);
		}

		public virtual void CopyTo (Array array, int index)
		{
			if (null == array)
				throw new ArgumentNullException ("array");

			if (list.Count > 0) {
				if (array.Rank > 1) {
					throw new ArgumentException (Locale.GetText (
						"Array has more than one dimension"));
				}
				if (index < 0 || index >= array.Length) {
					throw new IndexOutOfRangeException ("index");
				}

				list.CopyTo (array, index);
			}
		}

		[MonoTODO ("Assert, Deny and PermitOnly aren't yet supported")]
		public virtual void Demand ()
		{
			if (!SecurityManager.SecurityEnabled)
				return;

			// non CAS permissions (e.g. PrincipalPermission) do not requires a stack walk
			PermissionSet cas = this.Copy ();
			foreach (IPermission p in list) {
				Type t = p.GetType ();
				if (!t.IsSubclassOf (typeof (CodeAccessPermission))) {
					p.Demand ();
					// we wont have to process this one in the stack walk
					cas.RemovePermission (t);
				}
			}
			// don't start the walk if the permission set only contains non CAS permissions
			if (cas.Count == 0)
				return;

			Assembly a = null;
			StackTrace st = new StackTrace (1); // skip ourself
			StackFrame[] frames = st.GetFrames ();
			foreach (StackFrame sf in frames) {
				MethodBase mb = sf.GetMethod ();
				// declarative security checks, when present, must be checked
				// for each stack frame
				if ((MethodAttributes.HasSecurity & mb.Attributes) == MethodAttributes.HasSecurity) {
					// TODO
				}
				// however the "final" grant set is resolved by assembly, so
				// there's no need to check it every time (just when we're 
				// changing assemblies between frames).
				Assembly af = mb.ReflectedType.Assembly;
				if (a != af) {
					a = af;
					if (!a.Demand (cas)) {
						// TODO add more details
						throw new SecurityException ("Demand failed");
					}
				}
			}
		}

		[MonoTODO()]
		public virtual void Deny ()
		{
		}

		[MonoTODO ("adjust class version with current runtime - unification")]
		public virtual void FromXml (SecurityElement et)
		{
			if (et == null)
				throw new ArgumentNullException ("et");
			if (et.Tag != tagName) {
				string msg = String.Format ("Invalid tag {0} expected {1}", et.Tag, tagName);
				throw new ArgumentException (msg, "et");
			}

			if (CodeAccessPermission.IsUnrestricted (et))
				state = PermissionState.Unrestricted;
			else
				state = PermissionState.None;

			list.Clear ();
			if (et.Children != null) {
				foreach (SecurityElement se in et.Children) {
					string className = se.Attribute ("class");
					if (className == null) {
						throw new ArgumentException (Locale.GetText (
							"No permission class is specified."));
					}
					if (Resolver != null) {
						// policy class names do not have to be fully qualified
						className = Resolver.ResolveClassName (className);
					}
					// TODO: adjust class version with current runtime (unification)
					// http://blogs.msdn.com/shawnfa/archive/2004/08/05/209320.aspx
					Type classType = Type.GetType (className);
					if (classType != null) {
						object [] psNone = new object [1] { PermissionState.None };
						IPermission p = (IPermission) Activator.CreateInstance (classType, psNone);
						p.FromXml (se);
						list.Add (p);
					}
				}
			}
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public virtual bool IsSubsetOf (PermissionSet target)
		{
			// if target is empty we must be empty too
			if ((target == null) || (target.IsEmpty ()))
				return this.IsEmpty ();

			// TODO - non CAS permissions must be evaluated for unrestricted

			// if target is unrestricted then we are a subset
			if (!this.IsUnrestricted () && target.IsUnrestricted ())
				return true;
			// else target isn't unrestricted.
			// so if we are unrestricted, the we can't be a subset
			if (this.IsUnrestricted () && !target.IsUnrestricted ())
				return false;

			// if each of our permission is (a) present and (b) a subset of target
			foreach (IPermission p in list) {
				// for every type in both list
				IPermission i = target.GetPermission (p.GetType ());
				if (i == null)
					return false; // not present (condition a)
				if (!p.IsSubsetOf (i))
					return false; // not a subset (condition b)
			}
			return true;
		}

		[MonoTODO()]
		public virtual void PermitOnly ()
		{
		}

		public bool ContainsNonCodeAccessPermissions () 
		{
			foreach (IPermission p in list) {
				if (! p.GetType ().IsSubclassOf (typeof (CodeAccessPermission)))
					return true;
			}
			return false;
		}

		[MonoTODO ("little documentation in Fx 2.0 beta 1")]
		public static byte[] ConvertPermissionSet (string inFormat, byte[] inData, string outFormat) 
		{
			if (inFormat == null)
				throw new ArgumentNullException ("inFormat");
			if (outFormat == null)
				throw new ArgumentNullException ("outFormat");
			if (inData == null)
				return null;

			if (inFormat == outFormat)
				return inData;

			if (inFormat == "BINARY") {
				if (outFormat.StartsWith ("XML")) {
					// TODO - convert from binary format
					return inData;
				}
			}
			else if (inFormat.StartsWith ("XML")) {
				if (outFormat == "BINARY") {
					// TODO - convert to binary format
					return inData;
				}
				else if (outFormat.StartsWith ("XML")) {
					string msg = String.Format (Locale.GetText ("Can't convert from {0} to {1}"), inFormat, outFormat);
					throw new XmlSyntaxException (msg);
				}
			}
			else {
				// unknown inFormat, returns null
				return null;
			}
			// unknown outFormat, throw
			throw new SerializationException (String.Format (Locale.GetText ("Unknown output format {0}."), outFormat));
		}

		public virtual IPermission GetPermission (Type permClass) 
		{
			foreach (object o in list) {
				if (o.GetType ().Equals (permClass))
					return (IPermission) o;
			}
			// it's normal to return null for unrestricted sets
			return null;
		}

		public virtual PermissionSet Intersect (PermissionSet other) 
		{
			// no intersection possible
			if ((other == null) || (other.IsEmpty ()) || (this.IsEmpty ()))
				return null;

			PermissionState state = PermissionState.None;
			if (this.IsUnrestricted () && other.IsUnrestricted ())
				state = PermissionState.Unrestricted;

			PermissionSet interSet = new PermissionSet (state);
			if (state == PermissionState.Unrestricted) {
				InternalIntersect (interSet, this, other, true);
				InternalIntersect (interSet, other, this, true);
			}
			else if (this.IsUnrestricted ()) {
				InternalIntersect (interSet, this, other, true);
			}
			else if (other.IsUnrestricted ()) {
				InternalIntersect (interSet, other, this, true);
			}
			else {
				InternalIntersect (interSet, this, other, false);
			}
			return interSet;
		}

		internal void InternalIntersect (PermissionSet intersect, PermissionSet a, PermissionSet b, bool unrestricted)
		{
			foreach (IPermission p in b.list) {
				// for every type in both list
				IPermission i = a.GetPermission (p.GetType ());
				if (i != null) {
					// add intersection for this type
					intersect.AddPermission (p.Intersect (i));
				}
				else if (unrestricted && (p is IUnrestrictedPermission)) {
					intersect.AddPermission (p);
				}
				// or reject!
			}
		}

		public virtual bool IsEmpty () 
		{
			// note: Unrestricted isn't empty
			if (state == PermissionState.Unrestricted)
				return false;
			if ((list == null) || (list.Count == 0))
				return true;
			// the set may include some empty permissions
			foreach (IPermission p in list) {
				// an empty permission only has a class and/or version attributes
				SecurityElement se = p.ToXml ();
				int n = se.Attributes.Count;
				if (n <= 2) {
					if (se.Attribute ("class") != null)
						n--;
					if (se.Attribute ("version") != null)
						n--;
					if (n > 0)
						return false;	// not class or version - then not empty
				}
				else {
					// too much attributes - then not empty
					return false;
				}
			}
			return true;
		}

		public virtual bool IsUnrestricted () 
		{
			return (state == PermissionState.Unrestricted);
		}

		public virtual IPermission RemovePermission (Type permClass) 
		{
			if (permClass == null)
				return null;

			foreach (object o in list) {
				if (o.GetType ().Equals (permClass)) {
					list.Remove (o);
					return (IPermission) o;
				}
			}
			return null;
		}

		public virtual IPermission SetPermission (IPermission perm) 
		{
			if (perm == null)
				return null;
			if (perm is IUnrestrictedPermission)
				state = PermissionState.None;
			RemovePermission (perm.GetType ());
			list.Add (perm);
			return perm;
		}

		public override string ToString ()
		{
			return ToXml ().ToString ();
		}

		public virtual SecurityElement ToXml ()
		{
			SecurityElement se = new SecurityElement (tagName);
			se.AddAttribute ("class", GetType ().FullName);
			se.AddAttribute ("version", version.ToString ());
			if (state == PermissionState.Unrestricted)
				se.AddAttribute ("Unrestricted", "true");

			// required for permissions that do not implement IUnrestrictedPermission
			foreach (IPermission p in list) {
				se.AddChild (p.ToXml ());
			}
			return se;
		}

		public virtual PermissionSet Union (PermissionSet other)
		{
			if (other == null)
				return this.Copy ();

			PermissionSet copy = this.Copy ();
			if (this.IsUnrestricted () || other.IsUnrestricted ()) {
				// so we keep the "right" type
				copy.Clear ();
				copy.state = PermissionState.Unrestricted;
				// copy all permissions that do not implement IUnrestrictedPermission
				foreach (IPermission p in this.list) {
					if (!(p is IUnrestrictedPermission))
						copy.AddPermission (p);
				}
				foreach (IPermission p in other.list) {
					if (!(p is IUnrestrictedPermission))
						copy.AddPermission (p);
				}
			}
			else {
				// PermissionState.None -> copy all permissions
				foreach (IPermission p in other.list) {
					copy.AddPermission (p);
				}
			}
			return copy;
		}

		public virtual int Count {
			get { return list.Count; }
		}

		public virtual bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public virtual bool IsReadOnly {
			get { return false; } // always false
		}

		public virtual object SyncRoot {
			get { return this; }
		}

		[MonoTODO()]
		void IDeserializationCallback.OnDeserialization (object sender) 
		{
		}

#if NET_2_0
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			PermissionSet ps = (obj as PermissionSet);
			if (ps == null)
				return false;
			if (list.Count != ps.Count)
				return false;

			for (int i=0; i < list.Count; i++) {
				bool found = false;
				for (int j=0; i < ps.list.Count; j++) {
					if (list [i].Equals (ps.list [j])) {
						found = true;
						break;
					}
				}
				if (!found)
					return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			if (list.Count == 0)
				return (int) state;

			if (_hashcode == 0) {
				_hashcode = state.GetHashCode ();
				foreach (IPermission p in list)	{
					_hashcode ^= p.GetHashCode ();
				}
			}
			return _hashcode;
		}

		[MonoTODO ("what's it doing here?")]
		static public void RevertAssert ()
		{
		}
#endif

		// internal

		internal PolicyLevel Resolver {
			get { return _policyLevel; }
			set { _policyLevel = value; }
		}
	}
}
