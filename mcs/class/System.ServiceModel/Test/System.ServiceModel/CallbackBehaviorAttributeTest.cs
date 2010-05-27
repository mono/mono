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
		public void CallbackExample1 ()
		{
			//Start service and use net.tcp binding
			ServiceHost eventServiceHost = new ServiceHost (typeof (GreetingsService));
			NetTcpBinding tcpBindingpublish = new NetTcpBinding ();
			tcpBindingpublish.Security.Mode = SecurityMode.None;
			eventServiceHost.AddServiceEndpoint (typeof (IGreetings), tcpBindingpublish, "net.tcp://localhost:8000/GreetingsService");
			eventServiceHost.Open ();

			//Create client proxy
			NetTcpBinding clientBinding = new NetTcpBinding ();
			clientBinding.Security.Mode = SecurityMode.None;
			EndpointAddress ep = new EndpointAddress ("net.tcp://localhost:8000/GreetingsService");
			ClientCallback cb = new ClientCallback ();
			IGreetings proxy = DuplexChannelFactory<IGreetings>.CreateChannel (new InstanceContext (cb), clientBinding, ep);

			//Call service
			proxy.SendMessage ();

			//Wait for callback - sort of hack, but better than using wait handle to possibly block tests.
			Thread.Sleep (1000);

			//Cleanup
			eventServiceHost.Close ();

			Assert.IsTrue (CallbackSent, "#1");
			Assert.IsTrue (CallbackReceived, "#2");
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
		#endregion
	}
}
