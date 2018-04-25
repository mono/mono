// 
// System.Web.Services.Protocols.SoapHttpClientProtocolTest.cs
//
// Author:
//   Gert Driesen <drieseng@users.sourceforge.net>
//
// (C) 2007 Gert Driesen
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

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Web.Services.Protocols
{
	[TestFixture]
	public class SoapHttpClientProtocolTest
	{
		[Ignore ("this kind of connection oriented tests got non-working after some Windows updates in .NET 2.0 (1.1 still works).")]
		[Test] // bug #79988
		public void OutParametersTest ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 5000);
			using (SocketResponder sr = new SocketResponder (localEP, s => OutParametersResponse (s))) {
				FooService service = new FooService ();
				service.Url = "http://" + IPAddress.Loopback.ToString () + ":5000/";

				int a;
				bool b;
				Elem [] e = service.Req ("x", out a, out b);
				Assert.IsNull (e, "#A1");
				Assert.AreEqual (0, a, "#A2");
				Assert.IsFalse (b, "#A3");
				service.Dispose ();
			}
		}

		[Ignore ("this kind of connection oriented tests got non-working after some Windows updates in .NET 2.0 (1.1 still works).")]
		[Test] // bug #81886
		public void FaultTest ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 5000);
			using (SocketResponder sr = new SocketResponder (localEP, s => FaultResponse_Qualified (s))) {
				FooService service = new FooService ();
				service.Url = "http://" + IPAddress.Loopback.ToString () + ":5000/";
				try {
					service.Run ();
					Assert.Fail ("#A1");
				} catch (SoapException ex) {
					Assert.AreEqual ("Mono Web Service", ex.Actor, "#A2");
					Assert.AreEqual (SoapException.ServerFaultCode, ex.Code, "#A3");
					Assert.IsNotNull (ex.Detail, "#A4");
					Assert.AreEqual ("detail", ex.Detail.LocalName, "#A5");
					Assert.AreEqual ("http://schemas.xmlsoap.org/soap/envelope/", ex.Detail.NamespaceURI, "#A6");

					XmlNamespaceManager nsMgr = new XmlNamespaceManager (ex.Detail.OwnerDocument.NameTable);
					nsMgr.AddNamespace ("se", "http://www.mono-project/System");

					XmlElement systemError = (XmlElement) ex.Detail.SelectSingleNode (
						"se:systemerror", nsMgr);
					Assert.IsNotNull (systemError, "#A7");
					Assert.IsNull (ex.InnerException, "#A8");
					Assert.AreEqual ("Failure processing request.", ex.Message, "#A9");
				}
				service.Dispose ();
			}

			using (SocketResponder sr = new SocketResponder (localEP, s => FaultResponse_Unqualified (s))) {
				FooService service = new FooService ();
				service.Url = "http://" + IPAddress.Loopback.ToString () + ":5000/";
				try {
					service.Run ();
					Assert.Fail ("#B1");
				} catch (SoapException ex) {
					Assert.AreEqual ("Mono Web Service", ex.Actor, "#B2");
					Assert.AreEqual (SoapException.ServerFaultCode, ex.Code, "#B3");
					Assert.IsNotNull (ex.Detail, "#B4");
					Assert.AreEqual ("detail", ex.Detail.LocalName, "#B5");
					Assert.AreEqual (string.Empty, ex.Detail.NamespaceURI, "#B6");

					XmlNamespaceManager nsMgr = new XmlNamespaceManager (ex.Detail.OwnerDocument.NameTable);
					nsMgr.AddNamespace ("se", "http://www.mono-project/System");

					XmlElement systemError = (XmlElement) ex.Detail.SelectSingleNode (
						"se:systemerror", nsMgr);
					Assert.IsNotNull (systemError, "#B7");
					Assert.IsNull (ex.InnerException, "#B8");
					Assert.AreEqual ("Failure processing request.", ex.Message, "#B9");
				}
				service.Dispose ();
			}
		}

		static byte [] FaultResponse_Qualified (Socket socket)
		{
			string responseContent =
				"<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
				"  <soap:Body>" +
				"    <soap:Fault>" +
				"      <soap:faultcode>soap:Server</soap:faultcode>" +
				"      <soap:faultstring>Failure processing request.</soap:faultstring>" +
				"      <soap:faultactor>Mono Web Service</soap:faultactor>" +
				"      <soap:detail>" +
				"        <se:systemerror xmlns:se=\"http://www.mono-project/System\">" +
				"          <se:code>5000</se:code>" +
				"          <se:description>Invalid credentials.</se:description>" +
				"        </se:systemerror>" +
				"      </soap:detail>" +
				"    </soap:Fault>" +
				"  </soap:Body>" +
				"</soap:Envelope>";

			StringWriter sw = new StringWriter ();
			sw.WriteLine ("HTTP/1.1 200 OK");
			sw.WriteLine ("Content-Type: text/xml");
			sw.WriteLine ("Content-Length: " + responseContent.Length);
			sw.WriteLine ();
			sw.Write (responseContent);
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		static byte [] FaultResponse_Unqualified (Socket socket)
		{
			string responseContent =
				"<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
				"  <soap:Body>" +
				"    <soap:Fault>" +
				"      <faultcode>soap:Server</faultcode>" +
				"      <faultstring>Failure processing request.</faultstring>" +
				"      <faultactor>Mono Web Service</faultactor>" +
				"      <detail>" +
				"        <se:systemerror xmlns:se=\"http://www.mono-project/System\">" +
				"          <se:code>5000</se:code>" +
				"          <se:description>Invalid credentials.</se:description>" +
				"        </se:systemerror>" +
				"      </detail>" +
				"    </soap:Fault>" +
				"  </soap:Body>" +
				"</soap:Envelope>";

			StringWriter sw = new StringWriter ();
			sw.WriteLine ("HTTP/1.1 200 OK");
			sw.WriteLine ("Content-Type: text/xml");
			sw.WriteLine ("Content-Length: " + responseContent.Length);
			sw.WriteLine ();
			sw.Write (responseContent);
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		static byte [] OutParametersResponse (Socket socket)
		{
			string responseContent =
				"<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
				"  <soap:Body>" +
				"    <ReqResponse2 xmlns=\"urn:foo\">" +
				"      <Hits>ERERE</Hits>" +
				"    </ReqResponse2>" +
				"  </soap:Body>" +
				"</soap:Envelope>";

			StringWriter sw = new StringWriter ();
			sw.WriteLine ("HTTP/1.1 200 OK");
			sw.WriteLine ("Content-Type: text/xml");
			sw.WriteLine ("Content-Length: " + responseContent.Length);
			sw.WriteLine ();
			sw.Write (responseContent);
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		[WebServiceBindingAttribute (Name = "Foo", Namespace = "urn:foo")]
		public class FooService : SoapHttpClientProtocol
		{
			[SoapDocumentMethodAttribute ("", RequestElementName = "Req", RequestNamespace = "urn:foo", ResponseNamespace = "urn:foo", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
			[return: XmlElementAttribute ("Hits")]
			public Elem [] Req ([XmlAttributeAttribute ()] string arg, [XmlAttributeAttribute ()] out int status, [XmlAttributeAttribute ()] [XmlIgnoreAttribute ()] out bool statusSpecified)
			{
				object [] results = this.Invoke ("Req", new object [] { arg });
				status = ((int) (results [1]));
				statusSpecified = ((bool) (results [2]));
				return ((Elem []) (results [0]));
			}

			[SoapDocumentMethodAttribute ("", RequestElementName = "Run", RequestNamespace = "urn:foo", ResponseNamespace = "urn:foo", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
			[return: XmlElementAttribute ("Hits")]
			public Elem Run ()
			{
				this.Invoke ("Run", new object [0]);
				return new Elem ();
			}
		}

		[SerializableAttribute ()]
		public class Elem
		{
			private string attrField;

			[XmlAttributeAttribute ()]
			public string attr
			{
				get
				{
					return this.attrField;
				}
				set
				{
					this.attrField = value;
				}
			}
		}

		public class RequestHeader : SoapHeader
		{
		}

		public class ResponseHeader : SoapHeader
		{
		}

		[WebServiceBindingAttribute(Name = "ServiceWithHeaders", Namespace = "https://example.com")]
		public class ServiceWithHeaders : SoapHttpClientProtocol
		{
			public RequestHeader RequestHeader { get; set; }
			public ResponseHeader ResponseHeader { get; set; }

			[SoapHeaderAttribute("ResponseHeader", Direction = SoapHeaderDirection.Out)]
			[SoapHeaderAttribute("RequestHeader")]
			[SoapDocumentMethodAttribute("", RequestNamespace = "https://example.com", ResponseNamespace = "https://example.com", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
			public int method1()
			{
				return 0;
			}

			[SoapHeaderAttribute("ResponseHeader", Direction = SoapHeaderDirection.Out)]
			[SoapHeaderAttribute("RequestHeader")]
			[SoapDocumentMethodAttribute("", RequestNamespace = "https://example.com", ResponseNamespace = "https://example.com", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
			public int method2()
			{
				return 0;
			}
		}

		[Test] // Covers #41564
		public void ServiceWithHeader () {
			var service = new ServiceWithHeaders ();
			Assert.IsNotNull (service);
			// Should not throw an exception
			// XAMMAC specific bug
		}
	}
}
