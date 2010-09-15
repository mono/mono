//
// Utilities.cs:
//
// Author:
//	Ankit Jain (jankit@novell.com)
//
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.IO;

namespace Microsoft.Build.Tasks {
	internal static class Utilities {

		public static bool RunningOnWindows {
			get {
				// Code from Mono.GetOptions/Options.cs
				// check for non-Unix platforms - see FAQ for more details
				// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
				int platform = (int) Environment.OSVersion.Platform;
				return ((platform != 4) && (platform != 128));
			}

		}

		internal static string FromMSBuildPath (string relPath)
		{
			if (relPath == null || relPath.Length == 0)
				return null;

			bool is_windows = Path.DirectorySeparatorChar == '\\';
			string path = relPath;
			if (!is_windows)
				path = path.Replace ("\\", "/");

			// a path with drive letter is invalid/unusable on non-windows
			if (!is_windows && char.IsLetter (path [0]) && path.Length > 1 && path[1] == ':')
				return null;

			if (System.IO.File.Exists (path)){
				return Path.GetFullPath (path);
			}

			if (Path.IsPathRooted (path)) {

				// Windows paths are case-insensitive. When mapping an absolute path
				// we can try to find the correct case for the path.

				string[] names = path.Substring (1).Split ('/');
				string part = "/";

				for (int n=0; n<names.Length; n++) {
					string[] entries;

					if (names [n] == ".."){
						if (part == "/")
							return ""; // Can go further back. It's not an existing file
						part = Path.GetFullPath (part + "/..");
						continue;
					}

					entries = Directory.GetFileSystemEntries (part);

					string fpath = null;
					foreach (string e in entries) {
						if (string.Compare (Path.GetFileName (e), names[n], true) == 0) {
							fpath = e;
							break;
						}
					}
					if (fpath == null) {
						// Part of the path does not exist. Can't do any more checking.
						part = Path.GetFullPath (part);
						for (; n < names.Length; n++)
							part += "/" + names[n];
						return part;
					}

					part = fpath;
				}
				return Path.GetFullPath (part);
			} else {
				return Path.GetFullPath (path);
			}
		}

	}

}

#endif
