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
		object p = buildParser(new StringReader(s));
		do {
			n = getNextNode(p);
//			Process.Start("xmessage " + n.GetType());
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
		object p = buildParser(new StringReader(MAPPING + s));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(0, n.Depth, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "C1");
		
		n = getNextNode(p);
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
		object p = buildParser(new StringReader(MAPPING + s));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(0, n.Depth, "B2");
		string name = (string)n.GetType().GetProperty("name", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(n, null);
		Assert.AreEqual("nnn", name, "B3");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B4");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "C1");
		
		n = getNextNode(p);
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
		object p = buildParser(new StringReader(MAPPING + s));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode);

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 0);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 1);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode);

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode);
		
		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentEndNode);

	}

	[Test]
	public void TestSimplestAddChildWithObjectName()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter x:Name=\"XXX\"></ConsoleWriter>" +
			"</ConsoleApp>";

		XamlNode n;
		object p = buildParser(new StringReader(MAPPING + s));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(n.Depth, 0, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "C1");
		Assert.AreEqual(1, n.Depth, "C2");
		string name = (string)n.GetType().GetProperty("name", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(n, null);
		Assert.AreEqual("XXX", name, "C3");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter), "C4");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "D1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "E1");
		
		n = getNextNode(p);
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
		object p = buildParser(new StringReader(MAPPING + s));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode);

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 0);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 1);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlTextNode);
		Assert.AreEqual(((XamlTextNode)n).TextContent, "Hello");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode);

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode);
		
		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentEndNode);

	}
	
	[Test]
	public void TestTextProperty()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Text=\"Hello\" />" +
			"</ConsoleApp>";

		XamlNode n;
		object p = buildParser(new StringReader(MAPPING + s));

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
		object p = buildParser(new StringReader(MAPPING + s));

		basicPropertyTestStream(p, 1);
	}



	private void basicPropertyTestStream(object p, int depthIncreaseForProperty)
	{
		XamlNode n;
		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(0, n.Depth, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "C1");
		Assert.AreEqual(1, n.Depth, "C2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter), "C3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlPropertyNode, "D1");
		Assert.AreEqual(1 + depthIncreaseForProperty, n.Depth, "D2");
		Assert.AreEqual(((XamlPropertyNode)n).PropInfo, typeof(ConsoleWriter).GetProperty("Text"), "D3");
		
		n = getNextNode(p);
		Assert.IsTrue(n is XamlTextNode, "E1");
		Assert.AreEqual(((XamlTextNode)n).TextContent, "Hello", "E2");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "F1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "F2");
		
		n = getNextNode(p);
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
		object p = buildParser(new StringReader(MAPPING + s));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode);

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 0);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 1);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlPropertyNode);
		Assert.AreEqual(((XamlPropertyNode)n).DP, ConsoleApp.RepetitionsProperty);

		n = getNextNode(p);
		Assert.IsTrue(n is XamlTextNode);
		Assert.AreEqual(((XamlTextNode)n).TextContent, "3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode);

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode);
		
		n = getNextNode(p);
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
		object p = buildParser(new StringReader(MAPPING + s));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode);

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 0);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode);
		Assert.AreEqual(n.Depth, 1);
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter));

		n = getNextNode(p);
		Assert.IsTrue(n is XamlPropertyNode);
		Assert.AreEqual(n.Depth, 2);
		Assert.AreEqual(((XamlPropertyNode)n).DP, ConsoleApp.RepetitionsProperty);

		n = getNextNode(p);
		Assert.IsTrue(n is XamlTextNode);
		Assert.AreEqual(((XamlTextNode)n).TextContent, "3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode);

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode);
		
		n = getNextNode(p);
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
		object p = buildParser(new StringReader(MAPPING + s));
		XamlNode n;
		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(n.Depth, 0, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "C1");
		Assert.AreEqual(1, n.Depth, "C2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleReader), "C3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlPropertyNode, "D1");
		Assert.AreEqual(2, n.Depth, "D3");
		Assert.AreEqual(((XamlPropertyNode)n).PropInfo, typeof(ConsoleReader).GetProperty("Prompt"), "D2");
		
		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "E1" + n.GetType());
		Assert.AreEqual(3, n.Depth, "E2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter), "E3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "F1" + n.GetType());

		n = getNextNode(p);
		Assert.IsTrue(n is XamlPropertyComplexEndNode, "G1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "H1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "I1");
		
		n = getNextNode(p);
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
		object p = buildParser(new StringReader(MAPPING + s));
		XamlNode n;
		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(n.Depth, 0, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlClrEventNode, "C1");
		Assert.AreEqual(((XamlClrEventNode)n).EventMember, typeof(ConsoleApp).GetEvent("SomethingHappened"), "C2");
		
		
		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "D1");
		
		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentEndNode, "E1");
	}

	[Test]
	public void TestDelegateAsPropertyValue()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<ConsoleWriter Filter=\"filterfilter\" />\n"+
			"</ConsoleApp>";
		object p = buildParser(new StringReader(MAPPING + s));
		XamlNode n;
		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(n.Depth, 0, "B2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "C1");
		Assert.AreEqual(n.Depth, 1, "C2");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleWriter), "C3");
		
		n = getNextNode(p);
		Assert.IsTrue(n is XamlPropertyNode, "D1");
		Assert.AreEqual(n.Depth, 1, "D2");
		Assert.AreEqual(((XamlPropertyNode)n).PropInfo, typeof(ConsoleWriter).GetProperty("Filter"), "D3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlClrEventNode, "E1");
		Assert.AreEqual((MemberInfo)((XamlClrEventNode)n).EventMember, typeof(ConsoleWriter).GetProperty("Filter"), "E2");
		Assert.AreEqual(((XamlClrEventNode)n).Value, "filterfilter", "E3");
		
		
		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "F1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "G1");
		
		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentEndNode, "H1");
	}

	[Test]
	public void TestCode()
	{
		string s = "<ConsoleApp xmlns=\"console\" xmlns:x=\"http://schemas.microsoft.com/winfx/xaml/2005\">\n"+
			"<x:Code><![CDATA[Hi there <thing /> here there everywhere]]></x:Code>\n"+
			"</ConsoleApp>";
		object p = buildParser(new StringReader(MAPPING + s));
		XamlNode n;
		n = getNextNode(p);
		Assert.IsTrue(n is XamlDocumentStartNode, "A1");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementStartNode, "B1");
		Assert.AreEqual(((XamlElementStartNode)n).ElementType, typeof(ConsoleApp), "B3");

		n = getNextNode(p);
		Assert.IsTrue(n is XamlLiteralContentNode, "C1");
		Assert.AreEqual("Hi there <thing /> here there everywhere", ((XamlLiteralContentNode)n).Content, "C2");
		
		
		n = getNextNode(p);
		Assert.IsTrue(n is XamlElementEndNode, "D1");
		
		n = getNextNode(p);
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

	XamlNode getNextNode(object p)
	{
		MethodInfo getter = p.GetType().GetMethod("GetNextNode");
		Assert.IsTrue(getter != null, "Couldn't get next-node-getter");
		try {
			XamlNode n = (XamlNode)getter.Invoke(p, null);
			Assert.IsTrue(n != null, "Couldn't get next node");
			return n;
		} catch (TargetInvocationException ex) {
			throw ex.InnerException;
		}
	}

	object buildParser(TextReader input)
	{
		Type xamlParserType = (Type)typeof(Mono.Windows.Serialization.ParserToCode).GetField("xamlParserType", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		Assert.IsTrue(xamlParserType != null, "couldn't get type of parser");
		object p = Activator.CreateInstance(xamlParserType, new object[] { input});
		Assert.IsTrue(p != null, "couldn't get instance of parser");
		return p;
	}

}



}
