//
// SaveFileDialogTest.cs: Tests for SaveFileDialog class.
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
//
// Copyright (c) 2007 Gert Driesen
//
// Authors:
//	Gert Driesen (drieseng@user.sourceforge.net)
//

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class SaveFileDialogTest : TestHelper
	{
		[Test]
		public void AddExtension ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsTrue (sfd.AddExtension, "#1");
			sfd.AddExtension = false;
			Assert.IsFalse (sfd.AddExtension, "#2");
		}

		[Test]
		public void CheckFileExists ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsFalse (sfd.CheckFileExists, "#1");
			sfd.CheckFileExists = true;
			Assert.IsTrue (sfd.CheckFileExists, "#2");
		}

		[Test]
		public void CheckPathExists ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsTrue (sfd.CheckPathExists, "#1");
			sfd.CheckPathExists = false;
			Assert.IsFalse (sfd.CheckPathExists, "#2");
		}

		[Test]
		public void CreatePrompt ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsFalse (sfd.CreatePrompt, "#1");
			sfd.CreatePrompt = true;
			Assert.IsTrue (sfd.CreatePrompt, "#2");
		}

		[Test]
		public void DefaultExt ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsNotNull (sfd.DefaultExt, "#A1");
			Assert.AreEqual (string.Empty, sfd.DefaultExt, "#A2");

			sfd.DefaultExt = "txt";
			Assert.IsNotNull (sfd.DefaultExt, "#B1");
			Assert.AreEqual ("txt", sfd.DefaultExt, "#B2");

			sfd.DefaultExt = null;
			Assert.IsNotNull (sfd.DefaultExt, "#C1");
			Assert.AreEqual (string.Empty, sfd.DefaultExt, "#C2");

			sfd.DefaultExt = ".Xml";
			Assert.IsNotNull (sfd.DefaultExt, "#D1");
			Assert.AreEqual ("Xml", sfd.DefaultExt, "#D2");

			sfd.DefaultExt = ".tar.gz";
			Assert.IsNotNull (sfd.DefaultExt, "#E1");
			Assert.AreEqual ("tar.gz", sfd.DefaultExt, "#E2");

			sfd.DefaultExt = "..Xml";
			Assert.IsNotNull (sfd.DefaultExt, "#F1");
			Assert.AreEqual (".Xml", sfd.DefaultExt, "#F2");

			sfd.DefaultExt = "tar.gz";
			Assert.IsNotNull (sfd.DefaultExt, "#G1");
			Assert.AreEqual ("tar.gz", sfd.DefaultExt, "#G2");

			sfd.DefaultExt = ".";
			Assert.IsNotNull (sfd.DefaultExt, "#H1");
			Assert.AreEqual (string.Empty, sfd.DefaultExt, "#H2");
		}

		[Test]
		public void DereferenceLinks ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsTrue (sfd.DereferenceLinks, "#1");
			sfd.DereferenceLinks = false;
			Assert.IsFalse (sfd.DereferenceLinks, "#2");
		}

		[Test]
		public void FileName ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsNotNull (sfd.FileName, "#A1");
			Assert.AreEqual (string.Empty, sfd.FileName, "#A2");

			sfd.FileName = "default.build";
			Assert.IsNotNull (sfd.FileName, "#B1");
			Assert.AreEqual ("default.build", sfd.FileName, "#B2");

			sfd.FileName = null;
			Assert.IsNotNull (sfd.FileName, "#C1");
			Assert.AreEqual (string.Empty, sfd.FileName, "#C2");

			sfd.FileName = string.Empty;
			Assert.IsNotNull (sfd.FileName, "#D1");
			Assert.AreEqual (string.Empty, sfd.FileName, "#D2");
		}

		[Test]
		public void FileName_InvalidPathCharacter ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			sfd.FileName = Path.InvalidPathChars [0] + "file";
#if NET_2_0
			Assert.IsNotNull (sfd.FileName, "#1");
			Assert.AreEqual (Path.InvalidPathChars [0] + "file", sfd.FileName, "#2");
#else
			try {
				string fileName = sfd.FileName;
				Assert.Fail ("#1: " + fileName);
			} catch (ArgumentException ex) {
				// The path contains illegal characters
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				//Assert.IsNull (ex.ParamName, "#5");
			}
#endif
		}

		[Test]
		public void FileNames ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsNotNull (sfd.FileNames, "#A1");
			Assert.AreEqual (0, sfd.FileNames.Length, "#A2");

			sfd.FileName = "default.build";
			Assert.IsNotNull (sfd.FileNames, "#B1");
			Assert.AreEqual (1, sfd.FileNames.Length, "#B2");
			Assert.AreEqual ("default.build", sfd.FileNames [0], "#B3");

			sfd.FileName = null;
			Assert.IsNotNull (sfd.FileNames, "#C1");
			Assert.AreEqual (0, sfd.FileNames.Length, "#C2");
		}

		[Test]
		public void FileNames_InvalidPathCharacter ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			sfd.FileName = Path.InvalidPathChars [0] + "file";
#if NET_2_0
			Assert.IsNotNull (sfd.FileNames, "#1");
			Assert.AreEqual (1, sfd.FileNames.Length, "#2");
			Assert.AreEqual (Path.InvalidPathChars [0] + "file", sfd.FileNames [0], "#3");
#else
			try {
				string [] fileNames = sfd.FileNames;
				Assert.Fail ("#1: " + fileNames.Length);
			} catch (ArgumentException ex) {
				// The path contains illegal characters
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				//Assert.IsNull (ex.ParamName, "#5");
			}
#endif
		}

		[Test]
		public void Filter ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsNotNull (sfd.Filter, "#A1");
			Assert.AreEqual (string.Empty, sfd.Filter, "#A2");

			sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			Assert.IsNotNull (sfd.Filter, "#B1");
			Assert.AreEqual ("Text files (*.txt)|*.txt|All files (*.*)|*.*", sfd.Filter, "#B2");

			sfd.Filter = null;
			Assert.IsNotNull (sfd.Filter, "#C1");
			Assert.AreEqual (string.Empty, sfd.Filter, "#C2");

			sfd.Filter = string.Empty;
			Assert.IsNotNull (sfd.Filter, "#D1");
			Assert.AreEqual (string.Empty, sfd.Filter, "#D2");
		}

		[Test]
		public void Filter_InvalidFormat ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			try {
				sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)";
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The provided filter string is invalid. The filter string
				// should contain a description of the filter, followed by the
				// vertical bar (|) and the filter pattern. The strings for
				// different filtering options should also be separated by the
				// vertical bar. Example: "Text files (*.txt)|*.txt|All files
				// (*.*)|*.*"
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void FilterIndex ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.AreEqual (1, sfd.FilterIndex, "#1");
			sfd.FilterIndex = 99;
			Assert.AreEqual (99, sfd.FilterIndex, "#2");
			sfd.FilterIndex = -5;
			Assert.AreEqual (-5, sfd.FilterIndex, "#3");
		}

		[Test]
		public void InitialDirectory ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsNotNull (sfd.InitialDirectory, "#A1");
			Assert.AreEqual (string.Empty, sfd.InitialDirectory, "#A2");

			sfd.InitialDirectory = Path.GetTempPath ();
			Assert.IsNotNull (sfd.InitialDirectory, "#B1");
			Assert.AreEqual (Path.GetTempPath (), sfd.InitialDirectory, "#B2");

			sfd.InitialDirectory = null;
			Assert.IsNotNull (sfd.InitialDirectory, "#C1");
			Assert.AreEqual (string.Empty, sfd.InitialDirectory, "#C2");

			string initialDir = Path.Combine (Path.GetTempPath (), 
				"doesnotexistforsure");
			sfd.InitialDirectory = initialDir;
			Assert.IsNotNull (sfd.InitialDirectory, "#D1");
			Assert.AreEqual (initialDir, sfd.InitialDirectory, "#D2");

		}

		[Test]
		public void OverwritePrompt ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsTrue (sfd.OverwritePrompt, "#1");
			sfd.OverwritePrompt = false;
			Assert.IsFalse (sfd.OverwritePrompt, "#2");
		}

		[Test]
		public void Reset ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			sfd.AddExtension = false;
			sfd.CheckFileExists = true;
			sfd.CheckPathExists = false;
			sfd.CreatePrompt = true;
			sfd.DefaultExt = "txt";
			sfd.DereferenceLinks = false;
			sfd.FileName = "default.build";
			sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			sfd.FilterIndex = 5;
			sfd.InitialDirectory = Path.GetTempPath ();
			sfd.OverwritePrompt = false;
			sfd.RestoreDirectory = true;
			sfd.ShowHelp = true;
			sfd.Title = "Saving";
			sfd.ValidateNames = false;
			sfd.Reset ();

			Assert.IsTrue (sfd.AddExtension, "#1");
			Assert.IsFalse (sfd.CheckFileExists, "#2");
			Assert.IsTrue (sfd.CheckPathExists, "#3");
			Assert.IsFalse (sfd.CreatePrompt, "#4");
			Assert.IsNotNull (sfd.DefaultExt, "#5");
			Assert.AreEqual (string.Empty, sfd.DefaultExt, "#6");
			Assert.IsTrue (sfd.DereferenceLinks, "#7");
			Assert.IsNotNull (sfd.FileName, "#8");
			Assert.AreEqual (string.Empty, sfd.FileName, "#9");
			Assert.IsNotNull (sfd.FileNames, "#10");
			Assert.AreEqual (0, sfd.FileNames.Length, "#11");
			Assert.IsNotNull (sfd.Filter, "#12");
			Assert.AreEqual (string.Empty, sfd.Filter, "#13");
			Assert.AreEqual (1, sfd.FilterIndex, "#14");
			Assert.IsNotNull (sfd.InitialDirectory, "#15");
			Assert.AreEqual (string.Empty, sfd.InitialDirectory, "#16");
			Assert.IsTrue (sfd.OverwritePrompt, "#17");
			Assert.IsFalse (sfd.RestoreDirectory, "#18");
			Assert.IsFalse (sfd.ShowHelp, "#19");
			Assert.IsNotNull (sfd.Title, "#20");
			Assert.AreEqual (string.Empty, sfd.Title, "#21");
			Assert.IsTrue (sfd.ValidateNames, "#22");
		}

		[Test]
		public void RestoreDirectory ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsFalse (sfd.RestoreDirectory, "#1");
			sfd.RestoreDirectory = true;
			Assert.IsTrue (sfd.RestoreDirectory, "#2");
		}

		[Test]
		public void ShowHelp ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsFalse (sfd.ShowHelp, "#1");
			sfd.ShowHelp = true;
			Assert.IsTrue (sfd.ShowHelp, "#2");
		}

		[Test]
		public void Title ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsNotNull (sfd.Title, "#A1");
			Assert.AreEqual (string.Empty, sfd.Title, "#A2");

			sfd.Title = "Saving";
			Assert.IsNotNull (sfd.Title, "#B1");
			Assert.AreEqual ("Saving", sfd.Title, "#B2");

			sfd.Title = null;
			Assert.IsNotNull (sfd.Title, "#C1");
			Assert.AreEqual (string.Empty, sfd.Title, "#C2");
		}

		[Test]
		public void ToStringTest ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			sfd.CheckFileExists = true;
			sfd.CreatePrompt = true;
			sfd.DefaultExt = "txt";
			sfd.DereferenceLinks = false;
			sfd.FileName = "default.build";
			sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			sfd.FilterIndex = 5;
			sfd.InitialDirectory = Path.GetTempPath ();
			sfd.OverwritePrompt = false;
			sfd.RestoreDirectory = true;
			sfd.ShowHelp = true;
			sfd.Title = "Saving";
			sfd.ValidateNames = false;

			StringBuilder sb = new StringBuilder ();
			sb.Append (typeof (SaveFileDialog).FullName);
			sb.Append (": Title: ");
			sb.Append (sfd.Title);
			sb.Append (", FileName: ");
			sb.Append (sfd.FileName);

			Assert.AreEqual (sb.ToString (), sfd.ToString (), "#1");

			sfd.FileName = null;
			sfd.Title = null;

			sb.Length = 0;
			sb.Append (typeof (SaveFileDialog).FullName);
			sb.Append (": Title: ");
			sb.Append (sfd.Title);
			sb.Append (", FileName: ");
			sb.Append (sfd.FileName);

			Assert.AreEqual (sb.ToString (), sfd.ToString (), "#2");
		}

		[Test]
		public void ValidateNames ()
		{
			SaveFileDialog sfd = new SaveFileDialog ();
			Assert.IsTrue (sfd.ValidateNames, "#1");
			sfd.ValidateNames = false;
			Assert.IsFalse (sfd.ValidateNames, "#2");
		}
	}
}
