//
// System.Xml.XmlSchema.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaTests
	{
		[Test]
		public void TestRead ()
		{
			XmlSchema som = XmlSchema.Read (new XmlTextReader ("XmlFiles/xsd/xml.xsd"), null);
		}
	}
}
