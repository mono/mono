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

using System;
using System.Collections;
using System.Security.Permissions;
using System.Security;
using System.Runtime.Serialization;

namespace System.Security {

	[Serializable]
	public class PermissionSet: ISecurityEncodable, ICollection, IEnumerable, IStackWalk, IDeserializationCallback {

		private PermissionState state;
		private ArrayList list;
		private int _hashcode;

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

			IPermission existing = GetPermission (perm.GetType ());
			if (existing != null)
				perm = perm.Union (existing);

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
				throw new System.ArgumentNullException ("array"); // array is null.
			if (array.Rank > 1)
				throw new System.ArgumentException("Array has more than one dimension"); // array has more than one dimension.
			if (index < 0 || index >= array.Length)
				throw new System.IndexOutOfRangeException(); // index is outside the range of allowable values for array.
			list.CopyTo (array, index);
		}

		[MonoTODO()]
		public virtual void Demand ()
		{
		}

		[MonoTODO()]
		public virtual void Deny ()
		{
		}

		// to be re-used by NamedPermissionSet (and other derived classes)
		internal void FromXml (SecurityElement et, string className) 
		{
			if (et == null)
				throw new ArgumentNullException ("et");
			if (et.Tag != "PermissionSet")
				throw new ArgumentException ("not PermissionSet");
			if (!(et.Attributes ["class"] as string).EndsWith (className))
				throw new ArgumentException ("not " + className);
// version isn't checked
//			if ((et.Attributes ["version"] as string) != "1")
//				throw new ArgumentException ("wrong version");

			if ((et.Attributes ["Unrestricted"] as string) == "true")
				state = PermissionState.Unrestricted;
			else
				state = PermissionState.None;
		}

		public virtual void FromXml (SecurityElement et)
		{
			list.Clear ();
			FromXml (et, "PermissionSet");
			if (et.Children != null) {
				foreach (SecurityElement se in et.Children) {
					string className = (se.Attributes ["class"] as string);
					Type classType = Type.GetType (className);
					object [] psNone = new object [1] { PermissionState.None };
					IPermission p = (IPermission) Activator.CreateInstance (classType, psNone);
					p.FromXml (se);
					list.Add (p);
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
			// if we're unrestricted then target must also be unrestricted
			if (this.IsUnrestricted () && target.IsUnrestricted ())
				return true;

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
				if (! p.GetType ().IsSubclassOf (typeof(CodeAccessPermission)))
					return true;
			}
			return false;
		}

		// undocumented behavior
		[MonoTODO()]
		public static byte[] ConvertPermissionSet (string inFormat, byte[] inData, string outFormat) 
		{
			return null;
		}

		public virtual IPermission GetPermission (Type permClass) 
		{
			foreach (object o in list) {
				if (o.GetType ().Equals (permClass))
					return (IPermission) o;
			}
			return null;
		}

		public virtual PermissionSet Intersect (PermissionSet other) 
		{
			// no intersection possible
			if ((other == null) || (other.IsEmpty ()) || (this.IsEmpty ()))
				return new PermissionSet (PermissionState.None);
			// intersections with unrestricted
			if (this.IsUnrestricted ())
				return other.Copy ();
			if (other.IsUnrestricted ())
				return this.Copy ();

			PermissionSet interSet = new PermissionSet (PermissionState.None);
			foreach (IPermission p in other.list) {
				// for every type in both list
				IPermission i = interSet.GetPermission (p.GetType ());
				if (i != null) {
					// add intersection for this type
					interSet.AddPermission (p.Intersect (i));
				}
				// or reject!
			}
			return interSet;
		}

		public virtual bool IsEmpty () 
		{
			// note: Unrestricted isn't empty
			if (state == PermissionState.Unrestricted)
				return false;
			return ((list == null) || (list.Count == 0));
		}

		public virtual bool IsUnrestricted () 
		{
			return (state == PermissionState.Unrestricted);
		}

		public virtual IPermission RemovePermission (Type permClass) 
		{
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
			SecurityElement se = new SecurityElement ("PermissionSet");
			se.AddAttribute ("class", GetType ().FullName);
			se.AddAttribute ("version", "1");
			if (state == PermissionState.Unrestricted)
				se.AddAttribute ("Unrestricted", "true");
			else {
				foreach (IPermission p in list)
					se.AddChild (p.ToXml ());
			}
			return se;
		}

		public virtual PermissionSet Union (PermissionSet other)
		{
			if (other == null)
				return this.Copy ();
			if (this.IsUnrestricted () || other.IsUnrestricted ())
				return new PermissionSet (PermissionState.Unrestricted);
			
			PermissionSet copy = this.Copy ();
			foreach (IPermission p in other.list) {
				copy.AddPermission (p);
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
			get { return list.SyncRoot; }
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
	}
}
