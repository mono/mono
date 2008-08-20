using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class WebHttpBehaviorTest
	{
		ServiceEndpoint CreateEndpoint ()
		{
			return new ServiceEndpoint (ContractDescription.GetContract (typeof (IMyService)), new WebHttpBinding (),
						    new EndpointAddress ("http://localhost:37564"));
		}

		[Test]
		[Category("NotWorking")]
		public void AddBiningParameters ()
		{
			var se = CreateEndpoint ();
			var b = new WebHttpBehavior ();
			var pl = new BindingParameterCollection ();
			b.AddBindingParameters (se, pl);
			Assert.AreEqual (0, pl.Count, "#1");
		}

		[Test]
		[Category ("NotWorking")]
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

		[ServiceContract]
		public interface IMyService
		{
			[OperationContract]
			[WebGet]
			string Echo (string input);
		}

		public class MyService: IMyService
		{
			[OperationBehavior]
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
	}
}
