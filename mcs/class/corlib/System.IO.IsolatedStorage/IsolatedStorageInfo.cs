// System.IO.IsolatedStorage.IsolatedStorageInfo.cs:
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
using System.IO;

namespace System.IO.IsolatedStorage {

	internal sealed class IsolatedStorageInfo {
		
		[MonoTODO("Unix Specific; generalize for Win32")]
		internal static string GetIsolatedStorageDirectory ()
		{
			string home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);

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

