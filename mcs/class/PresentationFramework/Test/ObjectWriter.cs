// 
// Parser.cs - NUnit Test Cases for the xaml object builder
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
using System.Windows.Serialization;
using Xaml.TestVocab.Console;

namespace MonoTests.System.Windows.Serialization
{

[TestFixture]
public class ParserTest {
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
		ConsoleApp app = new ConsoleApp();
		compare(app);
	}

	[Test]
	public void TestTopLevelWithClassName()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" x:Class=\"nnn\">\n"+
			"</ConsoleApp>";
		ConsoleApp app = new ConsoleApp();
		compare(app);
	}
	
	[Test]
	public void TestTopLevelWithClassNameAndNamespace()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" x:Class=\"Test.Thing.nnn\">\n"+
			"</ConsoleApp>";
		ConsoleApp app = new ConsoleApp();
		compare(app);
	}
	
	[Test]
	public void TestSimplestAddChild()
	{

		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter></ConsoleWriter>" +
			"</ConsoleApp>";
		ConsoleApp app = new ConsoleApp();
		ConsoleWriter writer = new ConsoleWriter();
		app.AddChild(writer);

		compare(app);
	}

	[Test]
	public void TestSimplestAddChildWithInstanceName()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter x:Name=\"XXX\"></ConsoleWriter>" +
			"</ConsoleApp>";

		ConsoleApp app = new ConsoleApp();
		ConsoleWriter writer = new ConsoleWriter();
		app.AddChild(writer);

		compare(app);
	}

	
	[Test]
	public void TestSimplestAddChildAndText()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>Hello</ConsoleWriter>" +
			"</ConsoleApp>";

		ConsoleApp app = new ConsoleApp();
		ConsoleWriter writer = new ConsoleWriter();
		writer.AddText("Hello");
		app.AddChild(writer);

		compare(app);
	}

	[Test]
	public void TestTextProperty()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Text=\"Hello\" />" +
			"</ConsoleApp>";

		ConsoleApp app = new ConsoleApp();
		ConsoleWriter writer = new ConsoleWriter();
		writer.Text = new ConsoleValueString("Hello");
		app.AddChild(writer);

		compare(app);							
	}

	[Test]
	public void TestDependencyProperty()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter ConsoleApp.Repetitions=\"3\" />" +
			"</ConsoleApp>";

		ConsoleApp app = new ConsoleApp();
		ConsoleWriter writer = new ConsoleWriter();
		ConsoleApp.SetRepetitions(writer, 3);
		app.AddChild(writer);

		compare(app);
	}

	[Test]
	public void TestObjectAsPropertyValue()
	{
		code = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleReader>\n" +
			"<ConsoleReader.Prompt><ConsoleWriter /></ConsoleReader.Prompt>\n" +
			"</ConsoleReader>\n" +
			"</ConsoleApp>";

		ConsoleApp app = new ConsoleApp();
		ConsoleReader reader = new ConsoleReader();
		ConsoleWriter writer = new ConsoleWriter();
		reader.Prompt = writer;
		app.AddChild(reader);
		
		compare(app);
	}


	private void compare(object expected)
	{
		string mapping = "<?Mapping ClrNamespace=\"Xaml.TestVocab.Console\" Assembly=\"./TestVocab.dll\" XmlNamespace=\"console\" ?>\n";
		object o = Parser.LoadXml(new XmlTextReader(new StringReader(mapping + code)));
		Assert.AreEqual(expected, o);
	}

}

}
