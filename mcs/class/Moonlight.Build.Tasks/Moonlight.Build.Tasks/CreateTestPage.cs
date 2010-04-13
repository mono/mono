//
// CreateTestPage.cs: Generates test page for moonlight app
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
using System.IO;
using System.Reflection;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Moonlight.Build.Tasks {
	public class CreateTestPage : Task {

		public override bool Execute ()
		{
			Log.LogMessage (MessageImportance.Low, "Generating test page {0}", XapFilename);

			var sb = new StringBuilder ();
			using (var sr = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("PreviewTemplate.html")))
				sb.Append (sr.ReadToEnd ());

			sb.Replace ("@TITLE@", Title);
			sb.Replace ("@XAP_FILE@", XapFilename);

			try{
				File.WriteAllText (TestPageFilename, sb.ToString ());
			} catch (IOException e) {
				Log.LogError (String.Format (
						"Error generating test page file {0}: {1}", TestPageFilename, e.Message));
				return false;
			}

			return true;
		}

		[Required]
		public string XapFilename {
			get; set;
		}

		[Required]
		public string Title {
			get; set;
		}

		[Required]
		[Output]
		public string TestPageFilename {
			get; set;
		}
	}
}
