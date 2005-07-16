// XamlParser.cs - NUnit Test Cases for the xaml parser
// 
// Iain McCoy (iain@mccoy.id.au)
//
// (C) iain@mccoy.id.au
// 

using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
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
		string s = "<ConsoleApp></ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null), 
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}
	[Test]
	public void TestTopLevelWithClass()
	{
		string s = "<ConsoleApp xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" x:Class=\"nnn\">\n"+
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), "nnn"), 
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}
	
	[Test]
	[ExpectedException(typeof(Exception))]
	public void TestTopLevelWithName()
	{
		string s = "<ConsoleApp xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" x:Name=\"nnn\">\n"+
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
		string s = "<ConsoleApp xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
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
	public void TestSimplestAddChildAndText()
	{
		string s = "<ConsoleApp xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>Hello</ConsoleWriter>" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s, 
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreateElementTextHappening("Hello"),
				new EndObjectHappening(),
				new EndObjectHappening(),
				new FinishHappening());
		pt.Test();
	}
	
	[Test]
	public void TestTextProperty()
	{
		string s = "<ConsoleApp xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
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
	public void TestTextPropertyAsElement()
	{
		string s = "<ConsoleApp xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter><ConsoleWriter.Text>Hello</ConsoleWriter.Text></ConsoleWriter>\n" +
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
	public void testDependencyProperty()
	{
		string s = "<ConsoleApp xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter ConsoleApp.Repetitions=\"3\" />" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreateDependencyPropertyHappening(typeof(ConsoleApp), "Repetitions", typeof(int)),
				new CreateDependencyPropertyTextHappening("3"),
				new EndDependencyPropertyHappening(),
				new EndObjectHappening(), // ConsoleWriter
				new EndObjectHappening(), // ConsoleApp
				new FinishHappening());
		pt.Test();
	}

	[Test]
	public void testDependencyPropertyAsChildElement()
	{
		string s = "<ConsoleApp xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter><ConsoleApp.Repetitions>3</ConsoleApp.Repetitions></ConsoleWriter>" +
			"</ConsoleApp>";
		ParserTester pt = new ParserTester(MAPPING + s,
				new CreateTopLevelHappening(typeof(ConsoleApp), null),
				new CreateObjectHappening(typeof(ConsoleWriter), null),
				new CreateDependencyPropertyHappening(typeof(ConsoleApp), "Repetitions", typeof(int)),
				new CreateDependencyPropertyTextHappening("3"),
				new EndDependencyPropertyHappening(),
				new EndObjectHappening(), // ConsoleWriter
				new EndObjectHappening(), // ConsoleApp
				new FinishHappening());
		pt.Test();
	}
	public void testObjectAsPropertyValue()
	{
		string s = "<ConsoleApp xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
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
}



abstract class Happening {
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

class CreateElementTextHappening : Happening
{
	public string text;
	public CreateElementTextHappening(string text) {
		this.text = text;
	}
}

class EndObjectHappening : Happening
{
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
}

class EndPropertyHappening : Happening
{
}

class CreateEventHappening : Happening
{
}

class CreateEventDelegateHappening : Happening
{
}

class EndEventHappening : Happening
{
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
	public CreateDependencyPropertyTextHappening(string text) {
		this.text = text;
	}
}

class EndDependencyPropertyHappening : Happening
{
}

class CreateCodeHappening : Happening
{
}

class FinishHappening : Happening
{
}

		
class ParserTester : XamlWriter {
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
			Assert.Fail("WARNING, I CAN'T GET THIS ERROR MESSAGE RIGHT, MAY BE MISLEADING\n" + 
					"The happening was " + parent + ", but was expecting a " + child.GetType());
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

	public void CreateElementText(string text){
		d("CreateElementText");
		AssertSubclass(typeof(CreateElementTextHappening), false);
		
		CreateElementTextHappening h = (CreateElementTextHappening)getHappening();
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
	}

	public void EndProperty(){
		d("EndProperty");
		AssertSubclass(typeof(EndPropertyHappening), true);
	}


	public void CreateEvent(EventInfo evt){
	}

	public void CreateEventDelegate(string functionName, Type eventDelegateType){
	}

	public void EndEvent(){
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
	}

	public void EndDependencyProperty(){
		d("EndDependencyProperty");
		AssertSubclass(typeof(EndDependencyPropertyHappening), true);
	}


	public void CreateCode(string code){
	}


	public void Finish()
	{
		d("Finish");
		AssertSubclass(typeof(FinishHappening), true);
	}


}


}
