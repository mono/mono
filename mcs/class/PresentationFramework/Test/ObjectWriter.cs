// 
// ObjectWriter.cs - NUnit Test Cases for the xaml object builder
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
public class ObjectWriterTest {
	ObjectWriter ow;
	
	[SetUp]
	public void GetReady()
	{
		ow = new ObjectWriter();
	}

	[TearDown]
	public void Clean() {}

	[Test]
	public void TestTopLevel()
	{
		ow.CreateTopLevel(typeof(ConsoleApp), null);
		ow.EndObject();
		ow.Finish();
		ConsoleApp app = new ConsoleApp();
		compare(app);
	}

	[Test]
	public void TestTopLevelWithClassName()
	{
		ow.CreateTopLevel(typeof(ConsoleApp), "MyConsoleApp");
		ow.EndObject();
		ow.Finish();
		ConsoleApp app = new ConsoleApp();
		compare(app);
	}
	
	[Test]
	public void TestTopLevelWithClassNameAndNamespace()
	{
		ow.CreateTopLevel(typeof(ConsoleApp), "Test.Thing.MyConsoleApp");
		ow.EndObject();
		ow.Finish();
		ConsoleApp app = new ConsoleApp();
		compare(app);
	}
	
	[Test]
	public void TestSimplestAddChild()
	{
		ow.CreateTopLevel(typeof(ConsoleApp), null);
		ow.CreateObject(typeof(ConsoleWriter), null);
		ow.EndObject();
		ow.EndObject();
		ow.Finish();

		ConsoleApp app = new ConsoleApp();
		ConsoleWriter writer = new ConsoleWriter();
		app.AddChild(writer);

		compare(app);
	}

	[Test]
	public void TestSimplestAddChildWithInstanceName()
	{
		ow.CreateTopLevel(typeof(ConsoleApp), null);
		ow.CreateObject(typeof(ConsoleWriter), "XX");
		ow.EndObject();
		ow.EndObject();
		ow.Finish();

		ConsoleApp app = new ConsoleApp();
		ConsoleWriter writer = new ConsoleWriter();
		app.AddChild(writer);

		compare(app);
	}

	
	[Test]
	public void TestSimplestAddChildAndText()
	{
		ow.CreateTopLevel(typeof(ConsoleApp), null);
		ow.CreateObject(typeof(ConsoleWriter), null);
		ow.CreateObjectText("Hello");
		ow.EndObject();
		ow.EndObject();
		ow.Finish();

		ConsoleApp app = new ConsoleApp();
		ConsoleWriter writer = new ConsoleWriter();
		writer.AddText("Hello");
		app.AddChild(writer);

		compare(app);
	}

	[Test]
	public void TestTextProperty()
	{
		ow.CreateTopLevel(typeof(ConsoleApp), null);
		ow.CreateObject(typeof(ConsoleWriter), null);
		ow.CreateProperty(typeof(ConsoleWriter).GetProperty("Text"));
		ow.CreatePropertyText("Hello", typeof(ConsoleValue));
		ow.EndProperty();
		ow.EndObject();
		ow.EndObject();
		ow.Finish();

		ConsoleApp app = new ConsoleApp();
		ConsoleWriter writer = new ConsoleWriter();
		writer.Text = new ConsoleValueString("Hello");
		app.AddChild(writer);

		compare(app);							
	}

	[Test]
	public void TestDependencyProperty()
	{
		ow.CreateTopLevel(typeof(ConsoleApp), null);
		ow.CreateObject(typeof(ConsoleWriter), null);
		ow.CreateDependencyProperty(typeof(ConsoleApp), "Repetitions", typeof(int));
		ow.CreateDependencyPropertyText("3", typeof(int));
		ow.EndDependencyProperty();
		ow.EndObject();
		ow.EndObject();
		ow.Finish();

		ConsoleApp app = new ConsoleApp();
		ConsoleWriter writer = new ConsoleWriter();
		ConsoleApp.SetRepetitions(writer, 3);
		app.AddChild(writer);

		compare(app);
	}

	[Test]
	public void TestObjectAsPropertyValue()
	{
		ow.CreateTopLevel(typeof(ConsoleApp), null);
		ow.CreateObject(typeof(ConsoleReader), null);
		ow.CreateProperty(typeof(ConsoleReader).GetProperty("Prompt"));
		ow.CreatePropertyObject(typeof(ConsoleWriter), null);
		ow.EndPropertyObject(typeof(ConsoleWriter));
		ow.EndProperty();
		ow.EndObject();
		ow.EndObject();
		ow.Finish();

		ConsoleApp app = new ConsoleApp();
		ConsoleReader reader = new ConsoleReader();
		ConsoleWriter writer = new ConsoleWriter();
		reader.Prompt = writer;
		app.AddChild(reader);
		
		compare(app);
	}


	private void compare(object expected)
	{
		Assert.AreEqual(expected, ow.instance);
	}

}

}
