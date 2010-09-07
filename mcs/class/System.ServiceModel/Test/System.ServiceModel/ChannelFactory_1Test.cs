//
// ChannelFactory_1Test.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using MonoTests.System.ServiceModel.Channels;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ChannelFactory_1Test
	{
		class MyChannelFactory<T> : ChannelFactory<T>
		{
			public MyChannelFactory (Binding b, EndpointAddress a)
				: base (b, a)
			{
			}

			public void OpenAnyways ()
			{
				EnsureOpened ();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateChannelForClass ()
		{
			//ChannelFactory<TestService> f =
				new ChannelFactory<TestService> (
					new BasicHttpBinding (),
					new EndpointAddress ("http://localhost:37564"));
		}

		[Test]
		public void EndpointAddressAfterCreateChannel ()
		{
			var f = new ChannelFactory<ITestService> (new BasicHttpBinding ());
			f.CreateChannel (new EndpointAddress ("http://localhost:37564"), null);
			Assert.IsNull (f.Endpoint.Address, "#1");
		}

		[Test]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void CtorNullArgsAllowed ()
		{
			ChannelFactory<ICtorUseCase1> f1;
			f1 = new ChannelFactory<ICtorUseCase1> ("CtorUseCase1_1", null);
			Assert.AreEqual (new EndpointAddress ("http://test1_1"), f1.Endpoint.Address, "#01");
			f1 = new ChannelFactory<ICtorUseCase1> (new BasicHttpBinding (), (EndpointAddress)null);
			Assert.AreEqual (null, f1.Endpoint.Address, "#01");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateChannelFromDefaultConfigWithTwoConfigs ()
		{
			new ChannelFactory<ICtorUseCase2> ("*");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateChannelFromDefaultConfigWithNoConfigs ()
		{
			new ChannelFactory<ICtorUseCase3> ("*");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorArgsTest1 ()
		{
			new ChannelFactory<ICtorUseCase1> (new BasicHttpBinding (), (string)null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CtorArgsTest2 ()
		{
			new ChannelFactory<ICtorUseCase1> ("CtorUseCase1_Incorrect");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorArgsTest3 ()
		{
			new ChannelFactory<ICtorUseCase1> ((string)null, new EndpointAddress ("http://test"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullServiceEndpoint ()
		{
			new ChannelFactory<IFoo> ((ServiceEndpoint) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullBinding ()
		{
			new ChannelFactory<IFoo> ((Binding) null);
		}

		[Test]
		public void ConfigEmptyCtor ()
		{
			// It has no valid configuration, but goes on.
			new ChannelFactory<ICtorUseCase1> ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConfigEmptyCtor2 ()
		{
			var cf = new ChannelFactory<ICtorUseCase1> ();
			// It cannot go on further.
			cf.CreateChannel ();
		}

		[Test]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void ConfigCtor ()
		{
			new ChannelFactory<ICtorUseCase1> ("CtorUseCase1_1");
		}

		[Test]
		public void EnsureOpened ()
		{
			MyChannelFactory<ITestService> f =
				new MyChannelFactory<ITestService> (
					new BasicHttpBinding (),
					new EndpointAddress ("http://localhost:37564"));
			Assert.AreEqual (CommunicationState.Created,
				f.State, "#1");
			f.OpenAnyways ();
			Assert.AreEqual (CommunicationState.Opened,
				f.State, "#1");
		}

		[Test]
		// I was deceived by MSDN and currently ChannelFactory<T>
		// only accepts IChannel as T. It will be fixed. -> done.
		public void CreateChannel ()
		{
			ChannelFactory<ITestService> f =
				new ChannelFactory<ITestService> (
					new BasicHttpBinding (),
					new EndpointAddress ("http://localhost:37564"));
			f.CreateChannel ();
		}

		private T CreateChannel<T> (RequestSender handler)
		{
			CustomBinding b = new CustomBinding (new HandlerTransportBindingElement (handler));
			ChannelFactory<T> f = new ChannelFactory<T> ( b, new EndpointAddress ("urn:dummy"));
			return f.CreateChannel ();
		}

		[Test]
		public void InvokeFoo ()
		{
			ITestService ts = CreateChannel<ITestService> (
				delegate (Message input) {
					BodyWriter bw = new HandlerBodyWriter (
						delegate (XmlDictionaryWriter writer) {
							writer.WriteStartElement ("FooResponse", "http://tempuri.org/");
							writer.WriteElementString ("FooResult", "http://tempuri.org/", "cecil");
							writer.WriteEndElement ();
						}
					);
					return Message.CreateMessage (input.Version, input.Headers.Action + "Response", bw);
				}
			);
			Assert.AreEqual ("cecil", ts.Foo ("il offre sa confiance et son amour"));
		}

		[Test]
		public void InvokeBar ()
		{
			ITestService ts = CreateChannel<ITestService> (
				delegate (Message input) {
					BodyWriter bw = new HandlerBodyWriter (
						delegate (XmlDictionaryWriter writer) {
							writer.WriteStartElement ("BarResponse", "http://tempuri.org/");
							writer.WriteElementString ("DummyBarResponse", "http://tempuri.org/", "cecil");
							writer.WriteEndElement ();
						}
					);
					return Message.CreateMessage (input.Version, input.Headers.Action + "Response", bw);
				}
			);
			ts.Bar ("il offre sa confiance et son amour");
		}

		Message ToMessage<T> (Message input, bool isXml, T val)
		{
			TypedMessageConverter tm;
			if (isXml)
				tm = TypedMessageConverter.Create (typeof (T),
					input.Headers.Action + "Response", new XmlSerializerFormatAttribute ());
			else
				tm = TypedMessageConverter.Create (typeof (T),
					input.Headers.Action + "Response");
			return tm.ToMessage (val, input.Version);
		}

		T FromMessage<T> (Message input, bool isXml)
		{
			TypedMessageConverter tm;
			if (isXml)
				tm = TypedMessageConverter.Create (typeof (T), input.Headers.Action,
					new XmlSerializerFormatAttribute ());
			else
				tm = TypedMessageConverter.Create (typeof (T), input.Headers.Action);
			return (T)tm.FromMessage (input);
		}

		[Test]
		public void InvokeFooOutEnumParam ()
		{
			ITestService ts = CreateChannel<ITestService> (
				delegate (Message input) {
					// Test input for in and out Enum args.
					XmlDocument doc = new XmlDocument ();
					doc.LoadXml (input.ToString ());

					XmlNamespaceManager nss = new XmlNamespaceManager (doc.NameTable);
					nss.AddNamespace ("s", "http://www.w3.org/2003/05/soap-envelope");
					nss.AddNamespace ("t", "http://tempuri.org/");
					XmlElement el = doc.SelectSingleNode ("/s:Envelope/s:Body/t:FooOutEnumParam", nss) as XmlElement;
					Assert.IsNotNull (el, "I#0");
					XmlNode arg1 = el.SelectSingleNode ("t:arg1", nss);
					Assert.IsNotNull (arg1, "I#2");
					Assert.AreEqual ("Blue", arg1.InnerText, "I#3");

					return ToMessage (input, false,
						new FooOutEnumParamResponse (FooColor.Green, FooColor.Red));
				}
			);

			FooColor argOut;
			FooColor res = ts.FooOutEnumParam (FooColor.Blue, out argOut);
			Assert.AreEqual (FooColor.Green, res, "#1");
			Assert.AreEqual (FooColor.Red, argOut, "#2");
		}

		public T CreateVoidFooOutParamChannel<T> (bool isXml)
		{
			return CreateChannel<T> (
				delegate (Message input) {
					// Test input for in and ref args.
					XmlDocument doc = new XmlDocument ();
					doc.LoadXml (input.ToString ());

					XmlNamespaceManager nss = new XmlNamespaceManager (doc.NameTable);
					nss.AddNamespace ("s", "http://www.w3.org/2003/05/soap-envelope");
					nss.AddNamespace ("t", "http://tempuri.org/");
					XmlElement el = doc.SelectSingleNode ("/s:Envelope/s:Body/t:VoidFooOutParam", nss) as XmlElement;
					Assert.IsNotNull (el, "I#0");
					XmlNode arg1 = el.SelectSingleNode ("t:arg1", nss);
					Assert.IsNotNull (arg1, "I#2");
					Assert.AreEqual ("testIt", arg1.InnerText, "I#3");
					XmlNode arg2 = el.SelectSingleNode ("t:arg2", nss);
					Assert.IsNotNull (arg2, "I#4");
					Assert.AreEqual ("testRef", arg2.InnerText, "I#4");

					return ToMessage (input, isXml,
						new VoidFooOutParamResponse ("refArg", "outArg"));
				}
			);
		}

		[Test]
		public void InvokeVoidFooOutParam ()
		{
			ITestService ts = CreateVoidFooOutParamChannel<ITestService> (false);
			string argRef = "testRef";
			string argOut;
			ts.VoidFooOutParam ("testIt", ref argRef, out argOut);
			Assert.AreEqual ("refArg", argRef, "#1");
			Assert.AreEqual ("outArg", argOut, "#2");
		}

		[Test]
		public void XmlInvokeVoidFooOutParam ()
		{
			ITestServiceXml ts = CreateVoidFooOutParamChannel<ITestServiceXml> (true);
			string argRef = "testRef";
			string argOut;
			ts.VoidFooOutParam ("testIt", ref argRef, out argOut);
			Assert.AreEqual ("refArg", argRef, "#1");
			Assert.AreEqual ("outArg", argOut, "#2");
		}

		public T CreateFooOutParamChannel<T> (bool isXml)
		{
			return CreateChannel<T> (
				delegate (Message input) {
					// Test input for in and ref args.
					XmlDocument doc = new XmlDocument ();
					doc.LoadXml (input.ToString ());

					XmlNamespaceManager nss = new XmlNamespaceManager (doc.NameTable);
					nss.AddNamespace ("s", "http://www.w3.org/2003/05/soap-envelope");
					nss.AddNamespace ("t", "http://tempuri.org/");
					XmlElement el = doc.SelectSingleNode ("/s:Envelope/s:Body/t:FooOutParam", nss) as XmlElement;
					Assert.IsNotNull (el, "I#0");
					XmlNode arg1 = el.SelectSingleNode ("t:arg1", nss);
					Assert.IsNotNull (arg1, "I#2");
					Assert.AreEqual ("testIt", arg1.InnerText, "I#3");
					XmlNode arg2 = el.SelectSingleNode ("t:arg2", nss);
					Assert.IsNotNull (arg2, "I#4");
					Assert.AreEqual ("testRef", arg2.InnerText, "I#4");

					return ToMessage (input, isXml,
						new FooOutParamResponse ("callResult", "refArg", "outArg"));
				}
			);
		}

		[Test]
		public void InvokeFooOutParam ()
		{
			ITestService ts = CreateFooOutParamChannel<ITestService> (false);
			string argRef = "testRef";
			string argOut;
			string res = ts.FooOutParam ("testIt", ref argRef, out argOut);
			Assert.AreEqual ("callResult", res, "#1");
			Assert.AreEqual ("refArg", argRef, "#2");
			Assert.AreEqual ("outArg", argOut, "#3");
		}

		[Test]
		public void XmlInvokeFooOutParam ()
		{
			ITestServiceXml ts = CreateFooOutParamChannel<ITestServiceXml> (true);
			string argRef = "testRef";
			string argOut;
			string res = ts.FooOutParam ("testIt", ref argRef, out argOut);
			Assert.AreEqual ("callResult", res, "#1");
			Assert.AreEqual ("refArg", argRef, "#2");
			Assert.AreEqual ("outArg", argOut, "#3");
		}

		[Test]
		public void InvokeFooComplex ()
		{
			ITestService ts = CreateChannel<ITestService> (
				delegate (Message input) {
					// Test input for in and ref args.
					XmlDocument doc = new XmlDocument ();
					doc.LoadXml (input.ToString ());

					XmlNamespaceManager nss = new XmlNamespaceManager (doc.NameTable);
					nss.AddNamespace ("s", "http://www.w3.org/2003/05/soap-envelope");
					nss.AddNamespace ("t", "http://tempuri.org/");
					nss.AddNamespace ("v", "http://schemas.datacontract.org/2004/07/MonoTests.System.ServiceModel");
					XmlElement el = doc.SelectSingleNode ("/s:Envelope/s:Body/t:FooComplex", nss) as XmlElement;
					Assert.IsNotNull (el, "I#0");
					XmlNode arg1 = el.SelectSingleNode ("t:arg1/v:val", nss);
					Assert.IsNotNull (arg1, "I#2");
					Assert.AreEqual ("testIt", arg1.InnerText, "I#3");

					return ToMessage (input, false, new FooComplexResponse ("callResult"));
				}
			);

			TestData res = ts.FooComplex (new TestData ("testIt"));
			Assert.IsNotNull (res, "#1");
			Assert.AreEqual ("callResult", res.val, "#2");
		}

		[Test]
		[Ignore ("This somehow results in an infinite loop")]
		public void XmlInvokeFooComplex ()
		{
			ITestServiceXml ts = CreateChannel<ITestServiceXml> (
				delegate (Message input) {
					// Test input for in and ref args.
					XmlDocument doc = new XmlDocument ();
					doc.LoadXml (input.ToString ());

					XmlNamespaceManager nss = new XmlNamespaceManager (doc.NameTable);
					nss.AddNamespace ("s", "http://www.w3.org/2003/05/soap-envelope");
					nss.AddNamespace ("t", "http://tempuri.org/");
					XmlElement el = doc.SelectSingleNode ("/s:Envelope/s:Body/t:FooComplex", nss) as XmlElement;
					Assert.IsNotNull (el, "I#0");
					XmlElement arg1 = el.SelectSingleNode ("t:arg1", nss) as XmlElement;
					Assert.IsNotNull (arg1, "I#2");
					Assert.AreEqual ("testIt", arg1.GetAttribute ("val"), "I#3");

					return ToMessage (input, true, new FooComplexResponse ("callResult"));
				}
			);

			TestData res = ts.FooComplex (new TestData ("testIt"));
			Assert.IsNotNull (res, "#1");
			Assert.AreEqual ("callResult", res.val, "#2");
		}

#if NET_4_0
		[Test]
		public void ConstructorServiceEndpoint ()
		{
			// It is okay to pass ServiceEndpoint that does not have Binding or EndpointAddress.
			new ChannelFactory<IRequestChannel> (new ServiceEndpoint (ContractDescription.GetContract (typeof (IMetadataExchange)), null, null));
		}
#endif

		public T CreateFooComplexMC_Channel<T> (bool isXml)
		{
			return CreateChannel<T> (
				delegate (Message input) {
					TestMessage arg = FromMessage<TestMessage> (input, isXml);
					Assert.IsNotNull (arg.data, "I#0");
					Assert.AreEqual (arg.data.val, "testIt", "I#1");
					Assert.IsNotNull (arg.msg, "I#2");
					Assert.AreEqual (arg.msg.val, "testMsg", "I#3");

					return ToMessage (input, isXml, new TestResult ("callResult", "callArg"));
				}
			);
		}

		[Test]
		public void InvokeFooComplexMC ()
		{
			ITestService ts = CreateFooComplexMC_Channel<ITestService> (false);
			TestResult res = ts.FooComplexMC (new TestMessage ("testIt", "testMsg"));
			Assert.IsNotNull (res, "#1");
			Assert.AreEqual ("callResult", res.resData.val, "#2");
			Assert.AreEqual ("callArg", res.resMsg.val, "#3");
		}

		[Test]
		[Ignore ("This somehow results in an infinite loop")]
		public void XmlInvokeFooComplexMC ()
		{
			ITestServiceXml ts = CreateFooComplexMC_Channel<ITestServiceXml> (true);
			TestResult res = ts.FooComplexMC (new TestMessage ("testIt", "testMsg"));
			Assert.IsNotNull (res, "#1");
			Assert.AreEqual ("callResult", res.resData.val, "#2");
			Assert.AreEqual ("callArg", res.resMsg.val, "#3");
		}

		[Test]
		public void OneWayOperationWithRequestReplyChannel ()
		{
			var host = new ServiceHost (typeof (OneWayService));
			host.AddServiceEndpoint (typeof (IOneWayService),
				new BasicHttpBinding (),
				new Uri ("http://localhost:8080"));
			host.Open ();
			try {
				var cf = new ChannelFactory<IOneWayService> (
					new BasicHttpBinding (),
					new EndpointAddress ("http://localhost:8080"));
				var ch = cf.CreateChannel ();
				ch.GiveMessage ("test");
				
				Assert.IsTrue (OneWayService.WaitHandle.WaitOne (TimeSpan.FromSeconds (5)), "#1");
			} finally {
				host.Close ();
			}
		}

		[ServiceContract]
		public interface ITestService
		{
			[OperationContract]
			string Foo (string arg);

			[OperationContract]
			void Bar (string arg);

			[OperationContract]
			void Foo1 (string arg1, string arg2);

			[OperationContract]
			FooColor FooOutEnumParam (FooColor arg1, out FooColor arg2);

			[OperationContract]
			string FooOutParam (string arg1, ref string arg2, out string arg3);

			[OperationContract]
			void VoidFooOutParam (string arg1, ref string arg2, out string arg3);

			[OperationContract]
			TestData FooComplex (TestData arg1);

			[OperationContract]
			TestResult FooComplexMC (TestMessage arg1);
		}

		[ServiceContract]
		public interface ITestServiceXml
		{
			[OperationContract]
			string FooOutParam (string arg1, ref string arg2, out string arg3);

			[OperationContract]
			void VoidFooOutParam (string arg1, ref string arg2, out string arg3);

			[OperationContract]
			[XmlSerializerFormat]
			TestData FooComplex (TestData arg1);

			[OperationContract]
			[XmlSerializerFormat]
			TestResult FooComplexMC (TestMessage arg1);
		}

		[ServiceContract]
		public interface IOneWayService
		{
			[OperationContract (IsOneWay = true)]
			void GiveMessage (string input);
		}

		public class OneWayService : IOneWayService
		{
			public static ManualResetEvent WaitHandle = new ManualResetEvent (false);

			public void GiveMessage (string input)
			{
				WaitHandle.Set ();
			}
		}

		public enum FooColor { Red = 1, Green, Blue }

		[DataContract]
		public class TestData
		{
			TestData () {}
			public TestData (string val) { this.val = val; }

			[DataMember]
			[XmlAttribute]
			public string val;
		}

		[MessageContract]
		public class TestMessage
		{
			TestMessage () {}
			public TestMessage (string a, string b) { data = new TestData (a); msg = new TestData (b); }

			[MessageBodyMember]
			public TestData data;

			[MessageBodyMember]
			public TestData msg;
		}

		[MessageContract]
		public class TestResult
		{
			TestResult () {}
			public TestResult (string a, string b) { resData = new TestData (a); resMsg = new TestData (b); }

			[MessageBodyMember]
			public TestData resData;

			[MessageBodyMember]
			public TestData resMsg;
		}

		[MessageContract (WrapperNamespace = "http://tempuri.org/")]
		class FooOutParamResponse
		{
			FooOutParamResponse () {}
			public FooOutParamResponse (string ret, string refArg, string outArg) { FooOutParamResult = ret; this.arg2 = refArg; this.arg3 = outArg; }

			[MessageBodyMember]
			public string FooOutParamResult;

			[MessageBodyMember]
			public string arg2;

			[MessageBodyMember]
			public string arg3;
		}

		[MessageContract (WrapperNamespace = "http://tempuri.org/")]
		class FooOutEnumParamResponse
		{
			FooOutEnumParamResponse () {}
			public FooOutEnumParamResponse (FooColor ret, FooColor outArg) { FooOutEnumParamResult = ret; this.arg2 = outArg; }

			[MessageBodyMember]
			public FooColor FooOutEnumParamResult;

			[MessageBodyMember]
			public FooColor arg2;
		}

		[MessageContract (WrapperNamespace = "http://tempuri.org/")]
		class VoidFooOutParamResponse
		{
			VoidFooOutParamResponse () {}
			public VoidFooOutParamResponse (string refArg, string outArg) { this.arg2 = refArg; this.arg3 = outArg; }

			[MessageBodyMember]
			public string arg2;

			[MessageBodyMember]
			public string arg3;
		}

		[MessageContract (WrapperNamespace = "http://tempuri.org/")]
		class FooComplexResponse
		{
			FooComplexResponse () {}
			public FooComplexResponse (string val) { FooComplexResult  = new TestData (val); }

			[MessageBodyMember]
			public TestData FooComplexResult;
		}

		class TestService
		{
			public string Foo (string arg)
			{
				return arg;
			}
		}
	}
}
