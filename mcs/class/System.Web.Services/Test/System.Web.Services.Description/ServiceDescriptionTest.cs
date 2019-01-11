//
// ServiceDescriptionTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain  <jankit@novell.com>
//
// Copyright (C) 2005 Novell, Inc.
// Copyright (C) 2006 Novell, Inc.
//

using NUnit.Framework;

using System;
using System.IO;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;

using MonoTests.Helpers;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class ServiceDescriptionTest
	{
		[Test]
		[Category ("MacNotWorking")] // https://bugzilla.xamarin.com/show_bug.cgi?id=51254
		public void SimpleWrite ()
		{
			ServiceDescription sd = new ServiceDescription ();
			Assert.IsNull (sd.Name);
			sd.Write (TextWriter.Null);
		}

		[Test]
		public void Ctor ()
		{
			ServiceDescription sd = new ServiceDescription ();
			Assert.IsNotNull (sd.Bindings);
			Assert.IsNotNull (sd.Extensions);
			Assert.IsNotNull (sd.Imports);
			Assert.IsNotNull (sd.Messages);
			Assert.IsNotNull (sd.PortTypes);
			Assert.IsNotNull (sd.Services);
			Assert.IsNotNull (sd.Types);

			Assert.IsNull (sd.ServiceDescriptions);
			Assert.IsNull (sd.TargetNamespace);
		}

		[Test]
		public void ReadAndRetrievalUrl ()
		{
			Assert.AreEqual (String.Empty, new ServiceDescription ().RetrievalUrl, "#1");
			ServiceDescription sd = ServiceDescription.Read (TestResourceHelper.GetFullPathOfResource ("Test/System.Web.Services.Description/test2.wsdl"));
			Assert.AreEqual (String.Empty, sd.RetrievalUrl, "#2");
		}

		[Test]
		public void Namespaces ()
		{
			FileStream fs = new FileStream (TestResourceHelper.GetFullPathOfResource ("Test/System.Web.Services.Description/test.wsdl"), FileMode.Open, FileAccess.Read);
			XmlTextReader xtr = new XmlTextReader (fs);

			ServiceDescription sd = ServiceDescription.Read (xtr);
			fs.Close ();

			Assert.IsNotNull (sd.Namespaces);
			Assert.AreEqual (8, sd.Namespaces.Count, "#n0");

			ArrayList list = new ArrayList (sd.Namespaces.ToArray ());
			list.Sort (new qname_comparer ());

			Assert.AreEqual (new XmlQualifiedName ("", "http://schemas.xmlsoap.org/wsdl/"), list [0]);
			Assert.AreEqual (new XmlQualifiedName ("http", "http://schemas.xmlsoap.org/wsdl/http/"), list [1]);
			Assert.AreEqual (new XmlQualifiedName ("mime", "http://schemas.xmlsoap.org/wsdl/mime/"), list [2]);
			Assert.AreEqual (new XmlQualifiedName ("s", "http://www.w3.org/2001/XMLSchema"), list [3]);
			Assert.AreEqual (new XmlQualifiedName ("s0", "http://tempuri.org/"), list [4]);
			Assert.AreEqual (new XmlQualifiedName ("soap", "http://schemas.xmlsoap.org/wsdl/soap/"), list [5]);
			Assert.AreEqual (new XmlQualifiedName ("soapenc", "http://schemas.xmlsoap.org/soap/encoding/"), list [6]);
			Assert.AreEqual (new XmlQualifiedName ("tm", "http://microsoft.com/wsdl/mime/textMatching/"), list [7]);
		}

		[Test]
		public void ExtensibleAttributes ()
		{
		    FileStream fs = new FileStream (TestResourceHelper.GetFullPathOfResource ("Test/System.Web.Services.Description/test.wsdl"), FileMode.Open, FileAccess.Read);
		    XmlTextReader xtr = new XmlTextReader(fs);

		    ServiceDescription sd = ServiceDescription.Read(xtr);
		    CheckEA (sd, "sdAtt", "sdVal");
		    CheckEA (sd.Messages [0], "msgAtt", "msgVal");
		    CheckEA (sd.Messages [0].Parts [0], "partAtt", "partVal");

		    CheckEA (sd.PortTypes [0], "ptAtt", "ptVal");
		    CheckEA (sd.PortTypes [0].Operations [0], "opAtt", "opVal");
		    CheckEA (sd.PortTypes [0].Operations [0].Messages[0], "opmsgAtt", "opmsgVal");

		    CheckEA (sd.Services [0], "svcAtt", "svcVal");
		    CheckEA (sd.Services [0].Ports [0], "portAtt", "portVal");

		    fs.Close ();
		}

		[Test]
		public void Extensions ()
		{
			FileStream fs = new FileStream(TestResourceHelper.GetFullPathOfResource ("Test/System.Web.Services.Description/test.wsdl"), FileMode.Open, FileAccess.Read);
		    XmlTextReader xtr = new XmlTextReader(fs);

		    ServiceDescription sd = ServiceDescription.Read(xtr);
		    fs.Close ();

		    Assert.IsNotNull (sd.Extensions);
		    Assert.AreEqual (1, sd.Extensions.Count);

		    CheckExtensions (sd, "sdElem", "sdVal");
		    CheckExtensions (sd.Messages [0], "msgElem", "msgVal");
		    CheckExtensions (sd.Messages [0].Parts [0], "partElem", "partVal");

		    CheckExtensions (sd.PortTypes [0], "ptElem", "ptVal");
		    CheckExtensions (sd.PortTypes [0].Operations [0], "opElem", "opVal");

		    //Binding [0]
		    Assert.IsNotNull (sd.Bindings [0].Extensions);
		    Assert.AreEqual (2, sd.Bindings [0].Extensions.Count);
		    CheckXmlElement (sd.Bindings [0].Extensions [0], "binElem");
		    Assert.AreEqual (typeof (SoapBinding), sd.Bindings [0].Extensions [1].GetType ());
		
			//Binding [0].Operations [0]
			Assert.IsNotNull (sd.Bindings [0].Operations [0].Extensions);
			Assert.AreEqual (1, sd.Bindings [0].Operations [0].Extensions.Count);
			Assert.AreEqual (typeof (SoapOperationBinding), sd.Bindings [0].Operations [0].Extensions [0].GetType ());

		    //Service
		    CheckExtensions (sd.Services [0], "svcElem", "svcVal");

		    //Service.Port
		    Assert.IsNotNull (sd.Services [0].Ports [0].Extensions);
		    Assert.AreEqual (2, sd.Services [0].Ports [0].Extensions.Count);
		    Assert.AreEqual (typeof (SoapAddressBinding), sd.Services [0].Ports [0].Extensions [0].GetType ());	
		    CheckXmlElement (sd.Services [0].Ports [0].Extensions [1], "portElem");

		    string out_file = Path.GetTempFileName ();
		    try {
			    using (FileStream out_fs = new FileStream(out_file, FileMode.Create))
				    sd.Write (out_fs);
		    } finally {
			    if (!String.IsNullOrEmpty (out_file))
				    File.Delete (out_file);
		    }
		}

		void CheckExtensions (DocumentableItem di, string elemName, string val)
		{
			Assert.IsNotNull (di.Extensions);

			Assert.AreEqual (1, di.Extensions.Count);

			Assert.AreEqual (typeof (XmlElement), di.Extensions [0].GetType ());
			Assert.AreEqual (elemName, ((XmlElement) di.Extensions [0]).Name);
			Assert.AreEqual (val, ((XmlElement) di.Extensions [0]).InnerText);
		}

		void CheckXmlElement (object o, string name)
		{
			Assert.AreEqual (typeof (XmlElement), o.GetType ());
			Assert.AreEqual (name, ((XmlElement) o).Name);
		}

		void CheckEA (DocumentableItem di, string att, string val)
		{
			Assert.IsNotNull (di.ExtensibleAttributes);
			
			Assert.AreEqual (1, di.ExtensibleAttributes.Length);
			Assert.AreEqual (att, di.ExtensibleAttributes [0].Name);
			Assert.AreEqual (val, di.ExtensibleAttributes [0].Value);
		}

		[Test]
		public void ReadInvalid ()
		{
			ServiceDescription sd = ServiceDescription.Read (XmlReader.Create (new StringReader ("<definitions xmlns='http://schemas.xmlsoap.org/wsdl/'><hoge/></definitions>")));
		}

		[Test]
		public void ValidatingRead ()
		{
			ServiceDescription sd = ServiceDescription.Read (XmlReader.Create (new StringReader ("<definitions xmlns='http://schemas.xmlsoap.org/wsdl/'><hoge/></definitions>")), true);
			Assert.IsTrue (sd.ValidationWarnings.Count > 0);
		}

    }

	class qname_comparer : IComparer
	{
		public int Compare (object x, object y)
		{
			XmlQualifiedName a = (XmlQualifiedName) x;
			XmlQualifiedName b = (XmlQualifiedName) y;

			return String.Compare (a.Name, b.Name);
		}
	}
}

