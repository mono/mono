// System.IO.IsolatedStorage.IsolatedStorageFile
//
// Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2003 Jonathan Pryor

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace System.IO.IsolatedStorage
{
	// This is a terribly named class.  It doesn't actually represent a file as
	// much as a directory
	[MonoTODO]
	public sealed class IsolatedStorageFile : IsolatedStorage, IDisposable
	{
		private DirectoryInfo directory;

		private IsolatedStorageFile (string directory)
		{
			this.directory = new DirectoryInfo (directory);
			this.directory.Create ();
		}

		[CLSCompliant(false)]
		public override ulong CurrentSize {
			get {return IsolatedStorageInfo.GetDirectorySize(directory);}
		}

		[CLSCompliant(false)]
		[MonoTODO("The IsolatedStorage area should be limited, to prevent DOS attacks.  What's a reasonable size?")]
		public override ulong MaximumSize {
			get {return ulong.MaxValue;}
		}

		[MonoTODO ("Pay attention to scope")]
		public static IEnumerator GetEnumerator (IsolatedStorageScope scope)
		{
      Array a = Directory.GetFileSystemEntries (IsolatedStorageInfo.GetIsolatedStorageDirectory());
			return a.GetEnumerator ();
		}

		[MonoTODO]
		public static IsolatedStorageFile GetStore (
			IsolatedStorageScope scope,
			System.Security.Policy.Evidence domainEvidence,
			Type domainEvidenceType,
			System.Security.Policy.Evidence assemblyEvidence,
			Type assemblyEvidenceType)
		{
			return GetStore (scope);
		}

		[MonoTODO]
		public static IsolatedStorageFile GetStore (
			IsolatedStorageScope scope,
			object domainIdentity,
			object assemblyIdentity)
		{
			return GetStore (scope);
		}

		[MonoTODO]
		public static IsolatedStorageFile GetStore (
			IsolatedStorageScope scope,
			Type domainEvidenceType,
			Type assemblyEvidenceType)
		{
			return GetStore (scope);
		}

		private static IsolatedStorageFile GetStore (IsolatedStorageScope scope)
		{
			string dir = GetScopeDirectory (scope);

                        storage_scope = scope;

			return new IsolatedStorageFile (dir);
		}

		private static string GetScopeDirectory (IsolatedStorageScope scope)
		{
			string dir = "";

			if ((scope & IsolatedStorageScope.Domain) != 0)
				dir = IsolatedStorageInfo.CreateDomainFilename (
						Assembly.GetEntryAssembly (),
						AppDomain.CurrentDomain);
			else
				dir = IsolatedStorageInfo.CreateAssemblyFilename (
						Assembly.GetEntryAssembly ());

			return dir;
		}

		public static IsolatedStorageFile GetUserStoreForAssembly ()
		{
			return GetStore (IsolatedStorageScope.User | IsolatedStorageScope.Assembly);
		}

		public static IsolatedStorageFile GetUserStoreForDomain ()
		{
			return GetStore (IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly);
		}

		public static void Remove (IsolatedStorageScope scope)
		{
			string dir = GetScopeDirectory (scope);
			Directory.Delete (dir, true);
		}

		public void Close ()
		{
		}

		public void CreateDirectory (string dir)
		{
			directory.CreateSubdirectory (dir);
		}

		public void DeleteDirectory (string dir)
		{
			DirectoryInfo subdir = directory.CreateSubdirectory (dir);
			subdir.Delete ();
		}

		public void DeleteFile (string file)
		{
			File.Delete (directory.Name + "/" + file);
		}

		public void Dispose ()
		{
		}

		public string[] GetDirectoryNames (string searchPattern)
		{
			DirectoryInfo[] adi = directory.GetDirectories (searchPattern);
      return GetNames (adi);
		}

    private string[] GetNames (FileSystemInfo[] afsi)
    {
      string[] r = new string[afsi.Length];
      for (int i = 0; i != afsi.Length; ++i)
        r[i] = afsi[i].Name;
      return r;
    }

		public string[] GetFileNames (string searchPattern)
		{
      FileInfo[] afi = directory.GetFiles (searchPattern);
      return GetNames (afi);
		}

		public override void Remove ()
		{
			directory.Delete (true);
		}

		~IsolatedStorageFile ()
		{
		}

		[MonoTODO]
		protected override IsolatedStoragePermission GetPermission (PermissionSet ps)
		{
			throw new NotImplementedException ();
		}
	}
}

