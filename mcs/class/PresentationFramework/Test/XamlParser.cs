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

using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Windows;
using System.Windows.Serialization;
using Mono.Windows.Serialization;
using Xaml.TestVocab.Console;

namespace MonoTests.System.Windows.Serialization
{

[TestFixture]
public class XamlParserTest {
	const string MAPPING = "<?Mapping ClrNamespace=\"Xaml.TestVocab.Console\" Assembly=\"./TestVocab.dll\" XmlNamespace=\"console\" ?>\n";
	
	[SetUp]
	public void GetReady()
	{
		Debug.WriteLine("====================================");
	}

	[TearDown]
	public void Clean() {}

	private void makeGoBang(string s)
	{
		XamlNode n;
		XamlParser p = new XamlParser(new StringReader(s));
		do {
			n = p.GetNextNode();
		} while (!(n is XamlDocumentEndNode));
	}

	[Test]
	[ExpectedException(typeof(Exception), "Unknown processing instruction.")]
	public void TestIncorrectPIName()
	{
		string s = "<?Mapppping ClrNamespace=\"Xaml.TestVocab.Console\" Assembly=\"./TestVocab.dll\" XmlNamespace=\"console\" ?>\n";
		makeGoBang(s);
	}

	[Test]
	public void TestTopLevel()
	{
		string s = "<ConsoleApp xmlns=\"console\"></ConsoleApp>";
		XamlNode n;
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(0, n.Depth, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "C1");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode, "D1");

	}

	[Test]
	[ExpectedException(typeof(Exception), "Class 'ConsoleApple' not found.")]
	public void TestTopLevelWithIncorrectClassName()
	{
		string s = "<ConsoleApple xmlns=\"console\"></ConsoleApple>";
		makeGoBang(MAPPING + s);
	}

	[Test]
	[ExpectedException(typeof(XmlException))]
	public void TestTopLevelWithWrongEndingTag()
	{
		string s = "<ConsoleApp xmlns=\"console\"></ConsoleApple>";
		makeGoBang(MAPPING + s);

	}

	[Test]
	[ExpectedException(typeof(Exception), "No xml namespace specified.")]
	public void TestTopLevelWithoutNamespace()
	{
		string s = "<ConsoleApp></ConsoleApp>";
		makeGoBang(MAPPING + s);

	}

	[Test]
	public void TestTopLevelWithClassName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" x:Class=\"nnn\">\n"+
			"</ConsoleApp>";
		XamlNode n;
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(0, n.Depth, "B2");
		Assert.AreEqual("nnn", ((XamlElementStartNode)n).name, "B3");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B4");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "C1");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode, "D1");
	}
	
	[Test]
	[ExpectedException(typeof(Exception), "The XAML Name attribute can not be applied to top level elements\nDo you mean the Class attribute?")]
	public void TestTopLevelWithName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" x:Name=\"nnn\">\n"+
			"</ConsoleApp>";
		makeGoBang(MAPPING + s);
	}

	[Test]
	public void TestSimplestAddChild()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter></ConsoleWriter>" +
			"</ConsoleApp>";

		XamlNode n;
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode);

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 0);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 1);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode);

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode);
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode);

	}

	[Test]
	public void TestSimplestAddChildWithObjectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter x:Name=\"XXX\"></ConsoleWriter>" +
			"</ConsoleApp>";

		XamlNode n;
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(n.Depth, 0, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "C1");
		Assert.AreEqual(1, n.Depth, "C2");
		Assert.AreEqual("XXX", ((XamlElementStartNode)n).name, "C3");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter), "C4");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "D1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "E1");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode, "F1");

	}


	[Test]
	[ExpectedException(typeof(Exception), "Class 'ConsoleWritttter' not found.")]
	public void TestSimplestAddChildWithIncorrectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWritttter></ConsoleWritttter>" +
			"</ConsoleApp>";
		makeGoBang(MAPPING + s);
	}
	
	[Test]
	[ExpectedException(typeof(Exception), "The XAML Class attribute can not be applied to child elements\nDo you mean the Name attribute?")]
	public void TestSimplestAddChildWithWrongNamingAttribute()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter x:Class=\"abc\"></ConsoleWriter>" +
			"</ConsoleApp>";
		makeGoBang(MAPPING + s);
	}


	[Test]
	public void TestSimplestAddChildAndText()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>Hello</ConsoleWriter>" +
			"</ConsoleApp>";

		XamlNode n;
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode);

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 0);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 1);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlTextNode);
		Assert.AreEqual(((XamlTextNode)n).TextContent, "Hello");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode);

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode);
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode);

	}
	
	[Test]
	public void TestTextProperty()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Text=\"Hello\" />" +
			"</ConsoleApp>";

		XamlNode n;
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));

		basicPropertyTestStream(p, 0);
	}

	[Test]
	public void TestTextPropertyAsElement()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>\n" + 
			"<ConsoleWriter.Text>Hello</ConsoleWriter.Text>\n" +
			"</ConsoleWriter>\n" +
			"</ConsoleApp>";
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));

		basicPropertyTestStream(p, 1);
	}



	private void basicPropertyTestStream(XamlParser p, int depthIncreaseForProperty)
	{
		XamlNode n;
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(0, n.Depth, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "C1");
		Assert.AreEqual(1, n.Depth, "C2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter), "C3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlPropertyNode, "D1");
		Assert.AreEqual(1 + depthIncreaseForProperty, n.Depth, "D2");
		Assert.AreEqual(((XamlPropertyNode)n).PropInfo, typeof(ConsoleWriter).GetProperty("Text"), "D3");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlTextNode, "E1");
		Assert.AreEqual(((XamlTextNode)n).TextContent, "Hello", "E2");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "F1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "F2");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode, "F3");

	}

	[Test]
	[ExpectedException(typeof(Exception), "Property 'Texxxt' not found on 'ConsoleWriter'.")]
	public void TestTextPropertyWithIncorrectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Texxxt=\"Hello\" />" +
			"</ConsoleApp>";
		makeGoBang(MAPPING + s);
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
		makeGoBang(MAPPING + s);
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
		makeGoBang(MAPPING + s);
	}

	[Test]
	public void TestDependencyProperty()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter ConsoleApp.Repetitions=\"3\" />" +
			"</ConsoleApp>";

		XamlNode n;
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode);

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 0);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 1);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlPropertyNode);
		Assert.AreEqual(((XamlPropertyNode)n).DP, ConsoleApp.RepetitionsProperty);

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlTextNode);
		Assert.AreEqual(((XamlTextNode)n).TextContent, "3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode);

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode);
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode);

	}

	[Test]
	[ExpectedException(typeof(Exception), "Dependency properties can only be set on DependencyObjects (not ConsoleValueString)")]
	public void TestDependencyPropertyOnNotDependencyObject()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleValueString ConsoleApp.Repetitions=\"3\" />" +
			"</ConsoleApp>";
		makeGoBang(MAPPING + s);
	}

	[Test]
	[ExpectedException(typeof(Exception), "Property 'Reps' does not exist on 'ConsoleApp'.")]
	public void TestDependencyPropertyWithIncorrectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter ConsoleApp.Reps=\"3\" />" +
			"</ConsoleApp>";
		makeGoBang(MAPPING + s);
	}


	[Test]
	public void TestDependencyPropertyAsChildElement()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter>\n" + 
			"<ConsoleApp.Repetitions>3</ConsoleApp.Repetitions>\n" +
			"</ConsoleWriter>" +
			"</ConsoleApp>";

		XamlNode n;
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode);

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 0);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 1);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter));

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlPropertyNode);
		Assert.AreEqual(n.Depth, 2);
		Assert.AreEqual(((XamlPropertyNode)n).DP, ConsoleApp.RepetitionsProperty);

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlTextNode);
		Assert.AreEqual(((XamlTextNode)n).TextContent, "3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode);

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode);
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode);

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
		makeGoBang(MAPPING + s);
	}


	[Test]
	public void TestObjectAsPropertyValue()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleReader>\n" +
			"<ConsoleReader.Prompt><ConsoleWriter /></ConsoleReader.Prompt>\n" +
			"</ConsoleReader>\n" +
			"</ConsoleApp>";
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));
		XamlNode n;
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(n.Depth, 0, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "C1");
		Assert.AreEqual(1, n.Depth, "C2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleReader), "C3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlPropertyNode, "D1");
		Assert.AreEqual(2, n.Depth, "D3");
		Assert.AreEqual(((XamlPropertyNode)n).PropInfo, typeof(ConsoleReader).GetProperty("Prompt"), "D2");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "E1" + n.GetType());
		Assert.AreEqual(3, n.Depth, "E2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter), "E3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "F1" + n.GetType());

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlPropertyComplexEndNode, "G1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "H1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "I1");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode, "J1");

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
		makeGoBang(MAPPING + s);
	}

	[Test]
	[ExpectedException(typeof(Exception), "Cannot add text to instance of 'Xaml.TestVocab.Console.ConsoleValueString'.")]
	public void TestRestrictionOfAddingTextToIAddChilds()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleValueString>\n" +
			"ABC" +
			"</ConsoleValueString>\n" +
			"</ConsoleApp>";
		makeGoBang(MAPPING + s);
	}
	
	[Test]
	public void TestEvent()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\" SomethingHappened=\"handleSomething\">\n"+
			"</ConsoleApp>";
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));
		XamlNode n;
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(n.Depth, 0, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlClrEventNode, "C1");
		Assert.AreEqual(((XamlClrEventNode)n).EventMember, typeof(ConsoleApp).GetEvent("SomethingHappened"), "C2");
		
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "D1");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode, "E1");
	}

	[Test]
	public void TestDelegateAsPropertyValue()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Filter=\"filterfilter\" />\n"+
			"</ConsoleApp>";
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));
		XamlNode n;
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(n.Depth, 0, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "C1");
		Assert.AreEqual(n.Depth, 1, "C2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter), "C3");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlPropertyNode, "D1");
		Assert.AreEqual(n.Depth, 1, "D2");
		Assert.AreEqual(((XamlPropertyNode)n).PropInfo, typeof(ConsoleWriter).GetProperty("Filter"), "D3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlClrEventNode, "E1");
		Assert.AreEqual((MemberInfo)((XamlClrEventNode)n).EventMember, typeof(ConsoleWriter).GetProperty("Filter"), "E2");
		Assert.AreEqual(((XamlClrEventNode)n).Value, "filterfilter", "E3");
		
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "F1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "G1");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode, "H1");
	}

	[Test]
	public void TestCode()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<x:Code><![CDATA[Hi there <thing /> here there everywhere]]></x:Code>\n"+
			"</ConsoleApp>";
		XamlParser p = new XamlParser(new StringReader(MAPPING + s));
		XamlNode n;
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = p.GetNextNode();
		Assert.IsTrue(n is XamlLiteralContentNode, "C1");
		Assert.AreEqual("Hi there <thing /> here there everywhere", ((XamlLiteralContentNode)n).Content, "C2");
		
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlElementEndNode, "D1");
		
		n = p.GetNextNode();
		Assert.IsTrue(n is XamlDocumentEndNode, "E1");

	}


	[Test]
	[ExpectedException(typeof(Exception), "Code element children must be either text or CDATA nodes.")]
	public void TestCodeWithIncorrectChildren()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<x:Code>Hi there <thing /> here there everywhere</x:Code>\n"+
			"</ConsoleApp>";
		makeGoBang(MAPPING + s);
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
/*		XamlParser p = new XamlParser(new StringReader(document),
				this);
		p.Parse();
		// if this fails, then we haven't consumed all the happenings
		Assert.AreEqual(happenings.Length, c);*/
		Assert.IsTrue(false);
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
