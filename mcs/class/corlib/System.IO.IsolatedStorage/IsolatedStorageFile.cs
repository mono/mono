// System.IO.IsolatedStorage.IsolatedStorageFile
//
// Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2003 Jonathan Pryor

//
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace System.IO.IsolatedStorage
{
	// This is a terribly named class.  It doesn't actually represent a file as
	// much as a directory
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
		[MonoTODO ("The IsolatedStorage area should be limited, to prevent DOS attacks.  What's a reasonable size?")]
		public override ulong MaximumSize {
			get {return ulong.MaxValue;}
		}

		[MonoTODO ("Pay attention to scope")]
		public static IEnumerator GetEnumerator (IsolatedStorageScope scope)
		{
			Array a = Directory.GetFileSystemEntries (IsolatedStorageInfo.GetIsolatedStorageDirectory());
			return a.GetEnumerator ();
		}

		[MonoTODO ("Functional but missing CAS support")]
		public static IsolatedStorageFile GetStore (
			IsolatedStorageScope scope,
			System.Security.Policy.Evidence domainEvidence,
			Type domainEvidenceType,
			System.Security.Policy.Evidence assemblyEvidence,
			Type assemblyEvidenceType)
		{
			return GetStore (scope);
		}

		[MonoTODO ("Functional but missing CAS support")]
		public static IsolatedStorageFile GetStore (
			IsolatedStorageScope scope,
			object domainIdentity,
			object assemblyIdentity)
		{
			return GetStore (scope);
		}

		[MonoTODO ("Functional but missing CAS support")]
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

			IsolatedStorageFile storageFile = new IsolatedStorageFile (dir);
			storageFile.InitStore (scope, (Type) null, (Type) null);
			return storageFile;
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

		[MonoTODO ("Permissions are CAS related")]
		protected override IsolatedStoragePermission GetPermission (PermissionSet ps)
		{
			throw new NotImplementedException ();
		}
	}
}

