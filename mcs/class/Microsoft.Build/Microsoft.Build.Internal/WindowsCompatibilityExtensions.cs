//
// WindowsCompatibilityExtensions.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// (C) 2013 Xamarin Inc.
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
using Mono.XBuild.Utilities;

namespace Microsoft.Build.Internal
{
	static class WindowsCompatibilityExtensions
	{
		public static string NormalizeFilePath (string path)
		{
			if (MSBuildUtils.RunningOnWindows || string.IsNullOrWhiteSpace (path) || File.Exists (path) || Directory.Exists (path))
				return path;
			return path.Replace ('\\', Path.DirectorySeparatorChar);
		}
		
		public static string FindMatchingPath (string path)
		{
			if (MSBuildUtils.RunningOnWindows || string.IsNullOrWhiteSpace (path) || File.Exists (path) || Directory.Exists (path))
				return path;
			path = path.Replace ('\\', Path.DirectorySeparatorChar);
			var file = Path.GetFileName (path);
			var dir = FindMatchingPath (Path.GetDirectoryName (path));
			if (Directory.Exists (dir)) {
				foreach (FileSystemInfo e in new DirectoryInfo (dir.Length > 0 ? dir : ".").EnumerateFileSystemInfos ()) {
					if (e.Name.Equals (file, StringComparison.OrdinalIgnoreCase))
						return dir.Length > 0 ? Path.Combine (dir, e.Name) : e.Name;
				}
			}
			// The directory part is still based on case insensitive match.
			return dir.Length > 0 ? Path.Combine (dir, file) : file;
		}
	}
}

