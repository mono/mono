//
// Copy.cs: Task that can copy files
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class Copy : TaskExtension {
	
		ITaskItem[]	copiedFiles;
		ITaskItem[]	destinationFiles;
		ITaskItem	destinationFolder;
		bool		skipUnchangedFiles;
		ITaskItem[]	sourceFiles;
		
		public Copy ()
		{
		}

		public override bool Execute ()
		{
			try {
				List <ITaskItem> temporaryCopiedFiles = new List <ITaskItem> ();
			
				if (sourceFiles.Length != destinationFiles.Length)
					throw new Exception ("Number of source files is different than number of destination files.");
				if (destinationFiles != null && destinationFolder != null)
					throw new Exception ("You must specify only one attribute from DestinationFiles and DestinationFolder");
				if (destinationFiles != null) {
					IEnumerator <ITaskItem> source, destination;
					source = ((IEnumerable <ITaskItem>) sourceFiles).GetEnumerator ();
					destination = ((IEnumerable <ITaskItem>) destinationFiles).GetEnumerator ();
					while (source.MoveNext ()) {
						destination.MoveNext ();
						ITaskItem sourceItem = source.Current;
						ITaskItem destinationItem = destination.Current;
						string sourceFile = sourceItem.GetMetadata ("FullPath");
						string destinationFile = destinationItem.GetMetadata ("FullPath");

						if (skipUnchangedFiles == true) {
							FileInfo sourceInfo = new FileInfo (sourceFile);
							FileInfo destinationInfo = new FileInfo (destinationFile);
							if (sourceInfo.Length == destinationInfo.Length && File.GetLastWriteTime(sourceFile) <=
								File.GetLastWriteTime (destinationFile))
								continue;
						}
						Log.LogMessage ("Copying file from '{0}' to '{1}'", sourceFile, destinationFile);
						File.Copy (sourceFile, destinationFile, true);
						temporaryCopiedFiles.Add (source.Current);
					}
					
				} else if (destinationFolder != null) {
					bool directoryCreated = false;
					string destinationDirectory = destinationFolder.GetMetadata ("FullPath");
					if (Directory.Exists (destinationDirectory) == false) {
						Directory.CreateDirectory (destinationDirectory);
						directoryCreated = true;
					}
					
					IEnumerator <ITaskItem> source;
					source = (IEnumerator <ITaskItem>) sourceFiles.GetEnumerator ();
					while (source.MoveNext ()) {
						ITaskItem sourceItem = source.Current;
						string sourceFile = sourceItem.GetMetadata ("FullPath");
						string filename = sourceItem.GetMetadata ("Filename") + sourceItem.GetMetadata ("Extension");
						string destinationFile = Path.Combine (destinationDirectory,filename);

						if (skipUnchangedFiles == true && directoryCreated == false) {
							FileInfo sourceInfo = new FileInfo (sourceFile);
							FileInfo destinationInfo = new FileInfo (destinationFile);
							if (sourceInfo.Length == destinationInfo.Length && File.GetLastWriteTime(sourceFile) <=
								File.GetLastWriteTime (destinationFile))
								continue;
						}
						Log.LogMessage ("Copying file from '{0}' to '{1}'", sourceFile, destinationFile);
						File.Copy (sourceFile, destinationFile, true);
						temporaryCopiedFiles.Add (source.Current);
					}
				} else {
					throw new Exception ("You must specify DestinationFolder or DestinationFiles attribute.");
				}
				
				copiedFiles = temporaryCopiedFiles.ToArray ();

				return true;
			}
			catch (Exception ex) {
				Log.LogErrorFromException (ex);
				return false;
			}
		}

		[Output]
		public ITaskItem[] CopiedFiles {
			get {
				return copiedFiles;
			}
		}

		[Output]
		public ITaskItem[] DestinationFiles {
			get {
				return destinationFiles;
			}
			set {
				destinationFiles = value;
			}
		}

		public ITaskItem DestinationFolder {
			get {
				return destinationFolder;
			}
			set {
				destinationFolder = value;
			}
		}

		public bool SkipUnchangedFiles {
			get {
				return skipUnchangedFiles;
			}
			set {
				skipUnchangedFiles = value;
			}
		}

		[Required]
		public ITaskItem[] SourceFiles {
			get {
				return sourceFiles;
			}
			set {
				sourceFiles = value;
			}
		}

	}
}

#endif
