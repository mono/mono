//
// System.Xml.XmlAttributesTests
//
// Author:
//   Atsushi Enomoto
//
// (C) 2003 Atsushi Enomoto
//

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlAttributesTests : Assertion
	{
		StringWriter sw;
		XmlTextWriter xtw;
		XmlSerializer xs;

		private void SetUpWriter ()
		{
			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			xtw.Formatting = Formatting.None;
		}
		
		private string WriterText 
		{
			get
			{
				string val = sw.GetStringBuilder ().ToString();
				int offset = val.IndexOf ('>') + 1;
				val = val.Substring (offset);
				return val;
			}
		}

		private void Serialize (object o, XmlAttributeOverrides ao)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType (), ao);
			xs.Serialize (xtw, o);
		}
		
		private void Serialize (object o, XmlRootAttribute root)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType(), root);
			xs.Serialize (xtw, o);
		}

		// Testcases.

		[Test]
		public void NewXmlAttributes ()
		{
			// seems not different from Type specified ctor().
			XmlAttributes atts = new XmlAttributes ();
			AssertNull (atts.XmlAnyAttribute);
			AssertNotNull (atts.XmlAnyElements);
			AssertEquals (0, atts.XmlAnyElements.Count);
			AssertNull (atts.XmlArray);
			AssertNotNull (atts.XmlArrayItems);
			AssertEquals (0, atts.XmlArrayItems.Count);
			AssertNull (atts.XmlAttribute);
			AssertNull (atts.XmlChoiceIdentifier);
			AssertNotNull (atts.XmlDefaultValue);
			// DBNull??
			AssertEquals (DBNull.Value, atts.XmlDefaultValue);
			AssertNotNull (atts.XmlElements);
			AssertEquals (0, atts.XmlElements.Count);
			AssertNull (atts.XmlEnum);
			AssertNotNull (atts.XmlIgnore);
			AssertEquals (TypeCode.Boolean, atts.XmlIgnore.GetTypeCode ());
			AssertEquals (false, atts.Xmlns);
			AssertNull (atts.XmlRoot);
			AssertNull (atts.XmlText);
			AssertNull (atts.XmlType);
		}

		[Test]
		public void XmlTextAttribute ()
		{
			// based on default ctor.
			XmlTextAttribute attr = new XmlTextAttribute ();
			AssertEquals ("", attr.DataType);
			AssertNull (attr.Type);
			// based on a type.
			XmlTextAttribute attr2 = new XmlTextAttribute (typeof (XmlNode));
			AssertEquals ("", attr.DataType);
			AssertNull (attr.Type);
		}

		[Test]
		public void XmlInvalidElementAttribute ()
		{
			XmlAttributeOverrides ao = new XmlAttributeOverrides ();
			XmlAttributes atts = new XmlAttributes ();
			atts.XmlElements.Add (new XmlElementAttribute ("xInt"));
			ao.Add (typeof (int), atts);
			try {
				Serialize (10, ao);
				Fail ("Should be invalid.");
			} catch (InvalidOperationException ex) {
			}
		}
	}
}
