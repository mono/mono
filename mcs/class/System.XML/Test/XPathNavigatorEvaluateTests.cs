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
		XPathNavigator navigator;
		XmlDocument document2;
		XPathNavigator navigator2;
		XmlDocument document3;
		XPathNavigator navigator3;
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

			document3 = new XmlDocument ();
			document3.LoadXml ("<foo><bar/><baz/><qux/></foo>");
			navigator3 = document3.CreateNavigator ();
		}

		// Testing Core Function Library functions defined at: http://www.w3.org/TR/xpath#corelib
		public void TestCoreFunctionNodeSetLast ()
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

		public void TestCoreFunctionNodeSetCount ()
		{
			AssertEquals ("5", navigator.Evaluate ("count(//*)").ToString ());
			AssertEquals ("1", navigator.Evaluate ("count(//foo)").ToString ());
			AssertEquals ("1", navigator.Evaluate ("count(/foo)").ToString ());
			AssertEquals ("1", navigator.Evaluate ("count(/foo/bar)").ToString ());

			AssertEquals ("3", navigator2.Evaluate ("count(//bar)").ToString ());
		}

		public void saveTestCoreFunctionNodeSetID ()
		{
			document.LoadXml (
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

		public void TestCoreFunctionLocalName ()
		{
			AssertEquals("", navigator.Evaluate ("local-name()").ToString ());
			AssertEquals("", navigator.Evaluate ("local-name(/bogus)").ToString ());
			AssertEquals("foo", navigator.Evaluate ("local-name(/foo)").ToString ());
			AssertEquals("bar", navigator3.Evaluate ("local-name(/foo/*)").ToString ());
		}

		// TODO:  umm.  Unable to make this return a namespace-uri so far...
		public void TestCoreFunctionNamespaceURI ()
		{
			document.LoadXml ("<foo:bar xmlns:foo='#foo'><foo:baz><foo:qux /></foo:baz></foo:bar>");
			navigator = document.CreateNavigator();

			AssertEquals("", navigator.Evaluate ("namespace-uri()").ToString ());
			AssertEquals("", navigator.Evaluate ("namespace-uri(/bogus)").ToString ());
			//AssertEquals("foo", navigator.Evaluate ("namespace-uri(/bar)").ToString ());
			AssertEquals("", navigator2.Evaluate ("namespace-uri(//bar)").ToString ());
		}

		public void TestCoreFunctionString ()
		{
			document.LoadXml ("<foo>hello<bar>world</bar><baz>how are you</baz></foo>");
			navigator = document.CreateNavigator();

			AssertEquals("world", navigator.Evaluate ("string(/foo/*)").ToString ());
			AssertEquals("NaN", navigator.Evaluate ("string(0 div 0)").ToString ());
			//AssertEquals("0", navigator.Evaluate ("string(+0)").ToString ());
			//AssertEquals("0", navigator.Evaluate ("string(-0)").ToString ());
			AssertEquals("Infinity", navigator.Evaluate ("string(1 div 0)").ToString ());
			AssertEquals("-Infinity", navigator.Evaluate ("string(-1 div 0)").ToString ());
			AssertEquals("45", navigator.Evaluate ("string(45)").ToString ());
			AssertEquals("-22", navigator.Evaluate ("string(-22)").ToString ());
			AssertEquals("0.25", navigator.Evaluate ("string(.25)").ToString ());
			AssertEquals("-0.25", navigator.Evaluate ("string(-.25)").ToString ());
			AssertEquals("2", navigator.Evaluate ("string(2.0)").ToString ());
			AssertEquals("2.01", navigator.Evaluate ("string(2.01)").ToString ());
			AssertEquals("-3", navigator.Evaluate ("string(-3.0)").ToString ());
			AssertEquals("3.45", navigator.Evaluate ("string(3.45)").ToString ());

			// Wonder what this will look like under a different platform.
			AssertEquals("0.33333333333333331", navigator.Evaluate ("string(1 div 3)").ToString ());
		}

		public void saveTestCoreFunctionConcat ()
		{
		}

		public void saveTestCoreFunctionStartsWith ()
		{
		}

		public void saveTestCoreFunctionContains ()
		{
		}
	}
}
