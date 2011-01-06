//
// UriTemplate.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class UriTemplateTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull ()
		{
			new UriTemplate (null);
		}

		[Test]
		public void ConstructorEmpty ()
		{
			// it does not raise an error at this state.
			new UriTemplate (String.Empty);
		}

		[Test]
		public void ConstructorNullDictionary ()
		{
			new UriTemplate (String.Empty, null);
		}

		[Test]
		public void IgnoreTrailingSlashDefault ()
		{
			Assert.IsFalse (new UriTemplate (String.Empty).IgnoreTrailingSlash);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConstructorBrokenTemplate ()
		{
			// it used to be allowed but now it isn't in 3.5 SP1.
			new UriTemplate ("{");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConstructorBrokenTemplate2 ()
		{
			new UriTemplate ("http://localhost:8080/{foo}/{");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConstructorBrokenTemplate3 ()
		{
			new UriTemplate ("http://localhost:8080/{foo}/*/baz");
		}

		[Test]
		public void ToString ()
		{
			Assert.AreEqual ("urn:foo", new UriTemplate ("urn:foo").ToString (), "#1");
			// It used to be allowed but now it isn't in 3.5 SP1.
			//Assert.AreEqual ("{", new UriTemplate ("{").ToString (), "#2");
		}

		[Test]
		public void Variables ()
		{
			var t = new UriTemplate ("urn:foo");
			Assert.AreEqual (0, t.PathSegmentVariableNames.Count, "#1a");
			Assert.AreEqual (0, t.QueryValueVariableNames.Count, "#1b");

			t = new UriTemplate ("http://localhost:8080/");
			Assert.AreEqual (0, t.PathSegmentVariableNames.Count, "#2a");
			Assert.AreEqual (0, t.QueryValueVariableNames.Count, "#2b");

			t = new UriTemplate ("http://localhost:8080/foo/");
			Assert.AreEqual (0, t.PathSegmentVariableNames.Count, "#3a");
			Assert.AreEqual (0, t.QueryValueVariableNames.Count, "#3b");

			t = new UriTemplate ("http://localhost:8080/{foo}");
			Assert.AreEqual (1, t.PathSegmentVariableNames.Count, "#4a");
			Assert.AreEqual ("FOO", t.PathSegmentVariableNames [0], "#4b");
			Assert.AreEqual (0, t.QueryValueVariableNames.Count, "#4c");

			// This became invalid in 3.5 SP1
			//t = new UriTemplate ("http://localhost:8080/{foo}/{");
			//Assert.AreEqual (1, t.PathSegmentVariableNames.Count, "#5a");
			//Assert.AreEqual ("FOO", t.PathSegmentVariableNames [0], "#5b");
			//Assert.AreEqual (0, t.QueryValueVariableNames.Count, "#5c");

			t = new UriTemplate ("http://localhost:8080/hoge?test={foo}&test2={bar}");
			Assert.AreEqual (0, t.PathSegmentVariableNames.Count, "#6a");
			Assert.AreEqual (2, t.QueryValueVariableNames.Count, "#6b");
			Assert.AreEqual ("FOO", t.QueryValueVariableNames [0], "#6c");
			Assert.AreEqual ("BAR", t.QueryValueVariableNames [1], "#6d");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void VariablesInSameSegment ()
		{
			new UriTemplate ("http://localhost:8080/{foo}{bar}");
		}

		[Test]
		[Category ("NotDotNet")] //.NET 3.5 SP1 incorrectly matches the port part
		public void VariablesInNonPathQuery ()
		{
			var t = new UriTemplate ("http://localhost:{foo}/");
			Assert.AreEqual (0, t.PathSegmentVariableNames.Count, "#8a");
			Assert.AreEqual (0, t.QueryValueVariableNames.Count, "#8b");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DuplicateNameInTemplate ()
		{
			// one name to two places to match
			new UriTemplate ("http://localhost:8080/hoge?test={foo}&test2={foo}");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DuplicateNameInTemplate2 ()
		{
			// one name to two places to match
			new UriTemplate ("http://localhost:8080/hoge/{foo}?test={foo}");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void BindByNameNullBaseAddress ()
		{
			var t = new UriTemplate ("http://localhost:8080/");
			t.BindByName (null, new NameValueCollection ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BindByNameRelativeBaseAddress ()
		{
			var t = new UriTemplate ("http://localhost:8080/");
			t.BindByName (new Uri ("", UriKind.Relative), new NameValueCollection ());
		}

		[Test]
		[Category ("NotWorking")] // not worthy
		public void BindByNameFileUriBaseAddress ()
		{
			var t = new UriTemplate ("http://localhost:8080/");
			var u = t.BindByName (new Uri ("file:///"), new NameValueCollection ());
			Assert.AreEqual ("file:///http://localhost:8080/", u.ToString ());
		}

		[Test] // it is allowed.
		public void BindByNameFileExtraNames ()
		{
			var t = new UriTemplate ("http://localhost:8080/");
			var n = new NameValueCollection ();
			n.Add ("name", "value");
			t.BindByName (new Uri ("http://localhost/"), n);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BindByNameFileMissingName ()
		{
			var t = new UriTemplate ("/{foo}/");
			t.BindByName (new Uri ("http://localhost/"), new NameValueCollection ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BindInSameSegment ()
		{
			new UriTemplate ("/hoo/{foo}{bar}");
		}

		[Test]
		public void BindByName ()
		{
			var t = new UriTemplate ("/{foo}/{bar}/");
			var n = new NameValueCollection ();
			n.Add ("Bar", "value1"); // case insensitive
			n.Add ("FOO", "value2"); // case insensitive
			var u = t.BindByName (new Uri ("http://localhost/"), n);
			Assert.AreEqual ("http://localhost/value2/value1/", u.ToString ());
		}

		[Test]
		public void BindByNameManySlashes ()
		{
			var t = new UriTemplate ("////{foo}/{bar}/");
			var n = new NameValueCollection ();
			n.Add ("Bar", "value1"); // case insensitive
			n.Add ("FOO", "value2"); // case insensitive
			var u = t.BindByName (new Uri ("http://localhost/"), n);
			Assert.AreEqual ("http://localhost////value2/value1/", u.ToString ());
		}

		[Test]
		public void BindByNameManySlashes2 ()
		{
			var t = new UriTemplate ("////{foo}/{bar}/");
			var n = new NameValueCollection ();
			n.Add ("Bar", "value1"); // case insensitive
			n.Add ("FOO", "value2"); // case insensitive
			var u = t.BindByName (new Uri ("http://localhost//"), n);
			Assert.AreEqual ("http://localhost/////value2/value1/", u.ToString ());
		}
		
		[Test]
		public void BindByNameWithDefaults ()
		{
			var d = new Dictionary<string,string> ();
			d.Add ("Bar", "value1"); // case insensitive
			d.Add ("FOO", "value2"); // case insensitive
			var t = new UriTemplate ("/{foo}/{bar}/", d);
			var u = t.BindByName (new Uri ("http://localhost/"), new NameValueCollection ());
			Assert.AreEqual ("http://localhost/value2/value1/", u.ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BindByNameWithDefaults2 ()
		{
			var d = new Dictionary<string,string> ();
			d.Add ("Bar", "value1"); // case insensitive
			d.Add ("FOO", "value2"); // case insensitive
			var t = new UriTemplate ("/{foo}/{bar}/{baz}", d);
			t.BindByName (new Uri ("http://localhost/"), new NameValueCollection ()); // missing baz
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void BindByPositionNullBaseAddress ()
		{
			var t = new UriTemplate ("http://localhost:8080/");
			t.BindByPosition (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BindByPositionRelativeBaseAddress ()
		{
			var t = new UriTemplate ("http://localhost:8080/");
			t.BindByPosition (new Uri ("", UriKind.Relative));
		}

		[Test]
		[Category ("NotWorking")] // not worthy
		public void BindByPositionFileUriBaseAddress ()
		{
			var t = new UriTemplate ("http://localhost:8080/");
			Assert.AreEqual (new Uri ("file:///http://localhost:8080/"), t.BindByPosition (new Uri ("file:///")));
		}

		[Test] // it is NOT allowed (unlike BindByName)
		[ExpectedException (typeof (FormatException))]
		public void BindByPositionFileExtraValues ()
		{
			var t = new UriTemplate ("http://localhost:8080/");
			t.BindByPosition (new Uri ("http://localhost/"), "value");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void BindByPositionFileMissingValues ()
		{
			var t = new UriTemplate ("/{foo}/");
			t.BindByPosition (new Uri ("http://localhost/"));
		}

		[Test]
		public void BindByPosition ()
		{
			var t = new UriTemplate ("/{foo}/{bar}/");
			var u = t.BindByPosition (new Uri ("http://localhost/"), "value1", "value2");
			Assert.AreEqual ("http://localhost/value1/value2/", u.ToString ());
		}

		[Test]
		[ExpectedException (typeof (FormatException))] // it does not allow default values
		public void BindByPositionWithDefaults ()
		{
			var d = new Dictionary<string,string> ();
			d ["baz"] = "value3";
			var t = new UriTemplate ("/{foo}/{bar}/{baz}", d);
			t.BindByPosition (new Uri ("http://localhost/"), "value1", "value2");
		}

		[Test]
		public void MatchNoTemplateItem ()
		{
			var t = new UriTemplate ("/hooray");
			var n = new NameValueCollection ();
			Assert.IsNotNull (t.Match (new Uri ("http://localhost/"), new Uri ("http://localhost/hooray")), "#1");
			Assert.IsNull (t.Match (new Uri ("http://localhost/"), new Uri ("http://localhost/foobar")), "#2");
			Assert.IsNull (t.Match (new Uri ("http://localhost/"), new Uri ("http://localhost/hooray/foobar")), "#3");
		}

		[Test]
		public void MatchWrongTemplate ()
		{
			var t = new UriTemplate ("/hoo{foo}");
			var n = new NameValueCollection ();
			var m = t.Match (new Uri ("http://localhost/"), new Uri ("http://localhost/hooray"));
			Assert.AreEqual ("ray", m.BoundVariables ["foo"], "#1");
			Assert.IsNull (t.Match (new Uri ("http://localhost/"), new Uri ("http://localhost/foobar")), "#2");
			Assert.IsNull (t.Match (new Uri ("http://localhost/"), new Uri ("http://localhost/hooray/foobar")), "#3");
			Assert.IsNull (t.Match (new Uri ("http://localhost/"), new Uri ("http://localhost/hoo/ray")), "#4");
			Assert.IsNull (t.Match (new Uri ("http://localhost/"), new Uri ("http://localhost/hoo")), "#5");
			// this matches (as if there were no template).
			Assert.IsNotNull (t.Match (new Uri ("http://localhost/"), new Uri ("http://localhost/hoo{foo}")), "#6");
		}

		[Test]
		public void Match ()
		{
			var t = new UriTemplate ("/{foo}/{bar}");
			var n = new NameValueCollection ();
			Uri baseUri = new Uri ("http://localhost/");
			Assert.IsNull (t.Match (baseUri, new Uri ("http://localhost/hooray")), "#1");
			Assert.IsNull (t.Match (baseUri, new Uri ("http://localhost/v1/v2/extra")), "#2");
			Assert.IsNull (t.Match (baseUri, new Uri ("http://localhost/1/2/")), "#3");
			UriTemplateMatch m = t.Match (baseUri, new Uri ("http://localhost/foooo/baaar"));
			Assert.IsNotNull (m, "#4");
			Assert.AreEqual ("foooo", m.BoundVariables ["foo"], "#5");
			Assert.AreEqual ("baaar", m.BoundVariables ["bar"], "#6");
		}

		[Test]
		public void Match2 ()
		{
			var t = new UriTemplate ("/{foo}/{bar}?p1={baz}");
			var n = new NameValueCollection ();
			Uri baseUri = new Uri ("http://localhost/");
			Assert.IsNotNull (t.Match (baseUri, new Uri ("http://localhost/X/Y")), "#1");
			UriTemplateMatch m = t.Match (baseUri, new Uri ("http://localhost/X/Y?p2=v&p1=vv"));
			Assert.IsNotNull (m, "#2");
			// QueryParameters must contain non-template query parameters.
			Assert.AreEqual (2, m.QueryParameters.Count, "#3");
			Assert.AreEqual ("v", m.QueryParameters ["p2"], "#4");
			Assert.AreEqual ("vv", m.QueryParameters ["p1"], "#5");
		}

		[Test]
		public void MatchWildcard ()
		{
			var t = new UriTemplate ("/hoge/*?p1={foo}");
			var m = t.Match (new Uri ("http://localhost"), new Uri ("http://localhost/hoge/ppp/qqq?p1=v1"));
			Assert.IsNotNull (m, "#0");
			Assert.IsNotNull (m.QueryParameters, "#1.0");
			Assert.AreEqual ("v1", m.QueryParameters ["p1"], "#1");
			Assert.IsNotNull (m.WildcardPathSegments, "#2.0");
			Assert.AreEqual (2, m.WildcardPathSegments.Count, "#2");
			Assert.AreEqual ("ppp", m.WildcardPathSegments [0], "#3");
			Assert.AreEqual ("qqq", m.WildcardPathSegments [1], "#4");
		}

		[Test]
		public void IgnoreTrailingSlash ()
		{
			var t = new UriTemplate ("/{foo}/{bar}", true);
			var n = new NameValueCollection ();
			Uri baseUri = new Uri ("http://localhost/");
			Assert.IsNotNull (t.Match (baseUri, new Uri ("http://localhost/v1/v2/")), "#1");

			t = new UriTemplate ("/{foo}/{bar}", false);
			Assert.IsNull (t.Match (baseUri, new Uri ("http://localhost/v1/v2/")), "#2");
		}

		[Test]
		public void SimpleWebGet () {
			UriTemplate t = new UriTemplate ("GetBlog");
			Assert.IsNotNull(t.Match(new Uri("http://localhost:8000/BlogService"),
				new Uri("http://localhost:8000/BlogService/GetBlog")), "Matches simple WebGet method");
			Assert.IsNull(t.Match (new Uri ("http://localhost:8000/BlogService"),
				new Uri ("http://localhost:8000/BlogService/GetData")), "Doesn't match wrong WebGet method");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DictContainsNullValue ()
		{
			var t = new UriTemplate ("/id-{foo}/{bar}");
			var dic = new Dictionary<string,string> ();
			dic ["foo"] = null;
			dic ["bar"] = "bbb";
			t.BindByName (new Uri ("http://localhost:8080"), dic);
		}

		[Test]
		public void DictContainsCaseInsensitiveKey ()
		{
			var t = new UriTemplate ("/id-{foo}/{bar}");
			var dic = new Dictionary<string,string> ();
			dic ["foo"] = "aaa";
			dic ["Bar"] = "bbb";
			var uri = t.BindByName (new Uri ("http://localhost:8080"), dic);
			Assert.AreEqual ("http://localhost:8080/id-aaa/bbb", uri.ToString ());
		}

	}
}
