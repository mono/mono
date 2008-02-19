//
// XmlSchemasTests.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.
//

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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlSchemasTests
	{
		[Test]
		public void Bug360541 ()
		{
			XmlSchemaComplexType stype = GetStype ();
			
			XmlSchemaElement selem1 = new XmlSchemaElement ();
			selem1.Name = "schema";
			selem1.SchemaType = stype;

			XmlSchema schema = new XmlSchema ();
			schema.Items.Add (selem1);

			XmlSchemas xs = new XmlSchemas ();
			xs.Add (schema);

			xs.Find (XmlQualifiedName.Empty, typeof (XmlSchemaElement));

			selem1 = new XmlSchemaElement ();
			selem1.Name = "schema1";
			selem1.SchemaType = stype;

			schema = new XmlSchema ();
			schema.Items.Add (selem1);

			xs = new XmlSchemas ();
			xs.Add (schema);
			xs.Find (XmlQualifiedName.Empty, typeof (XmlSchemaElement));
		}

		XmlSchemaComplexType GetStype ()
		{
			XmlSchemaSequence seq = new XmlSchemaSequence ();
			seq.Items.Add (new XmlSchemaAny ());
		
			XmlSchemaComplexType stype = new XmlSchemaComplexType ();
			stype.Particle = seq;
			
			return stype;
		}
	}
}
