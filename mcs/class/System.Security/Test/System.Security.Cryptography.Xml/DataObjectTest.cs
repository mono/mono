//
// DataObjectTest.cs - NUnit Test Cases for DataObject
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace MonoTests.System.Security.Cryptography.Xml {

public class DataObjectTest : TestCase {

	public DataObjectTest () : base ("System.Security.Cryptography.Xml.DataObject testsuite") {}
	public DataObjectTest (string name) : base (name) {}

	protected override void SetUp () {}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (DataObjectTest)); 
		}
	}

	public void TestNewDataObject () 
	{
		string test = "<Test>DataObject</Test>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (test);

		DataObject obj1 = new DataObject ();
		Assert ("Data.Count==0", (obj1.Data.Count == 0));

		obj1.Id = "id";
		obj1.MimeType = "mime";
		obj1.Encoding = "encoding";
		AssertEquals ("Only attributes", "<Object Id=\"id\" MimeType=\"mime\" Encoding=\"encoding\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (obj1.GetXml ().OuterXml));

		obj1.Data = doc.ChildNodes;
		Assert ("Data.Count==1", (obj1.Data.Count == 1));

		XmlElement xel = obj1.GetXml ();

		DataObject obj2 = new DataObject ();
		obj2.LoadXml (xel);
		AssertEquals ("obj1==obj2", (obj1.GetXml ().OuterXml), (obj2.GetXml ().OuterXml));

		DataObject obj3 = new DataObject (obj1.Id, obj1.MimeType, obj1.Encoding, doc.DocumentElement);
		AssertEquals ("obj2==obj3", (obj2.GetXml ().OuterXml), (obj3.GetXml ().OuterXml));
	}

	public void TestImportDataObject () 
	{
		string value1 = "<Object Id=\"id\" MimeType=\"mime\" Encoding=\"encoding\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Test xmlns=\"\">DataObject1</Test><Test xmlns=\"\">DataObject2</Test></Object>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value1);

		DataObject obj1 = new DataObject ();
		obj1.LoadXml (doc.DocumentElement);
		Assert ("Data.Count==2", (obj1.Data.Count == 2));

		string s = (obj1.GetXml ().OuterXml);
		AssertEquals ("DataObject 1", value1, s);

		string value2 = "<Object xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Test xmlns=\"\" /></Object>";
		doc = new XmlDocument ();
		doc.LoadXml (value2);

		DataObject obj2 = new DataObject ();
		obj2.LoadXml (doc.DocumentElement);

		s = (obj2.GetXml ().OuterXml);
		AssertEquals ("DataObject 2", value2, s);

		string value3 = "<Object Id=\"id\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Test xmlns=\"\" /></Object>";
		doc = new XmlDocument ();
		doc.LoadXml (value3);

		DataObject obj3 = new DataObject ();
		obj3.LoadXml (doc.DocumentElement);

		s = (obj3.GetXml ().OuterXml);
		AssertEquals ("DataObject 3", value3, s);

		string value4 = "<Object MimeType=\"mime\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Test xmlns=\"\" /></Object>";
		doc = new XmlDocument ();
		doc.LoadXml (value4);

		DataObject obj4 = new DataObject ();
		obj4.LoadXml (doc.DocumentElement);

		s = (obj4.GetXml ().OuterXml);
		AssertEquals ("DataObject 4", value4, s);
	}

	public void TestInvalidDataObject () 
	{
		DataObject obj1 = new DataObject ();
		try {
			obj1.Data = null;
			Fail ("Expected ArgumentNullException but none");
		}
		catch (ArgumentNullException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}

		try {
			obj1.LoadXml (null);
			Fail ("Expected ArgumentNullException but none");
		}
		catch (ArgumentNullException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}

		// seems this isn't invalid !?!
		string value = "<Test>Bad</Test>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);
		obj1.LoadXml (doc.DocumentElement);
		string s = (obj1.GetXml ().OuterXml);
		AssertEquals ("DataObject Bad", value, s);
	}
}

}