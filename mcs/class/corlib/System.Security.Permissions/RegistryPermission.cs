//
// System.Security.Permissions.RegistryPermission.cs
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
using System.Globalization;
using System.Text;

#if NET_2_0
using System.Security.AccessControl;
#endif

namespace System.Security.Permissions {

	[Serializable]
	public sealed class RegistryPermission
		: CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private const int version = 1;

		private PermissionState _state;
		private RegistryPermissionAccess _access;
		private ArrayList createList;
		private ArrayList readList;
		private ArrayList writeList;
#if NET_2_0
		private AccessControlActions _control;
#endif
		// Constructors

		public RegistryPermission (PermissionState state)
		{
			_state = CheckPermissionState (state, true);
			createList = new ArrayList ();
			readList = new ArrayList ();
			writeList = new ArrayList ();
		}

		public RegistryPermission (RegistryPermissionAccess access, string pathList)
		{
			_state = PermissionState.None;
			createList = new ArrayList ();
			readList = new ArrayList ();
			writeList = new ArrayList ();
			AddPathList (access, pathList);
		}
#if NET_2_0
		public RegistryPermission (RegistryPermissionAccess access, AccessControlActions control, string pathList)
		{
			if (!Enum.IsDefined (typeof (AccessControlActions), control)) {
				string msg = String.Format (Locale.GetText ("Invalid enum {0}"), control);
				throw new ArgumentException (msg, "AccessControlActions");
			}
			_state = PermissionState.None;
			AddPathList (access, control, pathList);
		}
#endif
		// Properties

		// Methods

		public void AddPathList (RegistryPermissionAccess access, string pathList) 
		{
			if (pathList == null)
				throw new ArgumentNullException ("pathList");

			string[] paths;
			switch (access) {
				case RegistryPermissionAccess.AllAccess:
					AddWithUnionKey (createList, pathList);
					AddWithUnionKey (readList, pathList);
					AddWithUnionKey (writeList, pathList);
					break;
				case RegistryPermissionAccess.NoAccess:
					// ??? unit tests doesn't show removal using NoAccess ???
					break;
				case RegistryPermissionAccess.Create:
					AddWithUnionKey (createList, pathList);
					break;
				case RegistryPermissionAccess.Read:
					AddWithUnionKey (readList, pathList);
					break;
				case RegistryPermissionAccess.Write:
					AddWithUnionKey (writeList, pathList);
					break;
				default:
					ThrowInvalidFlag (access, false);
					break;
			}
		}
#if NET_2_0
		[MonoTODO]
		public void AddPathList (RegistryPermissionAccess access, AccessControlActions control, string pathList) 
		{
		}
#endif
		public string GetPathList (RegistryPermissionAccess access)
		{
			switch (access) {
				case RegistryPermissionAccess.AllAccess:
				case RegistryPermissionAccess.NoAccess:
					ThrowInvalidFlag (access, true);
					break;
				case RegistryPermissionAccess.Create:
					return GetPathList (createList);
				case RegistryPermissionAccess.Read:
					return GetPathList (readList);
				case RegistryPermissionAccess.Write:
					return GetPathList (writeList);
				default:
					ThrowInvalidFlag (access, false);
					break;
			}
			return null; // never reached
		}

		public void SetPathList (RegistryPermissionAccess access, string pathList)
		{
			if (pathList == null)
				throw new ArgumentNullException ("pathList");

			string[] paths;
			switch (access) {
				case RegistryPermissionAccess.AllAccess:
					createList.Clear ();
					readList.Clear ();
					writeList.Clear ();
					paths = pathList.Split (';');
					foreach (string path in paths) {
						createList.Add (path);
						readList.Add (path);
						writeList.Add (path);
					}
					break;
				case RegistryPermissionAccess.NoAccess:
					// ??? unit tests doesn't show removal using NoAccess ???
					break;
				case RegistryPermissionAccess.Create:
					createList.Clear ();
					paths = pathList.Split (';');
					foreach (string path in paths) {
						createList.Add (path);
					}
					break;
				case RegistryPermissionAccess.Read:
					readList.Clear ();
					paths = pathList.Split (';');
					foreach (string path in paths) {
						readList.Add (path);
					}
					break;
				case RegistryPermissionAccess.Write:
					writeList.Clear ();
					paths = pathList.Split (';');
					foreach (string path in paths) {
						writeList.Add (path);
					}
					break;
				default:
					ThrowInvalidFlag (access, false);
					break;
			}
		}

		public override IPermission Copy () 
		{
			RegistryPermission rp = new RegistryPermission (_state);

			string path = GetPathList (RegistryPermissionAccess.Create);
			if (path != null)
				rp.SetPathList (RegistryPermissionAccess.Create, path);

			path = GetPathList (RegistryPermissionAccess.Read);
			if (path != null)
				rp.SetPathList (RegistryPermissionAccess.Read, path);

			path = GetPathList (RegistryPermissionAccess.Write);
			if (path != null)
				rp.SetPathList (RegistryPermissionAccess.Write, path);
			return rp;
		}

		public override void FromXml (SecurityElement esd) 
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			if (IsUnrestricted (esd))
				_state = PermissionState.Unrestricted;

			string create = esd.Attribute ("Create");
			if ((create != null) && (create.Length > 0))
				SetPathList (RegistryPermissionAccess.Create, create);

			string read = esd.Attribute ("Read");
			if ((read != null) && (read.Length > 0))
				SetPathList (RegistryPermissionAccess.Read, read);

			string write = esd.Attribute ("Write");
			if ((write != null) && (write.Length > 0))
				SetPathList (RegistryPermissionAccess.Write, write);
		}

		public override IPermission Intersect (IPermission target) 
		{
			RegistryPermission rp = Cast (target);
			if (rp == null)
				return null;

			if (IsUnrestricted ())
				return rp.Copy ();
			if (rp.IsUnrestricted ())
				return Copy ();

			RegistryPermission result = new RegistryPermission (PermissionState.None);

			IntersectKeys (createList, rp.createList, result.createList);
			IntersectKeys (readList, rp.readList, result.readList);
			IntersectKeys (writeList, rp.writeList, result.writeList);

			return (result.IsEmpty () ? null : result);
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			RegistryPermission rp = Cast (target);
			if (rp == null) 
				return false;
			if (rp.IsEmpty ())
				return IsEmpty ();

			if (IsUnrestricted ())
				return rp.IsUnrestricted ();
			else if (rp.IsUnrestricted ())
				return true;

			if (!KeyIsSubsetOf (createList, rp.createList))
				return false;
			if (!KeyIsSubsetOf (readList, rp.readList))
				return false;
			if (!KeyIsSubsetOf (writeList, rp.writeList))
				return false;

			return true;
		}

		public bool IsUnrestricted () 
		{
			return (_state == PermissionState.Unrestricted);
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement se = Element (version);

			if (_state == PermissionState.Unrestricted) {
				se.AddAttribute ("Unrestricted", "true");
			}
			else {
				string path = GetPathList (RegistryPermissionAccess.Create);
				if (path != null)
					se.AddAttribute ("Create", path);
				path = GetPathList (RegistryPermissionAccess.Read);
				if (path != null)
					se.AddAttribute ("Read", path);
				path = GetPathList (RegistryPermissionAccess.Write);
				if (path != null)
					se.AddAttribute ("Write", path);
			}
			return se;
		}

		public override IPermission Union (IPermission target)
		{
			RegistryPermission rp = Cast (target);
			if (rp == null)
				return Copy ();

			if (IsUnrestricted () || rp.IsUnrestricted ())
				return new RegistryPermission (PermissionState.Unrestricted);

			if (IsEmpty () && rp.IsEmpty ())
				return null;

			RegistryPermission result = (RegistryPermission) Copy ();
			string path = rp.GetPathList (RegistryPermissionAccess.Create);
			if (path != null) 
				result.AddPathList (RegistryPermissionAccess.Create, path);
			path = rp.GetPathList (RegistryPermissionAccess.Read);
			if (path != null) 
				result.AddPathList (RegistryPermissionAccess.Read, path);
			path = rp.GetPathList (RegistryPermissionAccess.Write);
			if (path != null)
				result.AddPathList (RegistryPermissionAccess.Write, path);
			return result;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.Registry;
		}

		// helpers

		private bool IsEmpty ()
		{
			return ((_state == PermissionState.None) && (createList.Count == 0) &&
				(readList.Count == 0) && (writeList.Count == 0));
		}

		private RegistryPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			RegistryPermission rp = (target as RegistryPermission);
			if (rp == null) {
				ThrowInvalidPermission (target, typeof (RegistryPermission));
			}

			return rp;
		}

		internal void ThrowInvalidFlag (RegistryPermissionAccess flag, bool context) 
		{
			string msg = null;
			if (context)
				msg = Locale.GetText ("Unknown flag '{0}'.");
			else
				msg = Locale.GetText ("Invalid flag '{0}' in this context.");
			throw new ArgumentException (String.Format (msg, flag), "flag");
		}

		private string GetPathList (ArrayList list)
		{
			if (IsUnrestricted ())
				return String.Empty;
#if NET_2_0
			if (list.Count == 0)
				return String.Empty;
#else
			if (list.Count == 0)
				return null;
#endif
			StringBuilder sb = new StringBuilder ();
			foreach (string path in list) {
				sb.Append (path);
				sb.Append (";");
			}

			string result = sb.ToString ();
			// remove last ';'
			int n = result.Length;
			if (n > 0)
				return result.Substring (0, n - 1);
#if NET_2_0
			return String.Empty;
#else
			return ((_state == PermissionState.Unrestricted) ? String.Empty : null);
#endif
		}

		internal bool KeyIsSubsetOf (IList local, IList target)
		{
			bool result = false;
			foreach (string l in local) {
				foreach (string t in target) {
					if (l.StartsWith (t)) {
						result = true;
						break;
					}
				}
				if (!result)
					return false;
			}
			return true;
		}

		internal void AddWithUnionKey (IList list, string pathList)
		{
			string[] paths = pathList.Split (';');
			foreach (string path in paths) {
				int len = list.Count;
				if (len == 0) {
					list.Add (path);
				}
				else {
					for (int i=0; i < len; i++) {
						string s = (string) list [i];
						if (s.StartsWith (path)) {
							// replace (with reduced version)
							list [i] = path;
						}
						else if (path.StartsWith (s)) {
							// no need to add
						}
						else {
							list.Add (path);
						}
					}
				}
			}
		}

		internal void IntersectKeys (IList local, IList target, IList result)
		{
			foreach (string l in local) {
				foreach (string t in target) {
					if (t.Length > l.Length) {
						if (t.StartsWith (l))
							result.Add (t);
					}
					else {
						if (l.StartsWith (t))
							result.Add (l);
					}
				}
			}
		}
	}
}
