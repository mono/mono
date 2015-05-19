//
// Authors:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2002 Kral Ferch
// (C) 2003 Martin Willemoes Hansen
// Copyright (C) 2010 Novell, Inc.
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
// imported from XPathNavigatorEvaluateTests
//

using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class ExtensionsTest3
	{
		XDocument document;
		XPathNavigator navigator;
		XDocument document2;
		XPathNavigator navigator2;
		XDocument document3;
		XPathNavigator navigator3;
		XPathExpression expression;
		XPathNodeIterator iterator;

		[SetUp]
		public void GetReady ()
		{
			document = XDocument.Parse ("<foo><bar/><baz/><qux/><squonk/></foo>");
			navigator = document.CreateNavigator ();

			document2 = XDocument.Parse ("<foo><bar baz='1'/><bar baz='2'/><bar baz='3'/></foo>");
			navigator2 = document2.CreateNavigator ();

			document3 = XDocument.Parse ("<foo><bar/><baz/><qux/></foo>");
			navigator3 = document3.CreateNavigator ();
		}

		// Testing Core Funcetion Library functions defined at: http://www.w3.org/TR/xpath#corelib
		[Test]
		public void CoreFunctionNodeSetLast ()
		{
			expression = navigator.Compile("last()");
			iterator = navigator.Select("/foo");
			Assert.AreEqual ("1", navigator.Evaluate ("last()").ToString(), "#1");
			Assert.AreEqual ("1", navigator.Evaluate (expression, null).ToString (), "#2");
			Assert.AreEqual ("1", navigator.Evaluate (expression, iterator).ToString (), "#3");
			iterator = navigator.Select("/foo/*");
			Assert.AreEqual ("4", navigator.Evaluate (expression, iterator).ToString (), "#4");
			
			Assert.AreEqual("3", navigator2.Evaluate ("string(//bar[last()]/@baz)"), "#5");
		}

		[Test]
		public void CoreFunctionNodeSetPosition ()
		{
			expression = navigator.Compile("position()");
			iterator = navigator.Select("/foo");
			Assert.AreEqual ("1", navigator.Evaluate ("position()").ToString (), "#1");
			Assert.AreEqual ("1", navigator.Evaluate (expression, null).ToString (), "#2");
			Assert.AreEqual ("0", navigator.Evaluate (expression, iterator).ToString (), "#3");
			iterator = navigator.Select("/foo/*");
			Assert.AreEqual ("0", navigator.Evaluate (expression, iterator).ToString (), "#4");
			iterator.MoveNext();
			Assert.AreEqual ("1", navigator.Evaluate (expression, iterator).ToString (), "#5");
			iterator.MoveNext ();
			Assert.AreEqual ("2", navigator.Evaluate (expression, iterator).ToString (), "#6");
			iterator.MoveNext ();
			Assert.AreEqual ("3", navigator.Evaluate (expression, iterator).ToString (), "#7");
		}

		[Test]
		public void CoreFunctionNodeSetCount ()
		{
			Assert.AreEqual ("5", navigator.Evaluate ("count(//*)").ToString (), "#1");
			Assert.AreEqual ("1", navigator.Evaluate ("count(//foo)").ToString (), "#2");
			Assert.AreEqual ("1", navigator.Evaluate ("count(/foo)").ToString (), "#3");
			Assert.AreEqual ("1", navigator.Evaluate ("count(/foo/bar)").ToString (), "#4");

			Assert.AreEqual ("3", navigator2.Evaluate ("count(//bar)").ToString (), "#5");
		}

		public void saveTestCoreFunctionNodeSetID ()
		{
			document = XDocument.Parse (
				"<!DOCTYPE foo [" +
				"<!ELEMENT foo (bar)>" +
				"<!ELEMENT bar EMPTY>" +
				"<!ATTLIST bar baz ID #REQUIRED>" +
				"]>" +
				"<foo><bar baz='1' qux='hello' /><bar baz='2' qux='world' /></foo>");
			navigator = document.CreateNavigator ();

			Assert.AreEqual ("hello", navigator.Evaluate ("string(id('1')/@qux)").ToString (), "#1");
			Assert.AreEqual ("world", navigator.Evaluate ("string(id('2')/@qux)").ToString (), "#2");
		}

		[Test]
		public void CoreFunctionLocalName ()
		{
			Assert.AreEqual ("", navigator.Evaluate ("local-name()").ToString (), "#1");
			Assert.AreEqual ("", navigator.Evaluate ("local-name(/bogus)").ToString (), "#2");
			Assert.AreEqual ("foo", navigator.Evaluate ("local-name(/foo)").ToString (), "#3");
			Assert.AreEqual ("bar", navigator3.Evaluate ("local-name(/foo/*)").ToString (), "#4");
		}

		// TODO:  umm.  Unable to make this return a namespace-uri so far...
		[Test]
		public void CoreFunctionNamespaceURI ()
		{
			document = XDocument.Parse ("<foo:bar xmlns:foo='#foo'><foo:baz><foo:qux /></foo:baz></foo:bar>");
			navigator = document.CreateNavigator ();

			Assert.AreEqual ("", navigator.Evaluate ("namespace-uri()").ToString (), "#1");
			Assert.AreEqual ("", navigator.Evaluate ("namespace-uri(/bogus)").ToString (), "#2");
			//Assert.AreEqual("foo", navigator.Evaluate ("namespace-uri(/bar)").ToString (), "#3");
			Assert.AreEqual ("", navigator2.Evaluate ("namespace-uri(//bar)").ToString (), "#4");
		}

		public void saveTestCoreFunctionString ()
		{
			document = XDocument.Parse ("<foo>hello<bar>world</bar><baz>how are you</baz></foo>");
			navigator = document.CreateNavigator ();

			Assert.AreEqual ("world", navigator.Evaluate ("string(/foo/*)").ToString (), "#1");
			Assert.AreEqual ("NaN", navigator.Evaluate ("string(0 div 0)").ToString (), "#2");
			
			try {
				navigator.Evaluate ("string(+0)");
				Assert.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}
			
			Assert.AreEqual ("0", navigator.Evaluate ("string(-0)").ToString (), "#3");
			Assert.AreEqual ("Infinity", navigator.Evaluate ("string(1 div 0)").ToString (), "#4");
			Assert.AreEqual ("-Infinity", navigator.Evaluate ("string(-1 div 0)").ToString (), "#5");
			Assert.AreEqual ("45", navigator.Evaluate ("string(45)").ToString (), "#6");
			Assert.AreEqual ("-22", navigator.Evaluate ("string(-22)").ToString (), "#7");
			Assert.AreEqual ("0.25", navigator.Evaluate ("string(.25)").ToString (), "#8");
			Assert.AreEqual ("-0.25", navigator.Evaluate ("string(-.25)").ToString (), "#9");
			Assert.AreEqual ("2", navigator.Evaluate ("string(2.0)").ToString (), "#10");
			Assert.AreEqual ("2.01", navigator.Evaluate ("string(2.01)").ToString (), "#11");
			Assert.AreEqual ("-3", navigator.Evaluate ("string(-3.0)").ToString (), "#12");
			Assert.AreEqual ("3.45", navigator.Evaluate ("string(3.45)").ToString (), "#13");

			// Wonder what this will look like under a different platform.
			Assert.AreEqual("0.33333333333333331", navigator.Evaluate ("string(1 div 3)").ToString (), "#14");
		}

		[Test]
		public void CoreFunctionConcat ()
		{
			try {
				navigator.Evaluate ("concat()");
				Assert.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			try {
				navigator.Evaluate ("concat('foo')");
				Assert.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			Assert.AreEqual ("foobar", navigator.Evaluate ("concat('foo', 'bar')").ToString (), "#1");
			Assert.AreEqual ("foobarbaz", navigator.Evaluate ("concat('foo', 'bar', 'baz')").ToString (), "#2");
			Assert.AreEqual ("foobarbazqux", navigator.Evaluate ("concat('foo', 'bar', 'baz', 'qux')").ToString (), "#3");
			Assert.AreEqual ("foobarbazquxquux", navigator.Evaluate ("concat('foo', 'bar', 'baz', 'qux', 'quux')").ToString (), "#4");
		}

		[Test]
		public void CoreFunctionStartsWith ()
		{
			try {
				navigator.Evaluate ("starts-with()");
				Assert.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			try {
				navigator.Evaluate ("starts-with('foo')");
				Assert.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			try {
				navigator.Evaluate ("starts-with('foo', 'bar', 'baz')");
				Assert.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			Assert.IsTrue ((bool)navigator.Evaluate ("starts-with('foobar', 'foo')"));
			Assert.IsTrue (!(bool)navigator.Evaluate ("starts-with('foobar', 'bar')"));
		}

		[Test]
		public void CoreFunctionContains ()
		{
			try {
				navigator.Evaluate ("contains()");
				Assert.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			try {
				navigator.Evaluate ("contains('foo')");
				Assert.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			try {
				navigator.Evaluate ("contains('foobar', 'oob', 'baz')");
				Assert.Fail ("Expected an XPathException to be thrown.");
			} catch (XPathException) {}

			Assert.IsTrue ((bool)navigator.Evaluate ("contains('foobar', 'oob')"));
			Assert.IsTrue (!(bool)navigator.Evaluate ("contains('foobar', 'baz')"));
		}
	}
}
