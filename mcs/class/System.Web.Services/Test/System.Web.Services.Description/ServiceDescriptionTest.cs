//
// ServiceDescriptionTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.
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

		void CheckEA (DocumentableItem di, string att, string val)
		{
			Assert.IsNotNull (di.ExtensibleAttributes);
			
			for (int i = 0; i < di.ExtensibleAttributes.Length; i ++) {
				Assert.AreEqual (att, di.ExtensibleAttributes [i].Name);
				Assert.AreEqual (val, di.ExtensibleAttributes [i].Value);
			}
		}
#endif

	}
}
