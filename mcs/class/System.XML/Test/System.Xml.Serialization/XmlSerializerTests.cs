//
// System.Xml.XmlSerializerTests
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//
//
// NOTES:
//  Where possible, these tests avoid testing the order of
//  an object's members serialization. Mono and .NET do not
//  reflect members in the same order.
//
//  Only serializations tests so far, no deserialization.
//
// FIXME
//  test XmlArrayAttribute
//  test XmlArrayItemAttribute
//  test serialization of decimal type
//  test serialization of Guid type
//  test XmlNode serialization with and without modifying attributes.
//  test deserialization
//  FIXMEs found in this file

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSerializerTests : Assertion
	{
		StringWriter sw;
		XmlTextWriter xtw;
		XmlSerializer xs;

		private void SetUpWriter()
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
				string val = sw.GetStringBuilder().ToString();
				int offset = val.IndexOf('>') + 1;
				val = val.Substring(offset);
				return val;
			}
		}

		private void Serialize(object o)
		{
			SetUpWriter();
			xs = new XmlSerializer(o.GetType());
			xs.Serialize(xtw, o);
		}
		
		private void Serialize(object o, Type type)
		{
			SetUpWriter();
			xs = new XmlSerializer(type);
			xs.Serialize(xtw, o);
		}

		private void Serialize(object o, XmlSerializerNamespaces ns)
		{
			SetUpWriter();
			xs = new XmlSerializer(o.GetType());
			xs.Serialize(xtw, o, ns);
		}

		private void Serialize(object o, XmlAttributeOverrides ao)
		{
			SetUpWriter();
			xs = new XmlSerializer(o.GetType(), ao);
			xs.Serialize(xtw, o);
		}
		
		private void Serialize(object o, XmlRootAttribute root)
		{
			SetUpWriter();
			xs = new XmlSerializer(o.GetType(), root);
			xs.Serialize(xtw, o);
		}
		
		// test constructors
#if USE_VERSION_1_1	// It doesn't pass on MS.NET 1.1.
		[Test]
		public void TestConstructor()
		{
			XmlSerializer ser = new XmlSerializer (null, "");
		}
#else
#endif

		// test basic types ////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeInt()
		{
			Serialize(10);
			AssertEquals("<int>10</int>", WriterText);
		}

		[Test]
		public void TestSerializeBool()
		{
			Serialize(true);
			AssertEquals("<boolean>true</boolean>", WriterText);
			
			Serialize(false);
			AssertEquals("<boolean>false</boolean>", WriterText);
		}
		
		[Test]
		public void TestSerializeString()
		{
			Serialize("hello");
			AssertEquals("<string>hello</string>", WriterText);
		}

		[Test]
		public void TestSerializeEmptyString()
		{
			Serialize(String.Empty);
			AssertEquals("<string />", WriterText);
		}
		
		[Test]
		public void TestSerializeNullObject()
		{
			Serialize(null, typeof(object));
			AssertEquals("<anyType xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />", WriterText);
		}

		[Test]
		public void TestSerializeNullString()
		{
			Serialize(null, typeof(string));
			AssertEquals ("<string xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />", WriterText);
		}
			
		[Test]
		public void TestSerializeIntArray()
		{
			Serialize(new int[] {1, 2, 3, 4});
			AssertEquals ("<ArrayOfInt xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><int>1</int><int>2</int><int>3</int><int>4</int></ArrayOfInt>", WriterText);
		}
		
		[Test]
		public void TestSerializeEmptyArray()
		{
			Serialize(new int[] {});
			AssertEquals("<ArrayOfInt xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />", WriterText);
		}
		
		[Test]
		public void TestSerializeChar()
		{
			Serialize('A');
			AssertEquals("<char>65</char>", WriterText);
			
			Serialize('\0');
			AssertEquals("<char>0</char>", WriterText);
			
			Serialize('\n');
			AssertEquals("<char>10</char>", WriterText);
			
			Serialize('\uFF01');
			AssertEquals("<char>65281</char>", WriterText);
		}
		
		[Test]
		public void TestSerializeFloat()
		{
			Serialize(10.78);
			AssertEquals("<double>10.78</double>", WriterText);
			
			Serialize(-1e8);
			AssertEquals("<double>-100000000</double>", WriterText);
			
			// FIXME test INF and other boundary conditions that may exist with floats
		}
		
		
		[Test]
		public void TestSerializeEnumeration()
		{
			Serialize(SimpleEnumeration.FIRST);
			AssertEquals("<SimpleEnumeration>FIRST</SimpleEnumeration>", WriterText);
			
			Serialize(SimpleEnumeration.SECOND);
			AssertEquals("<SimpleEnumeration>SECOND</SimpleEnumeration>", WriterText);
		}
		
		[Test]
		public void TestSerializeQualifiedName()
		{
			Serialize(new XmlQualifiedName("me", "home.urn"));
			AssertEquals("<QName xmlns:q1='home.urn'>q1:me</QName>", WriterText);
		}
		
		[Test]
		public void TestSerializeBytes()
		{
			Serialize((byte)0xAB);
			AssertEquals("<unsignedByte>171</unsignedByte>", WriterText);
			
			Serialize((byte)15);
			AssertEquals("<unsignedByte>15</unsignedByte>", WriterText);
		}
		
		[Test]
		public void TestSerializeByteArrays()
		{
			Serialize(new byte[] {});
			AssertEquals("<base64Binary />", WriterText);
			
			Serialize(new byte[] {0xAB, 0xCD});
			AssertEquals("<base64Binary>q80=</base64Binary>", WriterText);
		}
		
		[Test]
		public void TestSerializeDateTime()
		{
			DateTime d = new DateTime();
			Serialize(d);
			AssertEquals ("<dateTime>0001-01-01T00:00:00.0000000", WriterText.Substring (0, 37));
		}

		/*
		FIXME
		 - decimal
		 - Guid
		 - XmlNode objects
		
		[Test]
		public void TestSerialize()
		{
			Serialize();
			AssertEquals(WriterText, "");
		}
		*/
		
		// test basic class serialization /////////////////////////////////////		
		[Test]
		public void TestSerializeSimpleClass()
		{
			SimpleClass simple = new SimpleClass();
			Serialize(simple);
			AssertEquals("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />", WriterText);
			
			simple.something = "hello";
			
			Serialize(simple);
			AssertEquals("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>", WriterText);
		}
		
		[Test]
		public void TestSerializeStringCollection()
		{
			StringCollection strings = new StringCollection();
			Serialize(strings);
			AssertEquals(WriterText, "<ArrayOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />");
			
			strings.Add("hello");
			strings.Add("goodbye");
			Serialize(strings);
			Assert(WriterText.EndsWith("><string>hello</string><string>goodbye</string></ArrayOfString>"));
		}
		
		[Test]
		public void TestSerializePlainContainer()
		{
			StringCollectionContainer container = new StringCollectionContainer();
			Serialize(container);
			Assert(WriterText.EndsWith("><Messages /></StringCollectionContainer>"));
			
			container.Messages.Add("hello");
			container.Messages.Add("goodbye");
			Serialize(container);
			Assert(WriterText.EndsWith("><Messages><string>hello</string><string>goodbye</string></Messages></StringCollectionContainer>"));
		}

		[Test]
		public void TestSerializeArrayContainer()
		{
			ArrayContainer container = new ArrayContainer();
			Serialize(container);
			AssertEquals(WriterText, "<ArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />");
			
			container.items = new object[] {10, 20};
			Serialize(container);
			Assert(WriterText.EndsWith("><items><anyType xsi:type='xsd:int'>10</anyType><anyType xsi:type='xsd:int'>20</anyType></items></ArrayContainer>"));
			
			container.items = new object[] {10, "hello"};
			Serialize(container);
			Assert(WriterText.EndsWith("><items><anyType xsi:type='xsd:int'>10</anyType><anyType xsi:type='xsd:string'>hello</anyType></items></ArrayContainer>"));
		}
		
		[Test]
		public void TestSerializeClassArrayContainer()
		{
			ClassArrayContainer container = new ClassArrayContainer();
			Serialize(container);
			AssertEquals(WriterText, "<ClassArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />");
			
			SimpleClass simple1 = new SimpleClass();
			simple1.something = "hello";
			SimpleClass simple2 = new SimpleClass();
			simple2.something = "hello";
			container.items = new SimpleClass[2];
			container.items[0] = simple1;
			container.items[1] = simple2;
			Serialize(container);
			Assert(WriterText.EndsWith("><items><SimpleClass><something>hello</something></SimpleClass><SimpleClass><something>hello</something></SimpleClass></items></ClassArrayContainer>"));
		}
		
		// test basic attributes ///////////////////////////////////////////////
		[Test]
		public void TestSerializeSimpleClassWithXmlAttributes()
		{
			SimpleClassWithXmlAttributes simple = new SimpleClassWithXmlAttributes();
			Serialize(simple);
			AssertEquals("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />", WriterText);

			simple.something = "hello";
			Serialize(simple);
			AssertEquals ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' member='hello' />", WriterText);
		}
		
		// test overrides ///////////////////////////////////////////////////////
		[Test]
		public void TestSerializeSimpleClassWithOverrides()
		{
			// Also tests XmlIgnore
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			
			XmlAttributes attr = new XmlAttributes();
			attr.XmlIgnore = true;
			overrides.Add(typeof(SimpleClassWithXmlAttributes), "something", attr);
			
			SimpleClassWithXmlAttributes simple = new SimpleClassWithXmlAttributes();
			simple.something = "hello";
			Serialize(simple, overrides);
			AssertEquals("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />", WriterText);
		}
		
		// test xmlText //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlTextAttribute()
		{
			SimpleClass simple = new SimpleClass();
			simple.something = "hello";
			
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes attr = new XmlAttributes();
			overrides.Add(typeof(SimpleClass), "something", attr);
			
			attr.XmlText = new XmlTextAttribute();
			Serialize(simple, overrides);
			AssertEquals("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>hello</SimpleClass>", WriterText);
			
			attr.XmlText = new XmlTextAttribute(typeof(string));
			Serialize(simple, overrides);
			Assert(WriterText.EndsWith(">hello</SimpleClass>"));
			
			try
			{
				attr.XmlText = new XmlTextAttribute(typeof(byte[]));
				Serialize(simple, overrides);
				Fail("XmlText.Type does not match the type it serializes: this should have failed");
			}
			catch (Exception)
			{
			}
			
			try
			{
				attr.XmlText = new XmlTextAttribute();
				attr.XmlText.DataType = "sometype";
				Serialize(simple, overrides);
				Fail("XmlText.DataType does not match the type it serializes: this should have failed");
			}
			catch (Exception)
			{
			}
		}
		
		// test xmlRoot //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlRootAttribute()
		{
			// constructor override & element name
			XmlRootAttribute root = new XmlRootAttribute();
			root.ElementName = "renamed";
			
			SimpleClassWithXmlAttributes simpleWithAttributes = new SimpleClassWithXmlAttributes();
			Serialize(simpleWithAttributes, root);
			Assert(WriterText.StartsWith("<renamed "));
			
			SimpleClass simple = null;
			root.IsNullable = false;
			try
			{
				Serialize(simple, root);
				Fail("Cannot serialize null object if XmlRoot's IsNullable == false");
			}
			catch (Exception)
			{
			}
			
			root.IsNullable = true;
			try
			{
				Serialize(simple, root);
				Fail("Cannot serialize null object if XmlRoot's IsNullable == true");
			}
			catch (Exception)
			{
			}
			
			simple = new SimpleClass();
			root.ElementName = null;
			root.Namespace = "some.urn";
			Serialize(simple, root);
			AssertEquals("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='some.urn' />", WriterText);
		}
		
		[Test]
		public void TestSerializeXmlRootAttributeOnMember()
		{			
			// nested root
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes childAttr = new XmlAttributes();
			childAttr.XmlRoot = new XmlRootAttribute("simple");
			overrides.Add(typeof(SimpleClass), childAttr);
			
			XmlAttributes attr = new XmlAttributes();
			attr.XmlRoot = new XmlRootAttribute("simple");
			overrides.Add(typeof(ClassArrayContainer), attr);
			
			ClassArrayContainer container = new ClassArrayContainer();
			container.items = new SimpleClass[1];
			container.items[0] = new SimpleClass();;
			Serialize(container, overrides);
			Assert(WriterText.EndsWith("><items><SimpleClass /></items></simple>"));
			
			// FIXME test data type
		}
		
		// test XmlAttribute /////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlAttributeAttribute()
		{	
			// null
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes attr = new XmlAttributes();
			attr.XmlAttribute = new XmlAttributeAttribute();
			overrides.Add(typeof(SimpleClass), "something", attr);
			
			SimpleClass simple = new SimpleClass();;
			Serialize(simple, overrides);
			AssertEquals("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />", WriterText);
			
			// regular
			simple.something = "hello";
			Serialize(simple, overrides);
			AssertEquals ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' something='hello' />", WriterText);
			
			// AttributeName
			attr.XmlAttribute.AttributeName = "somethingelse";
			Serialize(simple, overrides);
			AssertEquals ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' somethingelse='hello' />", WriterText);
			
			// Type
			// FIXME this should work, shouldnt it?
			// attr.XmlAttribute.Type = typeof(string);
			// Serialize(simple, overrides);
			// Assert(WriterText.EndsWith(" something='hello' />"));
			
			// Namespace
			attr.XmlAttribute.Namespace = "some:urn";
			Serialize(simple, overrides);
			Assert(WriterText.EndsWith(" d1p1:somethingelse='hello' xmlns:d1p1='some:urn' />"));
			
			// FIXME DataType
			// FIXME XmlSchemaForm Form
			
			// FIXME write XmlQualifiedName as attribute
		}
		
		// test XmlElement ///////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlElementAttribute()
		{
			
			
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes attr = new XmlAttributes();
			XmlElementAttribute element = new XmlElementAttribute();
			attr.XmlElements.Add(element);
			overrides.Add(typeof(SimpleClass), "something", attr);
			
			// null
			SimpleClass simple = new SimpleClass();;
			Serialize(simple, overrides);
			AssertEquals("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />", WriterText);
			
			// not null
			simple.something = "hello";
			Serialize(simple, overrides);
			AssertEquals ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>", WriterText);
			
			//ElementName
			element.ElementName = "saying";
			Serialize(simple, overrides);
			Assert(WriterText.EndsWith("><saying>hello</saying></SimpleClass>"));
			
			//IsNullable
			element.IsNullable = false;
			simple.something = null;
			Serialize(simple, overrides);
			AssertEquals(WriterText, "<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />");
			
			element.IsNullable = true;
			simple.something = null;
			Serialize(simple, overrides);
			Assert(WriterText.EndsWith("><saying xsi:nil='true' /></SimpleClass>"));
			
			//Namespace
			element.ElementName = null;
			element.IsNullable = false;
			element.Namespace = "some:urn";
			simple.something = "hello";
			Serialize(simple, overrides);
			Assert(WriterText.EndsWith("><something xmlns='some:urn'>hello</something></SimpleClass>"));
			
			//FIXME DataType
			//FIXME Form
			//FIXME Type
		}
		
		// test XmlElementAttribute with arrays and collections //////////////////
		[Test]
		public void TestSerializeCollectionWithXmlElementAttribute()
		{
			// the rule is:
			// if no type is specified or the specified type 
			//    matches the contents of the collection, 
			//    serialize each element in an element named after the member.
			// if the type does not match, or matches the collection itself,
			//    create a base wrapping element for the member, and then
			//    wrap each collection item in its own wrapping element based on type.
			
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes attr = new XmlAttributes();
			XmlElementAttribute element = new XmlElementAttribute();
			attr.XmlElements.Add(element);
			overrides.Add(typeof(StringCollectionContainer), "Messages", attr);
			
			// empty collection & no type info in XmlElementAttribute
			StringCollectionContainer container = new StringCollectionContainer();
			Serialize(container, overrides);
			AssertEquals("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />", WriterText);
			
			// non-empty collection & no type info in XmlElementAttribute
			container.Messages.Add("hello");
			Serialize(container, overrides);
			AssertEquals ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages>hello</Messages></StringCollectionContainer>", WriterText);
			
			// non-empty collection & only type info in XmlElementAttribute
			element.Type = typeof(StringCollection);
			Serialize(container, overrides);
			AssertEquals ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages><string>hello</string></Messages></StringCollectionContainer>", WriterText);
			
			// non-empty collection & only type info in XmlElementAttribute
			element.Type = typeof(string);
			Serialize(container, overrides);
			Assert(WriterText.EndsWith("><Messages>hello</Messages></StringCollectionContainer>"));
			
			// two elements
			container.Messages.Add("goodbye");
			element.Type = null;
			Serialize(container, overrides);
			Assert(WriterText.EndsWith("><Messages>hello</Messages><Messages>goodbye</Messages></StringCollectionContainer>"));
		}
		
		// test DefaultValue /////////////////////////////////////////////////////
		[Test]
		public void TestSerializeDefaultValueAttribute()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			
			XmlAttributes attr = new XmlAttributes();
			string defaultValueInstance = "nothing";
			attr.XmlDefaultValue = defaultValueInstance;
			overrides.Add(typeof(SimpleClass), "something", attr);
			
			// use the default
			SimpleClass simple = new SimpleClass();
			Serialize(simple, overrides);
			AssertEquals("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />", WriterText);
			
			// same value as default
			simple.something = defaultValueInstance;
			Serialize(simple, overrides);
			AssertEquals("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />", WriterText);
			
			// some other value
			simple.something = "hello";
			Serialize(simple, overrides);
			AssertEquals("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>", WriterText);
		}
		
		// test XmlEnum //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlEnumAttribute()
		{
			// technically this has an XmlIgnore attribute, 
			// but it is not being serialized as a member.
			Serialize(XmlSchemaForm.None);
			AssertEquals(WriterText, "<XmlSchemaForm>0</XmlSchemaForm>");
			
			Serialize(XmlSchemaForm.Qualified);
			AssertEquals(WriterText, "<XmlSchemaForm>qualified</XmlSchemaForm>");
			
			Serialize(XmlSchemaForm.Unqualified);
			AssertEquals(WriterText, "<XmlSchemaForm>unqualified</XmlSchemaForm>");
		}
	}
}
