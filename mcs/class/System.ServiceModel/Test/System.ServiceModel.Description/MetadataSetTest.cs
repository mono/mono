//
// MetadataSetTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2011 Novell, Inc.  http://www.novell.com
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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class MetadataSetTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ReadFromNull ()
		{
			MetadataSet.ReadFrom (null);
		}

		[Test]
		public void ReadFrom ()
		{
			XmlReader xr = XmlTextReader.Create ("Test/XmlFiles/one.xml");
			var metadata = MetadataSet.ReadFrom (xr);
			Assert.AreEqual (5, metadata.MetadataSections.Count, "#1");
			Assert.AreEqual (2, metadata.MetadataSections.Where (m => m.Dialect == MetadataSection.ServiceDescriptionDialect).Count (), "#2");
			Assert.AreEqual (3, metadata.MetadataSections.Where (m => m.Dialect == MetadataSection.XmlSchemaDialect).Count (), "#3");
		}
	}
}
#endif
