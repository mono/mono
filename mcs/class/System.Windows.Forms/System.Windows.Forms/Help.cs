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
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace System.Windows.Forms
{
	/// <summary>
	/// http://msdn.microsoft.com/en-us/library/System.Windows.Forms.Help(v=vs.110).aspx
	/// </summary>
	public class Help
	{
		#region Constructor
		private Help ()
		{
		}
		#endregion

		#region Public Static Methods
		public static void ShowHelp (Control parent, string url)
		{
			ShowHelp (parent, url, null);
		}

		public static void ShowHelp (Control parent, string url, HelpNavigator navigator)
		{
			ShowHelp(parent, url, navigator, null);
		}

		[MonoTODO ("Stub, does nothing")]
		public static void ShowHelp (Control parent, string url, HelpNavigator command, object parameter)
		{
		}

		public static void ShowHelp (Control parent, string url, string keyword)
		{
			ShowHelpTopic (url, keyword);
		}

		public static void ShowHelpIndex (Control parent, string url)
		{
			ShowHelp (parent, url, HelpNavigator.Index, null);
		}

		[MonoTODO ("Stub, does nothing")]
		public static void ShowPopup (Control parent, string caption, Point location)
		{
		}
		#endregion	// Public Static Methods

		/// <summary>
		/// Show a help file and topic using a help viewer
		/// </summary>
		/// <param name="helpFile">path to a .chm help file</param>
		/// <param name="helpTopic">path to a topic in helpFile, or null</param>
		private static void ShowHelpTopic (string helpFile, string helpTopic)
		{
			if (helpFile == null)
				throw new ArgumentNullException ();
			if (helpFile == String.Empty)
				throw new ArgumentException ();

			// Use forward slashes in helpFile path if needed
			helpFile = helpFile.Replace (@"\", Path.DirectorySeparatorChar.ToString ());

			string helpViewer = Environment.GetEnvironmentVariable ("MONO_HELP_VIEWER") ?? "chmsee";
			string arguments = String.Format ("\"{0}\"", helpFile);
			if (!String.IsNullOrEmpty (helpTopic)) {
				if (!helpTopic.StartsWith ("/"))
					helpTopic = "/" + helpTopic;
				helpTopic = helpTopic.TrimEnd (' ');
				arguments = String.Format ("\"{0}::{1}\"", helpFile, helpTopic);
			}

			try {
				RunNonblockingProcess (helpViewer, arguments);
			} catch (Exception e) {
				// Don't crash if the help viewer couldn't be launched. There
				// won't be an exception thrown if the help viewer can't find
				// the help file; it's up to the help viewer to display such an error.
				string message = String.Format ("The help viewer could not load. Maybe you don't have {0} installed or haven't set MONO_HELP_VIEWER. The specific error message was: {1}", helpViewer, e.Message);
				Console.Error.WriteLine (message);
				MessageBox.Show (message);
			}
		}

		/// <remarks>
		/// throws exception from Process.Start() if there was a problem starting
		/// </remarks>
		private static void RunNonblockingProcess (string command, string arguments)
		{
			using (Process process = new Process ()) {
				process.StartInfo.FileName = command;
				process.StartInfo.Arguments = arguments;
				process.StartInfo.UseShellExecute = false;

				process.Start ();
			}
		}
	}
}
