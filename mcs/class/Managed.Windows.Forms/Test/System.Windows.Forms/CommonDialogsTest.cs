//
// CommonDialogsTest.cs: Tests for common dialogs.
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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Alexander Olk <alex.olk@googlemail.com>
//

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class CommonDialogsTest : TestHelper
	{
		OpenFileDialog ofd;
		SaveFileDialog sfd;
		FontDialog fd;
		FolderBrowserDialog fbd;
		ColorDialog cd;
		
		[Test]
		public void ColorDialogTest ()
		{
			cd = new ColorDialog ();
			
			Assert.AreEqual (Color.Black, cd.Color, "#1");
			Assert.IsTrue (cd.AllowFullOpen, "#2");
			Assert.IsFalse (cd.AnyColor, "#3");
			Assert.IsFalse (cd.FullOpen, "#4");
			Assert.IsNotNull (cd.CustomColors, "#5");
			Assert.IsFalse (cd.ShowHelp, "#6");
			Assert.IsFalse (cd.SolidColorOnly, "#7");
			Assert.AreEqual ("System.Windows.Forms.ColorDialog,  Color: Color [Black]", cd.ToString (), "#8");
			
			cd.Color = Color.Red;
			Assert.AreEqual (Color.Red, cd.Color, "#9");
			
			cd.AllowFullOpen = false;
			cd.FullOpen = true;
			Assert.IsTrue (cd.FullOpen, "#10");
			
			int[] custom_colors = new int[] {Color.Yellow.ToArgb (), Color.Red.ToArgb ()};
			cd.CustomColors = custom_colors;
			Assert.IsNotNull (cd.CustomColors, "#10a");
			Assert.AreEqual (16, cd.CustomColors.Length, "#10aa");
			Assert.AreEqual (Color.Red.ToArgb (), cd.CustomColors[1], "#10ab");
			Assert.AreEqual (Color.FromArgb(0, 255, 255, 255).ToArgb (), cd.CustomColors[15], "#10ac");
			
			cd.CustomColors = null;
			Assert.IsNotNull (cd.CustomColors, "#10b");
			Assert.AreEqual (16, cd.CustomColors.Length, "#10bb");
			Assert.AreEqual (Color.FromArgb(0, 255, 255, 255).ToArgb (), cd.CustomColors[0], "#10bc");
			
			cd.AllowFullOpen = true;
			cd.CustomColors = custom_colors;
			Assert.IsNotNull (cd.CustomColors, "#10c");
			Assert.AreEqual (16, cd.CustomColors.Length, "#10cc");
		}
		
		[Test]
		public void OpenFileDialogTest ()
		{
			ofd = new OpenFileDialog ();
			
			Assert.IsTrue (ofd.AddExtension, "#11");
			Assert.IsTrue (ofd.CheckFileExists, "#12");
			Assert.IsTrue (ofd.CheckPathExists, "#13");
			Assert.AreEqual ("", ofd.DefaultExt, "#14");
			Assert.IsTrue (ofd.DereferenceLinks, "#15");
			Assert.AreEqual ("", ofd.FileName, "#16");
			Assert.IsNotNull (ofd.FileNames, "#17");
			Assert.AreEqual (0, ofd.FileNames.Length, "#17a");
			Assert.AreEqual ("", ofd.Filter, "#18");
			Assert.AreEqual (1, ofd.FilterIndex, "#19");
			Assert.AreEqual ("", ofd.InitialDirectory, "#20");
			Assert.IsFalse (ofd.Multiselect, "#21");
			Assert.IsFalse (ofd.ReadOnlyChecked, "#22");
			Assert.IsFalse (ofd.RestoreDirectory, "#23");
			Assert.IsFalse (ofd.ShowHelp, "#24");
			Assert.IsFalse (ofd.ShowReadOnly, "#25");
			Assert.AreEqual ("", ofd.Title, "#26");
			Assert.IsTrue (ofd.ValidateNames, "#27");
			Assert.AreEqual ("System.Windows.Forms.OpenFileDialog: Title: , FileName: ", ofd.ToString (), "#28");
			
			ofd.DefaultExt = ".TXT";
			Assert.AreEqual ("TXT", ofd.DefaultExt, "#29");
			
			ofd.Filter = null;
			Assert.AreEqual ("", ofd.Filter, "#30");
			
			ofd.Filter = "Text (*.txt)|*.txt|All (*.*)|*.*";
			
			try {
				ofd.Filter = "abcd";
			} catch (Exception) {
			}
			
			Assert.AreEqual ("Text (*.txt)|*.txt|All (*.*)|*.*", ofd.Filter, "#30a");
			
			ofd.FilterIndex = 10;
			Assert.AreEqual (10, ofd.FilterIndex, "#30aa");
			
			ofd.Filter = null;
			Assert.AreEqual ("", ofd.Filter, "#30b");
			Assert.AreEqual (10, ofd.FilterIndex, "#30ba");
			
			string current_path = Environment.CurrentDirectory;
			string current_file = Path.Combine(current_path, "test_file");
			if (!File.Exists (current_file))
				File.Create (current_file);
			
			ofd.FileName = current_file;
			
			Assert.AreEqual (current_file, ofd.FileName, "#31");
			
			string[] file_names = ofd.FileNames;
			Assert.AreEqual (current_file, file_names [0], "#32");
			
			ofd.Title = "Test";
			Assert.AreEqual ("System.Windows.Forms.OpenFileDialog: Title: Test, FileName: " + current_file, ofd.ToString (), "#33");
			
			ofd.FileName = null;
			Assert.AreEqual ("", ofd.FileName, "#33a");
			Assert.IsNotNull (ofd.FileNames, "#33b");
			Assert.AreEqual (0, ofd.FileNames.Length, "#33c");
			
			ofd.Reset ();
			
			// check again
			Assert.IsTrue (ofd.AddExtension, "#34");
			Assert.IsTrue (ofd.CheckFileExists, "#35");
			Assert.IsTrue (ofd.CheckPathExists, "#36");
			Assert.AreEqual ("", ofd.DefaultExt, "#37");
			Assert.IsTrue (ofd.DereferenceLinks, "#38");
			Assert.AreEqual ("", ofd.FileName, "#39");
			Assert.IsNotNull (ofd.FileNames, "#40");
			Assert.AreEqual ("", ofd.Filter, "#41");
			Assert.AreEqual (1, ofd.FilterIndex, "#42");
			Assert.AreEqual ("", ofd.InitialDirectory, "#43");
			Assert.IsFalse (ofd.Multiselect, "#44");
			Assert.IsFalse (ofd.ReadOnlyChecked, "#45");
			Assert.IsFalse (ofd.RestoreDirectory, "#46");
			Assert.IsFalse (ofd.ShowHelp, "#47");
			Assert.IsFalse (ofd.ShowReadOnly, "#48");
			Assert.AreEqual ("", ofd.Title, "#49");
			Assert.IsTrue (ofd.ValidateNames, "#50");
			Assert.AreEqual ("System.Windows.Forms.OpenFileDialog: Title: , FileName: ", ofd.ToString (), "#60");
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void FileDialogFilterArgumentException () {
			if (ofd == null)
				ofd = new OpenFileDialog ();
			
			ofd.Filter = "xyafj";
		}
		
		[Test]
		public void SaveFileDialogTest ()
		{
			// most of the OpenFileDialogTest are also valid for SaveFileDialg
			sfd = new SaveFileDialog ();
			
			Assert.IsFalse (sfd.CreatePrompt, "#61");
			
			Assert.IsTrue (sfd.OverwritePrompt, "#62");
		}
		
		[Test]
		public void FontDialogTest ()
		{
			fd = new FontDialog ();
			
			Assert.IsTrue (fd.AllowScriptChange, "#63");
			Assert.IsTrue (fd.AllowSimulations, "#64");
			Assert.IsTrue (fd.AllowVectorFonts, "#65");
			Assert.IsTrue (fd.AllowVerticalFonts, "#66");
			
			Assert.AreEqual (Color.Black, fd.Color, "#67");
			Assert.IsFalse (fd.FixedPitchOnly, "#68");
			
			//Assert.AreEqual ("[Font: Name=Microsoft Sans Serif, Size=8,25, Units=3, GdiCharSet=0, GdiVerticalFont=False]", fd.Font.ToString (), "#69");
			
			Assert.IsFalse (fd.FontMustExist, "#70");
			
			Assert.AreEqual (0, fd.MaxSize, "#71");
			Assert.AreEqual (0, fd.MinSize, "#72");
			
			Assert.IsFalse (fd.ScriptsOnly, "#73");
			
			Assert.IsFalse (fd.ShowApply, "#74");
			
			Assert.IsFalse (fd.ShowColor, "#75");
			
			Assert.IsTrue (fd.ShowEffects, "#76");
			
			Assert.IsFalse (fd.ShowHelp, "#77");
			
			//Assert.AreEqual ("System.Windows.Forms.FontDialog,  Font: [Font: Name=Microsoft Sans Serif, Size=8,25, Units=3, GdiCharSet=0, GdiVerticalFont=False]", fd.ToString (), "#78");
			
			fd.MaxSize = -1;
			Assert.AreEqual (0, fd.MaxSize, "#79");
			
			fd.MinSize = -1;
			Assert.AreEqual (0, fd.MinSize, "#80");
			
			fd.MinSize = 24;
			fd.MaxSize = 10;
			Assert.AreEqual (10, fd.MinSize, "#81");
			
			fd.MinSize = 48;
			Assert.AreEqual (48, fd.MaxSize, "#82");
		}
		
		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		/* a few people have been seeing the following:
Failures:
1) MonoTests.System.Windows.Forms.CommonDialogsTest.FolderBrowserDialogTest : System.ArgumentException : oldValue is the empty string.
  at System.String.Replace (System.String oldValue, System.String newValue) [0x00054] in /tmp/scratch/BUILD/mono-1.2.70507/mcs/class/corlib/System/String.cs:1285 
  at System.Windows.Forms.FolderBrowserDialog+FolderBrowserTreeView.SetSelectedPath (System.String path) [0x00061] in /tmp/scratch/BUILD/mono-1.2.70507/mcs/class/Managed.Windows.Forms/System.Windows.Forms/FolderBrowserDialog.cs:460 
  at System.Windows.Forms.FolderBrowserDialog+FolderBrowserTreeView.set_SelectedPath (System.String value) [0x0000c] in /tmp/scratch/BUILD/mono-1.2.70507/mcs/class/Managed.Windows.Forms/System.Windows.Forms/FolderBrowserDialog.cs:363 
  at (wrapper remoting-invoke-with-check) FolderBrowserTreeView:set_SelectedPath (string)
  at System.Windows.Forms.FolderBrowserDialog.set_SelectedPath (System.String value) [0x0001a] in /tmp/scratch/BUILD/mono-1.2.70507/mcs/class/Managed.Windows.Forms/System.Windows.Forms/FolderBrowserDialog.cs:223 
  at (wrapper remoting-invoke-with-check) System.Windows.Forms.FolderBrowserDialog:set_SelectedPath (string)
  at MonoTests.System.Windows.Forms.CommonDialogsTest.FolderBrowserDialogTest () [0x00094] in /tmp/scratch/BUILD/mono-1.2.70507/mcs/class/Managed.Windows.Forms/Test/System.Windows.Forms/CommonDialogsTest.cs:260 
  at <0x00000> <unknown method>
  at (wrapper managed-to-native) System.Reflection.MonoMethod:InternalInvoke (object,object[])
  at System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00040] in /tmp/scratch/BUILD/mono-1.2.70507/mcs/class/corlib/System.Reflection/MonoMethod.cs:143 
		*/
		public void FolderBrowserDialogTest ()
		{
			fbd = new FolderBrowserDialog ();
			
			Assert.AreEqual ("", fbd.Description, "#83");
			
			Assert.AreEqual (Environment.SpecialFolder.Desktop, fbd.RootFolder, "#84");
			
			Assert.AreEqual ("", fbd.SelectedPath, "#85");
			
			Assert.IsTrue (fbd.ShowNewFolderButton, "#86");
			
			Assert.AreEqual ("System.Windows.Forms.FolderBrowserDialog", fbd.ToString (), "#87");
			
			string current_path = Environment.CurrentDirectory;
			fbd.SelectedPath = current_path;
			
			Assert.AreEqual (current_path, fbd.SelectedPath, "#89");
		}
		
		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void FolderBrowserDialogInvalidEnumArgumentExceptionTest () {
			if (fbd == null)
				fbd = new FolderBrowserDialog ();
			
			fbd.RootFolder = (Environment.SpecialFolder)12;
		}

#if NET_2_0
		[Test]
		public void CommonDialogPropertyTag ()
		{
			MyDialog md = new MyDialog ();
			object s = "MyString";
			
			Assert.AreEqual (null, md.Tag, "A1");
			
			md.Tag = s;
			Assert.AreSame (s, md.Tag, "A2");
		}
		
		private class MyDialog : CommonDialog
		{
			public override void Reset ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			protected override bool RunDialog (IntPtr hwndOwner)
			{
				throw new Exception ("The method or operation is not implemented.");
			}
		}
#endif
	}
}

