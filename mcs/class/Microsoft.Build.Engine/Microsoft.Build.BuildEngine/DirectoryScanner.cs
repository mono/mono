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
			if (!HasWildcard (name)) {
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
					string itemName = fi.FullName;
					if (!Path.IsPathRooted (name) && itemName.Length > baseDirectory.FullName.Length && itemName.StartsWith (baseDirectory.FullName))
						itemName = itemName.Substring (baseDirectory.FullName.Length + 1);

					if (!excludedItems.ContainsKey (itemName) &&  !excludedItems.ContainsKey (Path.GetFullPath (itemName))) {
						TaskItem item = new TaskItem (include_item);
						item.ItemSpec = itemName;

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
			return ParseIncludeExclude (input, ptr, directory, false);
		}

		private FileInfo[] ParseIncludeExclude (string[] input, int ptr, DirectoryInfo directory, bool recursive)
		{
			DirectoryInfo[] di;
			List <FileInfo> fileInfos = new List<FileInfo> ();

			if (input.Length > 1 && ptr == 0 && input [0] == String.Empty)
				ptr++;

			string cur = input.Length > ptr ? input[ptr] : input[input.Length-1];
			bool dot = cur == ".";
			recursive = recursive || cur == "**";
			bool parent = cur == "..";

			if (input.Length <= ptr + 1) {
				if (parent)
					directory = directory.Parent;
				if ((input.Length == ptr + 1 && !recursive) || input.Length <= ptr)
					return directory.GetFiles (cur);
			}

			if (dot) {
				di = new DirectoryInfo [1];
				di [0] = directory;
			} else if (parent) {
				di = new DirectoryInfo [1];
				di [0] = directory.Parent;
			} else if (recursive)
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
			} else
				di = directory.GetDirectories (cur);

			foreach (DirectoryInfo info in di)
				fileInfos.AddRange (ParseIncludeExclude (input, ptr + 1, info, recursive));

			return fileInfos.ToArray ();
		}

		public static bool HasWildcard (string expression)
		{
			return expression.IndexOf ('?') >= 0 || expression.IndexOf ('*') >= 0;
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
