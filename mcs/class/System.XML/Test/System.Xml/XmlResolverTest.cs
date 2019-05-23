//
// XmlResolverTest.cs - NUnit Test Cases for XmlResolver
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Net;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml {

	[TestFixture]
	public class XmlResolverTest {

		class ConcreteXmlResolver : XmlResolver {

			public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
			{
				throw new NotImplementedException ();
			}

			public override ICredentials Credentials {
				set { throw new NotImplementedException (); }
			}
		}

		bool isWin32 = false;
		
		[SetUp]
		public void SetUp ()
		{
			isWin32 = (Path.DirectorySeparatorChar == '\\');
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ResolveUri_ ()
		{
			ConcreteXmlResolver xr = new ConcreteXmlResolver ();
			xr.ResolveUri (null, null);
		}
		[Test]
		public void ResolveUri ()
		{
			ConcreteXmlResolver xr = new ConcreteXmlResolver ();

			Uri uri = xr.ResolveUri (null, "/Moonlight");
			// note: this is *very* different from [silver|moon]light
			Assert.IsTrue (uri.IsAbsoluteUri, "null,string");
			if (isWin32) {
				string currentdir = Directory.GetCurrentDirectory ();
				string volume = currentdir.Substring (0, 2);
				Assert.AreEqual ("file:///" + volume + "/Moonlight", uri.ToString (), "ToString");
			} else
				Assert.AreEqual ("file:///Moonlight", uri.ToString (), "ToString");

			uri = new Uri ("http://www.example.com");
			Uri u2 = xr.ResolveUri (uri, null);
			Assert.AreEqual (uri, u2, "Equals");
			Assert.IsTrue (Object.ReferenceEquals (uri, u2), "ReferenceEquals");

			u2 = xr.ResolveUri (uri, "/Moonlight");
			Assert.IsTrue (uri.IsAbsoluteUri, "abs,string");
			Assert.AreEqual ("http://www.example.com/Moonlight", u2.ToString (), "ToString3");

			u2 = xr.ResolveUri (null, "http://www.example.com");
			Assert.IsTrue (u2.IsAbsoluteUri, "null,absolute/http");
			Assert.AreEqual (uri, u2, "Equals-2");

			u2 = xr.ResolveUri (null, "https://www.example.com");
			Assert.IsTrue (u2.IsAbsoluteUri, "null,absolute/https");

			u2 = xr.ResolveUri (null, "ftp://example.com/download");
			Assert.IsTrue (u2.IsAbsoluteUri, "null,absolute/ftp");

			u2 = xr.ResolveUri (null, "file:///mystuff");
			Assert.IsTrue (u2.IsAbsoluteUri, "null,absolute/file");
		}

		class AsyncXmlResolver : XmlResolver
		{
			public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
			{
				throw new AssertionException ("Should not be reached");
			}
		}

		[Test]
		[ExpectedException(typeof(NotImplementedException))]
		[Category("Async")]
		public void TestAsync ()
		{
			var ar = new AsyncXmlResolver ();
			var uri = new Uri ("http://www.example.com");
			ar.GetEntityAsync (uri, null, typeof(string));
		}
	}
}
