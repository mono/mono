//
// XsdDatatypeTests.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
//

using System;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;
using NUnit.Framework;

namespace MonoTests.Commons.Xml.Relaxng
{
/*
	[TestFixture]
	public class XsdDatatypeTests
	{
		[Test]
		[ExpectedException (typeof (RelaxngException))]
		public void SchemaDatatypeTest ()
		{
			string xml = "<foo>a</foo>";
			string rng = "<element xmlns='http://relaxng.org' name='foo'><data type='int' datatypeLibrary='http://www.w3.org/2001/XMLSchema-datatypes'/></element>";
			RelaxngValidatingReader r = new RelaxngValidatingReader (
				new XmlTextReader (xml, XmlNodeType.Document, null),
				new XmlTextReader (rng, XmlNodeType.Document, null));
			while (!r.EOF)
				r.Read ();
		}
	}
*/
}