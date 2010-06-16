//
// System.IO.IsolatedStorage.IsolatedStorageFile
//
// Authors
// 	Jonathan Pryor (jonpryor@vt.edu)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Jonathan Pryor
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
#if !MOONLIGHT
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;

using Mono.Security.Cryptography;

namespace System.IO.IsolatedStorage {

	// This is a terribly named class.  It doesn't actually represent a file as
	// much as a directory


	[ComVisible (true)]
	// FIXME: Further limit the assertion when imperative Assert is implemented
	[FileIOPermission (SecurityAction.Assert, Unrestricted = true)]
	public sealed class IsolatedStorageFile : IsolatedStorage, IDisposable {

		private bool _resolved;
		private ulong _maxSize;
		private Evidence _fullEvidences;
		private static Mutex mutex = new Mutex ();
#if NET_4_0
		private bool closed;
		private bool disposed;
#endif

		public static IEnumerator GetEnumerator (IsolatedStorageScope scope)
		{
			Demand (scope);

			switch (scope) {
			case IsolatedStorageScope.User:
			case IsolatedStorageScope.User | IsolatedStorageScope.Roaming:
			case IsolatedStorageScope.Machine:
				break;
			default:
				string msg = Locale.GetText ("Invalid scope, only User, User|Roaming and Machine are valid");
				throw new ArgumentException (msg);
			}

			return new IsolatedStorageFileEnumerator (scope, GetIsolatedStorageRoot (scope));
		}

		public static IsolatedStorageFile GetStore (IsolatedStorageScope scope,
			Evidence domainEvidence, Type domainEvidenceType,
			Evidence assemblyEvidence, Type assemblyEvidenceType)
		{
			Demand (scope);

			bool domain = ((scope & IsolatedStorageScope.Domain) != 0);
			if (domain && (domainEvidence == null))
				throw new ArgumentNullException ("domainEvidence");

			bool assembly = ((scope & IsolatedStorageScope.Assembly) != 0);
			if (assembly && (assemblyEvidence == null))
				throw new ArgumentNullException ("assemblyEvidence");

			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			if (domain) {
				if (domainEvidenceType == null) {
					storageFile._domainIdentity = GetDomainIdentityFromEvidence (domainEvidence);
				} else {
					storageFile._domainIdentity = GetTypeFromEvidence (domainEvidence, domainEvidenceType);
				}

				if (storageFile._domainIdentity == null)
					throw new IsolatedStorageException (Locale.GetText ("Couldn't find domain identity."));
			}

			if (assembly) {
				if (assemblyEvidenceType == null) {
					storageFile._assemblyIdentity = GetAssemblyIdentityFromEvidence (assemblyEvidence);
				} else {
					storageFile._assemblyIdentity = GetTypeFromEvidence (assemblyEvidence, assemblyEvidenceType);
				}

				if (storageFile._assemblyIdentity == null)
					throw new IsolatedStorageException (Locale.GetText ("Couldn't find assembly identity."));
			}

			storageFile.PostInit ();
			return storageFile;
		}

		public static IsolatedStorageFile GetStore (IsolatedStorageScope scope, object domainIdentity, object assemblyIdentity)
		{
			Demand (scope);

			if (((scope & IsolatedStorageScope.Domain) != 0) && (domainIdentity == null))
				throw new ArgumentNullException ("domainIdentity");

			bool assembly = ((scope & IsolatedStorageScope.Assembly) != 0);
			if (assembly && (assemblyIdentity == null))
				throw new ArgumentNullException ("assemblyIdentity");

			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			if (assembly)
				storageFile._fullEvidences = Assembly.GetCallingAssembly ().UnprotectedGetEvidence ();
			storageFile._domainIdentity = domainIdentity;
			storageFile._assemblyIdentity = assemblyIdentity;
			storageFile.PostInit ();
			return storageFile;
		}

		public static IsolatedStorageFile GetStore (IsolatedStorageScope scope, Type domainEvidenceType, Type assemblyEvidenceType)
		{
			Demand (scope);
			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			if ((scope & IsolatedStorageScope.Domain) != 0) {
				if (domainEvidenceType == null)
					domainEvidenceType = typeof (Url);
				storageFile._domainIdentity = GetTypeFromEvidence (AppDomain.CurrentDomain.Evidence, domainEvidenceType);
			}
			if ((scope & IsolatedStorageScope.Assembly) != 0) {
				Evidence e = Assembly.GetCallingAssembly ().UnprotectedGetEvidence ();
				storageFile._fullEvidences = e;
				if ((scope & IsolatedStorageScope.Domain) != 0) {
					if (assemblyEvidenceType == null)
						assemblyEvidenceType = typeof (Url);
					storageFile._assemblyIdentity = GetTypeFromEvidence (e, assemblyEvidenceType);
				} else {
					storageFile._assemblyIdentity = GetAssemblyIdentityFromEvidence (e);
				}
			}
			storageFile.PostInit ();
			return storageFile;
		}
		public static IsolatedStorageFile GetStore (IsolatedStorageScope scope, object applicationIdentity)
		{
			Demand (scope);
			if (applicationIdentity == null)
				throw new ArgumentNullException ("applicationIdentity");

			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			storageFile._applicationIdentity = applicationIdentity;
			storageFile._fullEvidences = Assembly.GetCallingAssembly ().UnprotectedGetEvidence ();
			storageFile.PostInit ();
			return storageFile;
		}

		public static IsolatedStorageFile GetStore (IsolatedStorageScope scope, Type applicationEvidenceType)
		{
			Demand (scope);
			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			storageFile.InitStore (scope, applicationEvidenceType);
			storageFile._fullEvidences = Assembly.GetCallingAssembly ().UnprotectedGetEvidence ();
			storageFile.PostInit ();
			return storageFile;
		}

		[IsolatedStorageFilePermission (SecurityAction.Demand, UsageAllowed = IsolatedStorageContainment.ApplicationIsolationByMachine)]
		public static IsolatedStorageFile GetMachineStoreForApplication ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.Machine | IsolatedStorageScope.Application;
			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			storageFile.InitStore (scope, null);
			storageFile._fullEvidences = Assembly.GetCallingAssembly ().UnprotectedGetEvidence ();
			storageFile.PostInit ();
			return storageFile;
		}

		[IsolatedStorageFilePermission (SecurityAction.Demand, UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByMachine)]
		public static IsolatedStorageFile GetMachineStoreForAssembly ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly;
			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			Evidence e = Assembly.GetCallingAssembly ().UnprotectedGetEvidence ();
			storageFile._fullEvidences = e;
			storageFile._assemblyIdentity = GetAssemblyIdentityFromEvidence (e);
			storageFile.PostInit ();
			return storageFile;
		}

		[IsolatedStorageFilePermission (SecurityAction.Demand, UsageAllowed = IsolatedStorageContainment.DomainIsolationByMachine)]
		public static IsolatedStorageFile GetMachineStoreForDomain ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.Machine | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;
			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			storageFile._domainIdentity = GetDomainIdentityFromEvidence (AppDomain.CurrentDomain.Evidence);
			Evidence e = Assembly.GetCallingAssembly ().UnprotectedGetEvidence ();
			storageFile._fullEvidences = e;
			storageFile._assemblyIdentity = GetAssemblyIdentityFromEvidence (e);
			storageFile.PostInit ();
			return storageFile;
		}

		[IsolatedStorageFilePermission (SecurityAction.Demand, UsageAllowed = IsolatedStorageContainment.ApplicationIsolationByUser)]
		public static IsolatedStorageFile GetUserStoreForApplication ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Application;
			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			storageFile.InitStore (scope, null);
			storageFile._fullEvidences = Assembly.GetCallingAssembly ().UnprotectedGetEvidence ();
			storageFile.PostInit ();
			return storageFile;
		}

		[IsolatedStorageFilePermission (SecurityAction.Demand, UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser)]
		public static IsolatedStorageFile GetUserStoreForAssembly ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Assembly;
			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			Evidence e = Assembly.GetCallingAssembly ().UnprotectedGetEvidence ();
			storageFile._fullEvidences = e;
			storageFile._assemblyIdentity = GetAssemblyIdentityFromEvidence (e);
			storageFile.PostInit ();
			return storageFile;
		}

		[IsolatedStorageFilePermission (SecurityAction.Demand, UsageAllowed = IsolatedStorageContainment.DomainIsolationByUser)]
		public static IsolatedStorageFile GetUserStoreForDomain ()
		{
			IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;
			IsolatedStorageFile storageFile = new IsolatedStorageFile (scope);
			storageFile._domainIdentity = GetDomainIdentityFromEvidence (AppDomain.CurrentDomain.Evidence);
			Evidence e = Assembly.GetCallingAssembly ().UnprotectedGetEvidence ();
			storageFile._fullEvidences = e;
			storageFile._assemblyIdentity = GetAssemblyIdentityFromEvidence (e);
			storageFile.PostInit ();
			return storageFile;
		}

#if NET_4_0
		[ComVisible (false)]
		public static IsolatedStorageFile GetUserStoreForSite ()
		{
			throw new NotSupportedException ();
		}
#endif

		public static void Remove (IsolatedStorageScope scope)
		{
			string dir = GetIsolatedStorageRoot (scope);
			if (!Directory.Exists (dir))
				return;

			try {
				Directory.Delete (dir, true);
			} catch (IOException) {
				throw new IsolatedStorageException ("Could not remove storage.");
			}
		}

		// internal static stuff

		// Security Note: We're using InternalGetFolderPath because 
		// IsolatedStorage must be able to work even if we do not have
		// FileIOPermission's PathDiscovery permissions
		internal static string GetIsolatedStorageRoot (IsolatedStorageScope scope)
		{
			// IsolatedStorageScope mixes several flags into one.
			// This first level deals with the root directory - it
			// is decided based on User, User+Roaming or Machine
			string root = null;

			if ((scope & IsolatedStorageScope.User) != 0) {
				if ((scope & IsolatedStorageScope.Roaming) != 0) {
					root = Environment.InternalGetFolderPath (Environment.SpecialFolder.LocalApplicationData);
				} else {
					root = Environment.InternalGetFolderPath (Environment.SpecialFolder.ApplicationData);
				}
			} else if ((scope & IsolatedStorageScope.Machine) != 0) {
				root = Environment.InternalGetFolderPath (Environment.SpecialFolder.CommonApplicationData);
			}

			if (root == null) {
				string msg = Locale.GetText ("Couldn't access storage location for '{0}'.");
				throw new IsolatedStorageException (String.Format (msg, scope));
			}

			return Path.Combine (root, ".isolated-storage");
		}

		private static void Demand (IsolatedStorageScope scope)
		{
			if (SecurityManager.SecurityEnabled) {
				IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
				isfp.UsageAllowed = ScopeToContainment (scope);
				isfp.Demand ();
			}
		}

		private static IsolatedStorageContainment ScopeToContainment (IsolatedStorageScope scope)
		{
			switch (scope) {
			case IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly | IsolatedStorageScope.User:
				return IsolatedStorageContainment.DomainIsolationByUser;
			case IsolatedStorageScope.Assembly | IsolatedStorageScope.User:
				return IsolatedStorageContainment.AssemblyIsolationByUser;
			case IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly | IsolatedStorageScope.User | IsolatedStorageScope.Roaming:
				return IsolatedStorageContainment.DomainIsolationByRoamingUser;
			case IsolatedStorageScope.Assembly | IsolatedStorageScope.User | IsolatedStorageScope.Roaming:
				return IsolatedStorageContainment.AssemblyIsolationByRoamingUser;
			case IsolatedStorageScope.Application | IsolatedStorageScope.User:
				return IsolatedStorageContainment.ApplicationIsolationByUser;
			case IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly | IsolatedStorageScope.Machine:
				return IsolatedStorageContainment.DomainIsolationByMachine;
			case IsolatedStorageScope.Assembly | IsolatedStorageScope.Machine:
				return IsolatedStorageContainment.AssemblyIsolationByMachine;
			case IsolatedStorageScope.Application | IsolatedStorageScope.Machine:
				return IsolatedStorageContainment.ApplicationIsolationByMachine;
			case IsolatedStorageScope.Application | IsolatedStorageScope.User | IsolatedStorageScope.Roaming:
				return IsolatedStorageContainment.ApplicationIsolationByRoamingUser;
			default:
				// unknown ?!?! then ask for maximum (unrestricted)
				return IsolatedStorageContainment.UnrestrictedIsolatedStorage;
			}
		}

		internal static ulong GetDirectorySize (DirectoryInfo di)
		{
			ulong size = 0;

			foreach (FileInfo fi in di.GetFiles ())
				size += (ulong) fi.Length;

			foreach (DirectoryInfo d in di.GetDirectories ())
				size += GetDirectorySize (d);

			return size;
		}

		// non-static stuff

		private DirectoryInfo directory;

		private IsolatedStorageFile (IsolatedStorageScope scope)
		{
			storage_scope = scope;
		}

		internal IsolatedStorageFile (IsolatedStorageScope scope, string location)
		{
			storage_scope = scope;
			directory = new DirectoryInfo (location);
			if (!directory.Exists) {
				string msg = Locale.GetText ("Invalid storage.");
				throw new IsolatedStorageException (msg);
			}
			// load the identities
		}

		~IsolatedStorageFile ()
		{
		}

		private void PostInit ()
		{
			string root = GetIsolatedStorageRoot (Scope);
			string dir = null;
			if (_applicationIdentity != null) {
				dir = String.Format ("a{0}{1}", SeparatorInternal, GetNameFromIdentity (_applicationIdentity));
			} else if (_domainIdentity != null) {
				dir = String.Format ("d{0}{1}{0}{2}", SeparatorInternal,
					GetNameFromIdentity (_domainIdentity), GetNameFromIdentity (_assemblyIdentity));
			} else if (_assemblyIdentity != null) {
				dir = String.Format ("d{0}none{0}{1}", SeparatorInternal, GetNameFromIdentity (_assemblyIdentity));
			} else {
				throw new IsolatedStorageException (Locale.GetText ("No code identity available."));
			}

			root = Path.Combine (root, dir);

			// identities have been selected
			directory = new DirectoryInfo (root);
			if (!directory.Exists) {
				try {
					directory.Create ();
					SaveIdentities (root);
				}
				catch (IOException) {
				}
			}
		}

		[CLSCompliant(false)]
#if NET_4_0
		[Obsolete]
#endif
		public override ulong CurrentSize {
			get { return GetDirectorySize (directory); }
		}

		[CLSCompliant(false)]
#if NET_4_0
		[Obsolete]
#endif
		public override ulong MaximumSize {
			// return an ulong but default is signed long
			get {
				if (!SecurityManager.SecurityEnabled)
					return Int64.MaxValue;

				if (_resolved)
					return _maxSize;

				Evidence e = null;
				if (_fullEvidences != null) {
					// if possible use the complete evidences we had
					// for computing the X identity
					e = _fullEvidences;
				} else {
					e = new Evidence ();
					// otherwise use what was provided
					if (_assemblyIdentity != null)
						e.AddHost (_assemblyIdentity);
				}
				if (e.Count < 1) {
					throw new InvalidOperationException (
						Locale.GetText ("Couldn't get the quota from the available evidences."));
				}

				PermissionSet denied = null;
				PermissionSet ps = SecurityManager.ResolvePolicy (e, null, null, null, out denied);
				IsolatedStoragePermission isp = GetPermission (ps);
				if (isp == null) {
					if (ps.IsUnrestricted ()) {
						_maxSize = Int64.MaxValue; /* default value */
					} else {
						throw new InvalidOperationException (
							Locale.GetText ("No quota from the available evidences."));
					}
				} else {
					_maxSize = (ulong) isp.UserQuota;
				}
				_resolved = true;
				return _maxSize;
			}
		}

		internal string Root {
			get { return directory.FullName; }
		}

#if NET_4_0
		[ComVisible (false)]
		public override long AvailableFreeSpace {
			get {
				CheckOpen ();

				// See the notes for 'Quota'
				return Int64.MaxValue;

			}
		}

		[ComVisible (false)]
		public override long Quota {
			get {
				CheckOpen ();

				// Since we don't fully support CAS, we are likely
				// going to return Int64.MaxValue always, but we return
				// MaximumSize just in case.
				return (long)MaximumSize;
			}
		}

		[ComVisible (false)]
		public override long UsedSize {
			get {
				CheckOpen ();
				return (long)GetDirectorySize (directory);
			}
		}

		[ComVisible (false)]
		public static bool IsEnabled {
			get {
				return true;
			}
		}

		internal bool IsClosed {
			get {
				return closed;
			}
		}

		internal bool IsDisposed {
			get {
				return disposed;
			}
		}
#endif

		// methods

		public void Close ()
		{
#if NET_4_0
			closed = true;
#endif
		}

		public void CreateDirectory (string dir)
		{
			if (dir == null)
				throw new ArgumentNullException ("dir");

			if (dir.IndexOfAny (Path.PathSeparatorChars) < 0) {
				if (directory.GetFiles (dir).Length > 0)
#if NET_4_0
					throw new IsolatedStorageException ("Unable to create directory.");
#else
					throw new IOException (Locale.GetText ("Directory name already exists as a file."));
#endif
				directory.CreateSubdirectory (dir);
			} else {
				string[] dirs = dir.Split (Path.PathSeparatorChars);
				DirectoryInfo dinfo = directory;

				for (int i = 0; i < dirs.Length; i++) {
					if (dinfo.GetFiles (dirs [i]).Length > 0)
#if NET_4_0
						throw new IsolatedStorageException ("Unable to create directory.");
#else
						throw new IOException (Locale.GetText (
							"Part of the directory name already exists as a file."));
#endif
					dinfo = dinfo.CreateSubdirectory (dirs [i]);
				}
			}
		}

#if NET_4_0
		[ComVisible (false)]
		public void CopyFile (string sourceFileName, string destinationFileName)
		{
			CopyFile (sourceFileName, destinationFileName, false);
		}

		[ComVisible (false)]
		public void CopyFile (string sourceFileName, string destinationFileName, bool overwrite)
		{
			if (sourceFileName == null)
				throw new ArgumentNullException ("sourceFileName");
			if (destinationFileName == null)
				throw new ArgumentNullException ("destinationFileName");
			if (sourceFileName.Trim ().Length == 0)
				throw new ArgumentException ("An empty file name is not valid.", "sourceFileName");
			if (destinationFileName.Trim ().Length == 0)
				throw new ArgumentException ("An empty file name is not valid.", "destinationFileName");

			CheckOpen ();

			string source_full_path = Path.Combine (directory.FullName, sourceFileName);
			string dest_full_path = Path.Combine (directory.FullName, destinationFileName);

			if (!IsPathInStorage (source_full_path) || !IsPathInStorage (dest_full_path))
				throw new IsolatedStorageException ("Operation not allowed.");
			// These excs can be thrown from File.Copy, but we can try to detect them from here.
			if (!Directory.Exists (Path.GetDirectoryName (source_full_path)))
				throw new DirectoryNotFoundException ("Could not find a part of path '" + sourceFileName + "'.");
			if (!File.Exists (source_full_path))
				throw new FileNotFoundException ("Could not find a part of path '" + sourceFileName + "'.");
			if (File.Exists (dest_full_path) && !overwrite)
				throw new IsolatedStorageException ("Operation not allowed.");

			try {
				File.Copy (source_full_path, dest_full_path, overwrite);
			} catch (IOException) {
				throw new IsolatedStorageException ("Operation not allowed.");
			}
		}

		[ComVisible (false)]
		public IsolatedStorageFileStream CreateFile (string path)
		{
			return new IsolatedStorageFileStream (path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, this);
		}
#endif

		public void DeleteDirectory (string dir)
		{
			try {
				DirectoryInfo subdir = directory.CreateSubdirectory (dir);
				subdir.Delete ();
			}
			catch {
				// hide the real exception to avoid leaking the full path
				throw new IsolatedStorageException (Locale.GetText ("Could not delete directory '{0}'", dir));
			}
		}

		public void DeleteFile (string file)
		{
			if (file == null)
				throw new ArgumentNullException ("file");

			string full_path = Path.Combine (directory.FullName, file);
			if (!File.Exists (full_path))
				throw new IsolatedStorageException (Locale.GetText ("Could not delete file '{0}'", file));

			try {
				File.Delete (Path.Combine (directory.FullName, file));
			} catch {
				// hide the internal exception, just as DeleteDirectory does.
				throw new IsolatedStorageException (Locale.GetText ("Could not delete file '{0}'", file));
			}
		}

		public void Dispose ()
		{
#if NET_4_0
			// Dispose may be calling Close, but we are not sure
			disposed = true;
#endif
			// nothing to dispose, anyway we want to please the tools
			GC.SuppressFinalize (this);
		}

#if NET_4_0
		[ComVisible (false)]
		public bool DirectoryExists (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			CheckOpen ();

			string full_path = Path.Combine (directory.FullName, path);
			if (!IsPathInStorage (full_path))
				return false;

			return Directory.Exists (full_path);
		}

		[ComVisible (false)]
		public bool FileExists (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			CheckOpen ();

			string full_path = Path.Combine (directory.FullName, path);
			if (!IsPathInStorage (full_path))
				return false;

			return File.Exists (full_path);
		}

		[ComVisible (false)]
		public DateTimeOffset GetCreationTime (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Trim ().Length == 0)
				throw new ArgumentException ("An empty path is not valid.");

			CheckOpen ();

			string full_path = Path.Combine (directory.FullName, path);
			if (File.Exists (full_path))
				return File.GetCreationTime (full_path);

			return Directory.GetCreationTime (full_path);
		}

		[ComVisible (false)]
		public DateTimeOffset GetLastAccessTime (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Trim ().Length == 0)
				throw new ArgumentException ("An empty path is not valid.");

			CheckOpen ();

			string full_path = Path.Combine (directory.FullName, path);
			if (File.Exists (full_path))
				return File.GetLastAccessTime (full_path);

			return Directory.GetLastAccessTime (full_path);
		}

		[ComVisible (false)]
		public DateTimeOffset GetLastWriteTime (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Trim ().Length == 0)
				throw new ArgumentException ("An empty path is not valid.");

			CheckOpen ();

			string full_path = Path.Combine (directory.FullName, path);
			if (File.Exists (full_path))
				return File.GetLastWriteTime (full_path);

			return Directory.GetLastWriteTime (full_path);
		}
#endif

		public string[] GetDirectoryNames (string searchPattern)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");
#if NET_4_0
			if (searchPattern.Contains (".."))
				throw new ArgumentException ("Search pattern cannot contain '..' to move up directories.", "searchPattern");
#endif

			// note: IsolatedStorageFile accept a "dir/file" pattern which is not allowed by DirectoryInfo
			// so we need to split them to get the right results
			string path = Path.GetDirectoryName (searchPattern);
			string pattern = Path.GetFileName (searchPattern);
			DirectoryInfo[] adi = null;
			if (path == null || path.Length == 0) {
				adi = directory.GetDirectories (searchPattern);
			} else {
				DirectoryInfo[] subdirs = directory.GetDirectories (path);
				// we're looking for a single result, identical to path (no pattern here)
				// we're also looking for something under the current path (not outside isolated storage)
				if ((subdirs.Length == 1) && (subdirs [0].Name == path) && (subdirs [0].FullName.IndexOf (directory.FullName) >= 0)) {
					adi = subdirs [0].GetDirectories (pattern);
				} else {
					// CAS, even in FullTrust, normally enforce IsolatedStorage
					throw new SecurityException ();
				}
			}
			 
			return GetNames (adi);
		}

#if NET_4_0
		[ComVisible (false)]
		public string [] GetDirectoryNames ()
		{
			return GetDirectoryNames ("*");
		}
#endif

		private string[] GetNames (FileSystemInfo[] afsi)
		{
			string[] r = new string[afsi.Length];
			for (int i = 0; i != afsi.Length; ++i)
				r[i] = afsi[i].Name;
			return r;
		}

		public string[] GetFileNames (string searchPattern)
		{
			if (searchPattern == null)
				throw new ArgumentNullException ("searchPattern");
#if NET_4_0
			if (searchPattern.Contains (".."))
				throw new ArgumentException ("Search pattern cannot contain '..' to move up directories.", "searchPattern");
#endif

			// note: IsolatedStorageFile accept a "dir/file" pattern which is not allowed by DirectoryInfo
			// so we need to split them to get the right results
			string path = Path.GetDirectoryName (searchPattern);
			string pattern = Path.GetFileName (searchPattern);
			FileInfo[] afi = null;
			if (path == null || path.Length == 0) {
				afi = directory.GetFiles (searchPattern);
			} else {
				DirectoryInfo[] subdirs = directory.GetDirectories (path);
				// we're looking for a single result, identical to path (no pattern here)
				// we're also looking for something under the current path (not outside isolated storage)
				if ((subdirs.Length == 1) && (subdirs [0].Name == path) && (subdirs [0].FullName.IndexOf (directory.FullName) >= 0)) {
					afi = subdirs [0].GetFiles (pattern);
				} else {
					// CAS, even in FullTrust, normally enforce IsolatedStorage
					throw new SecurityException ();
				}
			}

			return GetNames (afi);
		}

#if NET_4_0
		[ComVisible (false)]
		public string [] GetFileNames ()
		{
			return GetFileNames ("*");
		}

		[ComVisible (false)]
		public override bool IncreaseQuotaTo (long newQuotaSize)
		{
			if (newQuotaSize < Quota)
				throw new ArgumentException ();

			CheckOpen ();

			// .Net is supposed to be returning false, as mentioned in the docs.
			return false;
		}

		[ComVisible (false)]
		public void MoveDirectory (string sourceDirectoryName, string destinationDirectoryName)
		{
			if (sourceDirectoryName == null)
				throw new ArgumentNullException ("sourceDirectoryName");
			if (destinationDirectoryName == null)
				throw new ArgumentNullException ("sourceDirectoryName");
			if (sourceDirectoryName.Trim ().Length == 0)
				throw new ArgumentException ("An empty directory name is not valid.", "sourceDirectoryName");
			if (destinationDirectoryName.Trim ().Length == 0)
				throw new ArgumentException ("An empty directory name is not valid.", "destinationDirectoryName");

			CheckOpen ();

			string src_full_path = Path.Combine (directory.FullName, sourceDirectoryName);
			string dest_full_path = Path.Combine (directory.FullName, destinationDirectoryName);

			if (!IsPathInStorage (src_full_path) || !IsPathInStorage (dest_full_path))
				throw new IsolatedStorageException ("Operation not allowed.");
			if (!Directory.Exists (src_full_path))
				throw new DirectoryNotFoundException ("Could not find a part of path '" + sourceDirectoryName + "'.");
			if (!Directory.Exists (Path.GetDirectoryName (dest_full_path)))
				throw new DirectoryNotFoundException ("Could not find a part of path '" + destinationDirectoryName + "'.");

			try {
				Directory.Move (src_full_path, dest_full_path);
			} catch (IOException) {
				throw new IsolatedStorageException ("Operation not allowed.");
			}
		}

		[ComVisible (false)]
		public void MoveFile (string sourceFileName, string destinationFileName)
		{
			if (sourceFileName == null)
				throw new ArgumentNullException ("sourceFileName");
			if (destinationFileName == null)
				throw new ArgumentNullException ("sourceFileName");
			if (sourceFileName.Trim ().Length == 0)
				throw new ArgumentException ("An empty file name is not valid.", "sourceFileName");
			if (destinationFileName.Trim ().Length == 0)
				throw new ArgumentException ("An empty file name is not valid.", "destinationFileName");

			CheckOpen ();

			string source_full_path = Path.Combine (directory.FullName, sourceFileName);
			string dest_full_path = Path.Combine (directory.FullName, destinationFileName);

			if (!IsPathInStorage (source_full_path) || !IsPathInStorage (dest_full_path))
				throw new IsolatedStorageException ("Operation not allowed.");
			if (!File.Exists (source_full_path))
				throw new FileNotFoundException ("Could not find a part of path '" + sourceFileName + "'.");
			// I expected a DirectoryNotFound exception.
			if (!Directory.Exists (Path.GetDirectoryName (dest_full_path)))
				throw new IsolatedStorageException ("Operation not allowed.");

			try {
				File.Move (source_full_path, dest_full_path);
			} catch (IOException) {
				throw new IsolatedStorageException ("Operation not allowed.");
			}
		}

		[ComVisible (false)]
		public IsolatedStorageFileStream OpenFile (string path, FileMode mode)
		{
			return new IsolatedStorageFileStream (path, mode, this);
		}

		[ComVisible (false)]
		public IsolatedStorageFileStream OpenFile (string path, FileMode mode, FileAccess access)
		{
			return new IsolatedStorageFileStream (path, mode, access, this);
		}

		[ComVisible (false)]
		public IsolatedStorageFileStream OpenFile (string path, FileMode mode, FileAccess access, FileShare share)
		{
			return new IsolatedStorageFileStream (path, mode, access, share, this);
		}
#endif

		public override void Remove ()
		{
#if NET_4_0
			CheckOpen (false);
#endif
			try {
				directory.Delete (true);
			} catch {
				throw new IsolatedStorageException ("Could not remove storage.");
			}

			// It seems .Net is calling Close from here.
			Close ();
		}


		protected override IsolatedStoragePermission GetPermission (PermissionSet ps)
		{
			if (ps == null)
				return null;
			return (IsolatedStoragePermission) ps.GetPermission (typeof (IsolatedStorageFilePermission));
		}

		// internal stuff
#if NET_4_0
		void CheckOpen ()
		{
			CheckOpen (true);
		}

		void CheckOpen (bool checkDirExists)
		{
			if (disposed)
				throw new ObjectDisposedException ("IsolatedStorageFile");
			if (closed)
				throw new InvalidOperationException ("Storage needs to be open for this operation.");
			if (checkDirExists && !Directory.Exists (directory.FullName))
				throw new IsolatedStorageException ("Isolated storage has been removed or disabled.");
		}

		bool IsPathInStorage (string path)
		{
			return Path.GetFullPath (path).StartsWith (directory.FullName);
		}
#endif

		private string GetNameFromIdentity (object identity)
		{
			// Note: Default evidences return an XML string with ToString
			byte[] id = Encoding.UTF8.GetBytes (identity.ToString ());
			SHA1 hash = SHA1.Create ();
			// this create an unique name for an identity - bad identities like Url
			// results in bad (i.e. changing) names.
			byte[] full = hash.ComputeHash (id, 0, id.Length);
			byte[] half = new byte [10];
			Buffer.BlockCopy (full, 0, half, 0, half.Length);
			return CryptoConvert.ToHex (half);
		}

		private static object GetTypeFromEvidence (Evidence e, Type t)
		{
			foreach (object o in e) {
				if (o.GetType () == t)
					return o;
			}
			return null;
		}

		internal static object GetAssemblyIdentityFromEvidence (Evidence e)
		{
			// we prefer...
			// a. a Publisher evidence
			object identity = GetTypeFromEvidence (e, typeof (Publisher));
			if (identity != null)
				return identity;
			// b. a StrongName evidence
			identity = GetTypeFromEvidence (e, typeof (StrongName));
			if (identity != null)
				return identity;
			// c. a Url evidence
			return GetTypeFromEvidence (e, typeof (Url));
		}

		internal static object GetDomainIdentityFromEvidence (Evidence e)
		{
			// we prefer...
			// a. a ApplicationDirectory evidence
			object identity = GetTypeFromEvidence (e, typeof (ApplicationDirectory));
			if (identity != null)
				return identity;
			// b. a Url evidence
			return GetTypeFromEvidence (e, typeof (Url));
		}

		[Serializable]
		private struct Identities {
			public object Application;
			public object Assembly;
			public object Domain;

			public Identities (object application, object assembly, object domain)
			{
				Application = application;
				Assembly = assembly;
				Domain = domain;
			}
		}
/*
		[SecurityPermission (SecurityAction.Assert, SerializationFormatter = true)]
		private void LoadIdentities (string root)
		{
			if (!File.Exists (root + ".storage"))
				throw new IsolatedStorageException (Locale.GetText ("Missing identities."));

			BinaryFormatter deformatter = new BinaryFormatter ();
			using (FileStream fs = File.OpenRead (root + ".storage")) {
				Identities identities = (Identities) deformatter.Deserialize (fs);

				_applicationIdentity = identities.Application;
				_assemblyIdentity = identities.Assembly;
				_domainIdentity = identities.Domain;
			}
		}
*/
		[SecurityPermission (SecurityAction.Assert, SerializationFormatter = true)]
		private void SaveIdentities (string root)
		{
			Identities identities = new Identities (_applicationIdentity, _assemblyIdentity, _domainIdentity);
			BinaryFormatter formatter = new BinaryFormatter ();
			mutex.WaitOne ();
			try {
				using (FileStream fs = File.Create (root + ".storage")) {
					formatter.Serialize (fs, identities);
				}
			}
			finally {
				mutex.ReleaseMutex ();
			}
		}
	}
}
#endif
