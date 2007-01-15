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
using System.Net;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

using NUnit.Framework;

namespace System.Web.Services.Protocols
{
	[TestFixture]
	public class SoapHttpClientProtocolTest
	{
		[Test] // bug #79988
		public void OutParametersTest ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 5000);
			using (SocketResponder sr = new SocketResponder (localEP, new SocketRequestHandler (Response_OutParametersTest))) {
				sr.Start ();

				FooService service = new FooService ();
				service.Url = "http://" + IPAddress.Loopback.ToString () + ":5000/";

				int a;
				bool b;
				Elem [] e = service.Req ("x", out a, out b);
				Assert.IsNull (e, "#A1");
				Assert.AreEqual (0, a, "#A2");
				Assert.IsFalse (b, "#A3");
			}
		}

		static string Response_OutParametersTest ()
		{
			return "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
				"<soap:Body>" +
				"<ReqResponse2 xmlns=\"urn:foo\">" +
				"<Hits>ERERE</Hits>" +
				"</ReqResponse2>" +
				"</soap:Body>" +
				"</soap:Envelope>";
		}

		[WebServiceBindingAttribute (Name = "Foo", Namespace = "urn:foo")]
		public class FooService : SoapHttpClientProtocol
		{
			[SoapDocumentMethodAttribute ("", RequestElementName = "Req", RequestNamespace = "urn:foo", ResponseNamespace = "urn:foo", Use = SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
			[return: XmlElementAttribute ("Hits")]
			public Elem [] Req ([XmlAttributeAttribute ()] string arg, [XmlAttributeAttribute ()] out int status, [XmlAttributeAttribute ()] [XmlIgnoreAttribute ()] out bool statusSpecified)
			{
				object [] results = this.Invoke ("Req", new object [] { arg });
				status = ((int) (results [1]));
				statusSpecified = ((bool) (results [2]));
				return ((Elem []) (results [0]));
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
	}
}
