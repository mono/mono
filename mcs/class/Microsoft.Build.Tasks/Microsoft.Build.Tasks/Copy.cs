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
			if (sourceFiles.Length == 0)
				// nothing to copy!
				return true;

			try {
				List <ITaskItem> temporaryCopiedFiles = new List <ITaskItem> ();
			
				if (sourceFiles != null && destinationFiles != null &&
					sourceFiles.Length != destinationFiles.Length)
					throw new Exception ("Number of source files is different than number of destination files.");
				if (destinationFiles != null && destinationFolder != null)
					throw new Exception ("You must specify only one attribute from DestinationFiles and DestinationFolder");
				if (destinationFiles != null && destinationFiles.Length > 0) {
					for (int i = 0; i < sourceFiles.Length; i ++) {
						ITaskItem sourceItem = sourceFiles [i];
						ITaskItem destinationItem = destinationFiles [i];
						string sourceFile = sourceItem.GetMetadata ("FullPath");
						string destinationFile = destinationItem.GetMetadata ("FullPath");

						if (!skipUnchangedFiles || HasFileChanged (sourceFile, destinationFile))
							CopyFile (sourceFile, destinationFile, true);

						sourceItem.CopyMetadataTo (destinationItem);
						temporaryCopiedFiles.Add (destinationItem);
					}
					
				} else if (destinationFolder != null) {
					List<ITaskItem> temporaryDestinationFiles = new List<ITaskItem> ();
					string destinationDirectory = destinationFolder.GetMetadata ("FullPath");
					bool directoryCreated = CreateDirectoryIfRequired (destinationDirectory);
					
					foreach (ITaskItem sourceItem in sourceFiles) {
						string sourceFile = sourceItem.GetMetadata ("FullPath");
						string filename = sourceItem.GetMetadata ("Filename") + sourceItem.GetMetadata ("Extension");
						string destinationFile = Path.Combine (destinationDirectory,filename);

						if (!skipUnchangedFiles || directoryCreated ||
							HasFileChanged (sourceFile, destinationFile))
							CopyFile (sourceFile, destinationFile, false);

						temporaryCopiedFiles.Add (new TaskItem (
								Path.Combine (destinationFolder.GetMetadata ("Identity"), filename),
								sourceItem.CloneCustomMetadata ()));

						temporaryDestinationFiles.Add (new TaskItem (
								Path.Combine (destinationFolder.GetMetadata ("Identity"), filename),
								sourceItem.CloneCustomMetadata ()));
					}
					destinationFiles = temporaryDestinationFiles.ToArray ();
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

		// returns whether directory was created or not
		bool CreateDirectoryIfRequired (string name)
		{
			if (Directory.Exists (name))
				return false;

			Log.LogMessage ("Creating directory '{0}'", name);
			Directory.CreateDirectory (name);
			return true;
		}

		void CopyFile (string source, string dest, bool create_dir)
		{
			if (create_dir)
				CreateDirectoryIfRequired (Path.GetDirectoryName (dest));
			Log.LogMessage ("Copying file from '{0}' to '{1}'", source, dest);
			if (String.Compare (source, dest) != 0)
				File.Copy (source, dest, true);
		}

		bool HasFileChanged (string source, string dest)
		{
			if (!File.Exists (dest))
				return true;

			FileInfo sourceInfo = new FileInfo (source);
			FileInfo destinationInfo = new FileInfo (dest);

			return !(sourceInfo.Length == destinationInfo.Length &&
					File.GetLastWriteTime(source) <= File.GetLastWriteTime (dest));
		}

	}
}

#endif
