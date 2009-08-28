//
// FindUnderPath.cs: Find files under the path.
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

namespace Microsoft.Build.Tasks {
	public class FindUnderPath : TaskExtension {
	
		ITaskItem[]	files;
		ITaskItem[]	inPath;
		ITaskItem[]	outOfPath;
		ITaskItem	path;
		
		public FindUnderPath ()
		{
		}

		public override bool Execute ()
		{
			if (files == null || files.Length == 0)
				return true;

			List <ITaskItem> temporaryInPath = new List <ITaskItem> ();
			List <ITaskItem> temporaryOutOfPath = new List <ITaskItem> ();
			
			foreach (ITaskItem file in files) {
				try {
					string fullPath = path.GetMetadata ("FullPath");;
					string fileFullPath = file.GetMetadata ("FullPath");
					if (System.IO.Path.GetDirectoryName (fullPath) != null) {
						string fullPath1 = fullPath + System.IO.Path.DirectorySeparatorChar;
						string fullPath2 = fullPath + System.IO.Path.AltDirectorySeparatorChar;
						if (fileFullPath.StartsWith (fullPath1) || fileFullPath.StartsWith (fullPath2))
							temporaryInPath.Add (file);
						else
							temporaryOutOfPath.Add (file);
					} else if (System.IO.Path.GetDirectoryName (fullPath) == String.Empty) {
						throw new Exception ("Path contains no directory information.");
					} else {
						if (fileFullPath.StartsWith (fullPath))
							temporaryInPath.Add (file);
						else
							temporaryOutOfPath.Add (file);
					}
				}
				catch (Exception ex) {
					Log.LogErrorFromException (ex);
				}
			}
			
			inPath = temporaryInPath.ToArray ();
			outOfPath = temporaryOutOfPath.ToArray ();

			return true;
		}

		public ITaskItem[] Files {
			get {
				return files;
			}
			set {
				files = value;
			}
		}

		[Output]
		public ITaskItem[] InPath {
			get {
				return inPath;
			}
			set {
				inPath = value;
			}
		}

		[Output]
		public ITaskItem[] OutOfPath {
			get {
				return outOfPath;
			}
			set {
				outOfPath = value;
			}
		}

		[Required]
		public ITaskItem Path {
			get {
				return path;
			}
			set {
				path = value;
			}
		}
	}
}

#endif
