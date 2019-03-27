using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	public class WebHttpBehaviorExt : WebHttpBehavior
	{
		public IClientMessageFormatter DoGetReplyClientFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return GetReplyClientFormatter (operationDescription, endpoint);
		}

		public IClientMessageFormatter DoGetRequestClientFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return GetRequestClientFormatter (operationDescription, endpoint);
		}
#if !MOBILE
		public IDispatchMessageFormatter DoGetReplyDispatchFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return GetReplyDispatchFormatter (operationDescription, endpoint);
		}

		public IDispatchMessageFormatter DoGetRequestDispatchFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return GetRequestDispatchFormatter (operationDescription, endpoint);
		}
#endif
		public event Action<ServiceEndpoint, ClientRuntime> ApplyClientBehaviorInvoked;

		public override void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime client)
		{
			base.ApplyClientBehavior (endpoint, client);
			if (ApplyClientBehaviorInvoked != null)
				ApplyClientBehaviorInvoked (endpoint, client);
		}
	}

	[TestFixture]
	public class WebHttpBehaviorTest
	{
		ServiceEndpoint CreateEndpoint ()
		{
			return new ServiceEndpoint (ContractDescription.GetContract (typeof (IMyService)), new WebHttpBinding (),
						    new EndpointAddress ("http://localhost:37564"));
		}

		[Test]
		public void DefaultValues ()
		{
			var b = new WebHttpBehavior ();
			Assert.AreEqual (WebMessageBodyStyle.Bare, b.DefaultBodyStyle, "#1");
			Assert.AreEqual (WebMessageFormat.Xml, b.DefaultOutgoingRequestFormat, "#2");
			Assert.AreEqual (WebMessageFormat.Xml, b.DefaultOutgoingResponseFormat, "#3");
			Assert.IsFalse (b.AutomaticFormatSelectionEnabled, "#11");
			Assert.IsFalse (b.FaultExceptionEnabled, "#12");
			Assert.IsFalse (b.HelpEnabled, "#13");
		}

		[Test]
		public void AddBiningParameters ()
		{
			var se = CreateEndpoint ();
			var b = new WebHttpBehavior ();
			var pl = new BindingParameterCollection ();
			b.AddBindingParameters (se, pl);
			Assert.AreEqual (0, pl.Count, "#1");
		}

#if !MOBILE && !XAMMAC_4_5
		[Test]
		public void ApplyDispatchBehavior ()
		{
			var se = CreateEndpoint ();
			var od = se.Contract.Operations [0];
			// in .NET 3.5 it adds "OperationSelectorBehavior"
			int initCB = ContractDescription.GetContract (typeof (IMyService)).Behaviors.Count;
			// in .NET 3.5 it adds
			// - OperationInvokeBehavior, 
			// - OperationBehaviorAttribute, 
			// - DataContractSerializerOperationBehavior and 
			// - DataContractSerializerOperationGenerator
			int initOB = od.Behaviors.Count;
			// Assert.AreEqual (1, initCB, "#0-1");
			// Assert.AreEqual (4, initOB, "#0-2");

			var b = new WebHttpBehavior ();
			se.Behaviors.Add (b);
			var ed = new EndpointDispatcher (se.Address, se.Contract.Name, se.Contract.Namespace);
			IChannelListener l = new WebHttpBinding ().BuildChannelListener<IReplyChannel> (new BindingParameterCollection ());
			var cd = new ChannelDispatcher (l);
			cd.Endpoints.Add (ed); // without it this test results in NRE (it blindly adds IErrorHandler).
			Assert.AreEqual (0, cd.ErrorHandlers.Count, "#1-1");
			Assert.IsNull (ed.DispatchRuntime.OperationSelector, "#1-2");
			Assert.AreEqual (1, se.Behaviors.Count, "#1-3-1");
			Assert.AreEqual (initCB, se.Contract.Behaviors.Count, "#1-3-2");
			Assert.AreEqual (initOB, od.Behaviors.Count, "#1-3-3");

			Assert.IsTrue (ed.AddressFilter is EndpointAddressMessageFilter, "#1-4");

			b.ApplyDispatchBehavior (se, ed);
			// FIXME: implement and enable it later
			//Assert.AreEqual (1, cd.ErrorHandlers.Count, "#2-1");
			Assert.AreEqual (typeof (WebHttpDispatchOperationSelector),
					 ed.DispatchRuntime.OperationSelector.GetType (), "#2-2");
			Assert.AreEqual (1, se.Behaviors.Count, "#3-1");
			Assert.AreEqual (initCB, se.Contract.Behaviors.Count, "#3-2");
			Assert.AreEqual (initOB, od.Behaviors.Count, "#3-3");
			// ... i.e. nothing is added.

			Assert.IsTrue (ed.AddressFilter is PrefixEndpointAddressMessageFilter, "#3-4");

			Assert.AreEqual (0, ed.DispatchRuntime.Operations.Count, "#4-0"); // hmm... really?
		}
#endif

		[Test]
		public void GetMessageFormatters ()
		{
			var se = CreateEndpoint ();
			var od = se.Contract.Operations [0];
			var b = new WebHttpBehaviorExt ();
			Assert.IsNotNull (b.DoGetRequestClientFormatter (od, se), "#1");
			Assert.IsNotNull (b.DoGetReplyClientFormatter (od, se), "#2");
#if !MOBILE
			Assert.IsNotNull (b.DoGetRequestDispatchFormatter (od, se), "#3");
			Assert.IsNotNull (b.DoGetReplyDispatchFormatter (od, se), "#4");
#endif
		}

		[Test]
		public void RequestClientFormatter ()
		{
			var se = CreateEndpoint ();
			var od = se.Contract.Operations [0];
			var b = new WebHttpBehaviorExt ();
			var rcf = b.DoGetRequestClientFormatter (od, se);
			var msg = rcf.SerializeRequest (MessageVersion.None, new object [] {"foo"});
			var hp = msg.Properties [HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
			Assert.IsNotNull (hp, "#1");
			Assert.IsTrue (msg.IsEmpty, "#2");
			Assert.AreEqual (String.Empty, hp.QueryString, "#3");
			var mb = msg.CreateBufferedCopy (1000);
			try {
				rcf.DeserializeReply (mb.CreateMessage (), new object [0]);
				Assert.Fail ("It should not support reply deserialization");
			} catch (NotSupportedException) {
			}
		}

#if !MOBILE
		[Test]
		public void RequestClientFormatter2 ()
		{
			var se = CreateEndpoint ();
			var od = se.Contract.Operations [0];
			var b = new WebHttpBehaviorExt ();
			IClientMessageFormatter rcf = null;
			b.ApplyClientBehaviorInvoked += delegate (ServiceEndpoint e, ClientRuntime cr) {
				rcf = cr.Operations [0].Formatter;
			};
			se.Behaviors.Add (b);
			var ch = new WebChannelFactory<IMyServiceClient> (se).CreateChannel ();
			var msg = rcf.SerializeRequest (MessageVersion.None, new object [] {"foo"});
			var hp = msg.Properties [HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
			Assert.IsNotNull (hp, "#1");
			Assert.IsTrue (msg.IsEmpty, "#2");
			Assert.AreEqual (String.Empty, hp.QueryString, "#3");
			//var mb = msg.CreateBufferedCopy (1000);

			// TODO: test DeserializeReply too (it is supported unlike above).
		}
#endif

		[ServiceContract]
		public interface IMyService
		{
			[OperationContract]
			[WebGet]
			string Echo (string input);
		}

		public interface IMyServiceClient : IMyService, IClientChannel
		{
		}

		public class MyService: IMyService
		{
#if !MOBILE
			[OperationBehavior]
#endif
			public string Echo (string input)
			{
				return input;
			}
		}
		
		[Test]
		public void TestWebGetExists()
		{
			ContractDescription cd = ContractDescription.GetContract (typeof(IMyService), typeof (MyService));
			OperationDescription od = cd.Operations[0];
			Assert.IsTrue (od.Behaviors.Contains (typeof (WebGetAttribute)), "Operation is recognized as WebGet");
		}

#if !MOBILE
		[Test]
		public void MessageFormatterSupportsRaw ()
		{
			// serializing reply
			var ms = new MemoryStream ();
			var bytes = new byte [] {0, 1, 2, 0xFF};
			ms.Write (bytes, 0, bytes.Length);
			ms.Position = 0;
			var cd = ContractDescription.GetContract (typeof (ITestService));
			var od = cd.Operations [0];
			var se = new ServiceEndpoint (cd, new WebHttpBinding (), new EndpointAddress ("http://localhost:37564/"));
			var formatter = new WebHttpBehaviorExt ().DoGetReplyDispatchFormatter (od, se);

			var msg = formatter.SerializeReply (MessageVersion.None, null, ms);
			var wp = msg.Properties ["WebBodyFormatMessageProperty"] as WebBodyFormatMessageProperty;
			Assert.IsNotNull (wp, "#1");
			Assert.AreEqual (WebContentFormat.Raw, wp.Format, "#2");

			var wmebe = new WebMessageEncodingBindingElement ();
			var wme = wmebe.CreateMessageEncoderFactory ().Encoder;
			var ms2 = new MemoryStream ();
			wme.WriteMessage (msg, ms2);
			Assert.AreEqual (bytes, ms2.ToArray (), "#3");
		}

		[Test]
		public void MessageFormatterSupportsRaw2 ()
		{
			// deserializing request
			var ms = new MemoryStream ();
			ms.Write (new byte [] {0, 1, 2, 0xFF}, 0, 4);
			ms.Position = 0;
			var cd = ContractDescription.GetContract (typeof (ITestService));
			var od = cd.Operations [0];
			var se = new ServiceEndpoint (cd, new WebHttpBinding (), new EndpointAddress ("http://localhost:8080/"));
			var wmebe = new WebMessageEncodingBindingElement ();
			var wme = wmebe.CreateMessageEncoderFactory ().Encoder;
			var msg = wme.ReadMessage (ms, 100, null); // "application/xml" causes error.
			var formatter = new WebHttpBehaviorExt ().DoGetRequestDispatchFormatter (od, se);
			object [] pars = new object [1];
			formatter.DeserializeRequest (msg, pars);
			Assert.IsTrue (pars [0] is Stream, "ret");
		}
#endif
		[ServiceContract]
		public interface IMultipleParametersGet
		{
			[OperationContract]
			[WebGet (UriTemplate = "get")]
			void Get (string p1, string p2);
		}

		[ServiceContract]
		public interface IMultipleParameters
		{
			[OperationContract]
			[WebInvoke (UriTemplate = "posturi?p1={p1}&p2={p2}")]
			string PostUri (string p1, string p2);

			[OperationContract]
			[WebInvoke (UriTemplate = "postone?p1={p1}")]
			string PostOne (string p1, string p2);

			[OperationContract]
			[WebInvoke (UriTemplate = "postwrapped", BodyStyle=WebMessageBodyStyle.WrappedRequest)]
			string PostWrapped (string p1, string p2);

			[OperationContract]
			[WebInvoke (UriTemplate = "out?p1={p1}&p2={p2}", BodyStyle=WebMessageBodyStyle.WrappedResponse)]
			void PostOut (string p1, string p2, out string ret);
		}

		[Test]
		public void MultipleParameters ()
		{
			var behavior = new WebHttpBehaviorExt ();
			var cd = ContractDescription.GetContract (typeof (IMultipleParameters));
			var se = new ServiceEndpoint (cd, new WebHttpBinding (), new EndpointAddress ("http://localhost:8080/"));
			behavior.Validate (se);

			foreach (var od in cd.Operations)
				behavior.DoGetRequestClientFormatter (od, se);
		}

		[Test]
		[Category ("NotWorking")]
		public void MultipleParameters2 ()
		{
			var behavior = new WebHttpBehaviorExt ();
			var cd = ContractDescription.GetContract (typeof (IMultipleParametersGet));
			var se = new ServiceEndpoint (cd, new WebHttpBinding (), new EndpointAddress ("http://localhost:8080/"));
			behavior.Validate (se);

			try {
				foreach (var od in cd.Operations)
					behavior.DoGetRequestClientFormatter (od, se);
				Assert.Fail ("Should result in invalid operation");
			} catch (InvalidOperationException) {
			}
		}
	}

	[ServiceContract]
	public interface ITestService
	{
		[OperationContract]
		Stream Receive (Stream input);
	}
}
