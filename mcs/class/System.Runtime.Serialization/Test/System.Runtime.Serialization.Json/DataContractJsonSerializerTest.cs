//
// DataContractJsonSerializerTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <JAnkit@novell.com>
//	Antoine Cailliau <antoinecailliau@gmail.com>
//
// Copyright (C) 2005-2007 Novell, Inc.  http://www.novell.com

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
// This test code contains tests for DataContractJsonSerializer, which is
// imported from DataContractSerializerTest.cs.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization.Json
{
	[TestFixture]
	public class DataContractJsonSerializerTest
	{
		static readonly XmlWriterSettings settings;

		static DataContractJsonSerializerTest ()
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
			new DataContractJsonSerializer (null);
		}

		[Test]
		public void ConstructorKnownTypesNull ()
		{
			// null knownTypes is allowed.
			new DataContractJsonSerializer (typeof (Sample1), (IEnumerable<Type>) null);
			new DataContractJsonSerializer (typeof (Sample1), "Foo", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNameNull ()
		{
			new DataContractJsonSerializer (typeof (Sample1), (string) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorNegativeMaxObjects ()
		{
			new DataContractJsonSerializer (typeof (Sample1), "Sample1",
				null, -1, false, null, false);
		}

		[Test]
		public void ConstructorMisc ()
		{
			new DataContractJsonSerializer (typeof (JsonGlobalSample1)).WriteObject (new MemoryStream (), new JsonGlobalSample1 ());
		}

		[Test]
		public void WriteObjectContent ()
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter xw = XmlWriter.Create (sw, settings)) {
				DataContractJsonSerializer ser =
					new DataContractJsonSerializer (typeof (string));
				xw.WriteStartElement ("my-element");
				ser.WriteObjectContent (xw, "TEST STRING");
				xw.WriteEndElement ();
			}
			Assert.AreEqual ("<my-element>TEST STRING</my-element>",
				sw.ToString ());
		}

		// int

		[Test]
		public void SerializeIntXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeInt (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""number"">1</root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeIntJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeInt (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"1",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeInt (XmlWriter writer)
		{
			DataContractJsonSerializer ser =
				new DataContractJsonSerializer (typeof (int));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, 1);
			}
		}

		// int, with rootName

		[Test]
		public void SerializeIntXmlWithRootName ()
		{
			StringWriter sw = new StringWriter ();
			SerializeIntWithRootName (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<myroot type=""number"">1</myroot>",
				sw.ToString ());
		}

		[Test]
		// since JsonWriter supports only "root" as the root name, using
		// XmlWriter from JsonReaderWriterFactory will always fail with
		// an explicit rootName.
		[ExpectedException (typeof (SerializationException))]
		public void SerializeIntJsonWithRootName ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeIntWithRootName (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"1",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeIntWithRootName (XmlWriter writer)
		{
			DataContractJsonSerializer ser =
				new DataContractJsonSerializer (typeof (int), "myroot");
			using (XmlWriter w = writer) {
				ser.WriteObject (w, 1);
			}
		}

		// pass typeof(DCEmpty), serialize int

		[Test]
		public void SerializeIntForDCEmptyXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeIntForDCEmpty (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""number"">1</root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeIntForDCEmptyJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeIntForDCEmpty (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"1",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeIntForDCEmpty (XmlWriter writer)
		{
			DataContractJsonSerializer ser =
				new DataContractJsonSerializer (typeof (DCEmpty));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, 1);
			}
		}

		// DCEmpty

		[Test]
		public void SerializeEmptyClassXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeEmptyClass (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""object"" />",
				sw.ToString ());
		}

		[Test]
		public void SerializeEmptyClassJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeEmptyClass (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"{}",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeEmptyClass (XmlWriter writer)
		{
			DataContractJsonSerializer ser =
				new DataContractJsonSerializer (typeof (DCEmpty));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, new DCEmpty ());
			}
		}

		// string (primitive)

		[Test]
		public void SerializePrimitiveStringXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializePrimitiveString (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				"<root>TEST</root>",
				sw.ToString ());
		}

		[Test]
		public void SerializePrimitiveStringJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializePrimitiveString (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				@"""TEST""",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializePrimitiveString (XmlWriter writer)
		{
			XmlObjectSerializer ser =
				new DataContractJsonSerializer (typeof (string));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, "TEST");
			}
		}

		// QName (primitive but ...)

		[Test]
		public void SerializePrimitiveQNameXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializePrimitiveQName (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				"<root>foo:urn:foo</root>",
				sw.ToString ());
		}

		[Test]
		public void SerializePrimitiveQNameJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializePrimitiveQName (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				@"""foo:urn:foo""",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializePrimitiveQName (XmlWriter writer)
		{
			XmlObjectSerializer ser =
				new DataContractJsonSerializer (typeof (XmlQualifiedName));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, new XmlQualifiedName ("foo", "urn:foo"));
			}
		}

		// DBNull (primitive)

		[Test]
		public void SerializeDBNullXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeDBNull (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""object"" />",
				sw.ToString ());
		}

		[Test]
		public void SerializeDBNullJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeDBNull (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"{}",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeDBNull (XmlWriter writer)
		{
			DataContractJsonSerializer ser =
				new DataContractJsonSerializer (typeof (DBNull));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, DBNull.Value);
			}
		}

		// DCSimple1

		[Test]
		public void SerializeSimpleClass1Xml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeSimpleClass1 (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""object""><Foo>TEST</Foo></root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeSimpleClass1Json ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeSimpleClass1 (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				@"{""Foo"":""TEST""}",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeSimpleClass1 (XmlWriter writer)
		{
			DataContractJsonSerializer ser =
				new DataContractJsonSerializer (typeof (DCSimple1));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, new DCSimple1 ());
			}
		}

		// NonDC

		[Test]
		// NonDC is not a DataContract type.
		public void SerializeNonDCOnlyCtor ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (NonDC));
		}

		[Test]
		//[ExpectedException (typeof (InvalidDataContractException))]
		// NonDC is not a DataContract type.
		// UPDATE: non-DataContract types are became valid in RTM.
		public void SerializeNonDC ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (NonDC));
			using (XmlWriter w = XmlWriter.Create (TextWriter.Null, settings)) {
				ser.WriteObject (w, new NonDC ());
			}
		}

		// DCHasNonDC

		[Test]
		//[ExpectedException (typeof (InvalidDataContractException))]
		// DCHasNonDC itself is a DataContract type whose field is
		// marked as DataMember but its type is not DataContract.
		// UPDATE: non-DataContract types are became valid in RTM.
		public void SerializeDCHasNonDC ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (DCHasNonDC));
			using (XmlWriter w = XmlWriter.Create (TextWriter.Null, settings)) {
				ser.WriteObject (w, new DCHasNonDC ());
			}
		}

		// DCHasSerializable

		[Test]
		public void SerializeSimpleSerializable1Xml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeSimpleSerializable1 (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""object""><Ser type=""object""><Doh>doh!</Doh></Ser></root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeSimpleSerializable1Json ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeSimpleSerializable1 (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				@"{""Ser"":{""Doh"":""doh!""}}",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		// DCHasSerializable itself is DataContract and has a field
		// whose type is not contract but serializable.
		void SerializeSimpleSerializable1 (XmlWriter writer)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (DCHasSerializable));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, new DCHasSerializable ());
			}
		}

		[Test]
		public void SerializeDCWithNameXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeDCWithName (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""object""><FooMember>value</FooMember></root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeDCWithNameJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeDCWithName (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				@"{""FooMember"":""value""}",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeDCWithName (XmlWriter writer)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (DCWithName));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, new DCWithName ());
			}
		}

		[Test]
		public void SerializeDCWithEmptyName1 ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (DCWithEmptyName));
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
		public void SerializeDCWithEmptyName2 ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (DCWithName));
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
		[Category("NotWorking")]
		public void SerializeDCWithNullName ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (DCWithNullName));
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
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (DCWithEmptyNamespace));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, new DCWithEmptyNamespace ());
			}
		}

		[Test]
		public void SerializeWrappedClassXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeWrappedClass (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""object"" />",
				sw.ToString ());
		}

		[Test]
		public void SerializeWrappedClassJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeWrappedClass (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"{}",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeWrappedClass (XmlWriter writer)
		{
			DataContractJsonSerializer ser =
				new DataContractJsonSerializer (typeof (Wrapper.DCWrapped));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, new Wrapper.DCWrapped ());
			}
		}

		// CollectionContainer : Items must have a setter. (but became valid in RTM).
		[Test]
		public void SerializeReadOnlyCollectionMember ()
		{
			DataContractJsonSerializer ser =
				new DataContractJsonSerializer (typeof (CollectionContainer));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, null);
			}
		}

		// DataCollectionContainer : Items must have a setter. (but became valid in RTM).
		[Test]
		public void SerializeReadOnlyDataCollectionMember ()
		{
			DataContractJsonSerializer ser =
				new DataContractJsonSerializer (typeof (DataCollectionContainer));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, null);
			}
		}

		[Test]
		[Ignore ("https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=409970")]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeReadOnlyDataCollection_NullCollection ()
		{
			DataContractJsonSerializer ser =
				new DataContractJsonSerializer (typeof (CollectionContainer));
			StringWriter sw = new StringWriter ();
			var c = new CollectionContainer ();
			c.Items.Add ("foo");
			c.Items.Add ("bar");
			using (XmlWriter w = XmlWriter.Create (sw, settings))
				ser.WriteObject (w, c);
			// CollectionContainer.Items is null, so it cannot deserialize non-null collection.
			using (XmlReader r = XmlReader.Create (new StringReader (sw.ToString ())))
				c = (CollectionContainer) ser.ReadObject (r);
		}

		[Test]
		public void SerializeGuidXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeGuid (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root>00000000-0000-0000-0000-000000000000</root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeGuidJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeGuid (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				@"""00000000-0000-0000-0000-000000000000""",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeGuid (XmlWriter writer)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (Guid));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, Guid.Empty);
			}
		}

		[Test]
		public void SerializeEnumXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeEnum (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""number"">0</root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeEnumJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeEnum (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"0",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeEnum (XmlWriter writer)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (Colors));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, new Colors ());
			}
		}

		[Test]
		public void SerializeEnum2Xml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeEnum2 (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""number"">0</root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeEnum2Json ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeEnum2 (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"0",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeEnum2 (XmlWriter writer)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (Colors));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, 0);
			}
		}

		[Test] // so, DataContract does not affect here.
		public void SerializeEnumWithDCXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeEnumWithDC (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""number"">0</root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeEnumWithDCJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeEnumWithDC (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"0",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeEnumWithDC (XmlWriter writer)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (ColorsWithDC));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, new ColorsWithDC ());
			}
		}

		[Test]
		public void SerializeEnumWithNoDCXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeEnumWithNoDC (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""number"">0</root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeEnumWithNoDCJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeEnumWithNoDC (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"0",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeEnumWithNoDC (XmlWriter writer)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (ColorsEnumMemberNoDC));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, new ColorsEnumMemberNoDC ());
			}
		}

		[Test]
		public void SerializeEnumWithDC2Xml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeEnumWithDC2 (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""number"">3</root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeEnumWithDC2Json ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeEnumWithDC2 (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				"3",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeEnumWithDC2 (XmlWriter writer)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (ColorsWithDC));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, 3);
			}
		}

/*
		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void SerializeEnumWithDCInvalid ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (ColorsWithDC));
			StringWriter sw = new StringWriter ();
			ColorsWithDC cdc = ColorsWithDC.Blue;
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, cdc);
			}
		}
*/

		[Test]
		public void SerializeDCWithEnumXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializeDCWithEnum (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""object""><_colors type=""number"">0</_colors></root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeDCWithEnumJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializeDCWithEnum (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				@"{""_colors"":0}",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializeDCWithEnum (XmlWriter writer)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (DCWithEnum));
			using (XmlWriter w = writer) {
				ser.WriteObject (w, new DCWithEnum ());
			}
		}

		[Test]
		public void SerializerDCArrayXml ()
		{
			StringWriter sw = new StringWriter ();
			SerializerDCArray (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""array""><item type=""object""><_colors type=""number"">0</_colors></item><item type=""object""><_colors type=""number"">1</_colors></item></root>",
				sw.ToString ());
		}

		[Test]
		public void SerializerDCArrayJson ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializerDCArray (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				@"[{""_colors"":0},{""_colors"":1}]",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializerDCArray (XmlWriter writer)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (DCWithEnum []));
			DCWithEnum [] arr = new DCWithEnum [2];
			arr [0] = new DCWithEnum (); arr [0].colors = Colors.Red;
			arr [1] = new DCWithEnum (); arr [1].colors = Colors.Green;
			using (XmlWriter w = writer) {
				ser.WriteObject (w, arr);
			}
		}

		[Test]
		public void SerializerDCArray2Xml ()
		{
			StringWriter sw = new StringWriter ();
			SerializerDCArray2 (XmlWriter.Create (sw, settings));
			Assert.AreEqual (
				@"<root type=""array""><item __type=""DCWithEnum:#MonoTests.System.Runtime.Serialization.Json"" type=""object""><_colors type=""number"">0</_colors></item><item __type=""DCSimple1:#MonoTests.System.Runtime.Serialization.Json"" type=""object""><Foo>hello</Foo></item></root>",
				sw.ToString ());
		}

		[Test]
		public void SerializerDCArray2Json ()
		{
			MemoryStream ms = new MemoryStream ();
			SerializerDCArray2 (JsonReaderWriterFactory.CreateJsonWriter (ms));
			Assert.AreEqual (
				@"[{""__type"":""DCWithEnum:#MonoTests.System.Runtime.Serialization.Json"",""_colors"":0},{""__type"":""DCSimple1:#MonoTests.System.Runtime.Serialization.Json"",""Foo"":""hello""}]",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		void SerializerDCArray2 (XmlWriter writer)
		{
			List<Type> known = new List<Type> ();
			known.Add (typeof (DCWithEnum));
			known.Add (typeof (DCSimple1));
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (object []), known);
			object [] arr = new object [2];
			arr [0] = new DCWithEnum (); ((DCWithEnum)arr [0]).colors = Colors.Red;
			arr [1] = new DCSimple1 (); ((DCSimple1) arr [1]).Foo = "hello";

			using (XmlWriter w = writer) {
				ser.WriteObject (w, arr);
			}
		}

		[Test]
		public void SerializerDCArray3Xml ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (int []));
			StringWriter sw = new StringWriter ();
			int [] arr = new int [2];
			arr [0] = 1; arr [1] = 2;

			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (w, arr);
			}

			Assert.AreEqual (
				@"<root type=""array""><item type=""number"">1</item><item type=""number"">2</item></root>",
				sw.ToString ());
		}

		[Test]
		public void SerializerDCArray3Json ()
		{
			MemoryStream ms = new MemoryStream ();
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (int []));
			int [] arr = new int [2];
			arr [0] = 1; arr [1] = 2;

			using (XmlWriter w = JsonReaderWriterFactory.CreateJsonWriter (ms)) {
				ser.WriteObject (w, arr);
			}

			Assert.AreEqual (
				@"[1,2]",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		[Test]
		// ... so, non-JSON XmlWriter is still accepted.
		public void SerializeNonDCArrayXml ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (SerializeNonDCArrayType));
			StringWriter sw = new StringWriter ();
			using (XmlWriter xw = XmlWriter.Create (sw, settings)) {
				ser.WriteObject (xw, new SerializeNonDCArrayType ());
			}
			Assert.AreEqual (@"<root type=""object""><IPAddresses type=""array"" /></root>",
				sw.ToString ());
		}

		[Test]
		public void SerializeNonDCArrayJson ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (SerializeNonDCArrayType));
			MemoryStream ms = new MemoryStream ();
			using (XmlWriter xw = JsonReaderWriterFactory.CreateJsonWriter (ms)) {
				ser.WriteObject (xw, new SerializeNonDCArrayType ());
			}
			Assert.AreEqual (@"{""IPAddresses"":[]}",
				Encoding.UTF8.GetString (ms.ToArray ()));
		}

		[Test]
		public void SerializeNonDCArrayItems ()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof (SerializeNonDCArrayType));
			StringWriter sw = new StringWriter ();
			using (XmlWriter xw = XmlWriter.Create (sw, settings)) {
				SerializeNonDCArrayType obj = new SerializeNonDCArrayType ();
				obj.IPAddresses = new NonDCItem [] {new NonDCItem () { Data = new byte [] {1, 2, 3, 4} } };
				ser.WriteObject (xw, obj);
			}

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (sw.ToString ());
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			nsmgr.AddNamespace ("s", "http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization");
			nsmgr.AddNamespace ("n", "http://schemas.datacontract.org/2004/07/System.Net");
			nsmgr.AddNamespace ("a", "http://schemas.microsoft.com/2003/10/Serialization/Arrays");

			Assert.AreEqual (1, doc.SelectNodes ("/root/IPAddresses/item", nsmgr).Count, "#1");
			XmlElement el = doc.SelectSingleNode ("/root/IPAddresses/item/Data", nsmgr) as XmlElement;
			Assert.IsNotNull (el, "#3");
			Assert.AreEqual (4, el.SelectNodes ("item", nsmgr).Count, "#4");
		}

		[Test]
		public void MaxItemsInObjectGraph1 ()
		{
			// object count == maximum
			DataContractJsonSerializer s = new DataContractJsonSerializer (typeof (DCEmpty), null, 1, false, null, false);
			s.WriteObject (XmlWriter.Create (TextWriter.Null), new DCEmpty ());
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void MaxItemsInObjectGraph2 ()
		{
			// object count > maximum
			DataContractJsonSerializer s = new DataContractJsonSerializer (typeof (DCSimple1), null, 1, false, null, false);
			s.WriteObject (XmlWriter.Create (TextWriter.Null), new DCSimple1 ());
		}

		[Test]
		public void DeserializeString ()
		{
			Assert.AreEqual ("ABC", Deserialize ("\"ABC\"", typeof (string)));
		}

		[Test]
		public void DeserializeInt ()
		{
			Assert.AreEqual (5, Deserialize ("5", typeof (int)));
		}

		[Test]
		public void DeserializeArray ()
		{
			int [] ret = (int []) Deserialize ("[5,6,7]", typeof (int []));
			Assert.AreEqual (5, ret [0], "#1");
			Assert.AreEqual (6, ret [1], "#2");
			Assert.AreEqual (7, ret [2], "#3");
		}

		[Test]
		public void DeserializeArrayUntyped ()
		{
			object [] ret = (object []) Deserialize ("[5,6,7]", typeof (object []));
			Assert.AreEqual (5, ret [0], "#1");
			Assert.AreEqual (6, ret [1], "#2");
			Assert.AreEqual (7, ret [2], "#3");
		}

		[Test]
		public void DeserializeMixedArray ()
		{
			object [] ret = (object []) Deserialize ("[5,\"6\",false]", typeof (object []));
			Assert.AreEqual (5, ret [0], "#1");
			Assert.AreEqual ("6", ret [1], "#2");
			Assert.AreEqual (false, ret [2], "#3");
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEmptyAsString ()
		{
			// it somehow expects "root" which should have been already consumed.
			Deserialize ("", typeof (string));
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEmptyAsInt ()
		{
			// it somehow expects "root" which should have been already consumed.
			Deserialize ("", typeof (int));
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEmptyAsDBNull ()
		{
			// it somehow expects "root" which should have been already consumed.
			Deserialize ("", typeof (DBNull));
		}

		[Test]
		public void DeserializeEmptyObjectAsString ()
		{
			// looks like it is converted to ""
			Assert.AreEqual (String.Empty, Deserialize ("{}", typeof (string)));
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEmptyObjectAsInt ()
		{
			Deserialize ("{}", typeof (int));
		}

		[Test]
		public void DeserializeEmptyObjectAsDBNull ()
		{
			Assert.AreEqual (DBNull.Value, Deserialize ("{}", typeof (DBNull)));
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEnumByName ()
		{
			// enum is parsed into long
			Deserialize (@"""Red""", typeof (Colors));
		}

		[Test]
		public void DeserializeEnum2 ()
		{
			object o = Deserialize ("0", typeof (Colors));

			Assert.AreEqual (typeof (Colors), o.GetType (), "#de3");
			Colors c = (Colors) o;
			Assert.AreEqual (Colors.Red, c, "#de4");
		}
		
		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void DeserializeEnumInvalid ()
		{
			Deserialize ("", typeof (Colors));
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		[Ignore ("NotDotNet")] // 0.0 is an invalid Colors value.
		public void DeserializeEnumInvalid3 ()
		{
			//"0.0" instead of "0"
			Deserialize (
				"0.0",
				typeof (Colors));
		}

		[Test]
		public void DeserializeEnumWithDC ()
		{
			object o = Deserialize ("0", typeof (ColorsWithDC));
			
			Assert.AreEqual (typeof (ColorsWithDC), o.GetType (), "#de5");
			ColorsWithDC cdc = (ColorsWithDC) o;
			Assert.AreEqual (ColorsWithDC.Red, o, "#de6");
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		[Ignore ("NotDotNet")] // 4 is an invalid Colors value.
		[Category ("NotWorking")]
		public void DeserializeEnumWithDCInvalid ()
		{
			Deserialize (
				"4",
				typeof (ColorsWithDC));
		}

		[Test]
		public void DeserializeDCWithEnum ()
		{
			object o = Deserialize (
				"{\"_colors\":0}",
				typeof (DCWithEnum));

			Assert.AreEqual (typeof (DCWithEnum), o.GetType (), "#de7");
			DCWithEnum dc = (DCWithEnum) o;
			Assert.AreEqual (Colors.Red, dc.colors, "#de8");
		}

		[Test]
		public void ReadObjectVerifyObjectNameFalse ()
		{
			string xml = @"<any><Member1>bar</Member1></any>";
			object o = new DataContractJsonSerializer (typeof (VerifyObjectNameTestData))
				.ReadObject (XmlReader.Create (new StringReader (xml)), false);
			Assert.IsTrue (o is VerifyObjectNameTestData, "#1");

			string xml2 = @"<any><x:Member1 xmlns:x=""http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"">bar</x:Member1></any>";
			o = new DataContractJsonSerializer (typeof (VerifyObjectNameTestData))
				.ReadObject (XmlReader.Create (new StringReader (xml2)), false);
			Assert.IsTrue (o is VerifyObjectNameTestData, "#2");
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void ReadObjectVerifyObjectNameTrue ()
		{
			string xml = @"<any><Member1>bar</Member1></any>";
			new DataContractJsonSerializer (typeof (VerifyObjectNameTestData))
				.ReadObject (XmlReader.Create (new StringReader (xml)), true);
		}

		[Test] // member name is out of scope
		public void ReadObjectVerifyObjectNameTrue2 ()
		{
			string xml = @"<root><Member2>bar</Member2></root>";
			new DataContractJsonSerializer (typeof (VerifyObjectNameTestData))
				.ReadObject (XmlReader.Create (new StringReader (xml)), true);
		}

		[Test]
		public void ReadTypedObjectJson ()
		{
			object o = Deserialize (@"{""__type"":""DCWithEnum:#MonoTests.System.Runtime.Serialization.Json"",""_colors"":0}", typeof (DCWithEnum));
			Assert.AreEqual (typeof (DCWithEnum), o.GetType ());
		}

		[Test]
		public void ReadObjectDCArrayJson ()
		{
			object o = Deserialize (@"[{""__type"":""DCWithEnum:#MonoTests.System.Runtime.Serialization.Json"",""_colors"":0}]",
				typeof (object []), typeof (DCWithEnum));
			Assert.AreEqual (typeof (object []), o.GetType (), "#1");
			object [] arr = (object []) o;
			Assert.AreEqual (typeof (DCWithEnum), arr [0].GetType (), "#2");
		}

		[Test]
		public void ReadObjectDCArray2Json ()
		{
			object o = Deserialize (@"[{""__type"":""DCWithEnum:#MonoTests.System.Runtime.Serialization.Json"",""_colors"":0},{""__type"":""DCSimple1:#MonoTests.System.Runtime.Serialization.Json"",""Foo"":""hello""}]",
				typeof (object []), typeof (DCWithEnum), typeof (DCSimple1));
			Assert.AreEqual (typeof (object []), o.GetType (), "#1");
			object [] arr = (object []) o;
			Assert.AreEqual (typeof (DCWithEnum), arr [0].GetType (), "#2");
			Assert.AreEqual (typeof (DCSimple1), arr [1].GetType (), "#3");
		}

		private object Deserialize (string xml, Type type, params Type [] knownTypes)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer (type, knownTypes);
			XmlReader xr = JsonReaderWriterFactory.CreateJsonReader (Encoding.UTF8.GetBytes (xml), new XmlDictionaryReaderQuotas ());
			return ser.ReadObject (xr);
		}

		public T Deserialize<T>(string json)
		{
			var bytes = Encoding.Unicode.GetBytes (json);
			using (MemoryStream stream = new MemoryStream (bytes)) {
				var serializer = new DataContractJsonSerializer (typeof(T));
				return (T)serializer.ReadObject (stream);	
			}
		}

		[Test]
		public void IsStartObject ()
		{
			DataContractJsonSerializer s = new DataContractJsonSerializer (typeof (DCSimple1));
			Assert.IsTrue (s.IsStartObject (XmlReader.Create (new StringReader ("<root></root>"))), "#1");
			Assert.IsFalse (s.IsStartObject (XmlReader.Create (new StringReader ("<dummy></dummy>"))), "#2");
			Assert.IsFalse (s.IsStartObject (XmlReader.Create (new StringReader ("<Foo></Foo>"))), "#3");
			Assert.IsFalse (s.IsStartObject (XmlReader.Create (new StringReader ("<root xmlns='urn:foo'></root>"))), "#4");
		}

		[Test]
		public void SerializeNonDC2 ()
		{
			var ser = new DataContractJsonSerializer (typeof (TestData));
			StringWriter sw = new StringWriter ();
			var obj = new TestData () { Foo = "foo", Bar = "bar", Baz = "baz" };

			// XML
			using (var xw = XmlWriter.Create (sw))
				ser.WriteObject (xw, obj);
			var s = sw.ToString ();
			// since the order is not preserved, we compare only contents.
			Assert.IsTrue (s.IndexOf ("<Foo>foo</Foo>") > 0, "#1-1");
			Assert.IsTrue (s.IndexOf ("<Bar>bar</Bar>") > 0, "#1-2");
			Assert.IsFalse (s.IndexOf ("<Baz>baz</Baz>") > 0, "#1-3");

			// JSON
			MemoryStream ms = new MemoryStream ();
			using (var xw = JsonReaderWriterFactory.CreateJsonWriter (ms))
				ser.WriteObject (ms, obj);
			s = new StreamReader (new MemoryStream (ms.ToArray ())).ReadToEnd ().Replace ('"', '/');
			// since the order is not preserved, we compare only contents.
			Assert.IsTrue (s.IndexOf ("/Foo/:/foo/") > 0, "#2-1");
			Assert.IsTrue (s.IndexOf ("/Bar/:/bar/") > 0, "#2-2");
			Assert.IsFalse (s.IndexOf ("/Baz/:/baz/") > 0, "#2-3");
		}

		[Test]
		public void AlwaysEmitTypeInformation ()
		{
			var ms = new MemoryStream ();
			var ds = new DataContractJsonSerializer (typeof (string), "root", null, 10, false, null, true);
			ds.WriteObject (ms, "foobar");
			var s = Encoding.UTF8.GetString (ms.ToArray ());
			Assert.AreEqual ("\"foobar\"", s, "#1");
		}

		[Test]
		public void AlwaysEmitTypeInformation2 ()
		{
			var ms = new MemoryStream ();
			var ds = new DataContractJsonSerializer (typeof (TestData), "root", null, 10, false, null, true);
			ds.WriteObject (ms, new TestData () { Foo = "foo"});
			var s = Encoding.UTF8.GetString (ms.ToArray ());
			Assert.AreEqual (@"{""__type"":""TestData:#MonoTests.System.Runtime.Serialization.Json"",""Bar"":null,""Foo"":""foo""}", s, "#1");
		}

		[Test]
		public void AlwaysEmitTypeInformation3 ()
		{
			var ms = new MemoryStream ();
			var ds = new DataContractJsonSerializer (typeof (TestData), "root", null, 10, false, null, false);
			ds.WriteObject (ms, new TestData () { Foo = "foo"});
			var s = Encoding.UTF8.GetString (ms.ToArray ());
			Assert.AreEqual (@"{""Bar"":null,""Foo"":""foo""}", s, "#1");
		}

		[Test]
		public void TestNonpublicDeserialization ()
		{
			string s1= @"{""Bar"":""bar"", ""Foo"":""foo"", ""Baz"":""baz""}";
			TestData o1 = ((TestData)(new DataContractJsonSerializer (typeof (TestData)).ReadObject (JsonReaderWriterFactory.CreateJsonReader (Encoding.UTF8.GetBytes (s1), new XmlDictionaryReaderQuotas ()))));

			Assert.AreEqual (null, o1.Baz, "#1");

                        string s2 = @"{""TestData"":[{""key"":""key1"",""value"":""value1""}]}";
                        KeyValueTestData o2 = ((KeyValueTestData)(new DataContractJsonSerializer (typeof (KeyValueTestData)).ReadObject (JsonReaderWriterFactory.CreateJsonReader (Encoding.UTF8.GetBytes (s2), new XmlDictionaryReaderQuotas ()))));

			Assert.AreEqual (1, o2.TestData.Count, "#2");
			Assert.AreEqual ("key1", o2.TestData[0].Key, "#3");
			Assert.AreEqual ("value1", o2.TestData[0].Value, "#4");
		}

		// [Test] use this case if you want to check lame silverlight parser behavior. Seealso #549756
		public void QuotelessDeserialization ()
		{
			string s1 = @"{FooMember:""value""}";
			var ds = new DataContractJsonSerializer (typeof (DCWithName));
			ds.ReadObject (new MemoryStream (Encoding.UTF8.GetBytes (s1)));

			string s2 = @"{FooMember:"" \""{dummy:string}\""""}";
			ds.ReadObject (new MemoryStream (Encoding.UTF8.GetBytes (s2)));
		}

		[Test]
		[Category ("NotWorking")]
		public void TypeIsNotPartsOfKnownTypes ()
		{
			var dcs = new DataContractSerializer (typeof (string));
			Assert.AreEqual (0, dcs.KnownTypes.Count, "KnownTypes #1");
			var dcjs = new DataContractJsonSerializer (typeof (string));
			Assert.AreEqual (0, dcjs.KnownTypes.Count, "KnownTypes #2");
		}

		[Test]
		public void ReadWriteNullObject ()
		{
			DataContractJsonSerializer dcjs = new DataContractJsonSerializer (typeof (string));
			using (MemoryStream ms = new MemoryStream ()) {
				dcjs.WriteObject (ms, null);
				ms.Position = 0;
				using (StreamReader sr = new StreamReader (ms)) {
					string data = sr.ReadToEnd ();
					Assert.AreEqual ("null", data, "WriteObject(stream,null)");

					ms.Position = 0;
					Assert.IsNull (dcjs.ReadObject (ms), "ReadObject(stream)");
				}
			};
		}

		object ReadWriteObject (Type type, object obj, string expected)
		{
			using (MemoryStream ms = new MemoryStream ()) {
				DataContractJsonSerializer dcjs = new DataContractJsonSerializer (type);
				dcjs.WriteObject (ms, obj);
				ms.Position = 0;
				using (StreamReader sr = new StreamReader (ms)) {
					Assert.AreEqual (expected, sr.ReadToEnd (), "WriteObject");

					ms.Position = 0;
					return dcjs.ReadObject (ms);
				}
			}
		}

		[Test]
		[Ignore ("Wrong test case. See bug #573691")]
		public void ReadWriteObject_Single_SpecialCases ()
		{
			Assert.IsTrue (Single.IsNaN ((float) ReadWriteObject (typeof (float), Single.NaN, "NaN")));
			Assert.IsTrue (Single.IsNegativeInfinity ((float) ReadWriteObject (typeof (float), Single.NegativeInfinity, "-INF")));
			Assert.IsTrue (Single.IsPositiveInfinity ((float) ReadWriteObject (typeof (float), Single.PositiveInfinity, "INF")));
		}

		[Test]
		[Ignore ("Wrong test case. See bug #573691")]
		public void ReadWriteObject_Double_SpecialCases ()
		{
			Assert.IsTrue (Double.IsNaN ((double) ReadWriteObject (typeof (double), Double.NaN, "NaN")));
			Assert.IsTrue (Double.IsNegativeInfinity ((double) ReadWriteObject (typeof (double), Double.NegativeInfinity, "-INF")));
			Assert.IsTrue (Double.IsPositiveInfinity ((double) ReadWriteObject (typeof (double), Double.PositiveInfinity, "INF")));
		}

		[Test]
		public void ReadWriteDateTime ()
		{
			var ms = new MemoryStream ();
			DataContractJsonSerializer serializer = new DataContractJsonSerializer (typeof (Query));
			Query query = new Query () {
				StartDate = DateTime.SpecifyKind (new DateTime (2010, 3, 4, 5, 6, 7), DateTimeKind.Utc),
				EndDate = DateTime.SpecifyKind (new DateTime (2010, 4, 5, 6, 7, 8), DateTimeKind.Utc)
				};
			serializer.WriteObject (ms, query);
			Assert.AreEqual ("{\"StartDate\":\"\\/Date(1267679167000)\\/\",\"EndDate\":\"\\/Date(1270447628000)\\/\"}", Encoding.UTF8.GetString (ms.ToArray ()), "#1");
			ms.Position = 0;
			Console.WriteLine (new StreamReader (ms).ReadToEnd ());
			ms.Position = 0;
			var q = (Query) serializer.ReadObject(ms);
			Assert.AreEqual (query.StartDate, q.StartDate, "#2");
			Assert.AreEqual (query.EndDate, q.EndDate, "#3");
		}

		[DataContract(Name = "DateTest")]
		public class DateTest
		{
			[DataMember(Name = "should_have_value")]
			public DateTime? ShouldHaveValue { get; set; }
		}

		//
		// This tests both the extended format "number-0500" as well
		// as the nullable field in the structure
		[Test]
		public void BugXamarin163 ()
		{
			string json = @"{""should_have_value"":""\/Date(1277355600000)\/""}";

			byte[] bytes = global::System.Text.Encoding.UTF8.GetBytes(json);
			Stream inputStream = new MemoryStream(bytes);
			
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DateTest));
			DateTest t = serializer.ReadObject(inputStream) as DateTest;
			Assert.AreEqual (634129524000000000, t.ShouldHaveValue.Value.Ticks, "#1");
		}

		[Test]
		public void NullableFieldsShouldSupportNullValue ()
		{
			string json = @"{""should_have_value"":null}";
			var inputStream = new MemoryStream (Encoding.UTF8.GetBytes (json));
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DateTest));
			Console.WriteLine ("# serializer assembly: {0}", serializer.GetType ().Assembly.Location);
			DateTest t = serializer.ReadObject (inputStream) as DateTest;
			Assert.AreEqual (false, t.ShouldHaveValue.HasValue, "#2");
		}
		
		[Test]
		public void DeserializeNullMember ()
		{
			var ds = new DataContractJsonSerializer (typeof (ClassA));
			var stream = new MemoryStream ();
			var a = new ClassA ();
			ds.WriteObject (stream, a);
			stream.Position = 0;
			a = (ClassA) ds.ReadObject (stream);
			Assert.IsNull (a.B, "#1");
		}

		[Test]
		public void OnDeserializationMethods ()
		{
			var ds = new DataContractJsonSerializer (typeof (GSPlayerListErg));
			var obj = new GSPlayerListErg ();
			var ms = new MemoryStream ();
			ds.WriteObject (ms, obj);
			ms.Position = 0;
			ds.ReadObject (ms);
			Assert.IsTrue (GSPlayerListErg.A, "A");
			Assert.IsTrue (GSPlayerListErg.B, "B");
			Assert.IsTrue (GSPlayerListErg.C, "C");
		}
		
		[Test]
		public void WriteChar ()
		{
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof (CharTest));
			using (MemoryStream ms = new MemoryStream()) {
				serializer.WriteObject(ms, new CharTest ());
				ms.Position = 0L;
				using (StreamReader reader = new StreamReader(ms)) {
					reader.ReadToEnd();
				}
			}
		}

		[Test]
		public void DictionarySerialization ()
		{
			var dict = new JsonMyDictionary<string,string> ();
			dict.Add ("key", "value");
			var serializer = new DataContractJsonSerializer (dict.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, dict);
			stream.Position = 0;

			Assert.AreEqual ("[{\"Key\":\"key\",\"Value\":\"value\"}]", new StreamReader (stream).ReadToEnd (), "#1");
			stream.Position = 0;
			dict = (JsonMyDictionary<string,string>) serializer.ReadObject (stream);
			Assert.AreEqual (1, dict.Count, "#2");
			Assert.AreEqual ("value", dict ["key"], "#3");
		}

		[Test]
		public void ExplicitCustomDictionarySerialization ()
		{
			var dict = new MyExplicitDictionary<string,string> ();
			dict.Add ("key", "value");
			var serializer = new DataContractJsonSerializer (dict.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, dict);
			stream.Position = 0;

			Assert.AreEqual ("[{\"Key\":\"key\",\"Value\":\"value\"}]", new StreamReader (stream).ReadToEnd (), "#1");
			stream.Position = 0;
			dict = (MyExplicitDictionary<string,string>) serializer.ReadObject (stream);
			Assert.AreEqual (1, dict.Count, "#2");
			Assert.AreEqual ("value", dict ["key"], "#3");
		}

		[Test]
		public void Bug13485 ()
		{
			const string json = "{ \"Name\" : \"Test\", \"Value\" : \"ValueA\" }";

			string result = string.Empty;
			var serializer = new DataContractJsonSerializer (typeof (Bug13485Type));
			Bug13485Type entity;
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (json)))
				entity = (Bug13485Type) serializer.ReadObject (stream);

			result = entity.GetValue;
			Assert.AreEqual ("ValueA", result, "#1");
		}

		[DataContract(Name = "UriTest")]
		public class UriTest
		{
			[DataMember(Name = "members")]
			public Uri MembersRelativeLink { get; set; }
		}

		[Test]
		public void Bug15169 ()
		{
			const string json = "{\"members\":\"foo/bar/members\"}";
			var serializer = new DataContractJsonSerializer (typeof (UriTest));
			UriTest entity;
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (json)))
				entity = (UriTest) serializer.ReadObject (stream);

			Assert.AreEqual ("foo/bar/members", entity.MembersRelativeLink.ToString ());
		}
		
		#region Test methods for collection serialization
		
		[Test]
		public void TestArrayListSerialization ()
		{
			var collection = new ArrayListContainer ();
			var expectedOutput = "{\"Items\":[\"banana\",\"apple\"]}";
			var expectedItemsCount = 4;
			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);

			stream.Position = 0;
			Assert.AreEqual (expectedOutput, new StreamReader (stream).ReadToEnd (), "#1");

			stream.Position = 0;
			collection = (ArrayListContainer) serializer.ReadObject (stream);
			
			Assert.AreEqual (expectedItemsCount, collection.Items.Count, "#2");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void TestBitArraySerialization ()
		{
			var collection = new BitArrayContainer ();
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
		}
		
		[Test]
		public void TestHashtableSerialization ()
		{
			var collection = new HashtableContainer ();
			var expectedOutput_A = "{\"Items\":[{\"Key\":\"key1\",\"Value\":\"banana\"},{\"Key\":\"key2\",\"Value\":\"apple\"}]}";
			var expectedOutput_B = "{\"Items\":[{\"Key\":\"key2\",\"Value\":\"apple\"},{\"Key\":\"key1\",\"Value\":\"banana\"}]}";

			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);

			stream.Position = 0;
			var read = new StreamReader (stream).ReadToEnd ();

			Assert.IsTrue (expectedOutput_A == read || expectedOutput_B == read, "#1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestHashtableDeserialization ()
		{
			var collection = new HashtableContainer ();
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
			
			stream.Position = 0;
			serializer.ReadObject (stream);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void TestQueueSerialization ()
		{
			var collection = new QueueContainer ();
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
		}
		
		[Test]
		public void TestSortedListSerialization ()
		{
			var collection = new SortedListContainer ();
			var expectedOutput = "{\"Items\":[{\"Key\":\"key1\",\"Value\":\"banana\"},{\"Key\":\"key2\",\"Value\":\"apple\"}]}";
			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);

			stream.Position = 0;
			Assert.AreEqual (expectedOutput, new StreamReader (stream).ReadToEnd (), "#1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestSortedListDeserialization ()
		{
			var collection = new SortedListContainer ();
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
			
			stream.Position = 0;
			serializer.ReadObject (stream);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void TestStackSerialization ()
		{
			var collection = new StackContainer ();
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
		}
		
		[Test]
		public void TestEnumerableWithAddSerialization ()
		{
			var collection = new EnumerableWithAddContainer ();
			var expectedOutput = "{\"Items\":[\"banana\",\"apple\"]}";
			var expectedItemsCount = 4;
			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);

			stream.Position = 0;
			Assert.AreEqual (expectedOutput, new StreamReader (stream).ReadToEnd (), "#1");

			stream.Position = 0;
			collection = (EnumerableWithAddContainer) serializer.ReadObject (stream);
			
			Assert.AreEqual (expectedItemsCount, collection.Items.Count, "#2");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void TestEnumerableWithSpecialAddSerialization ()
		{
			var collection = new EnumerableWithSpecialAddContainer ();			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
		}
	
		[Test]
		public void TestHashSetSerialization ()
		{
			var collection = new GenericHashSetContainer ();
			var expectedOutput = "{\"Items\":[\"banana\",\"apple\"]}";
			var expectedItemsCount = 2;
			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);

			stream.Position = 0;
			Assert.AreEqual (expectedOutput, new StreamReader (stream).ReadToEnd (), "#1");

			stream.Position = 0;
			collection = (GenericHashSetContainer) serializer.ReadObject (stream);
			
			Assert.AreEqual (expectedItemsCount, collection.Items.Count, "#2");
		}
		
		[Test]
		public void TestLinkedListSerialization ()
		{
			var collection = new GenericLinkedListContainer ();
			var expectedOutput = "{\"Items\":[\"banana\",\"apple\"]}";
			var expectedItemsCount = 4;
			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);

			stream.Position = 0;
			Assert.AreEqual (expectedOutput, new StreamReader (stream).ReadToEnd (), "#1");

			stream.Position = 0;
			collection = (GenericLinkedListContainer) serializer.ReadObject (stream);
			
			Assert.AreEqual (expectedItemsCount, collection.Items.Count, "#2");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void TestGenericQueueSerialization ()
		{
			var collection = new GenericQueueContainer ();			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void TestGenericStackSerialization ()
		{
			var collection = new GenericStackContainer ();			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
		}
		
		[Test]
		public void TestGenericDictionarySerialization ()
		{
			var collection = new GenericDictionaryContainer ();			
			var expectedOutput = "{\"Items\":[{\"Key\":\"key1\",\"Value\":\"banana\"},{\"Key\":\"key2\",\"Value\":\"apple\"}]}";
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
			
			stream.Position = 0;
			Assert.AreEqual (expectedOutput, new StreamReader (stream).ReadToEnd (), "#1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestGenericDictionaryDeserialization ()
		{
			var collection = new GenericDictionaryContainer ();			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
			
			stream.Position = 0;
			serializer.ReadObject (stream);
		}
		
		[Test]
		public void TestGenericSortedListSerialization ()
		{
			var collection = new GenericSortedListContainer ();			
			var expectedOutput = "{\"Items\":[{\"Key\":\"key1\",\"Value\":\"banana\"},{\"Key\":\"key2\",\"Value\":\"apple\"}]}";
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
			
			stream.Position = 0;
			Assert.AreEqual (expectedOutput, new StreamReader (stream).ReadToEnd (), "#1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestGenericSortedListDeserialization ()
		{
			var collection = new GenericSortedListContainer ();			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
			
			stream.Position = 0;
			serializer.ReadObject (stream);
		}
		
		[Test]
		public void TestGenericSortedDictionarySerialization ()
		{
			var collection = new GenericSortedDictionaryContainer ();			
			var expectedOutput = "{\"Items\":[{\"Key\":\"key1\",\"Value\":\"banana\"},{\"Key\":\"key2\",\"Value\":\"apple\"}]}";
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
			
			stream.Position = 0;
			Assert.AreEqual (expectedOutput, new StreamReader (stream).ReadToEnd (), "#1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestGenericSortedDictionaryDeserialization ()
		{
			var collection = new GenericSortedDictionaryContainer ();			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
			
			stream.Position = 0;
			serializer.ReadObject (stream);
		}
		
		[Test]
		public void TestGenericEnumerableWithAddSerialization ()
		{
			var collection = new GenericEnumerableWithAddContainer ();
			var expectedOutput = "{\"Items\":[\"banana\",\"apple\"]}";
			var expectedItemsCount = 4;
			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);

			stream.Position = 0;
			Assert.AreEqual (expectedOutput, new StreamReader (stream).ReadToEnd (), "#1");

			stream.Position = 0;
			collection = (GenericEnumerableWithAddContainer) serializer.ReadObject (stream);
			
			Assert.AreEqual (expectedItemsCount, collection.Items.Count, "#2");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void TestGenericEnumerableWithSpecialAddSerialization ()
		{
			var collection = new GenericEnumerableWithSpecialAddContainer ();			
			var serializer = new DataContractJsonSerializer (collection.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, collection);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void TestNonCollectionGetOnlyProperty ()
		{
			var o = new NonCollectionGetOnlyContainer ();			
			var serializer = new DataContractJsonSerializer (o.GetType ());
			var stream = new MemoryStream ();
			serializer.WriteObject (stream, o);
		}
		
		// properly deserialize object with a polymorphic property (known derived type)
		[Test]
		public void Bug23058()
		{
			string serializedObj = @"{""PolymorphicProperty"":{""__type"":""KnownDerivedType:#MonoTests.System.Runtime.Serialization.Json"",""BaseTypeProperty"":""Base"",""DerivedProperty"":""Derived 1""},""Name"":""Parent2""}";
			ParentType deserializedObj = Deserialize<ParentType> (serializedObj);

			Assert.AreEqual (deserializedObj.PolymorphicProperty.GetType ().FullName, "MonoTests.System.Runtime.Serialization.Json.KnownDerivedType");
			Assert.AreEqual (deserializedObj.PolymorphicProperty.BaseTypeProperty, "Base");
			Assert.AreEqual ((deserializedObj.PolymorphicProperty as KnownDerivedType).DerivedProperty, "Derived 1");
			Assert.AreEqual (deserializedObj.Name, "Parent2");
		}

		// properly deserialize object with a polymorphic property (base type with __type hint)
		[Test]
		public void DeserializeBaseTypePropHint()
		{
			string serializedObj = @"{""PolymorphicProperty"":{""__type"":""BaseType:#MonoTests.System.Runtime.Serialization.Json"",""BaseTypeProperty"":""Base""},""Name"":""Parent2""}";
			ParentType deserializedObj = Deserialize<ParentType> (serializedObj);

			Assert.AreEqual (deserializedObj.PolymorphicProperty.GetType ().FullName, "MonoTests.System.Runtime.Serialization.Json.BaseType");
			Assert.AreEqual (deserializedObj.PolymorphicProperty.BaseTypeProperty, "Base");
		}

		// properly deserialize object with a polymorphic property (base type with __type hint)
		[Test]
		public void DeserializeBaseTypePropNoHint()
		{
			string serializedObj = @"{""PolymorphicProperty"":{""BaseTypeProperty"":""Base""},""Name"":""Parent2""}";
			ParentType deserializedObj = Deserialize<ParentType> (serializedObj);

			Assert.AreEqual (deserializedObj.PolymorphicProperty.GetType ().FullName, "MonoTests.System.Runtime.Serialization.Json.BaseType");
			Assert.AreEqual (deserializedObj.PolymorphicProperty.BaseTypeProperty, "Base");
		}

		// properly fail deserializing object with a polymorphic property (unknown derived type)
		[ExpectedException (typeof (SerializationException))]
		[Test]
		public void FailDeserializingUnknownTypeProp()
		{
			string serializedObj = @"{""PolymorphicProperty"":{""__type"":""UnknownDerivedType:#MonoTests.System.Runtime.Serialization.Json"",""BaseTypeProperty"":""Base"",""DerivedProperty"":""Derived 1""},""Name"":""Parent2""}";
			ParentType deserializedObj = Deserialize<ParentType> (serializedObj);
		}

		[Test]
		public void SubclassTest ()
		{
			var knownTypes = new List<Type> { typeof(IntList) };
	                var serializer = new DataContractJsonSerializer(typeof(ListOfNumbers), knownTypes);

			string json = "{\"Numbers\": [85]}";
			using (var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(json)))
			{
				var nums = (ListOfNumbers)serializer.ReadObject(stream);
				Assert.AreEqual (1, nums.Numbers.Count);
			}
		}
		[DataContract]
		public class ListOfNumbers
		{
			[DataMember]
			public IntList Numbers;
		}

		public class IntList : List<int>{}
		#endregion

		[Test]
		public void DefaultValueDeserialization ()
		{
			// value type
			var person = new JsonPerson { name = "John" };
			using (var ms = new MemoryStream()) {
				var serializer = new DataContractJsonSerializer (typeof (JsonPerson), new DataContractJsonSerializerSettings {
					SerializeReadOnlyTypes = true,
					UseSimpleDictionaryFormat = true
					});
				serializer.WriteObject (ms, person);
			}

			// reference type
			var person2 = new PersonWithContact {
				name = "Jane",
				contact = new Contact { url = "localhost", email = "jane@localhost" } };
			using (var ms = new MemoryStream ()) {
				var serializer = new DataContractJsonSerializer (typeof (PersonWithContact), new DataContractJsonSerializerSettings {
					SerializeReadOnlyTypes = true,
					UseSimpleDictionaryFormat = true
					});
				serializer.WriteObject (ms, person2);
			}
		}

		[Test]
		public void Bug15028()
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Bug15028));
			using (MemoryStream memoryStream = new MemoryStream())
			{
				ser.WriteObject(memoryStream, new Bug15028());
				string output = Encoding.Default.GetString(memoryStream.ToArray());
				Assert.AreEqual(@"{""Int0"":1,""Int1"":1,""IntZero1"":0,""Str0"":"""",""Str1"":"""",""StrNull1"":null}", output);
			}
		}

		[Test]
		public void Bug4230()
		{
			string serializedObj = @"{ ""Notifications"": null }";
			Bug4230Response deserializedObj = Deserialize<Bug4230Response> (serializedObj);

			Assert.IsNull (deserializedObj.Notifications);
		}
	}

	public class Bug4230Notification {
	}

	public class Bug4230Response
	{
		public Bug4230Notification[] Notifications
		{
			get;
			set;
		}
	}

	[DataContract]
	public class Bug15028
	{
		[DataMember(EmitDefaultValue = false)]
		public string StrNull0 { get; private set; }

		[DataMember(EmitDefaultValue = false)]
		public string Str0 { get; private set; }

		[DataMember(EmitDefaultValue = true)]
		public string StrNull1 { get; private set; }

		[DataMember(EmitDefaultValue = true)]
		public string Str1 { get; private set; }

		[DataMember(EmitDefaultValue = false)]
		public int IntZero0 { get; private set; }

		[DataMember(EmitDefaultValue = false)]
		public int Int0 { get; private set; }

		[DataMember(EmitDefaultValue = true)]
		public int IntZero1 { get; private set; }

		[DataMember(EmitDefaultValue = true)]
		public int Int1 { get; private set; }

		public Bug15028()
		{
			Str0 = string.Empty;
			Str1 = string.Empty;
			Int0 = 1;
			Int1 = 1;
		}
	}

	public class CharTest
	{
		public char Foo;
	}

	public class TestData
	{
		public string Foo { get; set; }
		public string Bar { get; set; }
		internal string Baz { get; set; }
	}

	public enum Colors {
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
	public class DCEmpty
	{
		// serializer doesn't touch it.
		public string Foo = "TEST";
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
		[DataMember]
		public string Foo;
	}

	[DataContract (Name = null)]
	public class DCWithNullName
	{
		[DataMember]
		public string Foo;
	}

	[DataContract (Namespace = "")]
	public class DCWithEmptyNamespace
	{
		[DataMember]
		public string Foo;
	}

	[Serializable]
	public class SimpleSer1
	{
		public string Doh = "doh!";
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
		public byte [] Data { get; set; }
	}

	[DataContract]
	public class VerifyObjectNameTestData
	{
		[DataMember]
		string Member1 = "foo";
	}

	[Serializable]
	public class KeyValueTestData {
		public List<KeyValuePair<string,string>> TestData = new List<KeyValuePair<string,string>>();
	}

	[DataContract] // bug #586169
	public class Query
	{
		[DataMember (Order=1)]
		public DateTime StartDate { get; set; }
		[DataMember (Order=2)]
		public DateTime EndDate { get; set; }
	}

	public class ClassA {
		public ClassB B { get; set; }
	}

	public class ClassB
	{
	}

	public class GSPlayerListErg
	{
		public GSPlayerListErg ()
		{
			Init ();
		}

		void Init ()
		{
			C = true;
			ServerTimeUTC = DateTime.SpecifyKind (DateTime.MinValue, DateTimeKind.Utc);
		}

		[OnDeserializing]
		public void OnDeserializing (StreamingContext c)
		{
			A = true;
			Init ();
		}

		[OnDeserialized]
		void OnDeserialized (StreamingContext c)
		{
			B = true;
		}

		public static bool A, B, C;

		[DataMember (Name = "T")]
		public long CodedServerTimeUTC { get; set; }
		public DateTime ServerTimeUTC { get; set; }
	}

	#region polymorphism test helper classes

	[DataContract]
	[KnownType (typeof (KnownDerivedType))]
	public class ParentType
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public BaseType PolymorphicProperty { get; set; }
	}

	[DataContract]
	public class BaseType
	{
		[DataMember]
		public string BaseTypeProperty { get; set; }
	}

	[DataContract]
	public class KnownDerivedType : BaseType
	{
		[DataMemberAttribute]
		public string DerivedProperty { get; set; }
	}

	[DataContract]
	public class UnknownDerivedType : BaseType
	{
		[DataMember]
		public string DerivedProperty { get; set; }
	}

	#endregion
}

[DataContract]
class JsonGlobalSample1
{
}


public class JsonMyDictionary<K, V> : System.Collections.Generic.IDictionary<K, V>
{
	Dictionary<K,V> dic = new Dictionary<K,V> ();

	public void Add (K key, V value)
	{
		dic.Add (key,  value);
	}

	public bool ContainsKey (K key)
	{
		return dic.ContainsKey (key);
	}

	public ICollection<K> Keys {
		get { return dic.Keys; }
	}

	public bool Remove (K key)
	{
		return dic.Remove (key);
	}

	public bool TryGetValue (K key, out V value)
	{
		return dic.TryGetValue (key, out value);
	}

	public ICollection<V> Values {
		get { return dic.Values; }
	}

	public V this [K key] {
		get { return dic [key]; }
		set { dic [key] = value; }
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return dic.GetEnumerator ();
	}

	ICollection<KeyValuePair<K,V>> Coll {
		get { return (ICollection<KeyValuePair<K,V>>) dic; }
	}

	public void Add (KeyValuePair<K, V> item)
	{
		Coll.Add (item);
	}

	public void Clear ()
	{
		dic.Clear ();
	}

	public bool Contains (KeyValuePair<K, V> item)
	{
		return Coll.Contains (item);
	}

	public void CopyTo (KeyValuePair<K, V> [] array, int arrayIndex)
	{
		Coll.CopyTo (array, arrayIndex);
	}

	public int Count {
		get { return dic.Count; }
	}

	public bool IsReadOnly {
		get { return Coll.IsReadOnly; }
	}

	public bool Remove (KeyValuePair<K, V> item)
	{
		return Coll.Remove (item);
	}

	public IEnumerator<KeyValuePair<K, V>> GetEnumerator ()
	{
		return Coll.GetEnumerator ();
	}
}

public class MyExplicitDictionary<K, V> : IDictionary<K, V> {

	Dictionary<K,V> dic = new Dictionary<K,V> ();

	public void Add (K key, V value)
	{
		dic.Add (key,  value);
	}

	public bool ContainsKey (K key)
	{
		return dic.ContainsKey (key);
	}

	ICollection<K> IDictionary<K, V>.Keys {
		get { return dic.Keys; }
	}

	public bool Remove (K key)
	{
		return dic.Remove (key);
	}

	public bool TryGetValue (K key, out V value)
	{
		return dic.TryGetValue (key, out value);
	}

	ICollection<V> IDictionary<K, V>.Values {
		get { return dic.Values; }
	}

	public V this [K key] {
		get { return dic [key]; }
		set { dic [key] = value; }
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return dic.GetEnumerator ();
	}

	ICollection<KeyValuePair<K,V>> Coll {
		get { return (ICollection<KeyValuePair<K,V>>) dic; }
	}

	public void Add (KeyValuePair<K, V> item)
	{
		Coll.Add (item);
	}

	public void Clear ()
	{
		dic.Clear ();
	}

	public bool Contains (KeyValuePair<K, V> item)
	{
		return Coll.Contains (item);
	}

	public void CopyTo (KeyValuePair<K, V> [] array, int arrayIndex)
	{
		Coll.CopyTo (array, arrayIndex);
	}

	public int Count {
		get { return dic.Count; }
	}

	public bool IsReadOnly {
		get { return Coll.IsReadOnly; }
	}

	public bool Remove (KeyValuePair<K, V> item)
	{
		return Coll.Remove (item);
	}

	public IEnumerator<KeyValuePair<K, V>> GetEnumerator ()
	{
		return Coll.GetEnumerator ();
	}
}

[DataContract]
public class Bug13485Type
{
	[DataMember]
	public string Name { get; set; }

	[DataMember (Name = "Value")]
	private string Value { get; set; }

	public string GetValue { get { return this.Value; } }
}

#region Test classes for Collection serialization

[DataContract]
	public abstract class CollectionContainer <V>
	{
		V items;

		[DataMember]
		public V Items
		{
			get {
				if (items == null) items = Init ();
				return items;
			}
		}
		
		public CollectionContainer ()
		{
			Init ();
		}
	
		protected abstract V Init ();
	}
	
	[DataContract]
	public class ArrayListContainer : CollectionContainer<ArrayList> {
		protected override ArrayList Init ()
		{
			return new ArrayList { "banana", "apple" };
		}
	}
	
	[DataContract]
	public class BitArrayContainer : CollectionContainer<BitArray> {
		protected override BitArray Init ()
		{
			return new BitArray (new [] { false, true });
		}
	}
	
	[DataContract]
	public class HashtableContainer : CollectionContainer<Hashtable> {
		protected override Hashtable Init ()
		{
			var ht = new Hashtable ();
			ht.Add ("key1", "banana");
			ht.Add ("key2", "apple");
			return ht;
		}
	}
	
	[DataContract]
	public class QueueContainer : CollectionContainer<Queue> {
		protected override Queue Init ()
		{
			var q = new Queue ();
			q.Enqueue ("banana");
			q.Enqueue ("apple");
			return q;
		}
	}
	
	[DataContract]
	public class SortedListContainer : CollectionContainer<SortedList> {
		protected override SortedList Init ()
		{
			var l = new SortedList ();
			l.Add ("key1", "banana");
			l.Add ("key2", "apple");
			return l;
		}
	}
	
	[DataContract]
	public class StackContainer : CollectionContainer<Stack> {
		protected override Stack Init ()
		{
			var s = new Stack ();
			s.Push ("banana");
			s.Push ("apple");
			return s;
		}
	}

	public class EnumerableWithAdd : IEnumerable
	{
		private ArrayList items;

		public EnumerableWithAdd()
		{
			items = new ArrayList();
		}

		public IEnumerator GetEnumerator()
		{
			return items.GetEnumerator();
		}

		public void Add(object value)
		{
			items.Add(value);
		}

		public int Count
		{
			get {
				return items.Count;
			}
		}
	}

	public class EnumerableWithSpecialAdd : IEnumerable
	{
		private ArrayList items;

		public EnumerableWithSpecialAdd()
		{
			items = new ArrayList();
		}

		public IEnumerator GetEnumerator()
		{
			return items.GetEnumerator();
		}

		public void Add(object value, int index)
		{
			items.Add(value);
		}

		public int Count
		{
			get
			{
				return items.Count;
			}
		}
	}

	[DataContract]
	public class EnumerableWithAddContainer : CollectionContainer<EnumerableWithAdd>
	{
		protected override EnumerableWithAdd Init()
		{
			var s = new EnumerableWithAdd();
			s.Add ("banana");
			s.Add ("apple");
			return s;
		}
	}

	[DataContract]
	public class EnumerableWithSpecialAddContainer : CollectionContainer<EnumerableWithSpecialAdd>
	{
		protected override EnumerableWithSpecialAdd Init()
		{
			var s = new EnumerableWithSpecialAdd();
			s.Add("banana", 0);
			s.Add("apple", 0);
			return s;
		}
	}

	[DataContract]
	public class GenericDictionaryContainer : CollectionContainer<Dictionary<string, string>> {
		protected override Dictionary<string, string> Init ()
		{
			var d = new Dictionary<string, string> ();
			d.Add ("key1", "banana");
			d.Add ("key2", "apple");
			return d;
		}
	}

	[DataContract]
	public class GenericHashSetContainer : CollectionContainer<HashSet<string>> {
		protected override HashSet<string> Init ()
		{
			return new HashSet<string> { "banana", "apple" };
		}
	}

	[DataContract]
	public class GenericLinkedListContainer : CollectionContainer<LinkedList<string>> {
		protected override LinkedList<string> Init ()
		{
			var l = new LinkedList<string> ();
			l.AddFirst ("apple");
			l.AddFirst ("banana");
			return l;
		}
	}

	[DataContract]
	public class GenericListContainer : CollectionContainer<List<string>> {
		protected override List<string> Init ()
		{
			return new List<string> { "banana", "apple" };
		}
	}

	[DataContract]
	public class GenericQueueContainer : CollectionContainer<Queue<string>> {
		protected override Queue<string> Init ()
		{
			var q = new Queue<string> ();
			q.Enqueue ("banana");
			q.Enqueue ("apple" );
			return q;
		}
	}

	[DataContract]
	public class GenericSortedDictionaryContainer : CollectionContainer<SortedDictionary<string, string>> {
		protected override SortedDictionary<string, string> Init ()
		{
			var d = new SortedDictionary<string, string> ();
			d.Add ("key1", "banana");
			d.Add ("key2", "apple");
			return d;
		}
	}

	[DataContract]
	public class GenericSortedListContainer : CollectionContainer<SortedList<string, string>> {
		protected override SortedList<string, string> Init ()
		{
			var d = new SortedList<string, string> ();
			d.Add ("key1", "banana");
			d.Add ("key2", "apple");
			return d;
		}
	}

	[DataContract]
	public class GenericStackContainer : CollectionContainer<Stack<string>> {
		protected override Stack<string> Init ()
		{
			var s = new Stack<string> ();
			s.Push ("banana");
			s.Push ("apple" );
			return s;
		}
	}

	public class GenericEnumerableWithAdd : IEnumerable<string>
	{
		private List<string> items;

		public GenericEnumerableWithAdd()
		{
			items = new List<string>();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator ();
		}

		public IEnumerator<string> GetEnumerator()
		{
			return items.GetEnumerator ();
		}

		public void Add(string value)
		{
			items.Add(value);
		}

		public int Count
		{
			get {
				return items.Count;
			}
		}
	}

	public class GenericEnumerableWithSpecialAdd : IEnumerable<string>
	{
		private List<string> items;

		public GenericEnumerableWithSpecialAdd()
		{
			items = new List<string>();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator ();
		}

		public IEnumerator<string> GetEnumerator()
		{
			return items.GetEnumerator ();
		}

		public void Add(string value, int index)
		{
			items.Add(value);
		}

		public int Count
		{
			get
			{
				return items.Count;
			}
		}
	}

	[DataContract]
	public class GenericEnumerableWithAddContainer : CollectionContainer<GenericEnumerableWithAdd>
	{
		protected override GenericEnumerableWithAdd Init()
		{
			var s = new GenericEnumerableWithAdd();
			s.Add ("banana");
			s.Add ("apple");
			return s;
		}
	}

	[DataContract]
	public class GenericEnumerableWithSpecialAddContainer : CollectionContainer<GenericEnumerableWithSpecialAdd>
	{
		protected override GenericEnumerableWithSpecialAdd Init()
		{
			var s = new GenericEnumerableWithSpecialAdd();
			s.Add("banana", 0);
			s.Add("apple", 0);
			return s;
		}
	}	

	[DataContract]
	public class NonCollectionGetOnlyContainer
	{
		string _test = "my string";
	
		[DataMember]
		public string MyString {
			get {
				return _test;
			}
		}
	}	

#endregion

#region DefaultValueDeserialization
    [DataContract]
    public class JsonPerson
    {
        [DataMember(EmitDefaultValue = false)]
        public string name { get; set; }
    }

    [DataContract]
    public class PersonWithContact
    {
        [DataMember(EmitDefaultValue = false)]
        public string name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Contact contact { get; set; }
    }

    [DataContract]
    public class Contact
    {
        [DataMember(EmitDefaultValue = false)]
        public string url { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string email{ get; set; }
    }
#endregion
