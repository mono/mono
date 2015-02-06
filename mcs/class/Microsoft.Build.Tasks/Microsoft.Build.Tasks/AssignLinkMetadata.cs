//
// AssignLinkMetadata.cs
//
// Author:
//   Lluis Sanchez (lluis@xamarin.com)
//
// Copyright 2015 Xamarin, Inc (http://www.xamarin.com)
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
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections.Generic;

namespace Microsoft.Build.Tasks {
	public class AssignLinkMetadata : TaskExtension {
	
		ITaskItem[]	assignedFiles;
		ITaskItem[]	files;
	
		public AssignLinkMetadata ()
		{
		}
		
		public override bool Execute ()
		{
			if (files == null || files.Length == 0)
				//nothing to do
				return true;

			List<ITaskItem> outFiles = new List<ITaskItem> ();

			for (int i = 0; i < files.Length; i ++) {
				string file = files [i].ItemSpec;
				string link = files [i].GetMetadata ("Link");
				string definingProjectPath = files [i].GetMetadata ("DefiningProjectFullPath");

				if (String.IsNullOrEmpty (link) && Path.IsPathRooted (file) && !string.IsNullOrEmpty (definingProjectPath)) {
					file = Path.GetFullPath (file);
					var projectDir = Path.GetFullPath (Path.GetDirectoryName (definingProjectPath));
					if (projectDir.Length == 0 || projectDir [projectDir.Length - 1] != Path.DirectorySeparatorChar)
						projectDir += Path.DirectorySeparatorChar;
					if (file.StartsWith (projectDir)) {
						// The file is in the same folder or a subfolder of the project that contains it.
						// Use the relative path wrt the containing project as link.
						var outFile = new TaskItem (files [i]);
						outFile.SetMetadata ("Link", file.Substring (projectDir.Length));
						outFiles.Add (outFile);
					}
				}
			}

			assignedFiles = outFiles.ToArray ();

			return true;
		}
		
		[Output]
		public ITaskItem[] OutputItems {
			get { return assignedFiles; }
		}
		
		public ITaskItem[] Items {
			get { return files; }
			set { files = value; }
		}
	}
}
