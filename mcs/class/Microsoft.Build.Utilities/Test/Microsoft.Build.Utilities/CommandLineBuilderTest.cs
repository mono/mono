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
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Utilities {

	internal class CLBTester : CommandLineBuilder {

		public new bool IsQuotingRequired (string parameter)
		{
			return base.IsQuotingRequired (parameter);
		}

		public new void VerifyThrowNoEmbeddedDoubleQuotes (string switchName,
						string parameter)
		{
			base.VerifyThrowNoEmbeddedDoubleQuotes (switchName, parameter);
		}

		public new void AppendFileNameWithQuoting (string filename)
		{
			base.AppendFileNameWithQuoting (filename);
		}

		public new void AppendSpaceIfNotEmpty ()
		{
			base.AppendSpaceIfNotEmpty ();
		}

		public new void AppendTextUnquoted (string textToAppend)
		{
			base.AppendTextUnquoted (textToAppend);
		}

		public new void AppendTextWithQuoting (string textToAppend)
		{
			base.AppendTextWithQuoting (textToAppend);
		}

		public new StringBuilder CommandLine
		{
			get { return base.CommandLine; }
		}
	}


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

			item = new TaskItem ();
			item.ItemSpec = itemSpec = "a b";
			clb = new CommandLineBuilder ();
			clb.AppendFileNameIfNotNull (item);

			Assert.AreEqual ("\"" + itemSpec + "\"", clb.ToString (), "A2");
		}
		
		[Test]
		public void TestAppendFileNameIfNotNull2 ()
		{
			string filename = "filename.txt";
			
			clb = new CommandLineBuilder ();	
			clb.AppendFileNameIfNotNull (filename);
			
			Assert.AreEqual (filename, clb.ToString (), "A1");

			filename = "a b";
			clb = new CommandLineBuilder ();
			clb.AppendFileNameIfNotNull (filename);

			Assert.AreEqual ("\"" + filename + "\"", clb.ToString (), "A2");
		}

		[Test]
		public void TestAppendFileNameIfNotNull3 ()
		{
			clb = new CommandLineBuilder ();

			clb.AppendFileNameIfNotNull ((string) null);

			Assert.AreEqual (String.Empty, clb.ToString (), "A1");
		}

		[Test]
		public void TestAppendFileNameIfNotNull4 ()
		{
			clb = new CommandLineBuilder ();

			clb.AppendFileNameIfNotNull ((ITaskItem) null);

			Assert.AreEqual (String.Empty, clb.ToString (), "A1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"delimiter\" cannot be null.")]
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

			clb = new CommandLineBuilder ();
			clb.AppendFileNamesIfNotNull (new string [] { "a", "b c"}, " ");

			Assert.AreEqual ("a \"b c\"", clb.ToString (), "A4");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"delimiter\" cannot be null.")]
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

			clb = new CommandLineBuilder ();
			clb.AppendFileNamesIfNotNull (new ITaskItem [] { new TaskItem ("a"), new TaskItem ("b c") }, " ");

			Assert.AreEqual ("a \"b c\"", clb.ToString (), "A3");
		}

		[Test]
		public void TestAppendFileNameWithQuoting1 ()
		{
			CLBTester clbt = new CLBTester ();

			clbt.AppendFileNameWithQuoting (null);

			Assert.AreEqual (String.Empty, clbt.ToString (), "A1");

			clbt.AppendFileNameWithQuoting ("abc abc");

			Assert.AreEqual ("\"abc abc\"", clbt.ToString (), "A2");

			clbt = new CLBTester ();

			clbt.AppendFileNameWithQuoting ("abc");

			Assert.AreEqual ("abc", clbt.ToString (), "A3");
		}

		[Test]
		public void TestAppendSpaceIfNotEmpty ()
		{
			CLBTester clbt = new CLBTester ();

			clbt.AppendSpaceIfNotEmpty ();

			Assert.AreEqual (String.Empty, clbt.ToString (), "A1");

			clbt.AppendFileNameIfNotNull ("a");
			clbt.AppendFileNameIfNotNull ("b");

			Assert.AreEqual ("a b", clbt.ToString (), "A2");
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
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"switchName\" cannot be null.")]
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
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"switchName\" cannot be null.")]
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
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"switchName\" cannot be null.")]
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
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchIfNotNull5 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull (null, array, "delimiter");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"delimiter\" cannot be null.")]
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

			clb.AppendSwitchIfNotNull ("/switch:", new string[] { "a'b", "c" }, ";");
			Assert.AreEqual ("/switch:a;b;c /switch:\"a'b\";c", clb.ToString(), "A3");

			clb.AppendSwitchIfNotNull ("/switch:", "a;b;c");
			Assert.AreEqual ("/switch:a;b;c /switch:\"a'b\";c /switch:\"a;b;c\"", clb.ToString(), "A4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchIfNotNull8 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchIfNotNull (null, items, "delimiter");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"delimiter\" cannot be null.")]
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

			clb.AppendSwitchIfNotNull("/switch:", new ITaskItem[] { new TaskItem("a'b"), new TaskItem("c") }, ";");
			Assert.AreEqual ("/switch:a;b /switch:\"a'b\";c", clb.ToString(), "A3");
		}

		[Test]
		public void TestAppendSwitchIfNotNul11()
		{
			clb = new CommandLineBuilder();

			clb.AppendSwitchIfNotNull("/z:", " a  b");
			clb.AppendSwitchIfNotNull("/z:", "c\tb");
			clb.AppendSwitchIfNotNull("/z:", "ab\n");
			clb.AppendSwitchIfNotNull("/z:", "xyz\u000babc");
			clb.AppendSwitchIfNotNull("/z:", "\u000cabc");
			clb.AppendSwitchIfNotNull("/z:", "a 'hello' b");
			clb.AppendSwitchIfNotNull("/z:", "a;b");

			Assert.AreEqual("/z:\" a  b\" /z:\"c\tb\" /z:\"ab\n\" " +
				"/z:\"xyz\u000babc\" /z:\"\u000cabc\" /z:\"a 'hello' b\" /z:\"a;b\"",
				clb.ToString(), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestAppendSwitchIfNotNull12()
		{
			clb = new CommandLineBuilder();
			clb.AppendSwitchIfNotNull("/z:", "x \"hello\" y");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"switchName\" cannot be null.")]
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
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"switchName\" cannot be null.")]
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
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchUnquotedIfNotNull5 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull (null, array, "delimiter");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"delimiter\" cannot be null.")]
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

			clb.AppendSwitchUnquotedIfNotNull ("/switch:", new string[] { "a'b", "c", "d" }, ";");
			Assert.AreEqual ("/switch:a;b;c /switch:a'b;c;d", clb.ToString(), "A3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"switchName\" cannot be null.")]
		public void TestAppendSwitchUnquotedIfNotNull8 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull (null, items, "delimiter");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Parameter \"delimiter\" cannot be null.")]
		public void TestAppendSwitchUnquotedIfNotNull9 ()
		{
			clb = new CommandLineBuilder ();
			
			clb.AppendSwitchUnquotedIfNotNull ("/switch", items, null);
		}

		[Test]
		public void TestAppendTextUnquoted ()
		{
			CLBTester clbt = new CLBTester ();

			clbt.AppendTextUnquoted (null);
			
			clbt.AppendTextUnquoted ("a b");

			Assert.AreEqual ("a b", clbt.ToString (), "A1");
		}

		[Test]
		public void TestAppendTextWithQuoting ()
		{
			CLBTester clbt = new CLBTester ();

			clbt.AppendTextWithQuoting (null);

			clbt.AppendTextUnquoted ("a b");

			Assert.AreEqual ("a b", clbt.ToString (), "A1");

			clbt = new CLBTester ();
			clbt.AppendTextWithQuoting ("a");

			Assert.AreEqual ("a", clbt.ToString (), "A2");
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

		[Test]
		public void TestCommandLine ()
		{
			CLBTester clbt = new CLBTester ();
			clbt.AppendFileNameIfNotNull ("a");
			Assert.AreEqual (clbt.ToString (), clbt.CommandLine.ToString (), "A1");
		}

		[Test]
		public void TestIsQuotingRequired ()
		{
			CLBTester clbt = new CLBTester ();

			Assert.AreEqual (false, clbt.IsQuotingRequired (null), "A0");
			Assert.AreEqual (false, clbt.IsQuotingRequired (""), "A1");
			Assert.AreEqual (true, clbt.IsQuotingRequired (" "), "A2");
			Assert.AreEqual (false, clbt.IsQuotingRequired ("a"), "A3");
			Assert.AreEqual (true, clbt.IsQuotingRequired ("a a"), "A4");
			Assert.AreEqual (true, clbt.IsQuotingRequired ("\'\'"), "A5");
			Assert.AreEqual (true, clbt.IsQuotingRequired ("\' \'"), "A6");
			Assert.AreEqual (true, clbt.IsQuotingRequired ("\"\""), "A7");
			Assert.AreEqual (true, clbt.IsQuotingRequired ("\" \""), "A8");
			Assert.AreEqual (true, clbt.IsQuotingRequired ("\n\n"), "A9");
			Assert.AreEqual (true, clbt.IsQuotingRequired ("\n \n"), "A10");
			Assert.AreEqual (true, clbt.IsQuotingRequired ("\t\t"), "A11");
			Assert.AreEqual (true, clbt.IsQuotingRequired ("\t \t"), "A12");
			Assert.AreEqual (true, clbt.IsQuotingRequired("a;b"), "A13");
			Assert.AreEqual (true, clbt.IsQuotingRequired("a\u000bc"), "A14");
			Assert.AreEqual (true, clbt.IsQuotingRequired("a\u000cx"), "A15");
			Assert.AreEqual (true, clbt.IsQuotingRequired("\""), "A16");
			Assert.AreEqual (true, clbt.IsQuotingRequired(" abc"), "A17");
		}

		[Test]
		public void TestVerifyThrowNoEmbeddedDoubleQuotes1 ()
		{
			CLBTester clbt = new CLBTester ();

			clbt.VerifyThrowNoEmbeddedDoubleQuotes (null, null);
			clbt.VerifyThrowNoEmbeddedDoubleQuotes ("", null);
			clbt.VerifyThrowNoEmbeddedDoubleQuotes (null, "");
			clbt.VerifyThrowNoEmbeddedDoubleQuotes (" ", "");
			clbt.VerifyThrowNoEmbeddedDoubleQuotes ("", " ");
			clbt.VerifyThrowNoEmbeddedDoubleQuotes ("\"abc\"", "");
			clbt.VerifyThrowNoEmbeddedDoubleQuotes ("x\"y", "");
			clbt.VerifyThrowNoEmbeddedDoubleQuotes ("\'\'", "\'\'");
		}

		[Test]
		public void TestVerifyThrowNoEmbeddedDoubleQuotes2 ()
		{
			CheckVerifyThrowNoEmbeddedDoubleQuotes (new CLBTester (), "a", 
					"\"a\"", true, typeof (ArgumentException), "A1");
		}

		[Test]
		public void TestVerifyThrowNoEmbeddedDoubleQuotes3 ()
		{
			CheckVerifyThrowNoEmbeddedDoubleQuotes (new CLBTester (), "a", "\"\"", true, typeof(ArgumentException), "A1");
		}

		[Test]
		public void TestVerifyThrowNoEmbeddedDoubleQuotes4 ()
		{
			CheckVerifyThrowNoEmbeddedDoubleQuotes (new CLBTester (), "a", "\"foo", true, typeof(ArgumentException), "A1");
		}

		[Test]
		public void TestVerifyThrowNoEmbeddedDoubleQuotes5 ()
		{
			CheckVerifyThrowNoEmbeddedDoubleQuotes (new CLBTester (), null, "a\"b", true, typeof(ArgumentException), "A1");
		}

		private void CheckVerifyThrowNoEmbeddedDoubleQuotes(CLBTester clbt, string switchName, string parameter,
				bool expectException, Type exceptionType, string id)
		{
			try
			{
				clbt.VerifyThrowNoEmbeddedDoubleQuotes(switchName, parameter);
			} catch (Exception e) {
				if (!expectException)
					Assert.Fail("({0}) Unexpected exception : {1}", id, exceptionType.ToString());
				if (e.GetType () != exceptionType)
					Assert.Fail("({0}) Expected exception of {1} type but got {2}", id, exceptionType.ToString(), e.ToString());
				return;
			}

			if (expectException)
				Assert.Fail("({0}) Didn't get expected exception {1}", id, exceptionType.ToString());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestEmbeddedQuotes1()
		{
			new CommandLineBuilder().AppendFileNameIfNotNull("a\"b");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestEmbeddedQuotes2()
		{
			new CommandLineBuilder().AppendFileNameIfNotNull(new TaskItem ("a\"b"));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestEmbeddedQuotes3()
		{
			new CommandLineBuilder().AppendFileNamesIfNotNull(new ITaskItem[] { new TaskItem ("xyz"), new TaskItem("a\"b") }, ":");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestEmbeddedQuotes4()
		{
			new CommandLineBuilder().AppendFileNamesIfNotNull(new string[] { "xyz", "a\"b" }, ":");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestEmbeddedQuotes5()
		{
			new CommandLineBuilder().AppendSwitchIfNotNull("foo", new TaskItem("a\"b"));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestEmbeddedQuotes6()
		{
			new CommandLineBuilder().AppendSwitchIfNotNull("foo", "a\"b");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestEmbeddedQuotes7()
		{
			new CommandLineBuilder().AppendSwitchIfNotNull("foo", new ITaskItem[] { new TaskItem ("xyz"), new TaskItem("a\"b") }, ":");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestEmbeddedQuotes8()
		{
			new CommandLineBuilder().AppendSwitchIfNotNull("foo", new string[] {"xyz", "a\"b" }, ":");
		}

		[Test]
		public void TestEmbeddedQuotes9()
		{
			CommandLineBuilder clb = new CommandLineBuilder();
			clb.AppendSwitchUnquotedIfNotNull("foo", new TaskItem("a\"b"));
			clb.AppendSwitchUnquotedIfNotNull("foo", "a\"b");
			clb.AppendSwitchUnquotedIfNotNull("foo", new ITaskItem[] { new TaskItem ("xyz"), new TaskItem("a\"b") }, ":");
			clb.AppendSwitchUnquotedIfNotNull("foo", new string[] { "xyz", "a\"b" }, ":");
		}
	}
}
