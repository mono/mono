//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class DiscoveryClientBindingElementTest
	{
		[Test]
		public void StaticMembers ()
		{
			Assert.AreEqual (new Uri ("http://schemas.microsoft.com/discovery/dynamic"), DiscoveryClientBindingElement.DiscoveryEndpointAddress.Uri, "#1");
		}
		
		DynamicEndpoint CreateDynamicEndpoint ()
		{
			var cd = ContractDescription.GetContract (typeof (ITestService));
			var binding = new BasicHttpBinding ();
			return new DynamicEndpoint (cd, binding);
		}

		[Test]
		public void Constructors ()
		{
			var de = CreateDynamicEndpoint ();
			var be = new DiscoveryClientBindingElement (de.DiscoveryEndpointProvider, de.FindCriteria);
			Assert.IsNotNull (be.FindCriteria, "#1");
			Assert.IsNotNull (be.DiscoveryEndpointProvider, "#2");
			var die = be.DiscoveryEndpointProvider.GetDiscoveryEndpoint ();
			Assert.AreEqual (DiscoveryVersion.WSDiscovery11, die.DiscoveryVersion, "#3");
			Assert.IsTrue (die is UdpDiscoveryEndpoint, "#3-2");
			Assert.AreEqual (ServiceDiscoveryMode.Adhoc, die.DiscoveryMode, "#4");
			Assert.AreEqual ("urn:docs-oasis-open-org:ws-dd:ns:discovery:2009:01", die.Address.Uri.ToString (), "#5");
			Assert.IsNotNull (die.Contract, "#6");
			Assert.AreEqual ("http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01", die.Contract.Namespace, "#6-2");
			Assert.AreEqual ("TargetService", die.Contract.Name, "#6-3");
			// could be either IPv4 or IPv6
			Assert.AreEqual (new UdpDiscoveryEndpoint ().MulticastAddress, die.ListenUri, "#7");
			Assert.AreEqual (ListenUriMode.Explicit, die.ListenUriMode, "#8");
			Assert.AreEqual (5, die.Behaviors.Count, "#9");

			// default constructor
			be = new DiscoveryClientBindingElement ();
			Assert.IsNotNull (be.FindCriteria, "#11");
			Assert.IsNotNull (be.DiscoveryEndpointProvider, "#12");
			die = be.DiscoveryEndpointProvider.GetDiscoveryEndpoint ();
			Assert.AreEqual (DiscoveryVersion.WSDiscovery11, die.DiscoveryVersion, "#13");
			Assert.IsTrue (die is UdpDiscoveryEndpoint, "#13-2");
		}

		[Test]
		public void CanBuildChannels ()
		{
			var de = CreateDynamicEndpoint ();
			// it is channel dependent - i.e. this binding element does not affect.
			var be = new DiscoveryClientBindingElement (de.DiscoveryEndpointProvider, de.FindCriteria);
			var bc = new BindingContext (new CustomBinding (new TcpTransportBindingElement ()), new BindingParameterCollection ());
			Assert.IsTrue (be.CanBuildChannelFactory<IDuplexSessionChannel> (bc), "#0");
			Assert.IsFalse (be.CanBuildChannelFactory<IDuplexChannel> (bc), "#1");
			Assert.IsFalse (be.CanBuildChannelFactory<IOutputChannel> (bc), "#2");
			Assert.IsFalse (be.CanBuildChannelFactory<IRequestChannel> (bc), "#3");

			// It always return false.
			Assert.IsFalse (be.CanBuildChannelListener<IDuplexSessionChannel> (bc), "#4"); // false!
			Assert.IsFalse (be.CanBuildChannelListener<IDuplexChannel> (bc), "#5");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestChannel> (bc), "#6");
			Assert.IsFalse (be.CanBuildChannelListener<IReplyChannel> (bc), "#7");

			bc = new BindingContext (new CustomBinding (new HttpTransportBindingElement ()), new BindingParameterCollection ());
			Assert.IsFalse (be.CanBuildChannelFactory<IDuplexSessionChannel> (bc), "#10");
			Assert.IsFalse (be.CanBuildChannelFactory<IDuplexChannel> (bc), "#11");
			Assert.IsFalse (be.CanBuildChannelFactory<IOutputChannel> (bc), "#12");
			Assert.IsTrue (be.CanBuildChannelFactory<IRequestChannel> (bc), "#13");

			// It always return false.
			Assert.IsFalse (be.CanBuildChannelListener<IDuplexSessionChannel> (bc), "#14");
			Assert.IsFalse (be.CanBuildChannelListener<IDuplexChannel> (bc), "#15");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestChannel> (bc), "#16");
			Assert.IsFalse (be.CanBuildChannelListener<IReplyChannel> (bc), "#17");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void BuildChannelFactory_OnlyTransport ()
		{
			var be = new DiscoveryClientBindingElement ();
			var bc = new BindingContext (new CustomBinding (new HttpTransportBindingElement ()), new BindingParameterCollection ());
			be.BuildChannelFactory<IRequestChannel> (bc);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BuildChannelFactory_CreateNonDynamicUriChannel ()
		{
			var be = new DiscoveryClientBindingElement ();
			var bc = new BindingContext (new CustomBinding (be, new HttpTransportBindingElement ()), new BindingParameterCollection ());
			var cf = be.BuildChannelFactory<IRequestChannel> (bc);
			cf.Open ();
			var uri = new Uri ("http://localhost:37564");
			cf.CreateChannel (new EndpointAddress (uri));
		}

		[Test]
		public void BuildChannelFactory ()
		{
			var be = new DiscoveryClientBindingElement ();
			var bc = new BindingContext (new CustomBinding (be, new HttpTransportBindingElement ()), new BindingParameterCollection ());
			var cf = be.BuildChannelFactory<IRequestChannel> (bc);
			cf.Open ();
			var ch = cf.CreateChannel (DiscoveryClientBindingElement.DiscoveryEndpointAddress);
			Assert.AreEqual (DiscoveryClientBindingElement.DiscoveryEndpointAddress, ch.RemoteAddress, "#1");
		}

		// This test takes a while, so I in face don't want to enable it ...
		[Test]
		[ExpectedException (typeof (EndpointNotFoundException))]
		public void RequestChannelOpenFails ()
		{
			var be = new DiscoveryClientBindingElement ();
			var bc = new BindingContext (new CustomBinding (be, new HttpTransportBindingElement ()), new BindingParameterCollection ());
			var cf = be.BuildChannelFactory<IRequestChannel> (bc);
			cf.Open ();
			Assert.IsNull (cf.GetProperty<DiscoveryEndpoint> (), "#1");
			var ch = cf.CreateChannel (DiscoveryClientBindingElement.DiscoveryEndpointAddress);
			Assert.IsNull (ch.GetProperty<DiscoveryEndpoint> (), "#2");
			ch.Open ();
		}

		[Test]
		[ExpectedException (typeof (EndpointNotFoundException))]
		public void RequestChannelOpenFails2 ()
		{
			var be = new DiscoveryClientBindingElement ();
			var bc = new BindingContext (new CustomBinding (be, new TcpTransportBindingElement ()), new BindingParameterCollection ());
			var cf = be.BuildChannelFactory<IDuplexSessionChannel> (bc);
			cf.Open ();
			var ch = cf.CreateChannel (DiscoveryClientBindingElement.DiscoveryEndpointAddress);
			ch.Open ();
		}

		[Test]
		public void CustomDiscoveryEndpointProvider ()
		{
			var be = new DiscoveryClientBindingElement () { DiscoveryEndpointProvider = new MyDiscoveryEndpointProvider () };
			var bc = new BindingContext (new CustomBinding (be, new HttpTransportBindingElement ()), new BindingParameterCollection ());
			var cf = be.BuildChannelFactory<IRequestChannel> (bc);
			cf.Open ();
			var ch = cf.CreateChannel (DiscoveryClientBindingElement.DiscoveryEndpointAddress);
			try {
				ch.Open ();
				Assert.Fail ("Should try to use failing endpoint provider.");
			} catch (MyException) {
			}
		}

		class MyDiscoveryEndpointProvider : DiscoveryEndpointProvider
		{
			public override DiscoveryEndpoint GetDiscoveryEndpoint ()
			{
				throw new MyException ();
			}
			
		}

		class MyException : Exception
		{
		}

		[Test]
		public void GetProperty ()
		{
			var be = new DiscoveryClientBindingElement ();
			var bc = new BindingContext (new CustomBinding (new HttpTransportBindingElement ()), new BindingParameterCollection ());
			// so, they are not part of GetProperty<T>() return values.
			Assert.IsNull (be.GetProperty<FindCriteria> (bc), "#1");
			Assert.IsNull (be.GetProperty<DiscoveryEndpointProvider> (bc), "#2");

			Assert.IsNull (be.GetProperty<DiscoveryEndpoint> (bc), "#3");
		}
	}
}
