//
// MonoTests.System.Xml.XPathNavigatorEvaluateTests
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XPathNavigatorEvaluateTests : TestCase
	{
		public XPathNavigatorEvaluateTests () : base ("MonoTests.System.Xml.XPathNavigatorEvaluateTests testsuite") {}
		public XPathNavigatorEvaluateTests (string name) : base (name) {}

		XmlDocument document;
		XmlDocument document2;
		XPathNavigator navigator;
		XPathNavigator navigator2;
		XPathExpression expression;
		XPathNodeIterator iterator;

		protected override void SetUp ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<foo><bar/><baz/><qux/><squonk/></foo>");
			navigator = document.CreateNavigator ();

			document2 = new XmlDocument ();
			document2.LoadXml ("<foo><bar baz='1'/><bar baz='2'/><bar baz='3'/></foo>");
			navigator2 = document2.CreateNavigator ();
		}

		// Testing Core Function Library functions defined at: http://www.w3.org/TR/xpath#corelib
		public void saveTestCoreFunctionNodeSetLast ()
		{
			expression = navigator.Compile("last()");
			iterator = navigator.Select("/foo");
			AssertEquals ("0", navigator.Evaluate ("last()").ToString());
			AssertEquals ("0", navigator.Evaluate (expression, null).ToString ());
			AssertEquals ("1", navigator.Evaluate (expression, iterator).ToString ());
			iterator = navigator.Select("/foo/*");
			AssertEquals ("4", navigator.Evaluate (expression, iterator).ToString ());
			
			AssertEquals("3", navigator2.Evaluate ("string(//bar[last()]/@baz)"));
		}

		public void TestCoreFunctionNodeSetPosition ()
		{
			expression = navigator.Compile("position()");
			iterator = navigator.Select("/foo");
			AssertEquals ("0", navigator.Evaluate ("position()").ToString ());
			AssertEquals ("0", navigator.Evaluate (expression, null).ToString ());
			AssertEquals ("0", navigator.Evaluate (expression, iterator).ToString ());
			iterator = navigator.Select("/foo/*");
			AssertEquals ("0", navigator.Evaluate (expression, iterator).ToString ());
			iterator.MoveNext();
			AssertEquals ("1", navigator.Evaluate (expression, iterator).ToString ());
			iterator.MoveNext ();
			AssertEquals ("2", navigator.Evaluate (expression, iterator).ToString ());
			iterator.MoveNext ();
			AssertEquals ("3", navigator.Evaluate (expression, iterator).ToString ());
		}

		public void saveTestCoreFunctionNodeSetCount ()
		{
			AssertEquals ("5", navigator.Evaluate ("count(//*)").ToString ());
			AssertEquals ("1", navigator.Evaluate ("count(//foo)").ToString ());
			AssertEquals ("1", navigator.Evaluate ("count(/foo)").ToString ());
			AssertEquals ("1", navigator.Evaluate ("count(/foo/bar)").ToString ());

			AssertEquals ("3", navigator2.Evaluate ("count(//bar)").ToString ());
		}

		public void saveTestCoreFunctionNodeSetID ()
		{
			document.LoadXml(
				"<!DOCTYPE foo [" +
				"<!ELEMENT foo (bar)>" +
				"<!ELEMENT bar EMPTY>" +
				"<!ATTLIST bar baz ID #REQUIRED>" +
				"]>" +
				"<foo><bar baz='1' qux='hello' /><bar baz='2' qux='world' /></foo>");
			navigator = document.CreateNavigator();

			AssertEquals("hello", navigator.Evaluate ("string(id('1')/@qux)").ToString ());
			AssertEquals("world", navigator.Evaluate ("string(id('2')/@qux)").ToString ());
		}

		public void saveTestCoreFunctionLocalName ()
		{
			Assert(true);
		}

		public void saveTestCoreFunctionNamespaceURI ()
		{
			Assert(true);
		}
	}
}
