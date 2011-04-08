//
// WebOperationContextTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Xml;
using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.ServiceModel.Web
{
	[TestFixture]
	public class WebOperationContextTest
	{
		[Test]
		public void Current ()
		{
			Assert.IsNull (WebOperationContext.Current, "#1");
			var binding = new WebHttpBinding ();
			var address = new EndpointAddress ("http://localhost:37564");
			var ch = (IContextChannel) WebChannelFactory<IHogeService>.CreateChannel (binding, address);
			using (var ocs = new OperationContextScope (ch)) {
				Assert.IsNotNull (WebOperationContext.Current, "#2");
				Assert.IsNotNull (WebOperationContext.Current.OutgoingRequest, "#3");
				Assert.IsNotNull (WebOperationContext.Current.IncomingRequest, "#4");
				Assert.IsNotNull (WebOperationContext.Current.IncomingResponse, "#5");
				Assert.IsNotNull (WebOperationContext.Current.OutgoingResponse, "#6"); // pointless though.
			}
			ch.Close ();
		}

#if NET_4_0
		[Test]
		public void CreateAtom10Response ()
		{
			CreateResponseTest (ch => ch.Join ("foo", "bar"));
		}

		[Test]
		public void CreateJsonResponse ()
		{
			CreateResponseTest (ch => ch.TestJson ("foo", "bar"));
		}

		[Test]
		[Category ("NotWorking")] // .NET rejects HogeData as an unkown  type.
		public void CreateJsonResponse2 ()
		{
			CreateResponseTest (ch => ch.TestJson2 ("foo", "bar"));
		}

		[Test]
		public void CreateJsonResponse3 ()
		{
			CreateResponseTest (ch => ch.TestJson3 ("foo", "bar"));
		}

		void CreateResponseTest (Action<IHogeService> a)
		{
			var host = new WebServiceHost (typeof (HogeService));
			host.AddServiceEndpoint (typeof (IHogeService), new WebHttpBinding (), new Uri ("http://localhost:37564"));
			host.Description.Behaviors.Find<ServiceDebugBehavior> ().IncludeExceptionDetailInFaults = true;
			host.Open ();
			try {
				using (var cf = new ChannelFactory<IHogeService> (new WebHttpBinding (), new EndpointAddress ("http://localhost:37564"))) {
					cf.Endpoint.Behaviors.Add (new WebHttpBehavior ());
					cf.Open ();
					var ch = cf.CreateChannel ();
					a (ch);
				}
			} finally {
				host.Close ();
			}
		}
#endif
	}

	[ServiceContract]
	public interface IHogeService
	{
		[WebGet]
		[OperationContract]
		string Join (string s1, string s2);

		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		[OperationContract]
		string TestJson (string s1, string s2);

		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		[OperationContract]
		string TestJson2 (string s1, string s2);

		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		[OperationContract]
		string TestJson3 (string s1, string s2);
	}

#if NET_4_0
	public class HogeService : IHogeService
	{
		static XmlWriterSettings settings = new XmlWriterSettings () { OmitXmlDeclaration = true };

		static string GetXml (Message msg)
		{
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw, settings))
				msg.WriteMessage (xw);
			return sw.ToString ();
		}

		public string Join (string s1, string s2)
		{
			try {
				// ServiceDocument
				var woc = WebOperationContext.Current;
				var sd = new ServiceDocument ();
				var msg = woc.CreateAtom10Response (sd);
				var xml = "<service xmlns:a10='http://www.w3.org/2005/Atom' xmlns='http://www.w3.org/2007/app' />";
			
				Assert.AreEqual (xml.Replace ('\'', '"'), GetXml (msg), "#1");
				// Feed
				var uid = new UniqueId ().ToString ();
				var updatedTime = DateTime.SpecifyKind (new DateTime (2011, 4, 8, 11, 46, 12), DateTimeKind.Utc);
				var feed = new SyndicationFeed () { Id = uid, LastUpdatedTime = updatedTime };
				msg = woc.CreateAtom10Response (feed);
				xml = @"<feed xmlns='http://www.w3.org/2005/Atom'><title type='text'></title><id>" + uid + @"</id><updated>2011-04-08T11:46:12Z</updated></feed>";
				Assert.AreEqual (xml.Replace ('\'', '"'), GetXml (msg), "#2");

				// Item
				var item = new SyndicationItem () { Id = uid, LastUpdatedTime = updatedTime };
				msg = woc.CreateAtom10Response (item);
				xml = @"<entry xmlns='http://www.w3.org/2005/Atom'><id>" + uid + "</id><title type='text'></title><updated>2011-04-08T11:46:12Z</updated></entry>";
				Assert.AreEqual (xml.Replace ('\'', '"'), GetXml (msg), "#2");
			} catch (Exception ex) {
				Console.Error.WriteLine (ex);
				throw;
			}
			return s1 + s2;
		}

		public string TestJson (string s1, string s2)
		{
			try {
				var woc = WebOperationContext.Current;
				var msg = woc.CreateJsonResponse<HogeData> (new HogeData () {Foo = "foo", Bar = "bar" });
				Assert.AreEqual ("<root type=\"object\"><Bar>bar</Bar><Foo>foo</Foo></root>", GetXml (msg), "#1");
			} catch (Exception ex) {
				Console.Error.WriteLine (ex);
				throw;
			}
			return s1 + s2;
		}
		
		public string TestJson2 (string s1, string s2)
		{
			try {
				var woc = WebOperationContext.Current;
				// passed <object> -> unknown type error
				var msg = woc.CreateJsonResponse<object> (new HogeData () {Foo = "foo", Bar = "bar" });
				Assert.AreEqual ("<root type=\"object\"><Bar>bar</Bar><Foo>foo</Foo></root>", GetXml (msg), "#1");

				Assert.Fail ("Test2 server should fail");
			} catch (SerializationException ex) {
			} catch (Exception ex) {
				Console.Error.WriteLine (ex);
				throw;
			}
			return s1 + s2;
		}
		
		public string TestJson3 (string s1, string s2)
		{
			try {
				var woc = WebOperationContext.Current;
				var msg = woc.CreateJsonResponse<HogeData2> (new HogeData2 () {Foo = "foo", Bar = "bar" });
				Assert.AreEqual ("<root type=\"object\"><Bar>bar</Bar><Foo>foo</Foo></root>", GetXml (msg), "#1");
			} catch (Exception ex) {
				Console.Error.WriteLine (ex);
				throw;
			}
			return s1 + s2;
		}
	}

	[DataContract]
	public class HogeData
	{
		[DataMember]
		public string Foo { get; set; }
		[DataMember]
		public string Bar { get; set; }
	}

	// non-contract
	public class HogeData2
	{
		public string Foo { get; set; }
		public string Bar { get; set; }
	}
#endif
}
