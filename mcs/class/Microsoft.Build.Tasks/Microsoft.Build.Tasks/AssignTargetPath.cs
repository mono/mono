//
// AssignTargetPath.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
//
// (C) 2006 Marek Sieradzki
// Copyright 2008 Novell, Inc (http://www.novell.com)
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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class AssignTargetPath : TaskExtension {
	
		ITaskItem[]	assignedFiles;
		ITaskItem[]	files;
		string		rootFolder;
	
		public AssignTargetPath ()
		{
		}
		
		public override bool Execute ()
		{
			if (files == null || files.Length == 0)
				//nothing to do
				return true;

			assignedFiles = new ITaskItem [files.Length];
			for (int i = 0; i < files.Length; i ++) {
				string file = files [i].ItemSpec;
				string link = files [i].GetMetadata ("Link");
				string afile = null;

				if (String.IsNullOrEmpty (link)) {
					//FIXME: Hack!
					string normalized_root = Path.GetFullPath (rootFolder);

					// cur dir should already be set to
					// the project dir
					file = Path.GetFullPath (file);

					if (file.StartsWith (normalized_root)) {
						afile = Path.GetFullPath (file).Substring (
								normalized_root.Length);
						// skip over "root/"
						if (afile [0] == '\\' ||
							afile [0] == '/')
							afile = afile.Substring (1);

					} else {
						afile = Path.GetFileName (file);
					}
				} else {
					afile = link;
				}

				assignedFiles [i] = new TaskItem (files [i]);
				assignedFiles [i].SetMetadata ("TargetPath", afile);
			}

			return true;
		}
		
		[Output]
		public ITaskItem[] AssignedFiles {
			get { return assignedFiles; }
		}
		
		public ITaskItem[] Files {
			get { return files; }
			set { files = value; }
		}
		
		[Required]
		public string RootFolder {
			get { return rootFolder; }
			set { rootFolder = value; }
		}
	}
}

#endif
