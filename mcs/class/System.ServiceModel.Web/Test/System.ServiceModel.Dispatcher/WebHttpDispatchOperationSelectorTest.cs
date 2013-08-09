//
// WebHttpDispatchOperationSelectorTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class WebHttpDispatchOperationSelectorTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullEndpoint ()
		{
			new WebHttpDispatchOperationSelector (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConstructorEndpointNullAddress ()
		{
			ServiceEndpoint se = new ServiceEndpoint (ContractDescription.GetContract (typeof (MyService)));
			new WebHttpDispatchOperationSelector (se);
		}

		#region SelectOperation

		[Test]
		public void SelectOperation ()
		{
			SelectOperationCore (Create ());
		}

		[Test]
		public void SelectOperation2 ()
		{
			SelectOperationCore (Create2 ());
		}

		[Test]
		public void SelectOperation3 ()
		{
			ContractDescription cd = ContractDescription.GetContract (typeof (MyService2));
			Assert.IsNotNull (cd.Operations [0].Behaviors.Find<WebGetAttribute> (), "#1");

			cd = ContractDescription.GetContract (typeof (MyService));
			Assert.IsNull (cd.Operations [0].Behaviors.Find<WebGetAttribute> (), "#2");
		}

		void SelectOperationCore (MySelector d)
		{
			string name;
			var msg = Message.CreateMessage (MessageVersion.Soap12, "http://temuri.org/MyService/Echo"); // looks like Version is not checked
			Assert.IsNull (msg.Headers.To, "#1-0");
			Assert.IsFalse (d.SelectOperation (ref msg, out name), "#1");
			Assert.AreEqual ("", name, "#1-2");
			Assert.IsNull (msg.Headers.To, "#1-3"); // no overwrite
			Assert.IsFalse (msg.Properties.ContainsKey ("UriMatched"), "#1-4");

			msg = Message.CreateMessage (MessageVersion.None, "http://temuri.org/MyService/Echo");
			Assert.IsFalse (d.SelectOperation (ref msg, out name), "#2");
			Assert.IsFalse (msg.Properties.ContainsKey ("UriMatched"), "#2-2");

			msg = Message.CreateMessage (MessageVersion.None, "http://foobar.org/MyService/NonExistent");
			Assert.IsFalse (d.SelectOperation (ref msg, out name), "#3");
			Assert.AreEqual ("", name, "#3-2");
			Assert.IsFalse (msg.Properties.ContainsKey ("UriMatched"), "#3-3");

			// version and action do not matter
			msg = Message.CreateMessage (MessageVersion.Soap12, "http://nonexistent.org/");
			var http = new HttpRequestMessageProperty ();
			// this mismatch is allowed. Lack of this value is OK.
//			http.Method = "POST";
			// this mismatch is allowed. Lack of this value is OK.
//			http.QueryString = "foo=bar";
			// this mismatch is allowed. Lack of this value is OK.
//			http.Headers.Add ("Content-Type", "application/json");
			// so, the http property can be empty, but is required.
			msg.Properties.Add (HttpRequestMessageProperty.Name, http);
			msg.Headers.To = new Uri ("http://localhost:8080/Echo?input=hoge");
			Assert.IsTrue (d.SelectOperation (ref msg, out name), "#4");
			// FIXME: hmm... isn'y "Echo" expected?
			// Assert.AreEqual ("", name, "#4-2");
		}

		[Test]
		[Category ("NotWorking")]
		public void SelectOperationOnlyMessage ()
		{
			// This test shows strange result ... it adds UriMatched property while it does not really match the URI.

			var d = Create ();
			var msg = Message.CreateMessage (MessageVersion.None, "http://temuri.org/MyService/Echo"); // looks like Version is not checked
			Assert.AreEqual ("", d.SelectOperation (ref msg), "#1");
			Assert.IsTrue (msg.Properties.ContainsKey ("UriMatched"), "#1-2");

			msg = Message.CreateMessage (MessageVersion.None, "http://foobar.org/MyService/NonExistent");
			Assert.AreEqual ("", d.SelectOperation (ref msg), "#2");
			Assert.IsNotNull (msg.Properties ["UriMatched"], "#2-2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SelectOperationCheckExistingProperty ()
		{
			var d = Create ();
			var msg = Message.CreateMessage (MessageVersion.None, "http://temuri.org/MyService/Echo"); // heh, I mistyped it and turned to prove that Action does not matter.
			msg.Headers.To = new Uri ("http://localhost:8080/Echo");

			// LAMESPEC: .NET does returns String.Empty, while we return the name of the operation (as IOperationSelector.SelectOperation is expected to do!)
			// Assert.AreEqual (String.Empty, d.SelectOperation (ref msg), "#1");
			d.SelectOperation (ref msg);

			// The second invocation should raise the expected exception
			d.SelectOperation (ref msg);
		}

		// Types

		class MySelector : WebHttpDispatchOperationSelector
		{
			public MySelector (ServiceEndpoint se)
				: base (se)
			{
			}

			public bool SelectOperation (ref Message msg, out string name)
			{
				bool ret;
				name = SelectOperation (ref msg, out ret);
				return ret;
			}
		}

		class MyBehavior : WebHttpBehavior
		{
			public WebHttpDispatchOperationSelector GetPublicOperationSelector (ServiceEndpoint se)
			{
				return GetOperationSelector (se);
			}

			protected override WebHttpDispatchOperationSelector GetOperationSelector (ServiceEndpoint se)
			{
				return new MySelector (se);
			}
		}

		[ServiceContract]
		public interface MyService
		{
			[OperationContract]
//			[WebGet (UriTemplate = "MyService/Echo?input={input}")]
//			[WebGet] // UriTemplate = "Echo?input={input}"
			string Echo (string input);
		}

		[ServiceContract (Name = "MyService")]
		public interface MyService2
		{
			[OperationContract (Name = "Echo")]
			[WebGet] // UriTemplate = "Echo?input={input}"
			string Echo (string input);
		}

		MySelector Create ()
		{
			ServiceEndpoint se = new ServiceEndpoint (ContractDescription.GetContract (typeof (MyService)));
			se.Address = new EndpointAddress ("http://localhost:8080");
			se.Contract.Operations [0].Behaviors.Add (new WebGetAttribute ());
			return new MySelector (se);
			//var b = new MyBehavior ();
			//se.Behaviors.Add (b);
			//return (MySelector) b.GetPublicOperationSelector (se);
		}

		MySelector Create2 ()
		{
			ServiceEndpoint se = new ServiceEndpoint (ContractDescription.GetContract (typeof (MyService2)));
			se.Address = new EndpointAddress ("http://localhost:8080");
			//se.Contract.Operations [0].Behaviors.Add (new WebGetAttribute ());
			return new MySelector (se);
		}

		#endregion

		#region "bug #656020"

		[DataContract]
		public class User
		{
			[DataMember]
			public string Name { get; set; }
		}

		[ServiceContract]
		interface IHello
		{
			[WebGet(UriTemplate = "{name}?age={age}&blah={blah}")]
			[OperationContract]
			string SayHi (string name, int age, string blah);

			[WebInvoke(UriTemplate = "blah")]
			[OperationContract]
			string SayHi2 (User user);
		}

		class Hello : IHello
		{
			public string SayHi (string name, int age, string blah)
			{
				return string.Format ("Hi {0}: {1}, {2}", name, age, blah == null);
			}
		
			public string SayHi2 (User user)
			{
				return string.Format ("Hi {0}.", user.Name);
			}
		}

		[Test]
		public void WebMessageFormats ()
		{
			var host = new WebServiceHost (typeof (Hello));
			host.AddServiceEndpoint (typeof (IHello), new WebHttpBinding (), "http://localhost:37564/");
			host.Description.Behaviors.Find<ServiceDebugBehavior> ().IncludeExceptionDetailInFaults = true;
			host.Open ();
			try {
				// run client
				using (ChannelFactory<IHello> factory = new ChannelFactory<IHello> (new WebHttpBinding (), "http://localhost:37564/"))
				{
					factory.Endpoint.Behaviors.Add (new WebHttpBehavior ());
					IHello h = factory.CreateChannel ();
					//Console.WriteLine(h.SayHi("Joe", 42, null));
					Assert.AreEqual ("Hi Joe.", h.SayHi2 (new User { Name = "Joe" }), "#1");
				}
			} finally {
				host.Close ();
			}
		}
		
		#endregion
	}
}
#endif