//
// DataObjectTest.cs - NUnit Test Cases for DataObject
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.Xml {

	[TestFixture]
	public class DataObjectTest {

		[Test]
		public void NewDataObject () 
		{
			string test = "<Test>DataObject</Test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);

			DataObject obj1 = new DataObject ();
			Assertion.Assert ("Data.Count==0", (obj1.Data.Count == 0));

			obj1.Id = "id";
			obj1.MimeType = "mime";
			obj1.Encoding = "encoding";
			Assertion.AssertEquals ("Only attributes", "<Object Id=\"id\" MimeType=\"mime\" Encoding=\"encoding\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (obj1.GetXml ().OuterXml));

			obj1.Data = doc.ChildNodes;
			Assertion.Assert ("Data.Count==1", (obj1.Data.Count == 1));

			XmlElement xel = obj1.GetXml ();

			DataObject obj2 = new DataObject ();
			obj2.LoadXml (xel);
			Assertion.AssertEquals ("obj1==obj2", (obj1.GetXml ().OuterXml), (obj2.GetXml ().OuterXml));

			DataObject obj3 = new DataObject (obj1.Id, obj1.MimeType, obj1.Encoding, doc.DocumentElement);
			Assertion.AssertEquals ("obj2==obj3", (obj2.GetXml ().OuterXml), (obj3.GetXml ().OuterXml));
		}

		[Test]
		public void ImportDataObject () 
		{
			string value1 = "<Object Id=\"id\" MimeType=\"mime\" Encoding=\"encoding\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Test xmlns=\"\">DataObject1</Test><Test xmlns=\"\">DataObject2</Test></Object>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (value1);

			DataObject obj1 = new DataObject ();
			obj1.LoadXml (doc.DocumentElement);
			Assertion.Assert ("Data.Count==2", (obj1.Data.Count == 2));

			string s = (obj1.GetXml ().OuterXml);
			Assertion.AssertEquals ("DataObject 1", value1, s);

			string value2 = "<Object xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Test xmlns=\"\" /></Object>";
			doc = new XmlDocument ();
			doc.LoadXml (value2);

			DataObject obj2 = new DataObject ();
			obj2.LoadXml (doc.DocumentElement);

			s = (obj2.GetXml ().OuterXml);
			Assertion.AssertEquals ("DataObject 2", value2, s);

			string value3 = "<Object Id=\"id\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Test xmlns=\"\" /></Object>";
			doc = new XmlDocument ();
			doc.LoadXml (value3);

			DataObject obj3 = new DataObject ();
			obj3.LoadXml (doc.DocumentElement);

			s = (obj3.GetXml ().OuterXml);
			Assertion.AssertEquals ("DataObject 3", value3, s);

			string value4 = "<Object MimeType=\"mime\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Test xmlns=\"\" /></Object>";
			doc = new XmlDocument ();
			doc.LoadXml (value4);

			DataObject obj4 = new DataObject ();
			obj4.LoadXml (doc.DocumentElement);

			s = (obj4.GetXml ().OuterXml);
			Assertion.AssertEquals ("DataObject 4", value4, s);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InvalidDataObject1 () 
		{
			DataObject obj1 = new DataObject ();
			obj1.Data = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InvalidDataObject2 () 
		{
			DataObject obj1 = new DataObject ();
			obj1.LoadXml (null);
		}

		[Test]
		public void InvalidDataObject3 () 
		{
			DataObject obj1 = new DataObject ();
			// seems this isn't invalid !?!
			// but no exception is thrown
			string value = "<Test>Bad</Test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (value);
			obj1.LoadXml (doc.DocumentElement);
			string s = (obj1.GetXml ().OuterXml);
			Assertion.AssertEquals ("DataObject Bad", value, s);
		}
	}
}