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

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class DirectoryScanner {
		
		DirectoryInfo	baseDirectory;
		ITaskItem[]	includes, excludes;
		ITaskItem[]	matchedItems;

		static bool _runningOnWindows;
		
		static DirectoryScanner ()
		{
			PlatformID pid = Environment.OSVersion.Platform;
			_runningOnWindows =((int) pid != 128 && (int) pid != 4 && (int) pid != 6);
		}

		public DirectoryScanner ()
		{
		}
		
		public void Scan ()
		{
			Dictionary <string, bool> excludedItems;
			List <ITaskItem> includedItems;
			string[] splitExclude;
			
			if (includes == null)
				throw new ArgumentNullException ("Includes");
			if (baseDirectory == null)
				throw new ArgumentNullException ("BaseDirectory");
			
			excludedItems = new Dictionary <string, bool> ();
			includedItems = new List <ITaskItem> ();
			
			if (excludes != null)
				foreach (ITaskItem excl in excludes)
					ProcessExclude (excl.ItemSpec, excludedItems);

			foreach (ITaskItem include_item in includes)
				ProcessInclude (include_item, excludedItems, includedItems);

			matchedItems = includedItems.ToArray ();
		}
		
		private void ProcessInclude (ITaskItem include_item, Dictionary <string, bool> excludedItems,
				List <ITaskItem> includedItems)
		{
			string[] separatedPath;
			FileInfo[] fileInfo;

			string name = include_item.ItemSpec;
			if (name.IndexOf ('?') == -1 && name.IndexOf ('*') == -1) {
				if (!excludedItems.ContainsKey (Path.GetFullPath(name)))
					includedItems.Add (include_item);
			} else {
				if (name.Split (Path.DirectorySeparatorChar).Length > name.Split (Path.AltDirectorySeparatorChar).Length) {
					separatedPath = name.Split (new char [] {Path.DirectorySeparatorChar},
							StringSplitOptions.RemoveEmptyEntries);
				} else {
					separatedPath = name.Split (new char [] {Path.AltDirectorySeparatorChar},
							StringSplitOptions.RemoveEmptyEntries);
				}
				if (separatedPath.Length == 1 && separatedPath [0] == String.Empty)
					return;

				int offset = 0;
				if (Path.IsPathRooted (name)) {
					baseDirectory = new DirectoryInfo (Path.GetPathRoot (name));
					if (IsRunningOnWindows)
						// skip the "drive:"
						offset = 1;
				}

				string full_path = Path.GetFullPath (Path.Combine (Environment.CurrentDirectory, include_item.ItemSpec));
				fileInfo = ParseIncludeExclude (separatedPath, offset, baseDirectory);

				int wildcard_offset = full_path.IndexOf ("**");
				foreach (FileInfo fi in fileInfo) {
					if (!excludedItems.ContainsKey (fi.FullName)) {
						TaskItem item = new TaskItem (include_item);
						item.ItemSpec = fi.FullName;
						if (wildcard_offset >= 0) {
							string rec_dir = Path.GetDirectoryName (fi.FullName.Substring (wildcard_offset));
							if (rec_dir.Length > 0)
								rec_dir += Path.DirectorySeparatorChar;
							item.SetMetadata ("RecursiveDir", rec_dir);
						}
						includedItems.Add (item);
					}
				}
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
					separatedPath = name.Split (new char [] {Path.DirectorySeparatorChar},
									StringSplitOptions.RemoveEmptyEntries);
				} else {
					separatedPath = name.Split (new char [] {Path.AltDirectorySeparatorChar},
									StringSplitOptions.RemoveEmptyEntries);
				}
				if (separatedPath.Length == 1 && separatedPath [0] == String.Empty)
					return;

				int offset = 0;
				if (Path.IsPathRooted (name)) {
					baseDirectory = new DirectoryInfo (Path.GetPathRoot (name));
					if (IsRunningOnWindows)
						// skip the "drive:"
						offset = 1;
				}
				fileInfo = ParseIncludeExclude (separatedPath, offset, baseDirectory);
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
						allDirectories.Insert (0, current);
						foreach (DirectoryInfo dir in current.GetDirectories())
						{
							currentDirectories.Push(dir);
						}						
					}
					
					// No further directories shall be read
					di = allDirectories.ToArray();					
				} else {
					di = directory.GetDirectories (input [ptr]);
				}
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
		
		public ITaskItem[] Includes {
			get { return includes; }
			set { includes = value; }
		}
		
		public ITaskItem[] Excludes {
			get { return excludes; }
			set { excludes = value; }
		}
		
		public ITaskItem[] MatchedItems {
			get { return matchedItems; }
		}
		
		static bool IsRunningOnWindows {
			get { return _runningOnWindows; }
		}
	}
}

#endif
