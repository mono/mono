//
// EndpointAddress10Test.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
#if !MOBILE
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class EndpointAddress10Test
	{
		[Test]
		public void ReadWriteXml ()
		{
			StringWriter sw = new StringWriter ();

			EndpointAddress10 e = EndpointAddress10.FromEndpointAddress (new EndpointAddress ("http://localhost:8080"));

			using (XmlWriter xw = XmlWriter.Create (sw)) {
				((IXmlSerializable) e).WriteXml (xw);
			}
			Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?><Address xmlns=""http://www.w3.org/2005/08/addressing"">http://localhost:8080/</Address>", sw.ToString ());

			// unlike WriteXml, ReadXml expects the root element.
			StringReader sr = new StringReader (@"<EndpointReference xmlns=""http://www.w3.org/2005/08/addressing""><Address>http://localhost:8080/</Address></EndpointReference>");
			using (XmlReader xr = XmlReader.Create (sr)) {
				((IXmlSerializable) e).ReadXml (xr);
			}

			sr = new StringReader (@"<EndpointReference xmlns=""http://www.w3.org/2005/08/addressing""><Address>http://localhost:8080/</Address></EndpointReference>");
			using (XmlReader xr = XmlReader.Create (sr))
				EndpointAddress.ReadFrom (AddressingVersion.WSAddressing10, xr);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadWriteXml2 ()
		{
			var sr = new StringReader (@"<Address>http://localhost:8080/</Address>");
			using (XmlReader xr = XmlReader.Create (sr))
				EndpointAddress.ReadFrom (AddressingVersion.WSAddressing10, xr);
		}

		[Test]
		public void SerializeDeserialize ()
		{
			StringWriter sw = new StringWriter ();

			EndpointAddress10 e = EndpointAddress10.FromEndpointAddress (new EndpointAddress ("http://localhost:8080"));

			XmlSerializer xs = new XmlSerializer (typeof (EndpointAddress10));

			sw = new StringWriter ();

			using (XmlWriter xw = XmlWriter.Create (sw)) {
				xs.Serialize (xw, e);
			}
			Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?><EndpointReference xmlns=""http://www.w3.org/2005/08/addressing""><Address>http://localhost:8080/</Address></EndpointReference>", sw.ToString ());
			StringReader sr = new StringReader (sw.ToString ());
			using (XmlReader xr = XmlReader.Create (sr)) {
				xs.Deserialize (xr);
			}
		}

		[Test]
		public void GetSchema ()
		{
			// actually it just returns null. That makes sense
			// since there's no way to include reasonable claim
			// schemas.
			EndpointAddress10.FromEndpointAddress (new EndpointAddress ("http://localhost:8080"));
			XmlSchemaSet xss = new XmlSchemaSet ();
			XmlQualifiedName q = EndpointAddress10.GetSchema (xss);
			Assert.AreEqual (1, xss.Count, "#1");
			Assert.AreEqual ("EndpointReferenceType", q.Name, "#2");
			Assert.AreEqual ("http://www.w3.org/2005/08/addressing", q.Namespace, "#2");
			foreach (XmlSchema xs in xss.Schemas ()) {
				Assert.AreEqual ("http://www.w3.org/2005/08/addressing", xs.TargetNamespace, "#4");
			}
		}

		[Test]
		public void IXmlSerializableGetSchema ()
		{
			// actually it just returns null.
			EndpointAddress10 e = EndpointAddress10.FromEndpointAddress (new EndpointAddress ("http://localhost:8080"));
			XmlSchema xs = ((IXmlSerializable) e).GetSchema ();
			Assert.IsNull (xs);
		}
	}
}
#endif
