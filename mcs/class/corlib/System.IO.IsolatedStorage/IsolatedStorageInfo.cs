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
		public static string GetIsolatedStorageDirectory ()
		{
			string home = Environment.GetEnvironmentVariable ("HOME");

			if (home == null)
				home = "~";

			return home + "/.mono/isolated-storage";
		}

		public static string CreateAssemblyFilename (object assembly)
		{
			return string.Format ("{0}/{1}", GetIsolatedStorageDirectory (),
					assembly.ToString());
		}

		public static string CreateDomainFilename (object assembly, object domain)
		{
			return string.Format ("{0}/{1}/{2}", GetIsolatedStorageDirectory (),
					assembly.ToString(), domain.ToString());
		}

		public static ulong GetDirectorySize (DirectoryInfo di)
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

