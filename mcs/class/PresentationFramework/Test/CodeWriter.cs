// 
// CodeWriter.cs - NUnit Test Cases for the xaml code generator
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
public class CodeWriterTest {
	CodeWriter cw;
	StringWriter w;
	
	[SetUp]
	public void GetReady()
	{
		ICodeGenerator generator = (new Microsoft.CSharp.CSharpCodeProvider()).CreateGenerator();
		w = new StringWriter();
		cw = new CodeWriter(generator, w, false);
	}

	[TearDown]
	public void Clean() {}

	[Test]
	public void TestTopLevel()
	{
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.EndObject();
		cw.Finish();
		compare(
				"namespace DefaultNamespace {\n"+
				"	public class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}

#if NET_2_0
	[Test]
	public void TestPartialTopLevel()
	{
		// we duplicate the setup code here, with the one change that
		// the third parameter (determining partialness) of CodeWriter's
		// constructor is set to true
		ICodeGenerator generator = (new Microsoft.CSharp.CSharpCodeProvider()).CreateGenerator();
		w = new StringWriter();
		cw = new CodeWriter(generator, w, true);
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.EndObject();
		cw.Finish();
		compare(
				"namespace DefaultNamespace {\n"+
				"	public partial class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}
#else
	[ExpectedException(typeof(Exception), "Cannot create partial class")]
	[Test]
	public void TestPartialTopLevel()
	{
		// we duplicate the setup code here, with the one change that
		// the third parameter (determining partialness) of CodeWriter's
		// constructor is set to true
		ICodeGenerator generator = (new Microsoft.CSharp.CSharpCodeProvider()).CreateGenerator();
		w = new StringWriter();
		cw = new CodeWriter(generator, w, true);
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.EndObject();
		cw.Finish();
		compare(
				"namespace DefaultNamespace {\n"+
				"	public partial class derivedConsoleApp : Xaml.TestVocab.Console.ConsoleApp {\n" +
				"		private derivedConsoleApp() {\n"+
				"		}\n" +
				"	}\n" +
				"}"
		);							
	}
#endif


	[Test]
	public void TestTopLevelWithClassName()
	{
		cw.CreateTopLevel(typeof(ConsoleApp), "MyConsoleApp");
		cw.EndObject();
		cw.Finish();
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
		cw.CreateTopLevel(typeof(ConsoleApp), "Test.Thing.MyConsoleApp");
		cw.EndObject();
		cw.Finish();
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
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.CreateObject(typeof(ConsoleWriter), null);
		cw.EndObject();
		cw.EndObject();
		cw.Finish();
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
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.CreateObject(typeof(ConsoleWriter), "XX");
		cw.EndObject();
		cw.EndObject();
		cw.Finish();
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
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.CreateObject(typeof(ConsoleWriter), null);
		cw.CreateObjectText("Hello");
		cw.EndObject();
		cw.EndObject();
		cw.Finish();
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
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.CreateObject(typeof(ConsoleWriter), null);
		cw.CreateProperty(typeof(ConsoleWriter).GetProperty("Text"));
		cw.CreatePropertyText("Hello", typeof(ConsoleValue));
		cw.EndProperty();
		cw.EndObject();
		cw.EndObject();
		cw.Finish();
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
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.CreateObject(typeof(ConsoleWriter), null);
		cw.CreateDependencyProperty(typeof(ConsoleApp), "Repetitions", typeof(int));
		cw.CreateDependencyPropertyText("3", typeof(int));
		cw.EndDependencyProperty();
		cw.EndObject();
		cw.EndObject();
		cw.Finish();
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
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.CreateObject(typeof(ConsoleReader), null);
		cw.CreateProperty(typeof(ConsoleReader).GetProperty("Prompt"));
		cw.CreatePropertyObject(typeof(ConsoleWriter), null);
		cw.EndPropertyObject(typeof(ConsoleWriter));
		cw.EndProperty();
		cw.EndObject();
		cw.EndObject();
		cw.Finish();
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
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.CreateObject(typeof(ConsoleReader), null);
		cw.CreateProperty(typeof(ConsoleReader).GetProperty("Prompt"));
		cw.CreatePropertyObject(typeof(ConsoleWriter), "prompt");
		cw.EndPropertyObject(typeof(ConsoleWriter));
		cw.EndProperty();
		cw.EndObject();
		cw.EndObject();
		cw.Finish();
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
	public void TestEvent()
	{
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.CreateEvent(typeof(ConsoleApp).GetEvent("SomethingHappened"));
		cw.CreateEventDelegate("handleSomething", typeof(SomethingHappenedHandler));
		cw.EndEvent();
		cw.EndObject();
		cw.Finish();
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
		cw.CreateTopLevel(typeof(ConsoleApp), null);
		cw.CreateObject(typeof(ConsoleWriter), null);
		cw.CreateProperty(typeof(ConsoleWriter).GetProperty("Filter"));
		cw.CreatePropertyDelegate("filterfilter", typeof(Filter));
		cw.EndProperty();
		cw.EndObject();
		cw.EndObject();
		cw.Finish();
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
		int i, j;
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
