//
// DirectoryScanner.cs: Class used by BuildItem.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Build.BuildEngine {
	internal class DirectoryScanner {
		
		DirectoryInfo	baseDirectory;
		string		includes;
		string		excludes;
		string[]	matchedFilenames;
		
		public DirectoryScanner ()
		{
		}
		
		public void Scan ()
		{
			Dictionary <string, bool> excludedItems;
			List <string> includedItems;
			string[] splitInclude, splitExclude;
			
			if (includes == null)
				throw new ArgumentNullException ("Includes");
			if (excludes == null)
				throw new ArgumentNullException ("Excludes");
			if (baseDirectory == null)
				throw new ArgumentNullException ("BaseDirectory");
			
			excludedItems = new Dictionary <string, bool> ();
			includedItems = new List <string> ();
			
			splitInclude = includes.Split (';');
			splitExclude = excludes.Split (';');
			
			if (excludes != String.Empty) {
				foreach (string si in splitExclude) {
					ProcessExclude (si, excludedItems);
				}
			}
			if (includes != String.Empty) {
				foreach (string si in splitInclude) {
					ProcessInclude (si, excludedItems, includedItems);
				}
			}

			matchedFilenames = includedItems.ToArray ();
		}
		
		private void ProcessInclude (string name, Dictionary <string, bool> excludedItems, List <string> includedItems)
		{
			string[] separatedPath;
			FileInfo[] fileInfo;

			if (name.IndexOf ('?') == -1 && name.IndexOf ('*') == -1) {
				if (!excludedItems.ContainsKey (Path.GetFullPath(name)))
					includedItems.Add (name);
			} else {
				if (name.Split (Path.DirectorySeparatorChar).Length > name.Split (Path.AltDirectorySeparatorChar).Length) {
					separatedPath = name.Split (Path.DirectorySeparatorChar);
				} else {
					separatedPath = name.Split (Path.AltDirectorySeparatorChar);
				}
				if (separatedPath.Length == 1 && separatedPath [0] == String.Empty)
					return;
				fileInfo = ParseIncludeExclude (separatedPath, 0, baseDirectory);
				foreach (FileInfo fi in fileInfo)
					if (!excludedItems.ContainsKey (fi.FullName))
						includedItems.Add (fi.FullName);
			}
		}
		
		private void ProcessExclude (string name, Dictionary <string, bool> excludedItems)
		{
			string[] separatedPath;
			FileInfo[] fileInfo;
			
			if (name.IndexOf ('?') == -1 && name.IndexOf ('*') == -1) {
				if (!excludedItems.ContainsKey (Path.GetFullPath (name)))
					excludedItems.Add (Path.GetFullPath (name), true);
			} else {
				if (name.Split (Path.DirectorySeparatorChar).Length > name.Split (Path.AltDirectorySeparatorChar).Length) {
					separatedPath = name.Split (Path.DirectorySeparatorChar);
				} else {
					separatedPath = name.Split (Path.AltDirectorySeparatorChar);
				}
				if (separatedPath.Length == 1 && separatedPath [0] == String.Empty)
					return;
				fileInfo = ParseIncludeExclude (separatedPath, 0, baseDirectory);
				foreach (FileInfo fi in fileInfo)
					if (!excludedItems.ContainsKey (fi.FullName))
						excludedItems.Add (fi.FullName, true);
			}
		}
		
		private FileInfo[] ParseIncludeExclude (string[] input, int ptr, DirectoryInfo directory)
		{
			if (input.Length > 1 && ptr == 0 && input [0] == String.Empty)
				ptr++;
			if (input.Length == ptr + 1) {
				FileInfo[] fi;
				fi = directory.GetFiles (input [ptr]);
				return fi;
			} else {
				DirectoryInfo[] di;
				FileInfo[] fi;
				List <FileInfo> fileInfos = new List <FileInfo> ();
				if (input [ptr] == ".") {
					di = new DirectoryInfo [1];
					di [0] = directory;
				} else if (input [ptr] == "..") {
					di = new DirectoryInfo [1];
					di [0] = directory.Parent;
				} else if (input[ptr] == "**")
				{
					// Read this directory and all subdirectories recursive
					Stack<DirectoryInfo> currentDirectories = new Stack<DirectoryInfo>();					
					currentDirectories.Push(directory);
					List<DirectoryInfo> allDirectories = new List<DirectoryInfo>();
					
					while (currentDirectories.Count > 0)
					{
						DirectoryInfo current = currentDirectories.Pop();
						allDirectories.Add (current);
						foreach (DirectoryInfo dir in current.GetDirectories())
						{
							currentDirectories.Push(dir);
						}						
					}
					
					// No further directories shall be read
					di = allDirectories.ToArray();					
				} else
					di = directory.GetDirectories (input [ptr]);
				foreach (DirectoryInfo info in di) {
					fi = ParseIncludeExclude (input, ptr + 1, info);
					foreach (FileInfo file in fi)
						fileInfos.Add (file);
				}
				fi = new FileInfo [fileInfos.Count];
				int i = 0;
				foreach (FileInfo file in fileInfos)
					fi [i++] = file;
				return fi;
			}
		}
		
		public DirectoryInfo BaseDirectory {
			get { return baseDirectory; }
			set { baseDirectory = value; }
		}
		
		public string Includes {
			get { return includes; }
			set { includes = value; }
		}
		
		public string Excludes {
			get { return excludes; }
			set { excludes = value; }
		}
		
		public string[] MatchedFilenames {
			get { return matchedFilenames; }
		}
		
	}
}

#endif
