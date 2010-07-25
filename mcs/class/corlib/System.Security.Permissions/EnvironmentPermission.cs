//
// System.Security.Permissions.EnvironmentPermission.cs
//
// Authors:
//	Tim Coleman <tim@timcoleman.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2002, Tim Coleman
// Portions Copyright (C) 2003 Motus Technologies (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Permissions {

	[ComVisible (true)]
	[Serializable]
	public sealed class EnvironmentPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		#region Fields

		private const int version = 1;

//		EnvironmentPermissionAccess flags;
		PermissionState _state;
		ArrayList readList;
		ArrayList writeList;

		#endregion // Fields

		#region Constructors

		public EnvironmentPermission (PermissionState state) : base ()
		{
			_state = CheckPermissionState (state, true);
			readList = new ArrayList ();
			writeList = new ArrayList ();
		}

		public EnvironmentPermission (EnvironmentPermissionAccess flag, string pathList) : base ()
		{
			readList = new ArrayList ();
			writeList = new ArrayList ();
			SetPathList (flag, pathList);
		}

		#endregion // Constructors

		#region Methods

		public void AddPathList (EnvironmentPermissionAccess flag, string pathList)
		{
			if (pathList == null)
				throw new ArgumentNullException ("pathList");

			string[] paths;
			switch (flag) {
				case EnvironmentPermissionAccess.AllAccess:
					paths = pathList.Split (';');
					foreach (string path in paths) {
						if (!readList.Contains (path))
							readList.Add (path);
						if (!writeList.Contains (path))
							writeList.Add (path);
					}
					break;
				case EnvironmentPermissionAccess.NoAccess:
					// ??? unit tests doesn't show removal using NoAccess ???
					break;
				case EnvironmentPermissionAccess.Read:
					paths = pathList.Split (';');
					foreach (string path in paths) {
						if (!readList.Contains (path))
							readList.Add (path);
					}
					break;
				case EnvironmentPermissionAccess.Write:
					paths = pathList.Split (';');
					foreach (string path in paths) {
						if (!writeList.Contains (path))
							writeList.Add (path);
					}
					break;
				default:
					ThrowInvalidFlag (flag, false);
					break;
			}
		}

		public override IPermission Copy ()
		{
			EnvironmentPermission ep = new EnvironmentPermission (_state);
			string path = GetPathList (EnvironmentPermissionAccess.Read);
			if (path != null)
				ep.SetPathList (EnvironmentPermissionAccess.Read, path);
			path = GetPathList (EnvironmentPermissionAccess.Write);
			if (path != null)
				ep.SetPathList (EnvironmentPermissionAccess.Write, path);
			return ep;
		}

		public override void FromXml (SecurityElement esd)
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			if (IsUnrestricted (esd))
				_state = PermissionState.Unrestricted;

			string read = esd.Attribute ("Read");
			if ((read != null) && (read.Length > 0))
				SetPathList (EnvironmentPermissionAccess.Read, read);

			string write = esd.Attribute ("Write");
			if ((write != null) && (write.Length > 0))
				SetPathList (EnvironmentPermissionAccess.Write, write);
		}

		public string GetPathList (EnvironmentPermissionAccess flag)
		{
			switch (flag) {
				case EnvironmentPermissionAccess.AllAccess:
				case EnvironmentPermissionAccess.NoAccess:
					ThrowInvalidFlag (flag, true);
					break;
				case EnvironmentPermissionAccess.Read:
					return GetPathList (readList);
				case EnvironmentPermissionAccess.Write:
					return GetPathList (writeList);
				default:
					ThrowInvalidFlag (flag, false);
					break;
			}
			return null; // never reached
		}

		public override IPermission Intersect (IPermission target)
		{
			EnvironmentPermission ep = Cast (target);
			if (ep == null)
				return null;

			if (IsUnrestricted ())
				return ep.Copy ();
			if (ep.IsUnrestricted ())
				return Copy ();

			int n = 0;
			EnvironmentPermission result = new EnvironmentPermission (PermissionState.None);
			string readTarget = ep.GetPathList (EnvironmentPermissionAccess.Read);
			if (readTarget != null) {
				string[] targets = readTarget.Split (';');
				foreach (string t in targets) {
					if (readList.Contains (t)) {
						result.AddPathList (EnvironmentPermissionAccess.Read, t);
						n++;
					}
				}
			}

			string writeTarget = ep.GetPathList (EnvironmentPermissionAccess.Write);
			if (writeTarget != null) {
				string[] targets = writeTarget.Split (';');
				foreach (string t in targets) {
					if (writeList.Contains (t)) {
						result.AddPathList (EnvironmentPermissionAccess.Write, t);
						n++;
					}
				}
			}
			return ((n > 0) ? result : null);
		}

		public override bool IsSubsetOf (IPermission target)
		{
			EnvironmentPermission ep = Cast (target);
			if (ep == null)
				return false;

			if (IsUnrestricted ())
				return ep.IsUnrestricted ();
			else if (ep.IsUnrestricted ())
				return true;

			foreach (string s in readList) {
				if (!ep.readList.Contains (s))
					return false;
			}

			foreach (string s in writeList) {
				if (!ep.writeList.Contains (s))
					return false;
			}

			return true;
		}

		public bool IsUnrestricted ()
		{
			return (_state == PermissionState.Unrestricted);
		}

		public void SetPathList (EnvironmentPermissionAccess flag, string pathList)
		{
			if (pathList == null)
				throw new ArgumentNullException ("pathList");
			string[] paths;
			switch (flag) {
				case EnvironmentPermissionAccess.AllAccess:
					readList.Clear ();
					writeList.Clear ();
					paths = pathList.Split (';');
					foreach (string path in paths) {
						readList.Add (path);
						writeList.Add (path);
					}
					break;
				case EnvironmentPermissionAccess.NoAccess:
					// ??? unit tests doesn't show removal using NoAccess ???
					break;
				case EnvironmentPermissionAccess.Read:
					readList.Clear ();
					paths = pathList.Split (';');
					foreach (string path in paths) {
						readList.Add (path);
					}
					break;
				case EnvironmentPermissionAccess.Write:
					writeList.Clear ();
					paths = pathList.Split (';');
					foreach (string path in paths) {
						writeList.Add (path);
					}
					break;
				default:
					ThrowInvalidFlag (flag, false);
					break;
			}
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = Element (version);

			if (_state == PermissionState.Unrestricted) {
				se.AddAttribute ("Unrestricted", "true");
			}
			else {
				string path = GetPathList (EnvironmentPermissionAccess.Read);
				if (path != null)
					se.AddAttribute ("Read", path);
				path = GetPathList (EnvironmentPermissionAccess.Write);
				if (path != null)
					se.AddAttribute ("Write", path);
			}
			return se;
		}

		public override IPermission Union (IPermission other)
		{
			EnvironmentPermission ep = Cast (other);
			if (ep == null)
				return Copy ();

			if (IsUnrestricted () || ep.IsUnrestricted ())
				return new EnvironmentPermission (PermissionState.Unrestricted);

			if (IsEmpty () && ep.IsEmpty ())
				return null;

			EnvironmentPermission result = (EnvironmentPermission) Copy ();
			string path = ep.GetPathList (EnvironmentPermissionAccess.Read);
			if (path != null) 
				result.AddPathList (EnvironmentPermissionAccess.Read, path);
			path = ep.GetPathList (EnvironmentPermissionAccess.Write);
			if (path != null)
				result.AddPathList (EnvironmentPermissionAccess.Write, path);
			return result;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.Environment;
		}

		// helpers

		private bool IsEmpty ()
		{
			return ((_state == PermissionState.None) && (readList.Count == 0) && (writeList.Count == 0));
		}

		private EnvironmentPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			EnvironmentPermission ep = (target as EnvironmentPermission);
			if (ep == null) {
				ThrowInvalidPermission (target, typeof (EnvironmentPermission));
			}

			return ep;
		}

		internal void ThrowInvalidFlag (EnvironmentPermissionAccess flag, bool context) 
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
			if (list.Count == 0)
				return String.Empty;
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

			return String.Empty;
		}

		#endregion // Methods
	}
}
