//
// WriteLineToFile.cs: Writes lines to file.
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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class WriteLinesToFile : TaskExtension {
	
		ITaskItem	file;
		ITaskItem[]	lines;
		bool		overwrite;
		
		StreamWriter	streamWriter;
	
		public WriteLinesToFile ()
		{
		}

		public override bool Execute ()
		{
			try {
				string fullpath = file.GetMetadata ("FullPath");
				if (lines == null && overwrite) {
					System.IO.File.Delete (fullpath);
					return true;
				}

				using (streamWriter = new StreamWriter (fullpath, !overwrite)) {
					if (lines != null)
						foreach (ITaskItem line in lines)
							streamWriter.WriteLine (line);
				}

				return true;
			}
			catch (Exception ex) {
				Log.LogErrorFromException (ex);
				return false;
			}
			finally {
				if (streamWriter != null)
					streamWriter.Close ();
			}
		}

		[Required]
		public ITaskItem File {
			get { return file; }
			set { file = value; }
		}

		public ITaskItem[] Lines {
			get { return lines; }
			set { lines  = value; }
		}

		public bool Overwrite {
			get { return overwrite; }
			set { overwrite = value; }
		}
	}
}

#endif
