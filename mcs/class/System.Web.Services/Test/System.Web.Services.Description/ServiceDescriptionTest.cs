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

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class ServiceDescriptionTest
	{
		[Test]
		public void SimpleWrite ()
		{
			ServiceDescription sd = new ServiceDescription ();
			Assert.IsNull (sd.Name);
			sd.Write (TextWriter.Null);
		}

#if NET_2_0
		[Test]
		public void ExtensibleAttributes ()
		{
		    FileStream fs = new FileStream("Test/System.Web.Services.Description/test.wsdl", FileMode.Open);
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
		    FileStream fs = new FileStream("Test/System.Web.Services.Description/test.wsdl", FileMode.Open);
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

#endif

	}
}
