//
// XamlParser.cs - NUnit Test Cases for the xaml parser
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
//
//
//
//
// As you may be aware, testing a parser is something of a beastly job. The
// approach taken by these tests is to feed a file to XamlParser and see if it
// tells the code generator to do what you'd expect. This tests both the parsing
// and type-checking bits of XamlParser.
//
// The various Happening classes each represent methods on the IXamlWriter
// interface that XamlParser could call. The constructor of a Happening takes 
// the same arguments as the IXamlWriter method it represents, and merely stashes
// those values in the suitable public fields.
//
// The ParserTester class takes a Xaml document and a list of Happenings, and 
// handles comparison of the calls to ParserTester's methods to the expected 
// sequence of Happenings.
//
// I think this strikes a tolerable balance between the need to keep the tests 
// simple and the need to avoid writing 10 zillion lines of hard-to-follow test
// code.

using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Windows;
using Mono.Windows.Serialization;
using Xaml.TestVocab.Console;

namespace MonoTests.System.Windows.Serialization
{

[TestFixture]
public class XamlParserTest : Assertion {
	const string MAPPING = "<?Mapping ClrNamespace=\"Xaml.TestVocab.Console\" Assembly=\"./TestVocab.dll\" XmlNamespace=\"console\" ?>\n";
	
	[SetUp]
	public void GetReady()
	{
		Debug.WriteLine("====================================");
	}

	[TearDown]
	public void Clean() {}

	[Test]
	public void TestTopLevel()
	{
		string s = "<ConsoleApp xmlns=\"console\"></ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null), 
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

	[Test]
	[ExpectedException(typeof(Exception), "Class 'ConsoleApple' not found.")]
	public void TestTopLevelWithIncorrectClassName()
	{
		string s = "<ConsoleApple xmlns=\"console\"></ConsoleApple>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null), 
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

	[Test]
	[ExpectedException(typeof(XmlException))]
	public void TestTopLevelWithWrongEndingTag()
	{
		string s = "<ConsoleApp xmlns=\"console\"></ConsoleApple>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null), 
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

	[Test]
	[ExpectedException(typeof(Exception), "No xml namespace specified.")]
	public void TestTopLevelWithoutNamespace()
	{
		string s = "<ConsoleApp></ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null), 
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

	[Test]
	public void TestTopLevelWithClassName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" x:Class=\"nnn\">\n"+
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), "nnn"), 
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}
	
	[Test]
	[ExpectedException(typeof(Exception), "The XAML Name attribute can not be applied to top level elements\nDo you mean the Class attribute?")]
	public void TestTopLevelWithName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" x:Name=\"nnn\">\n"+
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), "nnn"), // this is a lie, actually we expect
											// an exception just before this
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

	[Test]
	public void TestSimplestAddChild()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter></ConsoleWriter>" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

	[Test]
	public void TestSimplestAddChildWithObjectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter x:Name=\"XXX\"></ConsoleWriter>" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), "XXX"),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}


	[Test]
	[ExpectedException(typeof(Exception), "Class 'ConsoleWritttter' not found.")]
	public void TestSimplestAddChildWithIncorrectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWritttter></ConsoleWritttter>" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}
	
	[Test]
	[ExpectedException(typeof(Exception), "The XAML Class attribute can not be applied to child elements\nDo you mean the Name attribute?")]
	public void TestSimplestAddChildWithWrongNamingAttribute()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter x:Class=\"abc\"></ConsoleWriter>" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), "abc"),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}


	[Test]
	public void TestSimplestAddChildAndText()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>Hello</ConsoleWriter>" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreateObjectTextHappening("Hello"),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}
	
	[Test]
	public void TestTextProperty()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Text=\"Hello\" />" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreatePropertyHappening(typeof(ConsoleWriter).GetProperty("Text")),
				new CreatePropertyTextHappening("Hello", typeof(ConsoleValue)),
				new EndPropertyHappening(),
				new EndObjectHappening(), //ConsoleWriter
				new EndObjectHappening(), //ConsoleApp
				new FinishHappening());
		pt.Test();
	}

	[Test]
	[ExpectedException(typeof(Exception), "Property 'Texxxt' not found on 'ConsoleWriter'.")]
	public void TestTextPropertyWithIncorrectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Texxxt=\"Hello\" />" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreatePropertyHappening(typeof(ConsoleWriter).GetProperty("Text")),
				new CreatePropertyTextHappening("Hello", typeof(ConsoleValue)),
				new EndPropertyHappening(),
				new EndObjectHappening(), //ConsoleWriter
				new EndObjectHappening(), //ConsoleApp
				new FinishHappening());
		pt.Test();
	}

	[Test]
	public void TestTextPropertyAsElement()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>\n" + 
			"<ConsoleWriter.Text>Hello</ConsoleWriter.Text>\n" +
			"</ConsoleWriter>\n" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreatePropertyHappening(typeof(ConsoleWriter).GetProperty("Text")),
				new CreatePropertyTextHappening("Hello", typeof(ConsoleValue)),
				new EndPropertyHappening(),
				new EndObjectHappening(), //ConsoleWriter
				new EndObjectHappening(), //ConsoleApp
				new FinishHappening());
		pt.Test();
	}

	[Test]
	[ExpectedException(typeof(Exception), "Property node should not have attributes.")]
	public void TestTextPropertyAsElementWithAttribute()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>\n" + 
			"<ConsoleWriter.Text z=\"y\">Hello</ConsoleWriter.Text>\n"+
			"</ConsoleWriter>\n" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreatePropertyHappening(typeof(ConsoleWriter).GetProperty("Text")),
				new CreatePropertyTextHappening("Hello", typeof(ConsoleValue)),
				new EndPropertyHappening(),
				new EndObjectHappening(), //ConsoleWriter
				new EndObjectHappening(), //ConsoleApp
				new FinishHappening());
		pt.Test();
	}

	[Test]
	[ExpectedException(typeof(Exception), "Property 'Texxxt' not found on 'ConsoleWriter'.")]
	public void TestTextPropertyAsElementWithIncorrectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>\n" + 
			"<ConsoleWriter.Texxxt>Hello</ConsoleWriter.Text>\n" + 
			"</ConsoleWriter>\n" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreatePropertyHappening(typeof(ConsoleWriter).GetProperty("Text")),
				new CreatePropertyTextHappening("Hello", typeof(ConsoleValue)),
				new EndPropertyHappening(),
				new EndObjectHappening(), //ConsoleWriter
				new EndObjectHappening(), //ConsoleApp
				new FinishHappening());
		pt.Test();
	}

	[Test]
	public void TestDependencyProperty()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter ConsoleApp.Repetitions=\"3\" />" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreateDependencyPropertyHappening(typeof(ConsoleApp), "Repetitions", typeof(int)),
				new CreateDependencyPropertyTextHappening("3", typeof(int)),
				new EndDependencyPropertyHappening(),
				new EndObjectHappening(), // ConsoleWriter
				new EndObjectHappening(), // ConsoleApp
				new FinishHappening());
		pt.Test();
	}
	
	[Test]
	[ExpectedException(typeof(Exception), "Property 'Reps' does not exist on 'ConsoleApp'.")]
	public void TestDependencyPropertyWithIncorrectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter ConsoleApp.Reps=\"3\" />" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreateDependencyPropertyHappening(typeof(ConsoleApp), "Repetitions", typeof(int)),
				new CreateDependencyPropertyTextHappening("3", typeof(int)),
				new EndDependencyPropertyHappening(),
				new EndObjectHappening(), // ConsoleWriter
				new EndObjectHappening(), // ConsoleApp
				new FinishHappening());
		pt.Test();
	}


	[Test]
	public void TestDependencyPropertyAsChildElement()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>\n" + 
			"<ConsoleApp.Repetitions>3</ConsoleApp.Repetitions>\n" +
			"</ConsoleWriter>" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreateDependencyPropertyHappening(typeof(ConsoleApp), "Repetitions", typeof(int)),
				new CreateDependencyPropertyTextHappening("3", typeof(int)),
				new EndDependencyPropertyHappening(),
				new EndObjectHappening(), // ConsoleWriter
				new EndObjectHappening(), // ConsoleApp
				new FinishHappening());
		pt.Test();
	}

	[Test]
	[ExpectedException(typeof(Exception), "Property 'Reps' does not exist on 'ConsoleApp'.")]
	public void TestDependencyPropertyAsChildElementWithIncorrectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>\n"+
			"<ConsoleApp.Reps>3</ConsoleApp.Reps>\n" + 
			"</ConsoleWriter>" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreateDependencyPropertyHappening(typeof(ConsoleApp), "Repetitions", typeof(int)),
				new CreateDependencyPropertyTextHappening("3", typeof(int)),
				new EndDependencyPropertyHappening(),
				new EndObjectHappening(), // ConsoleWriter
				new EndObjectHappening(), // ConsoleApp
				new FinishHappening());
		pt.Test();
	}


	[Test]
	public void TestObjectAsPropertyValue()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleReader>\n" +
			"<ConsoleReader.Prompt><ConsoleWriter /></ConsoleReader.Prompt>\n" +
			"</ConsoleReader>\n" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleReader), null),
				new CreatePropertyHappening(typeof(ConsoleReader).GetProperty("Prompt")),
				new CreatePropertyObjectHappening(typeof(ConsoleWriter), null),
				new EndPropertyObjectHappening(typeof(ConsoleWriter)),
				new EndPropertyHappening(),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

	[Test]
	[ExpectedException(typeof(Exception), "Cannot add object to instance of 'Xaml.TestVocab.Console.ConsoleValueString'.")]
	public void TestRestrictionOfAddingObjectsToIAddChilds()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleValueString>\n" +
			"<ConsoleWriter />" +
			"</ConsoleValueString>\n" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleValueString), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

	[Test]
	[ExpectedException(typeof(Exception), "Cannot add text to instance of 'Xaml.TestVocab.Console.ConsoleValueString'.")]
	public void TestRestrictionOfAddingTextToIAddChilds()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleValueString>\n" +
			"xyz" +
			"</ConsoleValueString>\n" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleValueString), null),
				new CreateObjectTextHappening("xyz"),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

	[Test]
	public void TestEvent()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" SomethingHappened=\"handleSomething\">\n"+
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateEventHappening(typeof(ConsoleApp).GetEvent("SomethingHappened")),
				new CreateEventDelegateHappening("handleSomething", typeof(SomethingHappenedHandler)),
				new EndEventHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

	[Test]
	public void TestDelegateAsPropertyValue()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Filter=\"filterfilter\" />\n"+
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreatePropertyHappening(typeof(ConsoleWriter).GetProperty("Filter")),
				new CreatePropertyDelegateHappening("filterfilter", typeof(Filter)),
				new EndPropertyHappening(),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}

}



abstract class Happening {
	// this space deliberately left blank
}

class CreateTopLevelHappening : Happening
{
	public Type parent;
	public string className;
	public CreateTopLevelHappening(Type parent, string name) {
		this.parent = parent;
		this.className = name;
	}
}

class CreateObjectHappening : Happening
{
	public Type type;
	public string objectName;
	public CreateObjectHappening(Type type, string name) {
		this.type = type;
		this.objectName = name;
	}
}

class CreateObjectTextHappening : Happening
{
	public string text;
	public CreateObjectTextHappening(string text) {
		this.text = text;
	}
}

class EndObjectHappening : Happening
{
	// this space deliberately left blank
}

class CreatePropertyHappening : Happening
{
	public PropertyInfo property;
	public CreatePropertyHappening(PropertyInfo property){
		this.property = property;
	}
}

class CreatePropertyTextHappening : Happening
{
	public string text;
	public Type propertyType;
	public CreatePropertyTextHappening(string text, Type propertyType){
		this.text = text;
		this.propertyType = propertyType;
	}
}

class CreatePropertyObjectHappening : Happening
{
	public Type type;
	public string objectName;

	public CreatePropertyObjectHappening(Type type, string objectName) {
		this.type = type;
		this.objectName = objectName;
	}
}
class EndPropertyObjectHappening : Happening
{
	public Type type;

	public EndPropertyObjectHappening(Type type) {
		this.type = type;
	}
}
class CreatePropertyDelegateHappening : Happening
{
	public string functionName;
	public Type propertyType;

	public CreatePropertyDelegateHappening(string functionName, Type propertyType) {
		this.functionName = functionName;
		this.propertyType = propertyType;
	}
}

class EndPropertyHappening : Happening
{
	// this space deliberately left blank
}

class CreateEventHappening : Happening
{
	public EventInfo evt;
	public CreateEventHappening(EventInfo evt) {
		this.evt = evt;
	}
}

class CreateEventDelegateHappening : Happening
{
	public string functionName;
	public Type eventDelegateType;
	public CreateEventDelegateHappening(string functionName, Type eventDelegateType) {
		this.functionName = functionName;
		this.eventDelegateType = eventDelegateType;
	}
}

class EndEventHappening : Happening
{
	// this space deliberately left blank
}

class CreateDependencyPropertyHappening : Happening
{
	public Type attachedTo;
	public string propertyName;
	public Type propertyType;
	public CreateDependencyPropertyHappening(Type attachedTo, string propertyName, Type propertyType) {
		this.attachedTo = attachedTo;
		this.propertyName = propertyName;
		this.propertyType = propertyType;
	}
}

class CreateDependencyPropertyTextHappening : Happening
{
	public string text;
	public Type propertyType;
	public CreateDependencyPropertyTextHappening(string text, Type propertyType) {
		this.text = text;
		this.propertyType = propertyType;
	}
}

class EndDependencyPropertyHappening : Happening
{
	// this space deliberately left blank
}

class CreateCodeHappening : Happening
{
	public string code;
	public CreateCodeHappening(string code) {
		this.code = code;
	}
}

class FinishHappening : Happening
{
	// this space deliberately left blank
}

		
class ParserTester : IXamlWriter {
	string document;
	Happening[] happenings;
	int c;
	public ParserTester(string document, params Happening[] happenings)
	{
		this.document = document;
		this.happenings = happenings;
		this.c = 0;
	}
	public void Test()
	{
		XamlParser p = new XamlParser(new StringReader(document),
				this);
		p.Parse();
		// if this fails, then we haven't consumed all the happenings
		Assert.AreEqual(happenings.Length, c);
	}
	Happening getHappening()
	{
		Assert.IsFalse(c >= happenings.Length, "not enough happenings in list");
		return happenings[c++];
	}

	private void AssertSubclass(Type parent, bool consume)
	{
		Assert.IsFalse(c >= happenings.Length, "not enough happenings in list");
		Happening child = happenings[c];

		Debug.WriteLine("WRITER CURRENT EXPECTED THING: " + child.GetType());
//		
		if (child.GetType() != parent)
			Assert.Fail("The method called was a " + parent + ", but was expecting a " + child.GetType());
		if (consume)
			c++;
	}
	private void d(string step)
	{
		Debug.WriteLine("WRITER IN STEP" + step);
	}


	public void CreateTopLevel(Type parent, string className)
	{
		d("CreateTopLevel");
		AssertSubclass(typeof(CreateTopLevelHappening), false);
		
		CreateTopLevelHappening h = (CreateTopLevelHappening)getHappening();
		Assert.AreEqual(h.parent, parent);
		Assert.AreEqual(h.className, className);
	}

	public void CreateObject(Type type, string objectName){
		d("CreateObject");
		AssertSubclass(typeof(CreateObjectHappening), false);
		
		CreateObjectHappening h = (CreateObjectHappening)getHappening();
		Assert.AreEqual(h.type, type);
		Assert.AreEqual(h.objectName, objectName);
	}

	public void CreateObjectText(string text){
		d("CreateObjectText");
		AssertSubclass(typeof(CreateObjectTextHappening), false);
		
		CreateObjectTextHappening h = (CreateObjectTextHappening)getHappening();
		Assert.AreEqual(h.text, text);
	}

	public void EndObject(){
		d("EndObject");
		AssertSubclass(typeof(EndObjectHappening), true);
	}

	public void CreateProperty(PropertyInfo property){
		d("CreateProperty");
		AssertSubclass(typeof(CreatePropertyHappening), false);
		
		CreatePropertyHappening h = (CreatePropertyHappening)getHappening();
		Assert.AreEqual(h.property, property);
	}

	public void CreatePropertyObject(Type type, string objectName){
		d("CreatePropertyObject");
		AssertSubclass(typeof(CreatePropertyObjectHappening), false);

		CreatePropertyObjectHappening h = (CreatePropertyObjectHappening)getHappening();
		Assert.AreEqual(h.type, type);
		Assert.AreEqual(h.objectName, objectName);
	}
	public void EndPropertyObject(Type type) {
		d("EndPropertyObject");
		AssertSubclass(typeof(EndPropertyObjectHappening), false);

		EndPropertyObjectHappening h = (EndPropertyObjectHappening)getHappening();
		Assert.AreEqual(h.type, type);
	}

	public void CreatePropertyText(string text, Type propertyType){
		d("CreatePropertyText");
		AssertSubclass(typeof(CreatePropertyTextHappening), false);
		
		CreatePropertyTextHappening h = (CreatePropertyTextHappening)getHappening();
		Assert.AreEqual(h.text, text);
		Assert.AreEqual(h.propertyType, propertyType);
	}

	public void CreatePropertyDelegate(string functionName, Type propertyType){
		d("CreatePropertyDelegate");
		AssertSubclass(typeof(CreatePropertyDelegateHappening), false);

		CreatePropertyDelegateHappening h = (CreatePropertyDelegateHappening)getHappening();
		Assert.AreEqual(h.functionName, functionName);
		Assert.AreEqual(h.propertyType, propertyType);
	}

	public void EndProperty(){
		d("EndProperty");
		AssertSubclass(typeof(EndPropertyHappening), true);
	}


	public void CreateEvent(EventInfo evt){
		d("CreateEvent");
		AssertSubclass(typeof(CreateEventHappening), false);

		CreateEventHappening h = (CreateEventHappening)getHappening();
		Assert.AreEqual(h.evt, evt);
	}

	public void CreateEventDelegate(string functionName, Type eventDelegateType){
		d("CreateEventDelegate");
		AssertSubclass(typeof(CreateEventDelegateHappening), false);
		
		CreateEventDelegateHappening h = (CreateEventDelegateHappening)getHappening();
		Assert.AreEqual(h.functionName, functionName);
		Assert.AreEqual(h.eventDelegateType, eventDelegateType);
	}

	public void EndEvent(){
		d("EndEvent");
		AssertSubclass(typeof(EndEventHappening), true);
	}


	public void CreateDependencyProperty(Type attachedTo, string propertyName, Type propertyType){
		d("CreateDependencyProperty");
		AssertSubclass(typeof(CreateDependencyPropertyHappening), false);
		
		CreateDependencyPropertyHappening h = (CreateDependencyPropertyHappening)getHappening();
		Assert.AreEqual(h.attachedTo, attachedTo);
		Assert.AreEqual(h.propertyName, propertyName);
		Assert.AreEqual(h.propertyType, propertyType);
	}

	public void CreateDependencyPropertyText(string text, Type propertyType){
		d("CreateDependencyPropertyText");
		AssertSubclass(typeof(CreateDependencyPropertyTextHappening), false);
		
		CreateDependencyPropertyTextHappening h = (CreateDependencyPropertyTextHappening)getHappening();
		Assert.AreEqual(h.text, text);
		Assert.AreEqual(h.propertyType, propertyType);
	}

	public void EndDependencyProperty(){
		d("EndDependencyProperty");
		AssertSubclass(typeof(EndDependencyPropertyHappening), true);
	}


	public void CreateCode(string code){
		d("CreateCode");
		AssertSubclass(typeof(CreateCodeHappening), false);

		CreateCodeHappening h = (CreateCodeHappening)getHappening();
		Assert.AreEqual(h.code, code);
	}


	public void Finish()
	{
		d("Finish");
		AssertSubclass(typeof(FinishHappening), true);
	}


}


}
