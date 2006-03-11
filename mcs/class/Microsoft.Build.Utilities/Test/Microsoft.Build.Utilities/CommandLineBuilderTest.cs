//
// CommandLineBuilderTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Utilities {

	[TestFixture]
	public class CommandLineBuilderTest {

		CommandLineBuilder clb;
		string[] array;
		ITaskItem[] items;
		
		[SetUp]
		public void SetUp () {
			array = new string[] { "a", "b", "c"};
			items = new TaskItem [] { new TaskItem ("a"), new TaskItem ("b")};
		}
		
		[Test]
		public void TestAppendFileNameIfNotNull1 ()
		{
			
			ITaskItem item;
			string itemSpec = "itemSpec";
		
			item = new TaskItem ();
			item.ItemSpec = itemSpec;
			item.SetMetadata ("name", "value");
			clb = new CommandLineBuilder ();
			clb.AppendFileNameIfNotNull (item);
			
			Assert.AreEqual (itemSpec, clb.ToString (), "A1");
		}
		
		[Test]
		public void TestAppendFileNameIfNotNull2 ()
		{
			
			string filename = "filename.txt";
			
			clb = new CommandLineBuilder ();
			
			clb.AppendFileNameIfNotNull (filename);
			
			Assert.AreEqual (filename, clb.ToString (), "A1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"delimiter\" cannot be null.")]
		public void TestAppendFileNamesIfNotNull1 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendFileNamesIfNotNull (array, null);
		}
		
		[Test]
		public void TestAppendFileNamesIfNotNull2 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendFileNamesIfNotNull (array, String.Empty);
			
			Assert.AreEqual ("abc", clb.ToString (), "A1");
			
			clb = new CommandLineBuilder ();
			
			clb.AppendFileNamesIfNotNull (array, "\t");
			
			Assert.AreEqual ("a\tb\tc", clb.ToString (), "A2");
			
			clb.AppendFileNamesIfNotNull ((string[]) null, "sep");
			
			Assert.AreEqual ("a\tb\tc", clb.ToString (), "A3");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"delimiter\" cannot be null.")]
		public void TestAppendFileNamesIfNotNull3 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendFileNamesIfNotNull (items, null);
		}
		
		[Test]
		public void TestAppendFileNamesIfNotNull4 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendFileNamesIfNotNull (items, String.Empty);
			
			Assert.AreEqual ("ab", clb.ToString (), "A1");
			
			clb.AppendFileNamesIfNotNull ((ITaskItem[]) null, "sep");
			
			Assert.AreEqual ("ab", clb.ToString (), "A2");
		}
		
		[Test]
		public void TestAppendSwitch1 ()
		{
			string name = "/switch";
			
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitch (name);
			
			Assert.AreEqual (name, clb.ToString (), "A1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitch2 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitch (null);
		}
		
		[Test]
		public void TestAppendSwitch3 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitch (String.Empty);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchIfNotNull1 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull (null, "parameter");
		}
		
		[Test]
		public void TestAppendSwitchIfNotNull2 ()
		{
			string name = "/switch:";
			string parameter = "parameter";
			
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull (name, (string) null);
			
			Assert.AreEqual (String.Empty, clb.ToString (), "A1");
			
			clb.AppendSwitchIfNotNull (name, parameter);
			
			Assert.AreEqual (name + parameter, clb.ToString (), "A2");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchIfNotNull3 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull (null, items [0]);
		}
		
		[Test]
		public void TestAppendSwitchIfNotNull4 ()
		{
			string name = "/switch:";
			
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull (name, (ITaskItem) null);
			
			Assert.AreEqual (String.Empty, clb.ToString (), "A1");
			
			clb.AppendSwitchIfNotNull (name, items [0]);
			
			Assert.AreEqual (name + items [0].ItemSpec, clb.ToString (), "A2");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchIfNotNull5 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull (null, array, "delimiter");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"delimiter\" cannot be null.")]
		public void TestAppendSwitchIfNotNull6 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull ("/switch", array, null);
		}
		
		[Test]
		public void TestAppendSwitchIfNotNull7 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull ("/switch:", (string[]) null, ";");
			
			Assert.AreEqual (String.Empty, clb.ToString (), "A1");
			
			clb.AppendSwitchIfNotNull ("/switch:", array, ";");
			
			Assert.AreEqual ("/switch:a;b;c", clb.ToString (), "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchIfNotNull8 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull (null, items, "delimiter");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"delimiter\" cannot be null.")]
		public void TestAppendSwitchIfNotNull9 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull ("/switch", items, null);
		}
		
		[Test]
		public void TestAppendSwitchIfNotNull10 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull ("/switch:", (ITaskItem[]) null, ";");
			
			Assert.AreEqual (String.Empty, clb.ToString (), "A1");
			
			clb.AppendSwitchIfNotNull ("/switch:", items, ";");
			
			Assert.AreEqual ("/switch:a;b", clb.ToString (), "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchUnquotedIfNotNull1 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull (null, "parameter");
		}
		
		[Test]
		public void TestAppendSwitchUnquotedIfNotNull2 ()
		{
			string name = "/switch:";
			string parameter = "parameter";
			
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull (name, (string) null);
			
			Assert.AreEqual (String.Empty, clb.ToString (), "A1");
			
			clb.AppendSwitchUnquotedIfNotNull (name, parameter);
			
			Assert.AreEqual (name + parameter, clb.ToString (), "A2");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchUnquotedIfNotNull3 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull (null, items [0]);
		}
		
		[Test]
		public void TestAppendSwitchUnquotedIfNotNull4 ()
		{
			string name = "/switch:";
			
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull (name, (ITaskItem) null);
			
			Assert.AreEqual (String.Empty, clb.ToString (), "A1");
			
			clb.AppendSwitchUnquotedIfNotNull (name, items [0]);
			
			Assert.AreEqual (name + items [0].ItemSpec, clb.ToString (), "A2");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchUnquotedIfNotNull5 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull (null, array, "delimiter");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"delimiter\" cannot be null.")]
		public void TestAppendSwitchUnquotedIfNotNull6 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull ("/switch", array, null);
		}
		
		[Test]
		public void TestAppendSwitchUnquotedIfNotNull7 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull ("/switch:", (string[]) null, ";");
			
			Assert.AreEqual (String.Empty, clb.ToString (), "A1");
			
			clb.AppendSwitchUnquotedIfNotNull ("/switch:", array, ";");
			
			Assert.AreEqual ("/switch:a;b;c", clb.ToString (), "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchUnquotedIfNotNull8 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull (null, items, "delimiter");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), "Parameter \"delimiter\" cannot be null.")]
		public void TestAppendSwitchUnquotedIfNotNull9 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull ("/switch", items, null);
		}
		
		[Test]
		public void TestAppendUnquotedSwitchIfNotNull10 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull ("/switch:", (ITaskItem[]) null, ";");
			
			Assert.AreEqual (String.Empty, clb.ToString (), "A1");
			
			clb.AppendSwitchUnquotedIfNotNull ("/switch:", items, ";");
			
			Assert.AreEqual ("/switch:a;b", clb.ToString (), "A2");
		}
	}
}