// System.IO.IsolatedStorage.IsolatedStorageInfo.cs:
//
// Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2003 Jonathan Pryor

using System;
using System.IO;

namespace System.IO.IsolatedStorage {

	internal sealed class IsolatedStorageInfo {
		
		[MonoTODO("Unix Specific; generalize for Win32")]
		internal static string GetIsolatedStorageDirectory ()
		{
			string home = Environment.GetEnvironmentVariable ("HOME");

			if (home == null)
				home = "~";

			return home + "/.mono/isolated-storage";
		}

		internal static string CreateAssemblyFilename (object assembly)
		{
                        return Path.Combine (GetIsolatedStorageDirectory (), assembly.ToString ());
		}

		internal static string CreateDomainFilename (object assembly, object domain)
		{
			return Path.Combine (CreateAssemblyFilename (assembly), domain.ToString());
		}

		internal static ulong GetDirectorySize (DirectoryInfo di)
		{
			ulong size = 0;

			foreach (FileInfo fi in di.GetFiles ())
				size += (ulong) fi.Length;

			foreach (DirectoryInfo d in di.GetDirectories())
				size += GetDirectorySize (d);

			return size;
		}
	}
}

