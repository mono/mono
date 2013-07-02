//
// CallbackBehaviorAttributeTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Transactions;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class CallbackBehaviorAttributeTest
	{
		[Test]
		public void DefaultValues ()
		{
			var c = new CallbackBehaviorAttribute ();
			Assert.IsTrue (c.AutomaticSessionShutdown, "#1");
			Assert.AreEqual (ConcurrencyMode.Single, c.ConcurrencyMode, "#2");
			Assert.IsFalse (c.IgnoreExtensionDataObject, "#3");
			Assert.IsFalse (c.IncludeExceptionDetailInFaults, "#4");
			Assert.AreEqual (0x10000, c.MaxItemsInObjectGraph, "#5");
			Assert.AreEqual (IsolationLevel.Unspecified, c.TransactionIsolationLevel, "#6");
			Assert.IsNull (c.TransactionTimeout, "#7");
			Assert.IsTrue (c.UseSynchronizationContext, "#8");
			Assert.IsTrue (c.ValidateMustUnderstand, "#9");
		}

		[Test]
		public void AddBindingParameters ()
		{
			IEndpointBehavior eb = new CallbackBehaviorAttribute ();
			var cd = ContractDescription.GetContract (typeof (IFoo));
			var se = new ServiceEndpoint (cd);
			Assert.AreEqual (0, se.Behaviors.Count, "#1");
			var pc = new BindingParameterCollection ();
			eb.AddBindingParameters (se, pc);
			Assert.AreEqual (0, pc.Count, "#2");
			Assert.AreEqual (0, se.Behaviors.Count, "#3");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ApplyDispatchBehavior ()
		{
			// It cannot be applied to service endpoint dispatcher.
			IEndpointBehavior eb = new CallbackBehaviorAttribute () { ConcurrencyMode = ConcurrencyMode.Multiple, ValidateMustUnderstand = false };
			var cd = ContractDescription.GetContract (typeof (IFoo));
			var se = new ServiceEndpoint (cd);
			var ed = new EndpointDispatcher (new EndpointAddress ("http://localhost:37564"), "IFoo", "urn:foo");
			eb.ApplyDispatchBehavior (se, ed);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ApplyClientBehaviorNonDuplex ()
		{
			// It must be applied to duplex callback runtime
			IEndpointBehavior eb = new CallbackBehaviorAttribute () { ConcurrencyMode = ConcurrencyMode.Multiple, ValidateMustUnderstand = false };
			var cd = ContractDescription.GetContract (typeof (IFoo));
			var se = new ServiceEndpoint (cd);
			var ed = new EndpointDispatcher (new EndpointAddress ("http://localhost:37564"), "IFoo", "urn:foo");
			var cr = ed.DispatchRuntime.CallbackClientRuntime;
			eb.ApplyClientBehavior (se, cr);
		}

		/* There is no way that I can create ClientRuntime instance ...
		[Test]
		public void ApplyClientBehavior ()
		{
			IEndpointBehavior eb = new CallbackBehaviorAttribute () { ConcurrencyMode = ConcurrencyMode.Multiple, ValidateMustUnderstand = false };
			var cd = ContractDescription.GetContract (typeof (IDuplexFoo));
			var se = new ServiceEndpoint (cd);
			var ed = new EndpointDispatcher (new EndpointAddress ("http://localhost:37564"), "IDuplexFoo", "urn:foo");
			var cr = ed.DispatchRuntime.CallbackClientRuntime;
			eb.ApplyClientBehavior (se, cr);
			Assert.IsFalse (cr.ValidateMustUnderstand, "#2.1");
			//Assert.IsFalse (cr.CallbackDispatchRuntime.ValidateMustUnderstand, "#2.2");
			Assert.AreEqual (1, se.Behaviors.Count, "#3");
		}
		*/

		[ServiceContract]
		public interface IFoo
		{
			[OperationContract]
			string Join (string s1, string s2);
		}

		[ServiceContract (CallbackContract = typeof (IFoo))]
		public interface IDuplexFoo
		{
			[OperationContract]
			void Block (string s);
		}

		#region "bug #567672"
		[Test]
		[Category ("NotWorking")]
		public void CallbackExample1 ()
		{
			//Start service and use net.tcp binding
			ServiceHost eventServiceHost = new ServiceHost (typeof (GreetingsService));
			NetTcpBinding tcpBindingpublish = new NetTcpBinding ();
			tcpBindingpublish.Security.Mode = SecurityMode.None;
			eventServiceHost.AddServiceEndpoint (typeof (IGreetings), tcpBindingpublish, "net.tcp://localhost:8000/GreetingsService");
			var cd = eventServiceHost.Description.Endpoints [0].Contract;
			Assert.AreEqual (2, cd.Operations.Count, "Operations.Count");
			var send = cd.Operations.FirstOrDefault (od => od.Name == "SendMessage");
			var show = cd.Operations.FirstOrDefault (od => od.Name == "ShowMessage");
			Assert.IsNotNull (send, "OD:SendMessage");
			Assert.IsNotNull (show, "OD:ShowMessage");
			foreach (var md in send.Messages) {
				if (md.Direction == MessageDirection.Input)
					Assert.AreEqual ("http://tempuri.org/IGreetings/SendMessage", md.Action, "MD:SendMessage");
				else
					Assert.AreEqual ("http://tempuri.org/IGreetings/SendMessageResponse", md.Action, "MD:SendMessage");
			}
			foreach (var md in show.Messages) {
				if (md.Direction == MessageDirection.Output)
					Assert.AreEqual ("http://tempuri.org/IGreetings/ShowMessage", md.Action, "MD:ShowMessage");
				else
					Assert.AreEqual ("http://tempuri.org/IGreetings/ShowMessageResponse", md.Action, "MD:ShowMessage");
			}
			eventServiceHost.Open ();

			var chd = (ChannelDispatcher) eventServiceHost.ChannelDispatchers [0];
			Assert.IsNotNull (chd, "ChannelDispatcher");
			Assert.AreEqual (1, chd.Endpoints.Count, "ChannelDispatcher.Endpoints.Count");
			var ed = chd.Endpoints [0];
			var cr = ed.DispatchRuntime.CallbackClientRuntime;
			Assert.IsNotNull (cr, "CR");
			Assert.AreEqual (1, cr.Operations.Count, "CR.Operations.Count");
			Assert.AreEqual ("http://tempuri.org/IGreetings/ShowMessage", cr.Operations [0].Action, "ClientOperation.Action");

			//Create client proxy
			NetTcpBinding clientBinding = new NetTcpBinding ();
			clientBinding.Security.Mode = SecurityMode.None;
			EndpointAddress ep = new EndpointAddress ("net.tcp://localhost:8000/GreetingsService");
			ClientCallback cb = new ClientCallback ();
			IGreetings proxy = DuplexChannelFactory<IGreetings>.CreateChannel (new InstanceContext (cb), clientBinding, ep);

			//Call service
			proxy.SendMessage ();

			//Wait for callback - sort of hack, but better than using wait handle to possibly block tests.
			Thread.Sleep (2000);

			//Cleanup
			eventServiceHost.Close ();

			Assert.IsTrue (CallbackSent, "#1");
			Assert.IsTrue (CallbackReceived, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void CallbackExample2 ()
		{
			//Start service and use net.tcp binding
			ServiceHost eventServiceHost = new ServiceHost (typeof (GreetingsService2));
			NetTcpBinding tcpBindingpublish = new NetTcpBinding ();
			tcpBindingpublish.Security.Mode = SecurityMode.None;
			eventServiceHost.AddServiceEndpoint (typeof (IGreetings2), tcpBindingpublish, "net.tcp://localhost:8000/GreetingsService2");
			eventServiceHost.Open ();

			//Create client proxy
			NetTcpBinding clientBinding = new NetTcpBinding ();
			clientBinding.Security.Mode = SecurityMode.None;
			EndpointAddress ep = new EndpointAddress ("net.tcp://localhost:8000/GreetingsService2");
			ClientCallback2 cb = new ClientCallback2 ();
			IGreetings2 proxy = DuplexChannelFactory<IGreetings2>.CreateChannel (new InstanceContext (cb), clientBinding, ep);

			//Call service
			proxy.SendMessage ();

			//Wait for callback - sort of hack, but better than using wait handle to possibly block tests.
			Thread.Sleep (2000);

			//Cleanup
			eventServiceHost.Close ();

			Assert.IsTrue (CallbackSent2, "#1");
			Assert.IsTrue (CallbackReceived2, "#2");
		}

		public static bool CallbackSent, CallbackReceived;

		//Service implementation
		[ServiceBehavior (ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
		public class GreetingsService : IGreetings
		{
			public void SendMessage ()
			{
				//Make a callback
				IGreetingsCallback clientCallback = OperationContext.Current.GetCallbackChannel<IGreetingsCallback> ();

				clientCallback.ShowMessage ("Mono and WCF are GREAT!");
				CallbackBehaviorAttributeTest.CallbackSent = true;
			}
		}

		// Client callback interface implementation
		[CallbackBehavior (ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
		public class ClientCallback : IGreetingsCallback
		{
			public void ShowMessage (string message)
			{
				CallbackBehaviorAttributeTest.CallbackReceived = true;
			}
		}

		[ServiceContract (CallbackContract = typeof (IGreetingsCallback))]
		public interface IGreetings
		{
			[OperationContract (IsOneWay = true)]
			void SendMessage ();
		}

		[ServiceContract]
		public interface IGreetingsCallback
		{
			[OperationContract (IsOneWay = true)]
			void ShowMessage (string message);
		}

		public static bool CallbackSent2, CallbackReceived2;

		//Service implementation
		[ServiceBehavior (ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
		public class GreetingsService2 : IGreetings2
		{
			public void SendMessage ()
			{
				//Make a callback
				IGreetingsCallback2 clientCallback = OperationContext.Current.GetCallbackChannel<IGreetingsCallback2> ();

				clientCallback.ShowMessage ("Mono and WCF are GREAT!");
				CallbackBehaviorAttributeTest.CallbackSent2 = true;
			}
		}

		// Client callback interface implementation
		[CallbackBehavior (ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
		public class ClientCallback2 : IGreetingsCallback2
		{
			public void ShowMessage (string message)
			{
				CallbackBehaviorAttributeTest.CallbackReceived2 = true;
			}
		}

		[ServiceContract (CallbackContract = typeof (IGreetingsCallback2))]
		public interface IGreetings2
		{
			[OperationContract (IsOneWay = false)]
			void SendMessage ();
		}

		[ServiceContract]
		public interface IGreetingsCallback2
		{
			[OperationContract (IsOneWay = false)]
			void ShowMessage (string message);
		}
		#endregion

		#region ConcurrencyMode testing

		ManualResetEvent wait_handle = new ManualResetEvent (false);

		[Test]
		[Category ("NotWorking")]
		public void ConcurrencyModeSingleAndCallbackInsideServiceMethod ()
		{
			var host = new ServiceHost (typeof (TestService));
			var binding = new NetTcpBinding ();
			binding.Security.Mode = SecurityMode.None;
			host.AddServiceEndpoint (typeof (ITestService), binding, new Uri ("net.tcp://localhost:18080"));
			host.Description.Behaviors.Find<ServiceDebugBehavior> ().IncludeExceptionDetailInFaults = true;
			host.Open ();

			try {
				var cf = new DuplexChannelFactory<ITestService> (new TestCallback (), binding, new EndpointAddress ("net.tcp://localhost:18080"));
				var ch = cf.CreateChannel ();
				ch.DoWork ("test");

				wait_handle.WaitOne (10000);
				Assert.Fail ("should fail");
			} catch (FaultException) {
				// expected.
			} finally {
				Thread.Sleep (1000);
			}

			host.Close ();
		}

		[ServiceContract (CallbackContract = typeof (ITestCallback))]
		public interface ITestService
		{
			[OperationContract (IsOneWay = false)] // ConcurrencyMode error as long as IsOneWay == false.
			void DoWork (string name);
		}

		public interface ITestCallback
		{
			[OperationContract (IsOneWay = false)]
			void Report (string result);
		}

		public class TestService : ITestService
		{
			public void DoWork (string name)
			{
				var cb = OperationContext.Current.GetCallbackChannel<ITestCallback> ();
				cb.Report ("OK");
				Assert.Fail ("callback should have failed");
			}
		}

		public class TestCallback : ITestCallback
		{
			public void Report (string result)
			{
				Assert.Fail ("should not reach here");
			}
		}

		#endregion
	}
}
