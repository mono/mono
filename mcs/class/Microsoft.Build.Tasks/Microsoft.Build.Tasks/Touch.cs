//
// Touch.cs: Creates a new file.
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
	public class Touch : TaskExtension {
		bool		alwaysCreate;
		ITaskItem[]	files;
		bool		forceTouch;
		DateTime	time;
		ITaskItem[]	touchedFiles;
	
		public Touch ()
		{
			time = DateTime.Now;
		}

		public override bool Execute ()
		{
			if (files.Length == 0)
				return true;

			bool returnBoolean = true;
			List <ITaskItem> successfulFiles = new List <ITaskItem> ();
			Stream stream = null;
			
			foreach (ITaskItem file in files) {
				string fullname = file.GetMetadata ("FullPath");
				try {
					if (File.Exists (file.ItemSpec)) {
						if ((File.GetAttributes (fullname) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
							if (forceTouch) {
								File.SetLastAccessTime (fullname, time);
								File.SetLastWriteTime (fullname, time);
								successfulFiles.Add (file);
							}
						} else {
							File.SetLastAccessTime (fullname, time);
							File.SetLastWriteTime (fullname, time);
							successfulFiles.Add (file);
						}
					} else if (alwaysCreate == true) {
						stream = File.Create (fullname);
						stream.Close ();
						File.SetLastAccessTime (fullname, time);
						File.SetLastWriteTime (fullname, time);
						successfulFiles.Add (file);
					} else {
						continue;
					}

					touchedFiles = successfulFiles.ToArray ();
				}
				catch (Exception ex) {
					Log.LogErrorFromException (ex);
					returnBoolean = false;
				}
			}
			return returnBoolean;
		}

		public bool AlwaysCreate {
			get {
				return alwaysCreate;
			}
			set {
				alwaysCreate = value;
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

		public bool ForceTouch {
			get {
				return forceTouch;
			}
			set {
				forceTouch = value;
			}
		}

		public string Time {
			get {
				return time.ToString ();
			}
			set {
				time = DateTime.Parse (value);
			}
		}

		[Output]
		public ITaskItem[] TouchedFiles {
			get {
				return touchedFiles;
			}
			set {
				touchedFiles = value;
			}
		}
	}
}

#endif
