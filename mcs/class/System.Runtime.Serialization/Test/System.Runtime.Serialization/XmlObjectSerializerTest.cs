//
// XmlObjectSerializerTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <JAnkit@novell.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com

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

//
// This test code contains tests for both DataContractSerializer and
// NetDataContractSerializer. The code could be mostly common.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NUnit.Framework;

[assembly: ContractNamespace ("http://www.u2u.be/samples/wcf/2009", ClrNamespace = "U2U.DataContracts")] // bug #599889

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class DataContractSerializerTest
	{
		static readonly XmlWriterSettings settings;

		static DataContractSerializerTest ()
		{
			settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
		}

		[DataContract]
		class Sample1
		{
			[DataMember]
			public string Member1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorTypeNull ()
		{
			new DataContractSerializer (null);
		}

		[Test]
		public void ConstructorKnownTypesNull ()
		{
			// null knownTypes is allowed. Though the property is filled.
			Assert.IsNotNull (new DataContractSerializer (typeof (Sample1), null).KnownTypes, "#1");
			Assert.IsNotNull (new DataContractSerializer (typeof (Sample1), "Foo", String.Empty, null).KnownTypes, "#2");
			Assert.IsNotNull (new DataContractSerializer (typeof (Sample1), new XmlDictionary ().Add ("Foo"), XmlDictionaryString.Empty, null).KnownTypes, "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNameNull ()
		{
			new DataContractSerializer (typeof (Sample1), null, String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNamespaceNull ()
		{
			new DataContractSerializer (typeof (Sample1), "foo", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorNegativeMaxObjects ()
		{
			new DataContractSerializer (typeof (Sample1), null,
				-1, false, false, null);
		}

		[Test]
		public void ConstructorMisc ()
		{
			new DataContractSerializer (typeof (GlobalSample1));
		}

		[Test]
		public void WriteObjectContent ()
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter xw = XmlWriter.Create (sw, settings)) {
				DataContractSerializer ser =
					new DataContractSerializer (typeof (string));
				xw.WriteStartElement ("my-element");
				ser.WriteObjectContent (xw, "TEST STRING");
				xw.WriteEndElement ();
			}
			Assert.AreEqual ("<my-element>TEST STRING</my-element>",
				sw.ToString ());
		}

		[Test]
		public void WriteObjectToStream ()
		{
			DataContractSerializer ser =
				new DataContractSerializer (typeof (int));
			MemoryStream sw = new MemoryStream ();
			ser.WriteObject (sw, 1);
			string expected = "<int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">1</int>";
			byte[] buf = sw.ToArray ();
			Assert.AreEqual (expected, Encoding.UTF8.GetString (buf, 0, buf.Length));
		}

		[Test]
		public void ReadObjectFromStream ()
		{
			DataContractSerializer ser =
				new DataContractSerializer (typeof (int));
			string expected = "<int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">1</int>";
			byte[] buf = Encoding.UTF8.GetBytes (expected);
			MemoryStream sw = new MemoryStream (buf);
			object res = ser.ReadObject (sw);
			Assert.AreEqual (1, res);
		}

		// int

		[Test]
		public void SerializeInt ()
		{
			DataContractSerializer ser =
				new DataContractSerializer (typeof (int));
			SerializeInt (ser, "<int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">1</int>");
		}


		[Test]
		[Category ("NotWorking")]
		public void NetSerializeInt ()
		{
			NetDataContractSerializer ser =
				new NetDataContractSerializer ();
			// z:Assembly="0" ???
			SerializeInt (ser, String.Format ("<int z:Type=\"System.Int32\" z:Assembly=\"0\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\" xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">1</int>", typeof (int).Assembly.FullName));
		}

		void SerializeInt (XmlObjectSerializer ser, string expected)
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, 1);
			}
			Assert.AreEqual (expected, sw.ToString ());
		}

		// pass typeof(DCEmpty), serialize int

		[Test]
		public void SerializeIntForDCEmpty ()
		{
			DataContractSerializer ser =
				new DataContractSerializer (typeof (DCEmpty));
			// tricky!
			SerializeIntForDCEmpty (ser, "<DCEmpty xmlns:d1p1=\"http://www.w3.org/2001/XMLSchema\" i:type=\"d1p1:int\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization\">1</DCEmpty>");
		}

		void SerializeIntForDCEmpty (XmlObjectSerializer ser, string expected)
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, 1);
			}
			XmlComparer.AssertAreEqual (expected, sw.ToString ());
		}

		// DCEmpty

		[Test]
		public void SerializeEmptyClass ()
		{
			DataContractSerializer ser =
				new DataContractSerializer (typeof (DCEmpty));
			SerializeEmptyClass (ser, "<DCEmpty xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization\" />");
		}

		[Test]
		[Category ("NotWorking")]
		public void NetSerializeEmptyClass ()
		{
			NetDataContractSerializer ser =
				new NetDataContractSerializer ();
			SerializeEmptyClass (ser, String.Format ("<DCEmpty xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" z:Id=\"1\" z:Type=\"MonoTests.System.Runtime.Serialization.DCEmpty\" z:Assembly=\"{0}\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\" xmlns=\"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization\" />", this.GetType ().Assembly.FullName));
		}

		void SerializeEmptyClass (XmlObjectSerializer ser, string expected)
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new DCEmpty ());
			}
			Assert.AreEqual (expected, sw.ToString ());
		}

		// DCEmpty

		[Test]
		public void SerializeEmptyNoNSClass ()
		{
			var ser = new DataContractSerializer (typeof (DCEmptyNoNS));
			SerializeEmptyNoNSClass (ser, "<DCEmptyNoNS xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" />");
		}

		void SerializeEmptyNoNSClass (XmlObjectSerializer ser, string expected)
		{
			var sw = new StringWriter ();
			using (var w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new DCEmptyNoNS ());
			}
			Assert.AreEqual (expected, sw.ToString ());
		}
		// string (primitive)

		[Test]
		public void SerializePrimitiveString ()
		{
			XmlObjectSerializer ser =
				new DataContractSerializer (typeof (string));
			SerializePrimitiveString (ser, "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">TEST</string>");
		}

		[Test]
		[Category ("NotWorking")]
		public void NetSerializePrimitiveString ()
		{
			XmlObjectSerializer ser = new NetDataContractSerializer ();
			SerializePrimitiveString (ser, "<string z:Type=\"System.String\" z:Assembly=\"0\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\" xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">TEST</string>");
		}

		void SerializePrimitiveString (XmlObjectSerializer ser, string expected)
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, "TEST");
			}
			Assert.AreEqual (expected, sw.ToString ());
		}

		// QName (primitive but ...)

		[Test]
		[Ignore ("These tests would not make any sense right now since it's populated prefix is not testable.")]
		public void SerializePrimitiveQName ()
		{
			XmlObjectSerializer ser =
				new DataContractSerializer (typeof (XmlQualifiedName));
			SerializePrimitiveQName (ser, "<z:QName xmlns:d7=\"urn:foo\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\">d7:foo</z:QName>");
		}

		[Test]
		[Ignore ("These tests would not make any sense right now since it's populated prefix is not testable.")]
		public void NetSerializePrimitiveQName ()
		{
			XmlObjectSerializer ser = new NetDataContractSerializer ();
			SerializePrimitiveQName (ser, "<z:QName z:Type=\"System.Xml.XmlQualifiedName\" z:Assembly=\"System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" xmlns:d7=\"urn:foo\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\">d7:foo</z:QName>");
		}

		void SerializePrimitiveQName (XmlObjectSerializer ser, string expected)
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new XmlQualifiedName ("foo", "urn:foo"));
			}
			Assert.AreEqual (expected, sw.ToString ());
		}

		// DCSimple1

		[Test]
		public void SerializeSimpleClass1 ()
		{
			DataContractSerializer ser =
				new DataContractSerializer (typeof (DCSimple1));
			SerializeSimpleClass1 (ser, "<DCSimple1 xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization\"><Foo>TEST</Foo></DCSimple1>");
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		[Category ("NotWorking")] // behavior changed in 3.5/SP1
		public void SerializeSimpleXml ()
		{
			DataContractSerializer ser =
				new DataContractSerializer (typeof (SimpleXml));
			SerializeSimpleClass1 (ser, @"<simple i:type=""d1p1:DCSimple1"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><d1p1:Foo>TEST</d1p1:Foo></simple>");
		}

		[Test]
		[Category ("NotWorking")]
		public void NetSerializeSimpleClass1 ()
		{
			NetDataContractSerializer ser =
				new NetDataContractSerializer ();
			SerializeSimpleClass1 (ser, String.Format ("<DCSimple1 xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" z:Id=\"1\" z:Type=\"MonoTests.System.Runtime.Serialization.DCSimple1\" z:Assembly=\"{0}\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\" xmlns=\"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization\"><Foo z:Id=\"2\">TEST</Foo></DCSimple1>", this.GetType ().Assembly.FullName));
		}

		void SerializeSimpleClass1 (XmlObjectSerializer ser, string expected)
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new DCSimple1 ());
			}
			Console.WriteLine(sw.ToString());
			Assert.AreEqual (expected, sw.ToString ());
		}

		// NonDC (behavior changed in 3.5/SP1; not it's not rejected)

		[Test]
		public void SerializeNonDC ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (NonDC));
			var sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new NonDC ());
			}
			Assert.AreEqual ("<NonDC xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization'><Whee>whee!</Whee></NonDC>".Replace ('\'', '"'), sw.ToString ());
		}

		// DCHasNonDC

		[Test]
		public void SerializeDCHasNonDC ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DCHasNonDC));
			var sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new DCHasNonDC ());
			}
			Assert.AreEqual ("<DCHasNonDC xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization'><Hoge><Whee>whee!</Whee></Hoge></DCHasNonDC>".Replace ('\'', '"'), sw.ToString ());
		}

		// DCHasSerializable

		[Test]
		// DCHasSerializable itself is DataContract and has a field
		// whose type is not contract but serializable.
		public void SerializeSimpleSerializable1 ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DCHasSerializable));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new DCHasSerializable ());
			}
			Assert.AreEqual ("<DCHasSerializable xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization\"><Ser><Doh>doh!</Doh></Ser></DCHasSerializable>", sw.ToString ());
		}

		[Test]
		public void SerializeDCWithName ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DCWithName));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new DCWithName ());
			}
			Assert.AreEqual ("<Foo xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization\"><FooMember>value</FooMember></Foo>", sw.ToString ());
		}

		[Test]
		public void SerializeDCWithEmptyName1 ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DCWithEmptyName));
			StringWriter sw = new StringWriter ();
			DCWithEmptyName dc = new DCWithEmptyName ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				try {
					ser.WriteObject (w, dc);
				} catch (InvalidDataContractException) {
					return;
				}
			}
			Assert.Fail ("Expected InvalidDataContractException");
		}

		[Test]
		[Category ("NotWorking")]
		public void SerializeDCWithEmptyName2 ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DCWithName));
			StringWriter sw = new StringWriter ();

			/* DataContractAttribute.Name == "", not valid */
			DCWithEmptyName dc = new DCWithEmptyName ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				try {
					ser.WriteObject (w, dc);
				} catch (InvalidDataContractException) {
					return;
				}
			}
			Assert.Fail ("Expected InvalidDataContractException");
		}

		[Test]
		[Category ("NotWorking")]
		public void SerializeDCWithNullName ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DCWithNullName));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				try {
					/* DataContractAttribute.Name == "", not valid */
					ser.WriteObject (w, new DCWithNullName ());
				} catch (InvalidDataContractException) {
					return;
				}
			}
			Assert.Fail ("Expected InvalidDataContractException");
		}

		[Test]
		public void SerializeDCWithEmptyNamespace1 ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DCWithEmptyNamespace));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new DCWithEmptyNamespace ());
			}
		}

		// Wrapper.DCWrapped

		[Test]
		public void SerializeWrappedClass ()
		{
			DataContractSerializer ser =
				new DataContractSerializer (typeof (Wrapper.DCWrapped));
			SerializeWrappedClass (ser, "<Wrapper.DCWrapped xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization\" />");
		}

		[Test]
		[Category ("NotWorking")]
		public void NetSerializeWrappedClass ()
		{
			NetDataContractSerializer ser =
				new NetDataContractSerializer ();
			SerializeWrappedClass (ser, String.Format ("<Wrapper.DCWrapped xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" z:Id=\"1\" z:Type=\"MonoTests.System.Runtime.Serialization.Wrapper+DCWrapped\" z:Assembly=\"{0}\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\" xmlns=\"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization\" />", this.GetType ().Assembly.FullName));
		}

		void SerializeWrappedClass (XmlObjectSerializer ser, string expected)
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new Wrapper.DCWrapped ());
			}
			Assert.AreEqual (expected, sw.ToString ());
		}

		[Test]
		/* old code
		// CollectionContainer : Items must have a setter.
		[ExpectedException (typeof (InvalidDataContractException))]
		[Category ("NotWorking")]
		*/
		public void SerializeReadOnlyCollectionMember ()
		{
			DataContractSerializer ser =
				new DataContractSerializer (typeof (CollectionContainer));

			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, null);
			}
			Assert.AreEqual ("<CollectionContainer i:nil='true' xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization' />".Replace ('\'', '"'), sw.ToString (), "#1");

			sw = new StringWriter ();
			var c = new CollectionContainer ();
			c.Items.Add ("foo");
			c.Items.Add ("bar");
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, c);
			}
			Assert.AreEqual ("<CollectionContainer xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization'><Items xmlns:d2p1='http://schemas.microsoft.com/2003/10/Serialization/Arrays'><d2p1:string>foo</d2p1:string><d2p1:string>bar</d2p1:string></Items></CollectionContainer>".Replace ('\'', '"'), sw.ToString (), "#2");
		}

		// DataCollectionContainer : Items must have a setter.
		[Test]
		//[ExpectedException (typeof (InvalidDataContractException))]
		public void SerializeReadOnlyDataCollectionMember ()
		{
			DataContractSerializer ser =
				new DataContractSerializer (typeof (DataCollectionContainer));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, null);
			}
			Assert.AreEqual ("<DataCollectionContainer i:nil='true' xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization' />".Replace ('\'', '"'), sw.ToString (), "#1");

			sw = new StringWriter ();
			var c = new DataCollectionContainer ();
			c.Items.Add ("foo");
			c.Items.Add ("bar");
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, c);
			}
			// LAMESPEC: this is bogus behavior. .NET serializes 
			// System.String as "string" without overriding its 
			// element namespace, but then it must be regarded as
			// in parent's namespace. What if there already is an
			// element definition for "string" with the same
			// namespace?
			Assert.AreEqual ("<DataCollectionContainer xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization'><Items><string>foo</string><string>bar</string></Items></DataCollectionContainer>".Replace ('\'', '"'), sw.ToString (), "#2");
		}

		[Test]
		public void SerializeGuid ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (Guid));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, Guid.Empty);
			}
			Assert.AreEqual (
				"<guid xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">00000000-0000-0000-0000-000000000000</guid>",
				sw.ToString ());
		}

		[Test]
		public void SerializeEnum ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (Colors));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new Colors ());
			}

			Assert.AreEqual (
				@"<Colors xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">Red</Colors>",
				sw.ToString ());
		}

		[Test]
		public void SerializeEnum2 ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (Colors));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, 0);
			}

			XmlComparer.AssertAreEqual (
				@"<Colors xmlns:d1p1=""http://www.w3.org/2001/XMLSchema"" i:type=""d1p1:int"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">0</Colors>",
				sw.ToString ());
		}

		[Test]
		public void SerializeEnumWithDC ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (ColorsWithDC));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new ColorsWithDC ());
			}

			Assert.AreEqual (
				@"<_ColorsWithDC xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">_Red</_ColorsWithDC>",
				sw.ToString ());
		}

		[Test]
		public void SerializeEnumWithNoDC ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (ColorsEnumMemberNoDC));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new ColorsEnumMemberNoDC ());
			}

			Assert.AreEqual (
				@"<ColorsEnumMemberNoDC xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">Red</ColorsEnumMemberNoDC>",
				sw.ToString ());
		}

		[Test]
		public void SerializeEnumWithDC2 ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (ColorsWithDC));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, 3);
			}

			XmlComparer.AssertAreEqual (
				@"<_ColorsWithDC xmlns:d1p1=""http://www.w3.org/2001/XMLSchema"" i:type=""d1p1:int"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">3</_ColorsWithDC>",
				sw.ToString ());
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void SerializeEnumWithDCInvalid ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (ColorsWithDC));
			StringWriter sw = new StringWriter ();
			ColorsWithDC cdc = ColorsWithDC.Blue;
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, cdc);
			}
		}

		[Test]
		public void SerializeDCWithEnum ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DCWithEnum));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new DCWithEnum ());
			}
 
			Assert.AreEqual (
				@"<DCWithEnum xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><_colors>Red</_colors></DCWithEnum>",
				sw.ToString ());
		}

		[Test]
		public void SerializeDCWithTwoEnums ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DCWithTwoEnums));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				DCWithTwoEnums e = new DCWithTwoEnums ();
				e.colors = Colors.Blue;
				e.colors2 = Colors.Green;
				ser.WriteObject (w, e);
			}
 
			Assert.AreEqual (
				@"<DCWithTwoEnums xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><colors>Blue</colors><colors2>Green</colors2></DCWithTwoEnums>",
				sw.ToString ());
		}

		[Test]
		public void SerializeNestingDC2 ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (NestingDC2));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				NestingDC2 e = new NestingDC2 ();
				e.Field = new NestedDC2 ("Something");
				ser.WriteObject (w, e);
			}
 
			Assert.AreEqual (
				@"<NestingDC2 xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""test2""><Field xmlns:d2p1=""test1""><d2p1:Name>Something</d2p1:Name></Field></NestingDC2>",
				sw.ToString ());
		}

		[Test]
		public void SerializeNestingDC ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (NestingDC));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				NestingDC e = new NestingDC ();
				e.Field1 = new NestedDC ("test1");
				e.Field2 = new NestedDC ("test2");
				ser.WriteObject (w, e);
			}
 
			Assert.AreEqual (
				@"<NestingDC xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><Field1><Name>test1</Name></Field1><Field2><Name>test2</Name></Field2></NestingDC>",
				sw.ToString ());
			sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				NestingDC e = new NestingDC ();
				ser.WriteObject (w, e);
			}
 
			Assert.AreEqual (
				@"<NestingDC xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><Field1 i:nil=""true"" /><Field2 i:nil=""true"" /></NestingDC>",
				sw.ToString ());
		}

		[Test]
		public void SerializeDerivedDC ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DerivedDC));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				DerivedDC e = new DerivedDC ();
				ser.WriteObject (w, e);
			}
 
			Assert.AreEqual (
				@"<DerivedDC xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""Derived""><baseVal xmlns=""Base"">0</baseVal><derivedVal>0</derivedVal></DerivedDC>",
				sw.ToString ());
		}

		[Test]
		public void SerializerDCArray ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (DCWithEnum []));
			StringWriter sw = new StringWriter ();
			DCWithEnum [] arr = new DCWithEnum [2];
			arr [0] = new DCWithEnum (); arr [0].colors = Colors.Red;
			arr [1] = new DCWithEnum (); arr [1].colors = Colors.Green;
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, arr);
			}

			XmlComparer.AssertAreEqual (
				@"<ArrayOfDCWithEnum xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><DCWithEnum><_colors>Red</_colors></DCWithEnum><DCWithEnum><_colors>Green</_colors></DCWithEnum></ArrayOfDCWithEnum>",
				sw.ToString ());
		}

		[Test]
		public void SerializerDCArray2 ()
		{
			List<Type> known = new List<Type> ();
			known.Add (typeof (DCWithEnum));
			known.Add (typeof (DCSimple1));
			DataContractSerializer ser = new DataContractSerializer (typeof (object []), known);
			StringWriter sw = new StringWriter ();
			object [] arr = new object [2];
			arr [0] = new DCWithEnum (); ((DCWithEnum)arr [0]).colors = Colors.Red;
			arr [1] = new DCSimple1 (); ((DCSimple1) arr [1]).Foo = "hello";

			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, arr);
			}

			XmlComparer.AssertAreEqual (
				@"<ArrayOfanyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><anyType xmlns:d2p1=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"" i:type=""d2p1:DCWithEnum""><d2p1:_colors>Red</d2p1:_colors></anyType><anyType xmlns:d2p1=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"" i:type=""d2p1:DCSimple1""><d2p1:Foo>hello</d2p1:Foo></anyType></ArrayOfanyType>",
				sw.ToString ());
		}

		[Test]
		public void SerializerDCArray3 ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (int []));
			StringWriter sw = new StringWriter ();
			int [] arr = new int [2];
			arr [0] = 1; arr [1] = 2;

			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, arr);
			}

			XmlComparer.AssertAreEqual (
				@"<ArrayOfint xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><int>1</int><int>2</int></ArrayOfint>",
				sw.ToString ());
		}

		[Test]
		public void SerializeNonDCArray ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (SerializeNonDCArrayType));
			StringWriter sw = new StringWriter ();
			using (XmlWriter xw = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (xw, new SerializeNonDCArrayType ());
			}
			Assert.AreEqual (@"<SerializeNonDCArrayType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><IPAddresses /></SerializeNonDCArrayType>",
				sw.ToString ());
		}

		[Test]
		public void SerializeNonDCArrayItems ()
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (SerializeNonDCArrayType));
			StringWriter sw = new StringWriter ();
			using (XmlWriter xw = XmlWriter.Create (sw, settings)) {
				SerializeNonDCArrayType obj = new SerializeNonDCArrayType ();
				obj.IPAddresses = new NonDCItem [] {new NonDCItem () { Data = new int [] {1, 2, 3, 4} } };
				ser.WriteObject (xw, obj);
			}

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (sw.ToString ());
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			nsmgr.AddNamespace ("s", "http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization");
			nsmgr.AddNamespace ("n", "http://schemas.datacontract.org/2004/07/System.Net");
			nsmgr.AddNamespace ("a", "http://schemas.microsoft.com/2003/10/Serialization/Arrays");

			Assert.AreEqual (1, doc.SelectNodes ("/s:SerializeNonDCArrayType/s:IPAddresses/s:NonDCItem", nsmgr).Count, "#1");
			XmlElement el = doc.SelectSingleNode ("/s:SerializeNonDCArrayType/s:IPAddresses/s:NonDCItem/s:Data", nsmgr) as XmlElement;
			Assert.IsNotNull (el, "#3");
			Assert.AreEqual (4, el.SelectNodes ("a:int", nsmgr).Count, "#4");
		}

		[Test]
		public void DeserializeEnum ()
		{
			Colors c = Deserialize<Colors> (
				@"<Colors xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">Red</Colors>");

			Assert.AreEqual (Colors.Red, c, "#de2");
		}

		[Test]
		public void DeserializeEnum2 ()
		{
			Colors c = Deserialize<Colors> (
				@"<Colors xmlns:d1p1=""http://www.w3.org/2001/XMLSchema"" i:type=""d1p1:int"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">1</Colors>",
				typeof (int));

			Assert.AreEqual (Colors.Green, c, "#de4");
		}
		
		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEnumInvalid1 ()
		{
			Deserialize<Colors> (
				@"<Colors xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""></Colors>");
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEnumInvalid2 ()
		{
			Deserialize<Colors> (
				@"<Colors xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""/>");
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEnumInvalid3 ()
		{
			//"red" instead of "Red"
			Deserialize<Colors> (
				@"<Colors xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">red</Colors>");
		}

		[Test]
		public void DeserializeEnumFlags ()
		{
			Deserialize<Colors2> (
				@"<Colors2 xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""/>");
		}

		[Test]
		public void DeserializeEnumWithDC ()
		{
			ColorsWithDC cdc = Deserialize<ColorsWithDC> (
				@"<_ColorsWithDC xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">_Red</_ColorsWithDC>");
			
			Assert.AreEqual (ColorsWithDC.Red, cdc, "#de6");
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEnumWithDCInvalid ()
		{
			Deserialize<ColorsWithDC> (
				@"<_ColorsWithDC xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">NonExistant</_ColorsWithDC>");
		}

		[Test]
		public void DeserializeDCWithEnum ()
		{
			DCWithEnum dc = Deserialize<DCWithEnum> (
				@"<DCWithEnum xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><_colors>Red</_colors></DCWithEnum>");

			Assert.AreEqual (Colors.Red, dc.colors, "#de8");
		}

		[Test]
		public void DeserializeNestingDC ()
		{
			NestingDC dc = Deserialize<NestingDC> (
				@"<NestingDC xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><Field1><Name>test1</Name></Field1><Field2><Name>test2</Name></Field2></NestingDC>");

			Assert.IsNotNull (dc.Field1, "#N1: Field1 should not be null.");
			Assert.IsNotNull (dc.Field2, "#N2: Field2 should not be null.");
			Assert.AreEqual ("test1", dc.Field1.Name, "#1");
			Assert.AreEqual ("test2", dc.Field2.Name, "#2");
		}

		[Test]
		public void DeserializeNestingDC2 ()
		{
			NestingDC2 dc = Deserialize<NestingDC2> (
				@"<NestingDC2 xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""test2""><Field xmlns:d2p1=""test1""><d2p1:Name>Something</d2p1:Name></Field></NestingDC2>");

			Assert.IsNotNull (dc.Field, "#N1: Field should not be null.");
			Assert.AreEqual ("Something", dc.Field.Name, "#N2");
		}

		[Test]
		public void DeserializeDerivedDC ()
		{
			DerivedDC dc = Deserialize<DerivedDC> (
				@"<DerivedDC xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""Derived""><baseVal xmlns=""Base"">1</baseVal><derivedVal>2</derivedVal></DerivedDC>");

			Assert.AreEqual (1, dc.baseVal, "#N1");
			Assert.AreEqual (2, dc.derivedVal, "#N2");
		}

		[Test]
		public void DeserializeTwice ()
		{
			string xml = 
				@"<any><_ColorsWithDC xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">_Red</_ColorsWithDC> <_ColorsWithDC xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">_Red</_ColorsWithDC></any>";
			DataContractSerializer ser = new DataContractSerializer (typeof (ColorsWithDC));
			XmlReader xr = XmlReader.Create (new StringReader (xml), new XmlReaderSettings ());
			xr.ReadStartElement ();
			object o = ser.ReadObject (xr);
			Assert.AreEqual (typeof (ColorsWithDC), o.GetType (), "#de5");
			ColorsWithDC cdc = (ColorsWithDC) o;
			Assert.AreEqual (ColorsWithDC.Red, o, "#de6");

			o = ser.ReadObject (xr);
			Assert.AreEqual (typeof (ColorsWithDC), o.GetType (), "#de5");
			cdc = (ColorsWithDC) o;
			Assert.AreEqual (ColorsWithDC.Red, o, "#de6");
			Assert.AreEqual (XmlNodeType.EndElement, xr.NodeType, "#de6");
			Assert.AreEqual ("any", xr.LocalName, "#de6");
			xr.ReadEndElement ();
		}


		[Test]
		public void DeserializeEmptyNestingDC ()
		{
			NestingDC dc = Deserialize<NestingDC> (
				@"<NestingDC xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""></NestingDC>");

			Assert.IsNotNull (dc, "#A0: The object should not be null.");
			Assert.IsNull (dc.Field1, "#A1: Field1 should be null.");
			Assert.IsNull (dc.Field2, "#A2: Field2 should be null.");

			dc = Deserialize<NestingDC> (
				@"<NestingDC xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""/>");

			Assert.IsNotNull (dc, "#B0: The object should not be null.");
			Assert.IsNull (dc.Field1, "#B1: Field1 should be null.");
			Assert.IsNull (dc.Field2, "#B2: Field2 should be null.");

			dc = Deserialize<NestingDC> (
				@"<NestingDC xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><Field1 i:nil=""true"" /><Field2 i:nil=""true"" /></NestingDC>");

			Assert.IsNotNull (dc, "#B0: The object should not be null.");
			Assert.IsNull (dc.Field1, "#B1: Field1 should be null.");
			Assert.IsNull (dc.Field2, "#B2: Field2 should be null.");
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEmptyDCWithTwoEnums ()
		{
			Deserialize<DCWithTwoEnums> (
				@"<DCWithTwoEnums xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><colors i:nil=""true""/><colors2 i:nil=""true""/></DCWithTwoEnums>");
		}

		[Test]
		[Category ("NotWorking")]
		public void DeserializeDCWithNullableEnum ()
		{
			DCWithNullableEnum dc = Deserialize<DCWithNullableEnum> (
				@"<DCWithNullableEnum xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><colors i:nil=""true""/></DCWithNullableEnum>");

			Assert.IsNull (dc.colors, "#B1: Field should be null.");
		}

		[Test]
		public void DeserializeDCWithTwoEnums ()
		{
			DCWithTwoEnums dc = Deserialize<DCWithTwoEnums> (
				@"<DCWithTwoEnums xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><colors>Blue</colors><colors2>Green</colors2></DCWithTwoEnums>");

			Assert.AreEqual (Colors.Blue, dc.colors, "#0");
			Assert.AreEqual (Colors.Green, dc.colors2, "#1");
		}

		[Test]
		public void DeserializerDCArray ()
		{
			DCWithEnum [] dcArray = Deserialize<DCWithEnum []> (
				@"<ArrayOfDCWithEnum xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><DCWithEnum><_colors>Red</_colors></DCWithEnum><DCWithEnum><_colors>Green</_colors></DCWithEnum></ArrayOfDCWithEnum>");

			Assert.AreEqual (2, dcArray.Length, "#N1");
			Assert.AreEqual (Colors.Red, dcArray [0].colors, "#N2");
			Assert.AreEqual (Colors.Green, dcArray [1].colors, "#N3");
		}

		[Test]
		public void DeserializerDCArray2 ()
		{
			string xml = 
				@"<ArrayOfanyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><anyType xmlns:d2p1=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"" i:type=""d2p1:DCWithEnum""><d2p1:_colors>Red</d2p1:_colors></anyType><anyType xmlns:d2p1=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"" i:type=""d2p1:DCSimple1""><d2p1:Foo>hello</d2p1:Foo></anyType></ArrayOfanyType>";

			List<Type> known = new List<Type> ();
			known.Add (typeof (DCWithEnum));
			known.Add (typeof (DCSimple1));
			DataContractSerializer ser = new DataContractSerializer (typeof (object []), known);
			XmlReader xr = XmlReader.Create (new StringReader (xml));

			object [] dc = (object []) ser.ReadObject (xr);
			Assert.AreEqual (2, dc.Length, "#N1");
			Assert.AreEqual (typeof (DCWithEnum), dc [0].GetType (), "#N2");
			DCWithEnum dc0 = (DCWithEnum) dc [0];
			Assert.AreEqual (Colors.Red, dc0.colors, "#N3");
			Assert.AreEqual (typeof (DCSimple1), dc [1].GetType (), "#N4");
			DCSimple1 dc1 = (DCSimple1) dc [1];
			Assert.AreEqual ("hello", dc1.Foo, "#N4");
		}

		[Test]
		public void DeserializerDCArray3 ()
		{
			int [] intArray = Deserialize<int []> (
				@"<ArrayOfint xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><int>1</int><int>2</int></ArrayOfint>");

			Assert.AreEqual (2, intArray.Length, "#N0");
			Assert.AreEqual (1, intArray [0], "#N1");
			Assert.AreEqual (2, intArray [1], "#N2");
		}

		[Test]
		public void ReadObjectNoVerifyObjectName ()
		{
			string xml = @"<any><Member1 xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization1"">bar1</Member1><Member1 xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization2"">bar2</Member1><Member1 xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">bar</Member1></any>";
			VerifyObjectNameTestData res = (VerifyObjectNameTestData)new DataContractSerializer (typeof (VerifyObjectNameTestData))
				.ReadObject (XmlReader.Create (new StringReader (xml)), false);
			Assert.AreEqual ("bar", res.GetMember());
		}

		[Test]
		public void ReadObjectVerifyObjectName ()
		{
			string xml = @"<VerifyObjectNameTestData xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><Member1>bar</Member1></VerifyObjectNameTestData>";
			VerifyObjectNameTestData res = (VerifyObjectNameTestData)new DataContractSerializer (typeof (VerifyObjectNameTestData))
				.ReadObject (XmlReader.Create (new StringReader (xml)));
			Assert.AreEqual ("bar", res.GetMember());
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void ReadObjectWrongNamespace ()
		{
			string xml = @"<VerifyObjectNameTestData xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization2""><Member1>bar</Member1></VerifyObjectNameTestData>";
			new DataContractSerializer (typeof (VerifyObjectNameTestData))
				.ReadObject (XmlReader.Create (new StringReader (xml)));
		}

		[Test]
		public void ReferenceSerialization ()
		{
			var dc = new DataContractSerializer (typeof (ReferenceWrapper));
			var t = new ReferenceType ();
			StringWriter sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw)) {
				xw.WriteStartElement ("z", "root", "http://schemas.microsoft.com/2003/10/Serialization/");
				dc.WriteObject (xw, new ReferenceWrapper () {T = t, T2 = t});
				xw.WriteEndElement ();
			}
			string xml = @"<?xml version='1.0' encoding='utf-16'?><z:root xmlns:z='http://schemas.microsoft.com/2003/10/Serialization/'><ReferenceWrapper xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization'><T z:Id='i1'><F>x</F></T><T2 z:Ref='i1' /></ReferenceWrapper></z:root>";
			Assert.AreEqual (xml.Replace ('\'', '"'), sw.ToString (), "#1");

			ReferenceWrapper w;
			using (XmlReader r = XmlReader.Create (new StringReader (xml)))
	{
				r.ReadStartElement ();
				w = (ReferenceWrapper) dc.ReadObject (r);
				r.ReadEndElement ();
			}
			Assert.AreEqual (w.T, w.T2, "#2");
		}

		[Test]
		public void GenericSerialization ()
		{
			var sw = new StringWriter ();
			var ser  = new DataContractSerializer (typeof (Foo<string,int,int>));
			using (var xw = XmlWriter.Create (sw))
				ser.WriteObject (xw, new Foo<string,int,int> () {Field = "f"
			});
			var s = sw.ToString ();

			var ret = (Foo<string,int,int>) ser.ReadObject (XmlReader.Create (new StringReader (s)));
			Assert.AreEqual ("f", ret.Field);
		}

		[Test]
		public void GenericCollectionSerialization ()
		{
			var l = new MyList ();
			l.Add ("foo");
			l.Add ("bar");
			var ds = new DataContractSerializer (typeof (MyList));
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw))
				ds.WriteObject (xw, l);
			l = (MyList) ds.ReadObject (XmlReader.Create (new StringReader (sw.ToString ())));
			Assert.AreEqual (2, l.Count);
		}

		[Test]
		public void GenericListOfKeyValuePairSerialization ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><ArrayOfKeyValuePairOfstringstring xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.datacontract.org/2004/07/System.Collections.Generic'><KeyValuePairOfstringstring><key>foo</key><value>bar</value></KeyValuePairOfstringstring></ArrayOfKeyValuePairOfstringstring>".Replace ('\'', '"');

			var ds = new DataContractSerializer (typeof (List<KeyValuePair<string,string>>));
			var d = new List<KeyValuePair<string,string>> ();
			d.Add (new KeyValuePair<string,string> ("foo", "bar"));
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw))
			        ds.WriteObject (xw, d);
			Assert.AreEqual (xml, sw.ToString (), "#1");
			d = (List<KeyValuePair<string,string>>) ds.ReadObject (XmlReader.Create (new StringReader (xml)));
			Assert.AreEqual (1, d.Count, "#2");
			Assert.AreEqual ("bar", d [0].Value, "#3");
		}

		[Test]
		public void GenericListOfDictionaryEntrySerialization ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><ArrayOfDictionaryEntry xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.datacontract.org/2004/07/System.Collections'><DictionaryEntry><_key xmlns:d3p1='http://www.w3.org/2001/XMLSchema' i:type='d3p1:string'>foo</_key><_value xmlns:d3p1='http://www.w3.org/2001/XMLSchema' i:type='d3p1:string'>bar</_value></DictionaryEntry></ArrayOfDictionaryEntry>".Replace ('\'', '"');

			var ds = new DataContractSerializer (typeof (List<DictionaryEntry>));
			var d = new List<DictionaryEntry> ();
			d.Add (new DictionaryEntry ("foo", "bar"));
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw))
				ds.WriteObject (xw, d);
			Assert.AreEqual (xml, sw.ToString (), "#1");
			Assert.IsTrue (sw.ToString ().IndexOf ("i:type") >= 0);
			d = (List<DictionaryEntry>) ds.ReadObject (XmlReader.Create (new StringReader (xml)));
			Assert.AreEqual (1, d.Count, "#2");
			Assert.AreEqual ("bar", d [0].Value, "#3");
		}

		[Test]
		public void GenericDictionarySerialization ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><ArrayOfKeyValueOfstringstring xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.microsoft.com/2003/10/Serialization/Arrays'><KeyValueOfstringstring><Key>foo</Key><Value>bar</Value></KeyValueOfstringstring></ArrayOfKeyValueOfstringstring>".Replace ('\'', '"');

			var ds = new DataContractSerializer (typeof (Dictionary<string,string>));
			var d = new Dictionary<string,string> ();
			d ["foo"] = "bar";
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw))
			        ds.WriteObject (xw, d);
			Assert.AreEqual (xml, sw.ToString (), "#1");
			d = (Dictionary<string,string>) ds.ReadObject (XmlReader.Create (new StringReader (xml)));
			Assert.AreEqual (1, d.Count, "#2");
			Assert.AreEqual ("bar", d ["foo"], "#3");
		}

		[Test]
		public void HashtableSerialization ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><ArrayOfKeyValueOfanyTypeanyType xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.microsoft.com/2003/10/Serialization/Arrays'><KeyValueOfanyTypeanyType><Key xmlns:d3p1='http://www.w3.org/2001/XMLSchema' i:type='d3p1:string'>foo</Key><Value xmlns:d3p1='http://www.w3.org/2001/XMLSchema' i:type='d3p1:string'>bar</Value></KeyValueOfanyTypeanyType></ArrayOfKeyValueOfanyTypeanyType>".Replace ('\'', '"');

			var ds = new DataContractSerializer (typeof (Hashtable));
			var d = new Hashtable ();
			d ["foo"] = "bar";
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw))
			        ds.WriteObject (xw, d);
			Assert.AreEqual (xml, sw.ToString (), "#1");
			d = (Hashtable) ds.ReadObject (XmlReader.Create (new StringReader (xml)));
			Assert.AreEqual (1, d.Count, "#2");
			Assert.AreEqual ("bar", d ["foo"], "#3");
		}

		[Test]
		public void CollectionContarctDictionarySerialization ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><NAME xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:foo'><ITEM><KEY>foo</KEY><VALUE>bar</VALUE></ITEM></NAME>".Replace ('\'', '"');

			var ds = new DataContractSerializer (typeof (MyDictionary<string,string>));
			var d = new MyDictionary<string,string> ();
			d ["foo"] = "bar";
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw))
			        ds.WriteObject (xw, d);
			Assert.AreEqual (xml, sw.ToString (), "#1");
			d = (MyDictionary<string,string>) ds.ReadObject (XmlReader.Create (new StringReader (xml)));
			Assert.AreEqual (1, d.Count, "#2");
			Assert.AreEqual ("bar", d ["foo"], "#3");
		}

		[Test]
		public void SerializeInterfaceCollection ()
		{
			var ser = new DataContractSerializer (typeof (InterfaceCollectionType));
			var sw = new StringWriter ();
			var obj = new InterfaceCollectionType ();
			using (var xw = XmlWriter.Create (sw))
				ser.WriteObject (xw, obj);
			using (var xr = XmlReader.Create (new StringReader (sw.ToString ()))) {
				obj = (InterfaceCollectionType) ser.ReadObject (xr);
				Assert.IsNull (obj.Array, "#1");
			}

			sw = new StringWriter ();
			obj.Array = new List<int> ();
			obj.Array.Add (5);
			using (var xw = XmlWriter.Create (sw))
				ser.WriteObject (xw, obj);
			using (var xr = XmlReader.Create (new StringReader (sw.ToString ()))) {
				obj = (InterfaceCollectionType) ser.ReadObject (xr);
				Assert.AreEqual (5, obj.Array [0], "#2");
			}
		}

		[Test]
		public void EmptyChildren ()
		{
                string xml = @"
<DummyPlaylist xmlns='http://example.com/schemas/asx'>
        <Entries>
                <DummyEntry>
                        <EntryInfo xmlns:i='http://www.w3.org/2001/XMLSchema-instance' i:type='PartDummyEntryInfo'/>
                        <Href>http://vmsservices.example.com:8080/VideoService.svc?crid=45541/part=1/guid=ae968b5d-e4a5-41fe-9b23-ed631b27cd21/</Href>
                </DummyEntry>
        </Entries>
</DummyPlaylist>
";
			var reader = XmlReader.Create (new StringReader (xml));
			DummyPlaylist playlist = (DummyPlaylist) new DataContractSerializer (typeof (DummyPlaylist)).ReadObject (reader);
			Assert.AreEqual (1, playlist.entries.Count, "#1");
			Assert.IsTrue (playlist.entries [0] is DummyEntry, "#2");
			Assert.IsNotNull (playlist.entries [0].Href, "#3");
		}

		[Test]
		public void BaseKnownTypeAttributes ()
		{
			// bug #524088
			string xml = @"
<DummyPlaylist xmlns='http://example.com/schemas/asx'>
  <Entries>
    <DummyEntry>
      <EntryInfo xmlns:i='http://www.w3.org/2001/XMLSchema-instance' i:type='PartDummyEntryInfo'/>
    </DummyEntry>
  </Entries>
</DummyPlaylist>";

			using (XmlReader reader = XmlReader.Create (new StringReader (xml))) {
				DummyPlaylist playlist = new DataContractSerializer(typeof(DummyPlaylist)).ReadObject(reader) as DummyPlaylist;
				Assert.IsNotNull (playlist);
			}
		}

		[Test]
		public void Bug524083 ()
		{
			string xml = @"
<AsxEntryInfo xmlns='http://example.com/schemas/asx'>
	<AdvertPrompt/>
</AsxEntryInfo>";
						
			using (XmlReader reader = XmlReader.Create (new StringReader (xml)))
				new DataContractSerializer(typeof (AsxEntryInfo)).ReadObject (reader);
		}
		
		[Test]
		public void Bug539563 ()
		{
			new DataContractSerializer (typeof (NestedContractType));
		}

		[Test]
		public void Bug560155 ()
		{
			var g = Guid.NewGuid ();
			Person p1 = new Person ("UserName", g);
			Assert.AreEqual ("name=UserName,id=" + g, p1.ToString (), "#1");
			MemoryStream memStream = new MemoryStream ();
			DataContractSerializer ser =  new DataContractSerializer (typeof (Person));

			ser.WriteObject (memStream, p1);
			memStream.Seek (0, SeekOrigin.Begin);
			Person p2 = (Person) ser.ReadObject (memStream);
			Assert.AreEqual ("name=UserName,id=" + g, p2.ToString (), "#1");
		}

		private T Deserialize<T> (string xml)
		{
			return Deserialize<T> (xml, typeof (T));
		}

		private T Deserialize<T> (string xml, Type runtimeType)
		{
			DataContractSerializer ser = new DataContractSerializer (typeof (T));
			XmlReader xr = XmlReader.Create (new StringReader (xml), new XmlReaderSettings ());
			object o = ser.ReadObject (xr);
			Assert.AreEqual (runtimeType, o.GetType (), "#DS0");
			return (T)o;
		}

		public Dictionary<string, object> GenericDictionary (Dictionary<string, object> settings)
		{
			using (MemoryStream ms = new MemoryStream ()) {
				DataContractSerializer save = new DataContractSerializer (settings.GetType ());
				save.WriteObject (ms, settings);

				ms.Position = 0;

				DataContractSerializer load = new DataContractSerializer (typeof (Dictionary<string, object>));
				return (Dictionary<string, object>) load.ReadObject (ms);
			}
		}

		[Test]
		public void GenericDictionaryEmpty ()
		{
			Dictionary<string, object> in_settings = new Dictionary<string, object> ();
			Dictionary<string, object> out_settings = GenericDictionary (in_settings);
			out_settings.Clear ();
		}

		[Test]
		public void GenericDictionaryOneElement ()
		{
			Dictionary<string, object> in_settings = new Dictionary<string, object> ();
			in_settings.Add ("one", "ONE");
			Dictionary<string, object> out_settings = GenericDictionary (in_settings);
			Assert.AreEqual ("ONE", out_settings ["one"], "out");
			out_settings.Clear ();
		}

		[Test]
		public void IgnoreDataMember ()
		{
			var ser = new DataContractSerializer (typeof (MemberIgnored));
			var sw = new StringWriter ();
			using (var w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new MemberIgnored ());
			}
			Assert.AreEqual (@"<MemberIgnored xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization""><body><Bar>bar</Bar></body></MemberIgnored>", sw.ToString (), "#1");
		}

		[Test]
		public void DeserializeEmptyArray ()
		{
			var ds = new DataContractSerializer (typeof (string []));
			var sw = new StringWriter ();
			var xw = XmlWriter.Create (sw);
			ds.WriteObject (xw, new string [] {});
			xw.Close ();
			Console.WriteLine (sw.ToString ());
			var sr = new StringReader (sw.ToString ());
			var xr = XmlReader.Create (sr);
			var ret = ds.ReadObject (xr);
			Assert.AreEqual (typeof (string []), ret.GetType (), "#1");
		}
		
		[Test]
		public void ContractNamespaceAttribute ()
		{
			var ds = new DataContractSerializer (typeof (U2U.DataContracts.Person));
			string xml = "<?xml version='1.0' encoding='utf-16'?><Person xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://www.u2u.be/samples/wcf/2009'><Name>Rupert</Name><Occupation><Description>Monkey</Description></Occupation></Person>";
			var person = new U2U.DataContracts.Person () {
				Name = "Rupert",
				Occupation = new U2U.DataContracts.Job () { Description = "Monkey" }
				};
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw))
				ds.WriteObject (xw, person);
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		public void Bug610036 ()
		{
			var ms = new MemoryStream ();
			Type [] knownTypes = new Type [] { typeof (ParentClass), typeof (Foo), typeof (Bar) };

            		var ds = new DataContractSerializer (typeof (Root), "Root", "Company.Foo", knownTypes, 1000, false, true, null);

			var root = new Root ("root");
			var bar1 = new Bar ("bar1");
			var bar2 = new Bar ("bar2");
			var bar3 = new Bar ("bar3");
			
			var foo1 = new Foo ("foo1");
			var foo2 = new Foo ("foo2");
			
			foo1.FDict.Add (bar1);
			foo1.FDict.Add (bar2);
			
			foo2.FDict.Add (bar1);
			foo2.FDict.Add (bar3);
			
			root.FDict.Add (foo1);
			root.FDict.Add (foo2);

			ds.WriteObject (ms, root);
			string result = Encoding.UTF8.GetString (ms.ToArray ());
			ms.Position = 0;

			root = (Root) ds.ReadObject (ms);

			Assert.AreEqual (2, root.FDict.Count, "#1");
			int idx = result.IndexOf ("foo1");
			Assert.IsTrue (idx >= 0, "#2");
			// since "foo1" is stored as z:Ref for string, it must not occur twice.
			int idx2 = result.IndexOf ("foo1", idx + 1);
			Assert.IsTrue (idx2 < 0, "idx2 should not occur at " + idx2);
		}

		[Test]
		public void AncestralReference ()
		{
			// Reference to Parent comes inside the Parent itself.
			// In this case, adding reference after complete deserialization won't work (but it should).
			var ms = new MemoryStream ();
			Type [] knownTypes = new Type [] { typeof (ParentClass), typeof (Foo), typeof (Bar) };

            		var ds = new DataContractSerializer (typeof (Parent));

			var org = new Parent ();
			ds.WriteObject (ms, org);
			string result = Encoding.UTF8.GetString (ms.ToArray ());
			ms.Position = 0;

			var parent = (Parent) ds.ReadObject (ms);

			Assert.IsNotNull (parent.Child, "#1");
			Assert.AreEqual (parent, parent.Child.Parent, "#2");
		}

		[Test]
		public void IXmlSerializableCallConstructor ()
		{
			IXmlSerializableCallConstructor  (false);
			IXmlSerializableCallConstructor (true);
		}
		
		void IXmlSerializableCallConstructor (bool binary)
		{
			Stream s = IXmlSerializableCallConstructorSerialize (binary);
			var a = new byte [s.Length];
			s.Position = 0;
			s.Read (a, 0, a.Length);
			s.Position = 0;
			IXmlSerializableCallConstructorDeserialize (s, binary);
		}

		public Stream IXmlSerializableCallConstructorSerialize (bool binary)
		{
			var ds = new DataSet ("ds");
			var dt = new DataTable ("dt");
			ds.Tables.Add (dt);
			dt.Columns.Add ("n", typeof (int));
			dt.Columns.Add ("s", typeof (string));
			
			dt.Rows.Add (5, "five");
			dt.Rows.Add (10, "ten");
			
			ds.AcceptChanges ();
			
			var s = new MemoryStream ();
			
			var w = binary ? XmlDictionaryWriter.CreateBinaryWriter (s) : XmlDictionaryWriter.CreateTextWriter (s);
			
			var x = new DataContractSerializer (typeof (DataSet));
			x.WriteObject (w, ds);
			w.Flush ();
	
			return s;
		}
		
		public void IXmlSerializableCallConstructorDeserialize (Stream s, bool binary)
		{
			var r = binary ? XmlDictionaryReader.CreateBinaryReader (s, XmlDictionaryReaderQuotas.Max)
				: XmlDictionaryReader.CreateTextReader (s, XmlDictionaryReaderQuotas.Max);
			
			var x = new DataContractSerializer (typeof (DataSet));
			
			var ds = (DataSet) x.ReadObject (r);
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))] // BaseConstraintType1 is neither DataContract nor Serializable.
		public void BaseConstraint1 ()
		{
			new DataContractSerializer (typeof (BaseConstraintType3)).WriteObject (XmlWriter.Create (TextWriter.Null), new BaseConstraintType3 ());
		}

		[Test]
		public void BaseConstraint2 ()
		{
			new DataContractSerializer (typeof (BaseConstraintType4)).WriteObject (XmlWriter.Create (TextWriter.Null), new BaseConstraintType4 ());
		}
	}
	
	[DataContract]
	public class MemberIgnored
	{
		[DataMember]
		MemberIgnoredBody body = new MemberIgnoredBody ();
	}

	public class MemberIgnoredBody
	{
		[IgnoreDataMember]
		public string Foo = "foo";

		public string Bar = "bar";
	}

	public enum Colors {
		Red, Green, Blue
	}

	[Flags]
	public enum Colors2 {
		Red, Green, Blue
	}

	[DataContract (Name = "_ColorsWithDC")]
	public enum ColorsWithDC {

		[EnumMember (Value = "_Red")]
		Red, 
		[EnumMember]
		Green, 
		Blue
	}


	public enum ColorsEnumMemberNoDC {
		[EnumMember (Value = "_Red")]
		Red, 
		[EnumMember]
		Green, 
		Blue
	}

 	[DataContract]
	public class DCWithEnum {
		[DataMember (Name = "_colors")]
		public Colors colors;
	}

 	[DataContract]
	public class DCWithTwoEnums {
		[DataMember]
		public Colors colors;
		[DataMember]
		public Colors colors2;
	}

 	[DataContract]
	public class DCWithNullableEnum {
		[DataMember]
		public Colors? colors;
	}


	[DataContract (Namespace = "Base")]
	public class BaseDC {
		[DataMember]
		public int baseVal;
	}

	[DataContract (Namespace = "Derived")]
	public class DerivedDC : BaseDC {
		[DataMember]
		public int derivedVal;
	}

 	[DataContract]
	public class NestedDC {
		public NestedDC (string name) { this.Name = name; }

		[DataMember]
		public string Name;
	}

 	[DataContract]
	public class NestingDC {
		[DataMember]
		public NestedDC Field1;
		[DataMember]
		public NestedDC Field2;
	}

 	[DataContract (Namespace = "test1")]
	public class NestedDC2 {
		public NestedDC2 (string name) { this.Name = name; }

		[DataMember]
		public string Name;
	}

 	[DataContract (Namespace = "test2")]
	public class NestingDC2 {
		[DataMember]
		public NestedDC2 Field;
	}

	[DataContract]
	public class DCEmpty
	{
		// serializer doesn't touch it.
		public string Foo = "TEST";
	}

	[DataContract (Namespace = "")]
	public class DCEmptyNoNS
	{
	}

	[DataContract]
	public class DCSimple1
	{
		[DataMember]
		public string Foo = "TEST";
	}

	[DataContract]
	public class DCHasNonDC
	{
		[DataMember]
		public NonDC Hoge= new NonDC ();
	}

	public class NonDC
	{
		public string Whee = "whee!";
	}

	[DataContract]
	public class DCHasSerializable
	{
		[DataMember]
		public SimpleSer1 Ser = new SimpleSer1 ();
	}

	[DataContract (Name = "Foo")]
	public class DCWithName
	{
		[DataMember (Name = "FooMember")]
		public string DMWithName = "value";
	}

	[DataContract (Name = "")]
	public class DCWithEmptyName
	{
	}

	[DataContract (Name = null)]
	public class DCWithNullName
	{
	}

	[DataContract (Namespace = "")]
	public class DCWithEmptyNamespace
	{
	}

	[Serializable]
	public class SimpleSer1
	{
		public string Doh = "doh!";
		[NonSerialized]
		public string Bah = "bah!";
	}

	public class Wrapper
	{
		[DataContract]
		public class DCWrapped
		{
		}
	}

	[DataContract]
	public class CollectionContainer
	{
		Collection<string> items = new Collection<string> ();

		[DataMember]
		public Collection<string> Items {
			get { return items; }
		}
	}

	[CollectionDataContract]
	public class DataCollection<T> : Collection<T>
	{
	}

	[DataContract]
	public class DataCollectionContainer
	{
		DataCollection<string> items = new DataCollection<string> ();

		[DataMember]
		public DataCollection<string> Items {
			get { return items; }
		}
	}

	[DataContract]
	class SerializeNonDCArrayType
	{
		[DataMember]
		public NonDCItem [] IPAddresses = new NonDCItem [0];
	}

	public class NonDCItem
	{
		public int [] Data { get; set; }
	}

	[DataContract]
	public class VerifyObjectNameTestData
	{
		[DataMember]
		string Member1 = "foo";

		public string GetMember() { return Member1; }
	}

	[XmlRoot(ElementName = "simple", Namespace = "")]
	public class SimpleXml : IXmlSerializable 
	{
		void IXmlSerializable.ReadXml (XmlReader reader)
		{
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			return null;
		}

	}

	[DataContract]
	public class ReferenceWrapper
	{
	        [DataMember (Order = 1)]
	        public ReferenceType T;

	        [DataMember (Order = 2)]
	        public ReferenceType T2;
	}

	[DataContract (IsReference = true)]
	public class ReferenceType
	{
		[DataMember]
		public string F = "x";
	}

	public class MyList : IList<string>
	{
		List<string> l = new List<string> ();
		public void Clear () { l.Clear (); }
		public void Add(string s) { l.Add (s);}
		public void Insert(int idx, string s) { l.Insert(idx,s);}
		public bool Contains(string s) { return l.Contains(s); }
		public IEnumerator<string> GetEnumerator () { return l.GetEnumerator (); }
		IEnumerator IEnumerable.GetEnumerator () { return l.GetEnumerator (); }
		public bool Remove(string s) { return l.Remove(s); }
		public void RemoveAt(int i) { l.RemoveAt (i);}
		public void CopyTo (string [] arr, int index) { l.CopyTo (arr, index);}
		public int IndexOf (string s) { return l.IndexOf (s); }
	
		public int Count { get { return l.Count; } }
		public bool IsReadOnly { get { return ((IList<string>) l).IsReadOnly; } }
		public string this [int index] { get { return l [index]; } set { l [index] = value; } }
	}

	[DataContract]
	internal class InterfaceCollectionType
	{
		[DataMember]
		public IList<int> Array { get; set; }
	}

	[DataContract]
	public class NestedContractType
	{
		[DataMember]
		public NestedContractType Nested;
		[DataMember]
		public string X = "x";
	}

	class BaseConstraintType1 // non-serializable
	{
	}
	
	[Serializable]
	class BaseConstraintType2
	{
	}
	
	[DataContract]
	class BaseConstraintType3 : BaseConstraintType1
	{
	}
	
	[DataContract]
	class BaseConstraintType4 : BaseConstraintType2
	{
	}
}

[DataContract]
class GlobalSample1
{
}

[DataContract]
class Foo<X,Y,Z>
{
	[DataMember]
	public X Field;
}

[CollectionDataContract (Name = "NAME", Namespace = "urn:foo", ItemName = "ITEM", KeyName = "KEY", ValueName = "VALUE")]
public class MyDictionary<K,V> : Dictionary<K,V>
{
}

// bug #524086
[DataContract(Namespace="http://example.com/schemas/asx")]
public class DummyEntry
{
    [DataMember]
    public DummyEntryInfo EntryInfo { get; set; }
    [DataMember]
    public string Href { get; set; }
}

[DataContract(Namespace="http://example.com/schemas/asx"),
KnownType(typeof(PartDummyEntryInfo))]
public abstract class DummyEntryInfo
{
}

[DataContract(Namespace="http://example.com/schemas/asx")]
public class DummyPlaylist
{
    public IList<DummyEntry> entries = new List<DummyEntry> ();

    [DataMember]
    public IList<DummyEntry> Entries { get { return entries; } set {entries = value;} }
}

[DataContract(Namespace="http://example.com/schemas/asx")]
public class PartDummyEntryInfo : DummyEntryInfo
{
    public PartDummyEntryInfo() {}
}

// bug #524088

[DataContract(Namespace="http://example.com/schemas/asx")]
public class AsxEntryInfo
{
    [DataMember]
    public string AdvertPrompt { get; set; }
}

// bug #560155

[DataContract]
public class Person
{
	[DataMember]
	readonly public string name;
	[DataMember]
	readonly public Guid Id = Guid.Empty;

	public Person (string nameIn, Guid idIn)
	{
		name = nameIn;
		Id = idIn;
	}

	public override string ToString()
	{
		return string.Format ("name={0},id={1}", name, Id);
	}
}

// bug #599889
namespace U2U.DataContracts
{
	[DataContract]
	public class Person
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public Job Occupation { get; set; }
	}

	[DataContract]
	public class Job
	{
		[DataMember]
		public string Description { get; set; }
	}
}

#region bug #610036
//parent class with a name property
[DataContract (Namespace = "Company.Foo")]
public abstract class ParentClass
{
	
	//constructor
	public ParentClass (string name)
	{
		Name = name;
	}
	
	//the name
	[DataMember]
	public string Name{ get; set; }

}

//root object
[DataContract (Namespace = "Company.Foo")]
public class Root : ParentClass
{
	//dict
	[DataMember]
	public Dict<Foo> FDict;	
	
	//constructor
	public Root (string name)
		: base (name)
	{
		FDict = new Dict<Foo> ();
	}
}


//subclass
[DataContract (Namespace = "Company.Foo")]
public class Foo : ParentClass
{
	//here is one dict
	[DataMember]
	public Dict<Bar> FDict;
	
	//constructor
	public Foo (string name) 
		: base (name)
	{
		FDict = new Dict<Bar> ();
	}
	
}

//another sublass
[DataContract (Namespace = "Company.Foo")]
public class Bar : ParentClass
{
	//constructor
	public Bar (string name)
		: base (name)
	{
	}
	
}
//the custom dictionary
[CollectionDataContract (ItemName = "DictItem", Namespace = "Company.Foo")]
public class Dict<T> : Dictionary<string, T> where T : ParentClass
{
	public void Add (T item)
	{
		Add (item.Name, item);
	}
	
}

[DataContract (IsReference = true)]
public class Parent
{
	//constructor
	public Parent ()
	{
		Child = new Child (this);
	}

	[DataMember]
	public Child Child;
}

[DataContract]
public class Child
{
	public Child ()
	{
	}
	
	public Child (Parent parent)
	{
		this.Parent = parent;
	}
	
	[DataMember]
	public Parent Parent;
}

#endregion
