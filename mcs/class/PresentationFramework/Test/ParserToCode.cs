// 
// ParserToCode.cs - NUnit Test Cases for the xaml code generator
// 
// Author:
//   Iain McCoy (iain@mccoy.id.au)
//
// (C) 2005 Iain McCoy
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


using NUnit.Framework;

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Windows;
using System.CodeDom.Compiler;
using Mono.Windows.Serialization;
using Xaml.TestVocab.Console;

namespace MonoTests.System.Windows.Serialization
{

[TestFixture]
public class ParserToCodeTest {
	string code;
	
	[SetUp]
	public void GetReady()
	{
	}

	[TearDown]
	public void Clean()
	{
		code = null;
	}

	[Test]
	public void TestTopLevel()
	{
		code = "<ConsoleApp xmlns=\"console\"></ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	[Test]
#if !NET_2_0
	[ExpectedException(typeof(Exception), "Cannot create partial class")]
#endif
	public void TestPartialTopLevel()
	{
		code = "<ConsoleApp xmlns=\"console\"></ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public partial class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"		}\n" +
				"	}\n" +
				"}",
				true
		);							
	}


	[Test]
	public void TestTopLevelWithClassName()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" x:Class=\"MyConsoleApp\">\n"+
			"</ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class MyConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private MyConsoleApp() {\n"+
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}
	
	[Test]
	public void TestTopLevelWithClassNameAndNamespace()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" x:Class=\"Test.Thing.MyConsoleApp\">\n"+
			"</ConsoleApp>";
		compare(
				"namespace Test.Thing {\n"+
				"	public class MyConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private MyConsoleApp() {\n"+
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}
	
	[Test]
	public void TestSimplestAddChild()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter></ConsoleWriter>" +
			"</ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"			Xaml.TestVocab.Console.ConsoleWriter consoleWriter1 = new Xaml.TestVocab.Console.ConsoleWriter();\n"+
				"			this.AddChild(consoleWriter1);\n" +
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	[Test]
	public void TestSimplestAddChildWithInstanceName()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter x:Name=\"XX\"></ConsoleWriter>" +
			"</ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private Xaml.TestVocab.Console.ConsoleWriter XX = new Xaml.TestVocab.Console.ConsoleWriter();\n"+
				"		private derivedConsoleApp() {\n"+
				"			this.AddChild(XX);\n" +
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	
	[Test]
	public void TestSimplestAddChildAndText()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>Hello</ConsoleWriter>" +
			"</ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"			Xaml.TestVocab.Console.ConsoleWriter consoleWriter1 = new Xaml.TestVocab.Console.ConsoleWriter();\n"+
				"			this.AddChild(consoleWriter1);\n" +
				"			consoleWriter1.AddText(\"Hello\");\n" +
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	[Test]
	public void TestTextProperty()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Text=\"Hello\" />" +
			"</ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"			Xaml.TestVocab.Console.ConsoleWriter consoleWriter1 = new Xaml.TestVocab.Console.ConsoleWriter();\n"+
				"			this.AddChild(consoleWriter1);\n" +
				"			consoleWriter1.Text = ((Xaml.TestVocab.Console.ConsoleValue)(System.ComponentModel.TypeDescriptor.GetConverter(typeof(Xaml.TestVocab.Console.ConsoleValue)).ConvertFromString(\"Hello\")));\n" +
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	[Test]
	public void TestDependencyProperty()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter ConsoleApp.Repetitions=\"3\" />" +
			"</ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"			Xaml.TestVocab.Console.ConsoleWriter consoleWriter1 = new Xaml.TestVocab.Console.ConsoleWriter();\n"+
				"			this.AddChild(consoleWriter1);\n" +
				"			int temp0;\n" +
				"			temp0 = ((int)(System.ComponentModel.TypeDescriptor.GetConverter(typeof(int)).ConvertFromString(\"3\")));\n" +
				"			Xaml.TestVocab.Console.ConsoleApp.SetRepetitions(consoleWriter1, temp0);\n" +
				    
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	[Test]
	public void TestObjectAsPropertyValue()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleReader>\n" +
			"<ConsoleReader.Prompt><ConsoleWriter /></ConsoleReader.Prompt>\n" +
			"</ConsoleReader>\n" +
			"</ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"			Xaml.TestVocab.Console.ConsoleReader consoleReader1 = new Xaml.TestVocab.Console.ConsoleReader();\n"+
				"			this.AddChild(consoleReader1);\n" +
				"			Xaml.TestVocab.Console.ConsoleWriter consoleWriter1 = new Xaml.TestVocab.Console.ConsoleWriter();\n"+
				"			consoleReader1.Prompt = consoleWriter1;\n" +
				    
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	[Test]
	public void TestObjectAsPropertyValueWithSpecifiedName()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleReader>\n" +
			"<ConsoleReader.Prompt><ConsoleWriter x:Name=\"prompt\" /></ConsoleReader.Prompt>\n" +
			"</ConsoleReader>\n" +
			"</ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private Xaml.TestVocab.Console.ConsoleWriter prompt = new Xaml.TestVocab.Console.ConsoleWriter();\n"+
				"		private derivedConsoleApp() {\n"+
				"			Xaml.TestVocab.Console.ConsoleReader consoleReader1 = new Xaml.TestVocab.Console.ConsoleReader();\n"+
				"			this.AddChild(consoleReader1);\n" +
//				"			Xaml.TestVocab.Console.ConsoleWriter consoleWriter1 = new Xaml.TestVocab.Console.ConsoleWriter();\n"+
				"			consoleReader1.Prompt = prompt;\n" +
				    
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	[Test]
	public void TestObjectAsDependencyPropertyValue()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleReader>\n" +
			"<ConsoleApp.Repetitions><ConsoleValueString Text=\"3\" /></ConsoleApp.Repetitions>\n" +
			"</ConsoleReader>\n" +
			"</ConsoleApp>";

		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"			Xaml.TestVocab.Console.ConsoleReader consoleReader1 = new Xaml.TestVocab.Console.ConsoleReader();\n"+
				"			this.AddChild(consoleReader1);\n" +
				"			int temp0;\n" +
				"			Xaml.TestVocab.Console.ConsoleValueString consoleValueString1 = new Xaml.TestVocab.Console.ConsoleValueString();\n" +
				"			consoleValueString1.Text = \"3\";\n" +
				"			temp0 = ((int)(System.ComponentModel.TypeDescriptor.GetConverter(typeof(Xaml.TestVocab.Console.ConsoleValueString)).ConvertTo(consoleValueString1, typeof(int))));\n" +
				"			Xaml.TestVocab.Console.ConsoleApp.SetRepetitions(consoleReader1, temp0);\n" +
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	[Test]
	public void TestObjectAsDependencyPropertyValueWithSpecifiedName()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleReader>\n" +
			"<ConsoleApp.Repetitions><ConsoleValueString Text=\"3\" x:Name=\"xyz\"/></ConsoleApp.Repetitions>\n" +
			"</ConsoleReader>\n" +
			"</ConsoleApp>";

		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private Xaml.TestVocab.Console.ConsoleValueString xyz = new Xaml.TestVocab.Console.ConsoleValueString();\n" +
				"		private derivedConsoleApp() {\n"+
				"			Xaml.TestVocab.Console.ConsoleReader consoleReader1 = new Xaml.TestVocab.Console.ConsoleReader();\n"+
				"			this.AddChild(consoleReader1);\n" +
				"			int temp0;\n" +
				"			xyz.Text = \"3\";\n" +
				"			temp0 = ((int)(System.ComponentModel.TypeDescriptor.GetConverter(typeof(Xaml.TestVocab.Console.ConsoleValueString)).ConvertTo(xyz, typeof(int))));\n" +
				"			Xaml.TestVocab.Console.ConsoleApp.SetRepetitions(consoleReader1, temp0);\n" +
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	[Test]
	public void TestKeyAndStaticResource()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Text=\"xyz\" x:Key=\"foobar\" />\n" +
			"<ConsoleReader Prompt=\"{StaticResource foobar}\" />\n" +
			"</ConsoleApp>";

		compare(
				"namespace DefaultNamespace {\n" +
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n" +
				"			Xaml.TestVocab.Console.ConsoleWriter consoleWriter1 = new Xaml.TestVocab.Console.ConsoleWriter();\n" +
				"			this.AddChild(consoleWriter1);\n" +
				"			consoleWriter1.Text = ((Xaml.TestVocab.Console.ConsoleValue)(System.ComponentModel.TypeDescriptor.GetConverter(typeof(Xaml.TestVocab.Console.ConsoleValue)).ConvertFromString(\"xyz\")));\n" +
				"			Xaml.TestVocab.Console.ConsoleReader consoleReader1 = new Xaml.TestVocab.Console.ConsoleReader();\n" +
				"			this.AddChild(consoleReader1);\n" +
				"			consoleReader1.Prompt = consoleWriter1;\n" +
				"		}\n" +
				"	}\n" +
				"}");
	}


	[Test]
	public void TestEvent()
	{
		code = "<ConsoleApp xmlns=\"console\" SomethingHappened=\"handleSomething\"></ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"			this.SomethingHappened += new Xaml.TestVocab.Console.SomethingHappenedHandler(this.handleSomething);\n"+
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

	[Test]
	public void TestDelegateAsPropertyValue()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Filter=\"filterfilter\" />\n"+
			"</ConsoleApp>";
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"			Xaml.TestVocab.Console.ConsoleWriter consoleWriter1 = new Xaml.TestVocab.Console.ConsoleWriter();\n"+
				"			this.AddChild(consoleWriter1);\n" +
				"			consoleWriter1.Filter = new Xaml.TestVocab.Console.Filter(this.filterfilter);\n" +
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}


	private void compare(string expected)
	{
		compare(expected, false);
	}
	private void compare(string expected, bool isPartial)
	{
		int i, j;
		ICodeGenerator generator = (new Microsoft.CSharp.CSharpCodeProvider()).CreateGenerator();
		string mapping = "<?Mapping ClrNamespace=\"Xaml.TestVocab.Console\" Assembly=\"./TestVocab.dll\" XmlNamespace=\"console\" ?>\n";
		string w = ParserToCode.Parse(new XmlTextReader(new StringReader(mapping + code)), generator, isPartial);
		string[] actualLines = w.ToString().Split('\n');
		for (i = 0; i < actualLines.Length; i++) {
			// set commented-out lines to null
			if (actualLines[i].StartsWith("//")) {
				actualLines[i] = null;
				continue;
			}
			
			// set lines containing only whitespace to null
			j = 0;
			while (j < actualLines[i].Length && 
					Char.IsWhiteSpace(actualLines[i][j])) {
				j++;
			}
			if (j == actualLines[i].Length) {
				actualLines[i] = null;
				continue;
			}
		}
		// shift all null elements to end of list and join all non-null elements
		j = 0;
		for (i = 0; i < actualLines.Length; i++) {
			if (actualLines[i] != null)
				actualLines[j++] = actualLines[i];
		}
		string actual = String.Join("\n", actualLines, 0, j);

		string[] expectedLines = expected.Split('\n');
		for (i = 0; i < expectedLines.Length; i++) {
			expectedLines[i] = replaceTabsAtLineStart(expectedLines[i]);
		}
		expected = String.Join("\n", expectedLines);
		
		if (expected != actual) {
			Debug.WriteLine("FULL EXPECTED:");
			Debug.WriteLine(expected);
			Debug.WriteLine("===============================================");
			Debug.WriteLine("FULL ACTUAL:");
			Debug.WriteLine(actual);
		}
		Assert.AreEqual(expected, actual);
	}

	string replaceTabsAtLineStart(string s)
	{
		if (s == "" || !Char.IsWhiteSpace(s[0]))
			return s;
		string remainder = s.Substring(1);
		if (s[0] == '\t')
			return "    " + replaceTabsAtLineStart(remainder);
		else
			return remainder;
	}
	

}

}
