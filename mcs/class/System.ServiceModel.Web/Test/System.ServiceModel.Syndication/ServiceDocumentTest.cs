//
// ServiceDocumentTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
#if !MOBILE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.ServiceModel.Syndication;
using NUnit.Framework;

using QName = System.Xml.XmlQualifiedName;

namespace MonoTests.System.ServiceModel.Syndication
{
	[TestFixture]
	public class ServiceDocumentTest
	{
		static XmlWriterSettings settings = new XmlWriterSettings () { OmitXmlDeclaration = true};

		[Test]
		public void ConstructorNullWorkspaces ()
		{
			new ServiceDocument (null); // null workspaces is allowed
		}

		[Test]
		public void GetFormatter ()
		{
			var v = new ServiceDocument ();
			var f = v.GetFormatter ();
			Assert.IsTrue (f is AtomPub10ServiceDocumentFormatter, "#1");
			Assert.IsTrue (f.Document == v, "#2");
		}

		[Test]
		public void Save ()
		{
			var v = new ServiceDocument ();
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw, settings))
				v.Save (xw);
			Assert.AreEqual ("<app:service xmlns:a10=\"http://www.w3.org/2005/Atom\" xmlns:app=\"http://www.w3.org/2007/app\" />", sw.ToString ());
		}
	}
}
#endif