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
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlAttributesTests
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
			Assert.IsNull (atts.XmlAnyAttribute, "#1");
			Assert.IsNotNull (atts.XmlAnyElements, "#2");
			Assert.AreEqual (0, atts.XmlAnyElements.Count, "#3");
			Assert.IsNull (atts.XmlArray, "#4");
			Assert.IsNotNull (atts.XmlArrayItems, "#5");
			Assert.AreEqual (0, atts.XmlArrayItems.Count, "#6");
			Assert.IsNull (atts.XmlAttribute, "#7");
			Assert.IsNull (atts.XmlChoiceIdentifier, "#8");
			Assert.IsNotNull (atts.XmlDefaultValue, "#9");
			// DBNull??
			Assert.AreEqual (DBNull.Value, atts.XmlDefaultValue, "#10");
			Assert.IsNotNull (atts.XmlElements, "#11");
			Assert.AreEqual (0, atts.XmlElements.Count, "#12");
			Assert.IsNull (atts.XmlEnum, "#13");
			Assert.IsNotNull (atts.XmlIgnore, "#14");
			Assert.AreEqual (TypeCode.Boolean, atts.XmlIgnore.GetTypeCode (), "#15");
			Assert.AreEqual (false, atts.Xmlns, "#16");
			Assert.IsNull (atts.XmlRoot, "#17");
			Assert.IsNull (atts.XmlText, "#18");
			Assert.IsNull (atts.XmlType, "#19");
		}

		[Test]
		public void XmlTextAttribute ()
		{
			// based on default ctor.
			XmlTextAttribute attr = new XmlTextAttribute ();
			Assert.AreEqual ("", attr.DataType, "#1");
			Assert.IsNull (attr.Type, "#2");
			// based on a type.
			XmlTextAttribute attr2 = new XmlTextAttribute (typeof (XmlNode));
			Assert.AreEqual ("", attr.DataType, "#3");
			Assert.IsNull (attr.Type, "#4");
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
				Assert.Fail ("Should be invalid.");
			} catch (InvalidOperationException ex) {
			}
		}
		
		[Test]
		public void XmlIgnore ()
		{
			FieldInfo field = GetType ().GetField ("XmlIgnoreField");
			XmlAttributes atts = new XmlAttributes (field);
			Assert.AreEqual (true, atts.XmlIgnore, "#1");
			Assert.AreEqual (0, atts.XmlElements.Count, "#2");
			Assert.AreEqual (0, atts.XmlAnyElements.Count, "#3");
		}
		
		[XmlIgnore]
		[XmlElement (IsNullable = true)]
		[XmlAnyElement]
		public int XmlIgnoreField;
	}
}
