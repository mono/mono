//
// System.Security.Permissions.EnvironmentPermission.cs
//
// Authors:
//	Tim Coleman <tim@timcoleman.com>
//	Sebastien Pouliot <spouliot@motus.com>
//
// Copyright (C) 2002, Tim Coleman
// Portions Copyright (C) 2003 Motus Technologies (http://www.motus.com)
//

using System;
using System.Collections;
using System.Security.Permissions;
using System.Text;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class EnvironmentPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		#region Fields

		EnvironmentPermissionAccess flags;
		PermissionState _state;
		ArrayList readList;
		ArrayList writeList;

		#endregion // Fields

		#region Constructors

		public EnvironmentPermission (PermissionState state) : base ()
		{
			switch (state) {
				case PermissionState.None:
				case PermissionState.Unrestricted:
					_state = state;
					readList = new ArrayList ();
					writeList = new ArrayList ();
					break;
				default:
					throw new ArgumentException ("Invalid PermissionState", "state");
			}
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
					throw new ArgumentException ("Invalid EnvironmentPermissionAccess", "flag");
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
			if (esd == null)
				throw new ArgumentNullException ("esd");
			if (esd.Tag != "IPermission")
				throw new ArgumentException ("not IPermission");
			if (!(esd.Attributes ["class"] as string).StartsWith ("System.Security.Permissions.EnvironmentPermission"))
				throw new ArgumentException ("not EnvironmentPermission");
			if ((esd.Attributes ["version"] as string) != "1")
				throw new ArgumentException ("wrong version");

			string read = (esd.Attributes ["Read"] as string);
			if ((read != null) && (read.Length > 0))
				SetPathList (EnvironmentPermissionAccess.Read, read);
			string write = (esd.Attributes ["Write"] as string);
			if ((write != null) && (write.Length > 0))
				SetPathList (EnvironmentPermissionAccess.Write, write);

			// Unrestricted ???
		}

		public string GetPathList (EnvironmentPermissionAccess flag)
		{
			StringBuilder sb = new StringBuilder ();
			switch (flag) {
				case EnvironmentPermissionAccess.AllAccess:
				case EnvironmentPermissionAccess.NoAccess:
					throw new ArgumentException ("Invalid EnvironmentPermissionAccess in context", "flag");
				case EnvironmentPermissionAccess.Read:
					foreach (string path in readList) {
						sb.Append (path);
						sb.Append (";");
					}
					break;
				case EnvironmentPermissionAccess.Write:
					foreach (string path in writeList) {
						sb.Append (path);
						sb.Append (";");
					}
					break;
				default:
					throw new ArgumentException ("Unknown EnvironmentPermissionAccess", "flag");
			}
			string result = sb.ToString ();
			// remove last ';'
			int n = result.Length;
			if (n > 0)
				return result.Substring (0, n - 1);
			return null;
		}

		public override IPermission Intersect (IPermission target)
		{
			if (target == null)
				return null;
			if (! (target is EnvironmentPermission))
				throw new ArgumentException ("wrong type");

			EnvironmentPermission o = (EnvironmentPermission) target;
			int n = 0;
			if (IsUnrestricted ())
				return o.Copy ();
			if (o.IsUnrestricted ())
				return Copy ();

			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			string readTarget = o.GetPathList (EnvironmentPermissionAccess.Read);
			if (readTarget != null) {
				string[] targets = readTarget.Split (';');
				foreach (string t in targets) {
					if (readList.Contains (t)) {
						ep.AddPathList (EnvironmentPermissionAccess.Read, t);
						n++;
					}
				}
			}

			string writeTarget = o.GetPathList (EnvironmentPermissionAccess.Write);
			if (writeTarget != null) {
				string[] targets = writeTarget.Split (';');
				foreach (string t in targets) {
					if (writeList.Contains (t)) {
						ep.AddPathList (EnvironmentPermissionAccess.Write, t);
						n++;
					}
				}
			}
			return ((n > 0) ? ep : null);
		}

		public override bool IsSubsetOf (IPermission target)
		{
			if (target == null)
				return false;

			if (! (target is EnvironmentPermission))
				throw new ArgumentException ("wrong type");

			EnvironmentPermission o = (EnvironmentPermission) target;
			if (IsUnrestricted ())
				return o.IsUnrestricted ();
			else if (o.IsUnrestricted ())
				return true;

			foreach (string s in readList) {
				if (!o.readList.Contains (s))
					return false;
			}

			foreach (string s in writeList) {
				if (!o.writeList.Contains (s))
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
					throw new ArgumentException ("Invalid EnvironmentPermissionAccess", "flag");
			}
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = Element (this, 1);
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
			if (other == null)
				return Copy ();
			if (! (other is EnvironmentPermission))
				throw new ArgumentException ("wrong type");

			EnvironmentPermission o = (EnvironmentPermission) other;
			if (IsUnrestricted () || o.IsUnrestricted ())
				return new EnvironmentPermission (PermissionState.Unrestricted);

			EnvironmentPermission ep = (EnvironmentPermission) Copy ();
			string path = o.GetPathList (EnvironmentPermissionAccess.Read);
			if (path != null) 
				ep.AddPathList (EnvironmentPermissionAccess.Read, path);
			path = o.GetPathList (EnvironmentPermissionAccess.Write);
			if (path != null)
				ep.AddPathList (EnvironmentPermissionAccess.Write, path);
			return ep;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 0;
		}

		#endregion // Methods
	}
}
