// 
// System.Security.Permissions.FileIOPermission.cs 
//
// Authors:
//	Nick Drochak, ndrochak@gol.com
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
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
using System.IO;
using System.Text;

#if NET_2_0
using System.Security.AccessControl;
#endif

namespace System.Security.Permissions {

	[Serializable]
	public sealed class FileIOPermission
                : CodeAccessPermission, IBuiltInPermission, IUnrestrictedPermission {

		private const int version = 1;
		private static char[] m_badCharacters = {'\"','<', '>', '|', '*', '?'};

		private bool m_Unrestricted = false;
		private FileIOPermissionAccess m_AllFilesAccess = FileIOPermissionAccess.NoAccess;
		private FileIOPermissionAccess m_AllLocalFilesAccess = FileIOPermissionAccess.NoAccess;
		private ArrayList readList;
		private ArrayList writeList;
		private ArrayList appendList;
		private ArrayList pathList;

		public FileIOPermission (PermissionState state)
		{
			if (CheckPermissionState (state, true) == PermissionState.Unrestricted) {
				m_Unrestricted = true;
				m_AllFilesAccess = FileIOPermissionAccess.AllAccess;
				m_AllLocalFilesAccess = FileIOPermissionAccess.AllAccess;
			}
			CreateLists ();
		}

		public FileIOPermission (FileIOPermissionAccess access, string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			CreateLists ();
			// access and path will be validated in AddPathList
			AddPathList (access, path);
		}

		public FileIOPermission (FileIOPermissionAccess access, string[] pathList)
		{
			if (pathList == null)
				throw new ArgumentNullException ("pathList");

			CreateLists ();
			// access and path will be validated in AddPathList
			AddPathList (access, pathList);
		}

		internal void CreateLists ()
		{
			readList = new ArrayList ();
			writeList = new ArrayList ();
			appendList = new ArrayList ();
			pathList = new ArrayList ();
		}

#if NET_2_0
		[MonoTODO ("Access Control isn't implemented")]
		public FileIOPermission (FileIOPermissionAccess access, AccessControlActions control, string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Access Control isn't implemented")]
		public FileIOPermission (FileIOPermissionAccess access, AccessControlActions control, string[] pathList)
		{
			throw new NotImplementedException ();
		}
#endif

		public FileIOPermissionAccess AllFiles {
			get { return m_AllFilesAccess; } 
			set {
				// if we are already set to unrestricted, don't change this property
				if (!m_Unrestricted){
					m_AllFilesAccess = value;
				}
			}
		}

		public FileIOPermissionAccess AllLocalFiles {
			get { return m_AllLocalFilesAccess; } 
			set {
				// if we are already set to unrestricted, don't change this property
				if (!m_Unrestricted){
					m_AllLocalFilesAccess = value;
				}
			}
		}

		public void AddPathList (FileIOPermissionAccess access, string path)
		{
                        if ((FileIOPermissionAccess.AllAccess & access) != access)
				ThrowInvalidFlag (access, true);
			ThrowIfInvalidPath (path);
			AddPathInternal (access, path);
		}

		public void AddPathList (FileIOPermissionAccess access, string[] pathList)
		{
                        if ((FileIOPermissionAccess.AllAccess & access) != access)
				ThrowInvalidFlag (access, true);
			ThrowIfInvalidPath (pathList);
			foreach (string path in pathList) {
				AddPathInternal (access, path);
			}
		}

		// internal to avoid duplicate checks
		internal void AddPathInternal (FileIOPermissionAccess access, string path)
		{
			path = Path.GetFullPath (path);
/*			switch (access) {
				case FileIOPermissionAccess.AllAccess:
					readList.Add (path);
					writeList.Add (path);
					appendList.Add (path);
					pathList.Add (path);
					break;
				case FileIOPermissionAccess.NoAccess:
					// ??? unit tests doesn't show removal using NoAccess ???
					break;
				case FileIOPermissionAccess.Read:
					readList.Add (path);
					break;
				case FileIOPermissionAccess.Write:
					writeList.Add (path);
					break;
				case FileIOPermissionAccess.Append:
					appendList.Add (path);
					break;
				case FileIOPermissionAccess.PathDiscovery:
					pathList.Add (path);
					break;
				default:
					ThrowInvalidFlag (access, true);
					break;
			}*/

			if ((access & FileIOPermissionAccess.Read) == FileIOPermissionAccess.Read)
				readList.Add (path);
			if ((access & FileIOPermissionAccess.Write) == FileIOPermissionAccess.Write)
				writeList.Add (path);
			if ((access & FileIOPermissionAccess.Append) == FileIOPermissionAccess.Append)
				appendList.Add (path);
			if ((access & FileIOPermissionAccess.PathDiscovery) == FileIOPermissionAccess.PathDiscovery)
				pathList.Add (path);
		}

		public override IPermission Copy ()
		{
			if (m_Unrestricted)
				return new FileIOPermission (PermissionState.Unrestricted);

			FileIOPermission copy = new FileIOPermission (PermissionState.None);
			copy.readList = (ArrayList) readList.Clone ();
			copy.writeList = (ArrayList) writeList.Clone ();
			copy.appendList = (ArrayList) appendList.Clone ();
			copy.pathList = (ArrayList) pathList.Clone ();
			copy.m_AllFilesAccess = m_AllFilesAccess;
			copy.m_AllLocalFilesAccess = m_AllLocalFilesAccess;
			return copy;
		}

		public override void FromXml (SecurityElement esd)
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			if (IsUnrestricted (esd)) {
				m_Unrestricted = true;
			}
			else{
				m_Unrestricted = false;
				string fileList = esd.Attribute ("Read");
				string[] files;
				if (fileList != null){
					files = fileList.Split (';');
					AddPathList (FileIOPermissionAccess.Read, files);
				}
				fileList = esd.Attribute ("Write");
				if (fileList != null){
					files = fileList.Split (';');
					AddPathList (FileIOPermissionAccess.Write, files);
				}
				fileList = esd.Attribute ("Append");
				if (fileList != null){
					files = fileList.Split (';');
					AddPathList (FileIOPermissionAccess.Append, files);
				}
				fileList = esd.Attribute ("PathDiscovery");
				if (fileList != null){
					files = fileList.Split (';');
					AddPathList (FileIOPermissionAccess.PathDiscovery, files);
				}
			}
		}

		public string[] GetPathList (FileIOPermissionAccess access)
		{
                        if ((FileIOPermissionAccess.AllAccess & access) != access)
				ThrowInvalidFlag (access, true);

			//LAMESPEC: docs says it returns (semicolon separated) list, but return
			//type is array.  I think docs are wrong and it just returns an array
			ArrayList result = new ArrayList ();
			switch (access) {
				case FileIOPermissionAccess.NoAccess:
					break;
				case FileIOPermissionAccess.Read:
					result.AddRange (readList);
					break;
				case FileIOPermissionAccess.Write:
					result.AddRange (writeList);
					break;
				case FileIOPermissionAccess.Append:
					result.AddRange (appendList);
					break;
				case FileIOPermissionAccess.PathDiscovery:
					result.AddRange (pathList);
					break;
				default:
					ThrowInvalidFlag (access, false);
					break;
			}
			return (result.Count > 0) ? (string[]) result.ToArray (typeof (string)) : null;
		}

		public override IPermission Intersect (IPermission target)
		{
			FileIOPermission fiop = Cast (target);
			if (fiop == null)
				return null;

			if (IsUnrestricted ())
				return fiop.Copy ();
			if (fiop.IsUnrestricted ())
				return Copy ();

			FileIOPermission result = new FileIOPermission (PermissionState.None);
			result.AllFiles = m_AllFilesAccess & fiop.AllFiles;
			result.AllLocalFiles = m_AllLocalFilesAccess & fiop.AllLocalFiles;

			IntersectKeys (readList, fiop.readList, result.readList);
			IntersectKeys (writeList, fiop.writeList, result.writeList);
			IntersectKeys (appendList, fiop.appendList, result.appendList);
			IntersectKeys (pathList, fiop.pathList, result.pathList);

			return (result.IsEmpty () ? null : result);
		}


		public override bool IsSubsetOf (IPermission target)
		{
			FileIOPermission fiop = Cast (target);
			if (fiop == null) 
				return false;
			if (fiop.IsEmpty ())
				return IsEmpty ();

			if (IsUnrestricted ())
				return fiop.IsUnrestricted ();
			else if (fiop.IsUnrestricted ())
				return true;

			if ((m_AllFilesAccess & fiop.AllFiles) != m_AllFilesAccess)
				return false;
			if ((m_AllLocalFilesAccess & fiop.AllLocalFiles) != m_AllLocalFilesAccess)
				return false;

			if (!KeyIsSubsetOf (appendList, fiop.appendList))
				return false;
			if (!KeyIsSubsetOf (readList, fiop.readList))
				return false;
			if (!KeyIsSubsetOf (writeList, fiop.writeList))
				return false;
			if (!KeyIsSubsetOf (pathList, fiop.pathList))
				return false;

			return true;
		}

		public bool IsUnrestricted ()
		{
			return m_Unrestricted;
		}

		public void SetPathList (FileIOPermissionAccess access, string path)
		{
                        if ((FileIOPermissionAccess.AllAccess & access) != access)
				ThrowInvalidFlag (access, true);
			ThrowIfInvalidPath (path);
			// note: throw before clearing the actual list
			Clear (access);
			AddPathInternal (access, path);
		}
		
		public void SetPathList (FileIOPermissionAccess access, string[] pathList)
		{
                        if ((FileIOPermissionAccess.AllAccess & access) != access)
				ThrowInvalidFlag (access, true);
			ThrowIfInvalidPath (pathList);
			// note: throw before clearing the actual list
			Clear (access);
			foreach (string path in pathList)
				AddPathInternal (access, path);
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = Element (1);
			if (m_Unrestricted) {
				se.AddAttribute ("Unrestricted", "true");
			}
			else {
				string[] paths = GetPathList (FileIOPermissionAccess.Append);
				if (null != paths && paths.Length > 0) {
					se.AddAttribute ("Append", String.Join (";", paths));
				}
				paths = GetPathList (FileIOPermissionAccess.Read);
				if (null != paths && paths.Length > 0) {
					se.AddAttribute ("Read", String.Join (";", paths));
				}
				paths = GetPathList (FileIOPermissionAccess.Write);
				if (null != paths && paths.Length > 0) {
					se.AddAttribute ("Write", String.Join  (";", paths));
				}
				paths = GetPathList (FileIOPermissionAccess.PathDiscovery);
				if (null != paths && paths.Length > 0) {
					se.AddAttribute ("PathDiscovery", String.Join  (";", paths));
				}
			}
			return se;
		}

		public override IPermission Union (IPermission other)
		{
			FileIOPermission fiop = Cast (other);
			if (fiop == null)
				return Copy ();

			if (IsUnrestricted () || fiop.IsUnrestricted ())
				return new FileIOPermission (PermissionState.Unrestricted);

			if (IsEmpty () && fiop.IsEmpty ())
				return null;

			FileIOPermission result = (FileIOPermission) Copy ();
			result.AllFiles |= fiop.AllFiles;
			result.AllLocalFiles |= fiop.AllLocalFiles;

			string[] paths = fiop.GetPathList (FileIOPermissionAccess.Read);
			if (paths != null) 
				UnionKeys (result.readList, paths);

			paths = fiop.GetPathList (FileIOPermissionAccess.Write);
			if (paths != null)
				UnionKeys (result.writeList, paths);

			paths = fiop.GetPathList (FileIOPermissionAccess.Append);
			if (paths != null) 
				UnionKeys (result.appendList, paths);

			paths = fiop.GetPathList (FileIOPermissionAccess.PathDiscovery);
			if (paths != null) 
				UnionKeys (result.pathList, paths);
			
			return result;
		}

#if NET_2_0
		[MonoTODO]
		public override bool Equals (object obj)
		{
			return false;
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
#endif

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.FileIO;
		}

		// helpers

		private bool IsEmpty ()
		{
			return ((!m_Unrestricted) && (appendList.Count == 0) && (readList.Count == 0)
				&& (writeList.Count == 0) && (pathList.Count == 0));
		}

		private FileIOPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			FileIOPermission fiop = (target as FileIOPermission);
			if (fiop == null) {
				ThrowInvalidPermission (target, typeof (FileIOPermission));
			}

			return fiop;
		}

		internal void ThrowInvalidFlag (FileIOPermissionAccess access, bool context) 
		{
			string msg = null;
			if (context)
				msg = Locale.GetText ("Unknown flag '{0}'.");
			else
				msg = Locale.GetText ("Invalid flag '{0}' in this context.");
			throw new ArgumentException (String.Format (msg, access), "access");
		}

		internal void ThrowIfInvalidPath (string path)
		{
			if (path.LastIndexOfAny (m_badCharacters) >= 0) {
				string msg = String.Format (Locale.GetText ("Invalid characters in path: '{0}'"), path);
				throw new ArgumentException (msg, "path");
			}
			// LAMESPEC: docs don't say it must be a rooted path, but the MS implementation enforces it, so we will too.
			if (!Path.IsPathRooted (path)) {
				string msg = Locale.GetText ("Absolute path information is required.");
				throw new ArgumentException (msg, "path");
			}
		}

		internal void ThrowIfInvalidPath (string[] paths)
		{
			foreach (string path in paths)
				ThrowIfInvalidPath (path);
		}

		// we known that access is valid at this point
		internal void Clear (FileIOPermissionAccess access)
		{
			if ((access & FileIOPermissionAccess.Read) == FileIOPermissionAccess.Read)
				readList.Clear ();
			if ((access & FileIOPermissionAccess.Write) == FileIOPermissionAccess.Write)
				writeList.Clear ();
			if ((access & FileIOPermissionAccess.Append) == FileIOPermissionAccess.Append)
				appendList.Clear ();
			if ((access & FileIOPermissionAccess.PathDiscovery) == FileIOPermissionAccess.PathDiscovery)
				pathList.Clear ();
		}

		internal bool KeyIsSubsetOf (IList local, IList target)
		{
			bool result = false;
			foreach (string l in local) {
				string c14nl = Path.GetFullPath (l);
				foreach (string t in target) {
					if (c14nl.StartsWith (Path.GetFullPath (t))) {
						result = true;
						break;
					}
				}
				if (!result)
					return false;
			}
			return true;
		}

		internal void UnionKeys (IList list, string[] paths)
		{
			foreach (string p in paths) {
				// c14n
				string path = Path.GetFullPath (p);
				int len = list.Count;
				if (len == 0) {
					list.Add (path);
				}
				else {
					for (int i=0; i < len; i++) {
						// c14n
						string s = Path.GetFullPath ((string) list [i]);
						if (s.StartsWith (path)) {
							// replace (with reduced version)
							list [i] = path;
							break;
						}
						else if (path.StartsWith (s)) {
							// no need to add
							break;
						}
						else {
							list.Add (path);
							break;
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
