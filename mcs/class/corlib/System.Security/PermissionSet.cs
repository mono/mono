//
// System.Security.PermissionSet.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) Nick Drochak
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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

		public PermissionSet (PermissionState state)
		{
			if (!Enum.IsDefined(typeof(System.Security.Permissions.PermissionState), state))
				throw new System.ArgumentException(); // state is not a valid System.Security.Permissions.PermissionState value.
			this.state = state;
			list = new ArrayList ();
		}

		public PermissionSet (PermissionSet permSet) : this (PermissionState.Unrestricted)
		{
			// LAMESPEC: This would be handled by the compiler.  No way permSet is not a PermissionSet.
			//if (!(permSet is PermissionSet))
			//	throw new System.ArgumentException(); // permSet is not an instance of System.Security.PermissionSet.
			if (permSet != null) {
				foreach (IPermission p in permSet.list)
					list.Add (p);
			}
		}

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

		[MonoTODO()]
		public virtual PermissionSet Copy ()
		{
			return null;
		}

		public virtual void CopyTo (Array array, int index)
		{
			if (array.Rank > 1)
				throw new System.ArgumentException("Array has more than one dimension"); // array has more than one dimension.
			if (index < 0 || index >= array.Length)
				throw new System.IndexOutOfRangeException(); // index is outside the range of allowable values for array.
			if (null == array)
				throw new System.ArgumentNullException(); // array is null.
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
			if (!(et.Attributes ["class"] as string).StartsWith (className))
				throw new ArgumentException ("not " + className);
			if ((et.Attributes ["version"] as string) != "1")
				throw new ArgumentException ("wrong version");

			if ((et.Attributes ["Unrestricted"] as string) == "true")
				state = PermissionState.Unrestricted;
			else
				state = PermissionState.None;
		}

		public virtual void FromXml (SecurityElement et)
		{
			FromXml (et, "System.Security.PermissionSet");
			foreach (SecurityElement se in et.Children) {
				string className = (se.Attributes ["class"] as string);
				Type classType = Type.GetType (className);
				IPermission p = (IPermission) Activator.CreateInstance (classType);
				p.FromXml (se);
				list.Add (p);
			}
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		[MonoTODO()]
		public virtual bool IsSubsetOf (PermissionSet target)
		{
			return false;
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

		[MonoTODO()]
		public virtual PermissionSet Intersect (PermissionSet other) 
		{
			return null;
		}

		public virtual bool IsEmpty () 
		{
			// note: Unrestricted isn't empty
			return ((state == PermissionState.Unrestricted) ? false : (list.Count == 0));
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

		[MonoTODO()]
		public virtual PermissionSet Union (PermissionSet other)
		{
			return null;
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public virtual bool IsReadOnly {
			get { return false; } // always false
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		[MonoTODO()]
		void IDeserializationCallback.OnDeserialization (object sender) 
		{
		}
	}
}
