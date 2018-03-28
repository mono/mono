//
// ClientBase_InteractiveChannelInitializerTest.cs
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
#if !FEATURE_NO_BSD_SOCKETS
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	// FIXME: it does not contain testcase for successful initialization yet
	// (MyChannelInitializer implementation hangs on .NET)
	public class ClientBase_InteractiveChannelInitializerTest
	{
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NotAllowedInitializationUI ()
		{
			var f = new FooProxy (new BasicHttpBinding (), new EndpointAddress ("http://localhost:" + NetworkHelpers.FindFreePort ()));
			f.Endpoint.Contract.Behaviors.Add (new MyContractBehavior ());
			f.InnerChannel.AllowInitializationUI = false;
			f.DisplayInitializationUI ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void OpenBeforeDisplayInitializationUI ()
		{
			var f = new FooProxy (new BasicHttpBinding (), new EndpointAddress ("http://localhost:" + NetworkHelpers.FindFreePort ()));
			f.Endpoint.Contract.Behaviors.Add (new MyContractBehavior ());
			f.Open ();
		}

		public class MyContractBehavior2 : MyContractBehavior
		{
		}

		public class MyContractBehavior : IContractBehavior
		{
			public void AddBindingParameters (ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
			{
			}

			public void ApplyClientBehavior (ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
			{
				clientRuntime.InteractiveChannelInitializers.Add (new MyChannelInitializer ());
			}

			public void ApplyDispatchBehavior (ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
			{
			}

			public void Validate (ContractDescription contractDescription, ServiceEndpoint endpoint)
			{
			}
		}

		public class MyChannelInitializer : IInteractiveChannelInitializer
		{
			delegate void DoWork ();
			DoWork d;
			static int instance;
			int number;

			public MyChannelInitializer ()
			{
				number = instance++;
			}

			public IAsyncResult BeginDisplayInitializationUI (IClientChannel channel, AsyncCallback callback, object state)
			{
				Console.WriteLine ("Begin");
				d = new DoWork (DisplayInitializationUI);
				return d.BeginInvoke (null, null);
			}

			public void EndDisplayInitializationUI (IAsyncResult result)
			{
				Console.WriteLine ("End");
				d.EndInvoke (result);
			}

			public void DisplayInitializationUI ()
			{
				Console.WriteLine ("Core: " + number);
			}
		}

		public class FooProxy : ClientBase<IFoo>, IFoo
		{
			public FooProxy (Binding binding, EndpointAddress address)
				: base (binding, address)
			{
			}

			public string Echo (string msg)
			{
				return Channel.Echo (msg);
			}
		}

		[ServiceContract]
		public interface IFoo
		{
			[OperationContract]
			string Echo (string input);
		}

		public class FooService : IFoo
		{
			public string Echo (string input)
			{
				return input;
			}
		}
	}
}
#endif
