//------------------------------------------------------------------------------
// 
// System.Security.Permissions.FileIOPermission.cs 
//
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
// 
// Author:         Nick Drochak, ndrochak@gol.com
// Created:        2002-01-09 
//
//------------------------------------------------------------------------------

using System.Collections;
using System.Text;
using System.Security.Permissions;
using System.IO;

namespace System.Security.Permissions {

	[SerializableAttribute()]
	public sealed class FileIOPermission : CodeAccessPermission, IUnrestrictedPermission {

		private static char[] m_badCharacters = {'\"','<', '>', '|', '*', '?'};
		private bool m_Unrestricted = false;
		private Hashtable m_PathList = new Hashtable();
		private FileIOPermissionAccess m_AllFilesAccess = FileIOPermissionAccess.NoAccess;
		private FileIOPermissionAccess m_AllLocalFilesAccess = FileIOPermissionAccess.NoAccess;

		public FileIOPermission(PermissionState state) {
			if (!Enum.IsDefined(typeof(PermissionState), state)){
				throw new ArgumentException("Invalid permission state.", "state");
			}
			m_Unrestricted = (PermissionState.Unrestricted == state);
			if (m_Unrestricted) {
				m_AllFilesAccess = FileIOPermissionAccess.AllAccess;
				m_AllLocalFilesAccess = FileIOPermissionAccess.AllAccess;
			}
		}

		public FileIOPermission(FileIOPermissionAccess access, string path){
			if ((FileIOPermissionAccess.AllAccess & access) != access){
				throw new ArgumentException("Illegal enum value: "+access.ToString()+".");
			}

			if (path.LastIndexOfAny(m_badCharacters) >= 0){
				throw new ArgumentException("Illegal characters found in input.  Security checks can not contain wild card characters.", "path");
			}
			
			AddPathList(access, path);
		}

		public FileIOPermission(FileIOPermissionAccess access, string[] pathList){
			if ((FileIOPermissionAccess.AllAccess & access) != access){
				throw new ArgumentException("Illegal enum value: "+access.ToString()+".");
			}

			AddPathList(access, pathList);
		}

		public FileIOPermissionAccess AllFiles {
			get {
				return m_AllFilesAccess;
			} 
			set {
				// if we are already set to unrestricted, don't change this property
				if (!m_Unrestricted){
					m_AllFilesAccess = value;
				}
			}
		}

		public FileIOPermissionAccess AllLocalFiles {
			get {
				return m_AllLocalFilesAccess;
			} 
			set {
				// if we are already set to unrestricted, don't change this property
				if (!m_Unrestricted){
					m_AllLocalFilesAccess = value;
				}
			}
		}

		public void AddPathList(FileIOPermissionAccess access, string path){
			if ((FileIOPermissionAccess.AllAccess & access) != access){
				throw new ArgumentException("Illegal enum value: {0}.",access.ToString());
			}

			if (path.LastIndexOfAny(m_badCharacters) >= 0){
				throw new ArgumentException("Invalid characters in path: '{0}'", path);
			}

			// LAMESPEC: docs don't say it must be a rooted path, but the MS implementation enforces it, so we will too.
			if(!Path.IsPathRooted(path)) {
				throw new ArgumentException("Absolute path information is required.");
			}

			// don't add the same path twice, instead overwrite access entry for that path
			if (m_PathList.ContainsKey(path)) {
				FileIOPermissionAccess currentPermission = (FileIOPermissionAccess)m_PathList[path];
				currentPermission |= access;
				m_PathList[path] = currentPermission;
			}
			else {
				m_PathList.Add(path, access);
			}
		}

		public void AddPathList(FileIOPermissionAccess access, string[] pathList	){
			foreach(string path in pathList){
				AddPathList(access, path);
			}
			
		}

		// private constructor used by Copy() method
		private FileIOPermission(Hashtable pathList, FileIOPermissionAccess allFiles, 
						FileIOPermissionAccess allLocalFiles, bool unrestricted){
			m_PathList = pathList;
			m_AllFilesAccess = allFiles;
			m_AllLocalFilesAccess = allLocalFiles;
			m_Unrestricted = unrestricted;
		}

		public override IPermission Copy(){
			if (m_Unrestricted) {
				return new FileIOPermission(PermissionState.Unrestricted);
			}
			else{
				FileIOPermission retVal = new FileIOPermission(m_PathList, m_AllFilesAccess, m_AllLocalFilesAccess, m_Unrestricted);
				return retVal;
			}
		}

		/*  XML Schema for FileIOPermission
				<IPermission class=”FileIOPermission” 
								 version=”1”
								 (
								 Read=”[list of files or folders]” | 
								 Write=”[list of files or folders]” |
								 Append=”[list of files or folders]”   
								 ) v Unrestricted=”true” 
								 />
		*/
		public override void FromXml(SecurityElement esd){
			if (null == esd) {
				throw new ArgumentNullException();
			}
			if (esd.Tag != "IPermission" || (string)esd.Attributes["class"] != "FileIOPermission"
					|| (string)esd.Attributes["version"] != "1"){
				throw new ArgumentException("Not a valid permission element");
			}
			m_PathList.Clear();
			if ("true" == (string)esd.Attributes["Unrestricted"]){
				m_Unrestricted = true;
			}
			else{
				m_Unrestricted = false;
				string fileList;
				fileList = (string)esd.Attributes["Read"];
				string[] files;
				if (fileList != null){
					files = fileList.Split(';');
					AddPathList(FileIOPermissionAccess.Read, files);
				}
				fileList = (string)esd.Attributes["Write"];
				if (fileList != null){
					files = fileList.Split(';');
					AddPathList(FileIOPermissionAccess.Write, files);
				}
				fileList = (string)esd.Attributes["Append"];
				if (fileList != null){
					files = fileList.Split(';');
					AddPathList(FileIOPermissionAccess.Append, files);
				}
			}
		}

		public string[] GetPathList(FileIOPermissionAccess access){
			//LAMESPEC: docs says it returns (semicolon separated) list, but return
			//type is array.  I think docs are wrong and it just returns an array
			if ((FileIOPermissionAccess.AllAccess & access) != access){
				throw new ArgumentException("Illegal enum value: "+access.ToString()+".");
			}

			ArrayList matchingPaths = new ArrayList();
			System.Collections.IDictionaryEnumerator pathListIterator = m_PathList.GetEnumerator();
			while (pathListIterator.MoveNext()) {
				if (((FileIOPermissionAccess)pathListIterator.Value & access) != 0) {
					matchingPaths.Add((string)pathListIterator.Key);
				}
			}
			if (matchingPaths.Count == 0) {
				return null;
			}
			else {
				return (string[])matchingPaths.ToArray(typeof(string));
			}
		}

		public override IPermission Intersect(IPermission target){ 
			if (null == target){
				return null;
			}
			else {
				if (target.GetType() != typeof(FileIOPermission)){
					throw new ArgumentException();
				}
			}
			FileIOPermission FIOPTarget = (FileIOPermission)target;
			if (FIOPTarget.IsUnrestricted() && m_Unrestricted){
				return new FileIOPermission(PermissionState.Unrestricted);
			}
			else if (FIOPTarget.IsUnrestricted()){
				return Copy();
			}
			else if (m_Unrestricted){
				return FIOPTarget.Copy();
			}
			else{
				FileIOPermission retVal = new FileIOPermission(PermissionState.None);
				retVal.AllFiles = m_AllFilesAccess & FIOPTarget.AllFiles;
				retVal.AllLocalFiles = m_AllLocalFilesAccess & FIOPTarget.AllLocalFiles;

				string[] paths;
				paths = FIOPTarget.GetPathList(FileIOPermissionAccess.Append);
				if (null != paths) {
					foreach (string path in paths){
						if (m_PathList.ContainsKey(path) 
							&& ((FileIOPermissionAccess)m_PathList[path] & FileIOPermissionAccess.Append) != 0){
							retVal.AddPathList(FileIOPermissionAccess.Append, path);
						}
					}
				}

				paths = FIOPTarget.GetPathList(FileIOPermissionAccess.Read);
				if (null != paths) {
					foreach (string path in paths){
						if (m_PathList.ContainsKey(path) 
							&& ((FileIOPermissionAccess)m_PathList[path] & FileIOPermissionAccess.Read) != 0){
							retVal.AddPathList(FileIOPermissionAccess.Read, path);
						}
					}
				}

				paths = FIOPTarget.GetPathList(FileIOPermissionAccess.Write);
				if (null != paths) {
					foreach (string path in paths){
						if (m_PathList.ContainsKey(path) 
							&& ((FileIOPermissionAccess)m_PathList[path] & FileIOPermissionAccess.Write) != 0){
							retVal.AddPathList(FileIOPermissionAccess.Write, path);
						}
					}
				}

				return retVal;
			}
		}


		public override bool IsSubsetOf(IPermission target){
			// X.IsSubsetOf(Y) is true if permission Y includes everything allowed by X.
			if (target != null && target.GetType() != typeof(FileIOPermission)){
				throw new ArgumentException();
			}
			FileIOPermission FIOPTarget = (FileIOPermission)target;
			if (FIOPTarget.IsUnrestricted()){
				return true;
			}
			else if (m_Unrestricted){
				return false;
			}
			else if ((m_AllFilesAccess & FIOPTarget.AllFiles) != m_AllFilesAccess) {
				return false;
			}
			else if ((m_AllLocalFilesAccess & FIOPTarget.AllLocalFiles) != m_AllLocalFilesAccess) {
				return false;
			}
			else{
				string[] pathsNeeded;
				string[] pathsInTarget;

				pathsNeeded = GetPathList(FileIOPermissionAccess.Append);
				if (null != pathsNeeded) {
					pathsInTarget = FIOPTarget.GetPathList(FileIOPermissionAccess.Append);
					foreach (string path in pathsNeeded){
						if (Array.IndexOf(pathsInTarget, path) <0) {
							return false;
						}
					}
				}

				pathsNeeded = GetPathList(FileIOPermissionAccess.Read);
				if (null != pathsNeeded) {
					pathsInTarget = FIOPTarget.GetPathList(FileIOPermissionAccess.Read);
					foreach (string path in pathsNeeded){
						if (Array.IndexOf(pathsInTarget, path) <0) {
							return false;
						}
					}
				}

				pathsNeeded = GetPathList(FileIOPermissionAccess.Write);
				if (null != pathsNeeded) {
					pathsInTarget = FIOPTarget.GetPathList(FileIOPermissionAccess.Write);
					foreach (string path in pathsNeeded){
						if (Array.IndexOf(pathsInTarget, path) <0) {
							return false;
						}
					}
				}

				return true;
			}
		}

		public bool IsUnrestricted(){
			return m_Unrestricted;
		}

		public void SetPathList(FileIOPermissionAccess access, string path){
			if ((FileIOPermissionAccess.AllAccess & access) != access){
				throw new ArgumentException("Illegal enum value: "+access.ToString()+".");
			}
			if (path.LastIndexOfAny(m_badCharacters) >= 0){
				throw new ArgumentException("Invalid characters in path: '{0}'", path);
			}

			m_PathList.Clear();
			AddPathList(access, path);
		}
		
		public void SetPathList(FileIOPermissionAccess access, string[] pathList){
			if ((FileIOPermissionAccess.AllAccess & access) != access){
				throw new ArgumentException("Illegal enum value: "+access.ToString()+".");
			}
			foreach(string path in pathList){
				if (path.LastIndexOfAny(m_badCharacters) >= 0){
					throw new ArgumentException("Invalid characters in path entry: '{0}'", path);
				}
			}

			m_PathList.Clear();
			AddPathList(access, pathList);
		}

		public override SecurityElement ToXml(){
			//Encode the the current permission to XML using the 
			//security element class.
			SecurityElement element = new SecurityElement("IPermission");
			Type type = this.GetType();
			StringBuilder AsmName = new StringBuilder(type.Assembly.ToString());
			AsmName.Replace('\"', '\'');
			element.AddAttribute("class", type.FullName + ", " + AsmName);
			element.AddAttribute("version", "1");
			if(m_Unrestricted){
				element.AddAttribute("Unrestricted", "true");
			}
			else {
				string[] paths;
				paths = GetPathList(FileIOPermissionAccess.Append);
				if (null != paths && paths.Length >0){
					element.AddAttribute("Append", String.Join(";",paths));
				}
				paths = GetPathList(FileIOPermissionAccess.Read);
				if (null != paths && paths.Length >0){
					element.AddAttribute("Read", String.Join(";",paths));
				}
				paths = GetPathList(FileIOPermissionAccess.Write);
				if (null != paths && paths.Length >0){
					element.AddAttribute("Write", String.Join(";",paths));
				}
			}
			return element;
		}

		public override IPermission Union(IPermission other){
			if (null == other){
				return null;
			}
			else {
				if (other.GetType() != typeof(FileIOPermission)){
					throw new ArgumentException();
				}
			}
			FileIOPermission FIOPTarget = (FileIOPermission)other;
			if (FIOPTarget.IsUnrestricted() || m_Unrestricted){
				return new FileIOPermission(PermissionState.Unrestricted);
			}
			else{
				FileIOPermission retVal = (FileIOPermission)Copy();
				retVal.AllFiles |= FIOPTarget.AllFiles;
				retVal.AllLocalFiles |= FIOPTarget.AllLocalFiles;
				string[] paths;
				paths = FIOPTarget.GetPathList(FileIOPermissionAccess.Append);
				if (null != paths){
						retVal.AddPathList(FileIOPermissionAccess.Append, paths);
				}
				paths = FIOPTarget.GetPathList(FileIOPermissionAccess.Read);
				if (null != paths){
					retVal.AddPathList(FileIOPermissionAccess.Read, paths);
				}
				paths = FIOPTarget.GetPathList(FileIOPermissionAccess.Write);
				if (null != paths){
					retVal.AddPathList(FileIOPermissionAccess.Write, paths);
				}
				return retVal;
			}
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 2;C
		}
                

	}
}
