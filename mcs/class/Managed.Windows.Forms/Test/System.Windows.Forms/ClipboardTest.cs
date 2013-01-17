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
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Carlos Alberto Cortez <ccortes@novell.com>
//

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ClipboardTest
	{
#if NET_2_0
		[Test]
		public void UnicodeTextTest ()
		{
			// Put some unicode chars
			string text = "hello \u1000 mono!";
			Clipboard.SetText (text);

			Assert.AreEqual (true, Clipboard.ContainsText (TextDataFormat.UnicodeText), "#A1");
			Assert.AreEqual (text, Clipboard.GetText (TextDataFormat.UnicodeText), "#A2");

			Clipboard.Clear ();
		}

		[Test]
		public void RtfTextTest ()
		{
			string rtf_text = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Arial;}}{\*\generator Mono RichTextBox;}\pard\f0\fs16 hola\par}";
			string plain_text = "hola";

			Clipboard.SetText (rtf_text, TextDataFormat.Rtf);

			Assert.AreEqual (false, Clipboard.ContainsText (TextDataFormat.Text), "#A1");
			Assert.AreEqual (false, Clipboard.ContainsText (TextDataFormat.UnicodeText), "#A2");
			Assert.AreEqual (true, Clipboard.ContainsText (TextDataFormat.Rtf), "#A3");
			Assert.AreEqual (rtf_text, Clipboard.GetText (TextDataFormat.Rtf), "#A4");

			// Now use a IDataObject, so we can have more than one format at the time
			DataObject data = new DataObject ();
			data.SetData (DataFormats.Rtf, rtf_text);
			data.SetData (DataFormats.UnicodeText, plain_text);

			Clipboard.SetDataObject (data);

			Assert.AreEqual (true, Clipboard.ContainsText (TextDataFormat.Text), "#B1");
			Assert.AreEqual (true, Clipboard.ContainsText (TextDataFormat.UnicodeText), "#B2");
			Assert.AreEqual (true, Clipboard.ContainsText (TextDataFormat.Rtf), "#B3");
			Assert.AreEqual (rtf_text, Clipboard.GetText (TextDataFormat.Rtf), "#B4");
			Assert.AreEqual (plain_text, Clipboard.GetText (), "#B5");

			Clipboard.Clear ();
		}

		[Test]
		public void CustomSerializableData ()
		{
			CustomSerializableClass obj = new CustomSerializableClass ();
			obj.Name = "mono101";
			obj.Id = -3;

			Clipboard.SetData ("CustomSerializable", obj);

			//Assert.AreEqual (true, Clipboard.ContainsData ("CustomSerializable"), "#A1");

			CustomSerializableClass obj2 = (CustomSerializableClass)Clipboard.GetData ("CustomSerializable");

			Assert.AreEqual ("mono101", obj2.Name, "#B1");
			Assert.AreEqual (-3, obj2.Id, "#B2");
		}

		[Serializable]
		private class CustomSerializableClass 
		{
			public string Name;
			public int Id;
		}

		[Test]
		public void DataRemainsOnClipboard_Xamarin4959 ()
		{
			// Compile an app that puts something on the clipboard
			var source = @"
using System;
using System.Windows.Forms;
public static class MainClass
{
	public static void Main ()
	{
		Clipboard.SetDataObject (""testing bug 4959"", true, 10, 100);
	}
}
";
			var exeName = Path.GetTempFileName ();
			try {
				var parameters = new CompilerParameters ();
				parameters.GenerateExecutable = true;
				parameters.ReferencedAssemblies.Add ("System.Windows.Forms.dll");
				parameters.OutputAssembly = exeName;
				var compiler = CodeDomProvider.CreateProvider ("CSharp");
				var compilerResults = compiler.CompileAssemblyFromSource (parameters, source);
				Assert.AreEqual (0, compilerResults.Errors.Count);

				// Execute the app
				using (var app = Process.Start (exeName)) {
					app.WaitForExit ();
				}

				// Text should still be on the clipboard
				Assert.AreEqual ("testing bug 4959", Clipboard.GetText ());
			} finally {
				File.Delete (exeName);
			}
		}

		[Test]
		public void DataGetsCleared_Xamarin4959 ()
		{
			// This is the reverse of the previous test

			// Compile an app that puts something on the clipboard
			var source = @"
using System;
using System.Windows.Forms;
public static class MainClass
{
	public static void Main ()
	{
		Clipboard.SetDataObject (""testing bug 4959"", false, 10, 100);
	}
}
";
			var exeName = Path.GetTempFileName ();
			try {
				var parameters = new CompilerParameters ();
				parameters.GenerateExecutable = true;
				parameters.ReferencedAssemblies.Add ("System.Windows.Forms.dll");
				parameters.OutputAssembly = exeName;
				var compiler = CodeDomProvider.CreateProvider ("CSharp");
				var compilerResults = compiler.CompileAssemblyFromSource (parameters, source);
				Assert.AreEqual (0, compilerResults.Errors.Count);

				// Execute the app
				using (var app = Process.Start (exeName)) {
					app.WaitForExit ();
				}

				// Text should no longer be on the clipboard
				Assert.IsTrue (string.IsNullOrEmpty (Clipboard.GetText ()));
			} finally {
				File.Delete (exeName);
			}
		}
#endif
	}
}

