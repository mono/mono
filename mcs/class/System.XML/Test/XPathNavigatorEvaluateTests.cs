//
// MonoTests.System.Xml.XPathNavigatorEvaluateTests
//
// Authors:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Kral Ferch
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XPathNavigatorEvaluateTests
	{
		XmlDocument document;
		XPathNavigator navigator;
		XmlDocument document2;
		XPathNavigator navigator2;
		XmlDocument document3;
		XPathNavigator navigator3;
		XPathExpression expression;
		XPathNodeIterator iterator;

		[SetUp]
		public void GetReady ()
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

		// Testing Core Funcetion Library functions defined at: http://www.w3.org/TR/xpath#corelib
		[Test]
		public void CoreFunctionNodeSetLast ()
		{
			expression = navigator.Compile("last()");
			iterator = navigator.Select("/foo");
			Assertion.AssertEquals ("0", navigator.Evaluate ("last()").ToString());
			Assertion.AssertEquals ("0", navigator.Evaluate (expression, null).ToString ());
			Assertion.AssertEquals ("1", navigator.Evaluate (expression, iterator).ToString ());
			iterator = navigator.Select("/foo/*");
			Assertion.AssertEquals ("4", navigator.Evaluate (expression, iterator).ToString ());
			
			Assertion.AssertEquals("3", navigator2.Evaluate ("string(//bar[last()]/@baz)"));
		}

		[Test]
		public void CoreFunctionNodeSetPosition ()
		{
			expression = navigator.Compile("position()");
			iterator = navigator.Select("/foo");
			Assertion.AssertEquals ("0", navigator.Evaluate ("position()").ToString ());
			Assertion.AssertEquals ("0", navigator.Evaluate (expression, null).ToString ());
			Assertion.AssertEquals ("0", navigator.Evaluate (expression, iterator).ToString ());
			iterator = navigator.Select("/foo/*");
			Assertion.AssertEquals ("0", navigator.Evaluate (expression, iterator).ToString ());
			iterator.MoveNext();
			Assertion.AssertEquals ("1", navigator.Evaluate (expression, iterator).ToString ());
			iterator.MoveNext ();
			Assertion.AssertEquals ("2", navigator.Evaluate (expression, iterator).ToString ());
			iterator.MoveNext ();
			Assertion.AssertEquals ("3", navigator.Evaluate (expression, iterator).ToString ());
		}

		[Test]
		public void CoreFunctionNodeSetCount ()
		{
			Assertion.AssertEquals ("5", navigator.Evaluate ("count(//*)").ToString ());
			Assertion.AssertEquals ("1", navigator.Evaluate ("count(//foo)").ToString ());
			Assertion.AssertEquals ("1", navigator.Evaluate ("count(/foo)").ToString ());
			Assertion.AssertEquals ("1", navigator.Evaluate ("count(/foo/bar)").ToString ());

			Assertion.AssertEquals ("3", navigator2.Evaluate ("count(//bar)").ToString ());
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

			Assertion.AssertEquals ("hello", navigator.Evaluate ("string(id('1')/@qux)").ToString ());
			Assertion.AssertEquals ("world", navigator.Evaluate ("string(id('2')/@qux)").ToString ());
		}

		[Test]
		public void CoreFunctionLocalName ()
		{
			Assertion.AssertEquals ("", navigator.Evaluate ("local-name()").ToString ());
			Assertion.AssertEquals ("", navigator.Evaluate ("local-name(/bogus)").ToString ());
			Assertion.AssertEquals ("foo", navigator.Evaluate ("local-name(/foo)").ToString ());
			Assertion.AssertEquals ("bar", navigator3.Evaluate ("local-name(/foo/*)").ToString ());
		}

		// TODO:  umm.  Unable to make this return a namespace-uri so far...
		[Test]
		public void CoreFunctionNamespaceURI ()
		{
			document.LoadXml ("<foo:bar xmlns:foo='#foo'><foo:baz><foo:qux /></foo:baz></foo:bar>");
			navigator = document.CreateNavigator ();

			Assertion.AssertEquals ("", navigator.Evaluate ("namespace-uri()").ToString ());
			Assertion.AssertEquals ("", navigator.Evaluate ("namespace-uri(/bogus)").ToString ());
			//Assertion.AssertEquals("foo", navigator.Evaluate ("namespace-uri(/bar)").ToString ());
			Assertion.AssertEquals ("", navigator2.Evaluate ("namespace-uri(//bar)").ToString ());
		}

		public void saveTestCoreFunctionString ()
		{
			document.LoadXml ("<foo>hello<bar>world</bar><baz>how are you</baz></foo>");
			navigator = document.CreateNavigator ();

			Assertion.AssertEquals ("world", navigator.Evaluate ("string(/foo/*)").ToString ());
			Assertion.AssertEquals ("NaN", navigator.Evaluate ("string(0 div 0)").ToString ());
			
			try {
				navigator.Evaluate ("string(+0)");
				Assertion.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}
			
			Assertion.AssertEquals ("0", navigator.Evaluate ("string(-0)").ToString ());			
			Assertion.AssertEquals ("Infinity", navigator.Evaluate ("string(1 div 0)").ToString ());
			Assertion.AssertEquals ("-Infinity", navigator.Evaluate ("string(-1 div 0)").ToString ());
			Assertion.AssertEquals ("45", navigator.Evaluate ("string(45)").ToString ());
			Assertion.AssertEquals ("-22", navigator.Evaluate ("string(-22)").ToString ());
			Assertion.AssertEquals ("0.25", navigator.Evaluate ("string(.25)").ToString ());
			Assertion.AssertEquals ("-0.25", navigator.Evaluate ("string(-.25)").ToString ());
			Assertion.AssertEquals ("2", navigator.Evaluate ("string(2.0)").ToString ());
			Assertion.AssertEquals ("2.01", navigator.Evaluate ("string(2.01)").ToString ());
			Assertion.AssertEquals ("-3", navigator.Evaluate ("string(-3.0)").ToString ());
			Assertion.AssertEquals ("3.45", navigator.Evaluate ("string(3.45)").ToString ());

			// Wonder what this will look like under a different platform.
			Assertion.AssertEquals("0.33333333333333331", navigator.Evaluate ("string(1 div 3)").ToString ());
		}

		[Test]
		public void CoreFunctionConcat ()
		{
			try {
				navigator.Evaluate ("concat()");
				Assertion.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			try {
				navigator.Evaluate ("concat('foo')");
				Assertion.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			Assertion.AssertEquals ("foobar", navigator.Evaluate ("concat('foo', 'bar')").ToString ());
			Assertion.AssertEquals ("foobarbaz", navigator.Evaluate ("concat('foo', 'bar', 'baz')").ToString ());
			Assertion.AssertEquals ("foobarbazqux", navigator.Evaluate ("concat('foo', 'bar', 'baz', 'qux')").ToString ());
			Assertion.AssertEquals ("foobarbazquxquux", navigator.Evaluate ("concat('foo', 'bar', 'baz', 'qux', 'quux')").ToString ());
		}

		[Test]
		public void CoreFunctionStartsWith ()
		{
			try {
				navigator.Evaluate ("starts-with()");
				Assertion.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			try {
				navigator.Evaluate ("starts-with('foo')");
				Assertion.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			try {
				navigator.Evaluate ("starts-with('foo', 'bar', 'baz')");
				Assertion.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			Assertion.Assert ((bool)navigator.Evaluate ("starts-with('foobar', 'foo')"));
			Assertion.Assert (!(bool)navigator.Evaluate ("starts-with('foobar', 'bar')"));
		}

		[Test]
		public void CoreFunctionContains ()
		{
			try {
				navigator.Evaluate ("contains()");
				Assertion.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			try {
				navigator.Evaluate ("contains('foo')");
				Assertion.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			try {
				navigator.Evaluate ("contains('foobar', 'oob', 'baz')");
				Assertion.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			Assertion.Assert ((bool)navigator.Evaluate ("contains('foobar', 'oob')"));
			Assertion.Assert (!(bool)navigator.Evaluate ("contains('foobar', 'baz')"));
		}
	}
}
