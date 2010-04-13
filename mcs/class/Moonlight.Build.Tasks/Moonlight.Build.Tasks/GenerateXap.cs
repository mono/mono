//
// GenerateXap.cs
//
// Author:
//	Michael Hutchinson <mhutchinson@novell.com>
//	Ankit Jain <jankit@novell.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml;

using Microsoft.CSharp;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Moonlight.Build.Tasks {
	public class GenerateXap : Task {

		public override bool Execute ()
		{
			if (InputFiles.Length == 0)
				return true;

			return Zip ();
		}

		bool Zip ()
		{
			var xapName = XapFilename.ItemSpec;
			if (File.Exists (xapName)) {
				DateTime lastMod = File.GetLastWriteTime (xapName);
				bool needsWrite = false;
				foreach (ITaskItem file_item in InputFiles) {
					if (File.GetLastWriteTime (file_item.ItemSpec) > lastMod) {
						needsWrite = true;
						break;
					}
				}
				if (!needsWrite) {
					Log.LogMessage (MessageImportance.Low, "Skipping xap file {0} generation, its up-to date");
					return true;
				}
			}

			Log.LogMessage (MessageImportance.Normal, "Generating compressed xap file {0}", xapName);
			try {
				using (FileStream fs = new FileStream (xapName, FileMode.Create)) {
					var zip_stream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream (fs);
					zip_stream.SetLevel (9);

					AddFilesToZip (InputFiles, zip_stream);
					AddFilesToZip (LocalCopyReferences, zip_stream);

					zip_stream.Finish ();
					zip_stream.Close ();
				}
			} catch (IOException ex) {
				Log.LogError ("Error writing xap file.", ex);
				Log.LogMessage (MessageImportance.Low, "Error writing xap file:" + ex.ToString ());

				try {
					if (File.Exists (xapName))
						File.Delete (xapName);
				} catch {}

				return false;
			}

			return true;
		}

		void AddFilesToZip (ITaskItem [] files, ICSharpCode.SharpZipLib.Zip.ZipOutputStream zipStream)
		{
			if (files == null)
				return;

			foreach (ITaskItem item in files) {
				string target_path = item.GetMetadata ("TargetPath");
				if (String.IsNullOrEmpty (target_path))
					target_path = Path.GetFileName (item.ItemSpec);

				zipStream.PutNextEntry (new ICSharpCode.SharpZipLib.Zip.ZipEntry (target_path));
				using (FileStream inStream = File.OpenRead (item.ItemSpec)) {
					int readCount;
					byte[] buffer = new byte[4096];

					do {
						readCount = inStream.Read (buffer, 0, buffer.Length);
						zipStream.Write (buffer, 0, readCount);
					} while (readCount > 0);
				}
			}
		}

		[Output]
		[Required]
		public ITaskItem XapFilename {
			get; set;
		}

		[Required]
		public ITaskItem[] InputFiles {
			get; set;
		}

		public ITaskItem[] LocalCopyReferences {
			get; set;
		}

	}


}
