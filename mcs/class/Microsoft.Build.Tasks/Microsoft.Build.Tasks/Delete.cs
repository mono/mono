//
// Delete.cs: Task that deletes files.
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
using System.IO;
using System.Security;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks {
	public sealed class Delete : TaskExtension {
	
		ITaskItem[]	deletedFiles;
		ITaskItem[]	files;
		bool		treatErrorsAsWarnings;
		
		public Delete ()
		{
			this.treatErrorsAsWarnings = false;
		}

		public override bool Execute ()
		{
			ArrayList temporaryDeletedFiles = new ArrayList ();
		
			foreach (ITaskItem file in files) {
				try {
					File.Delete (file.GetMetadata ("FullPath"));
					temporaryDeletedFiles.Add (file);
				}
				catch (ArgumentNullException ex) {
					LogException (ex);
				}
				catch (ArgumentException ex) {
					LogException (ex);
				}
				catch (DirectoryNotFoundException ex) {
					LogException (ex);
				}
				catch (SecurityException ex) {
					LogException (ex);
				}
				catch (UnauthorizedAccessException ex) {
					LogException (ex);
				}
				catch (PathTooLongException ex) {
					LogException (ex);
				}
				catch (IOException ex) {
					LogException (ex);
				}
				catch (Exception ex) {
					LogException (ex);
				}
			}
			
			deletedFiles = new ITaskItem [temporaryDeletedFiles.Count];
			int i = 0;
			foreach (ITaskItem file in temporaryDeletedFiles)
				deletedFiles [i++] = file;
			
			return true;
		}
		
		private void LogException (Exception ex)
		{
			if (treatErrorsAsWarnings == true)
				Log.LogWarningFromException (ex);
			else
				Log.LogErrorFromException (ex);
		}

		[Output]
		public ITaskItem[] DeletedFiles {
			get {
				return deletedFiles;
			}
			set {
				deletedFiles = value;
			}
		}

		[Required]
		public ITaskItem[] Files {
			get {
				return files;
			}
			set {
				files = value;
			}
		}

		public bool TreatErrorsAsWarnings {
			get {
				return treatErrorsAsWarnings;
			}
			set {
				treatErrorsAsWarnings = value;
			}
		}
	}
}

#endif