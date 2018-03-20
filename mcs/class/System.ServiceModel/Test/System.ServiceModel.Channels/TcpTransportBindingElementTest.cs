//
// TcpTransportBindingElementTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.  http://www.novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class TcpTransportBindingElementTest
	{
		static BindingParameterCollection empty_params =
			new BindingParameterCollection ();

		[Test]
		public void DefaultValues ()
		{
			TcpTransportBindingElement be =
				new TcpTransportBindingElement ();
			Assert.AreEqual (TimeSpan.FromSeconds (5), be.ChannelInitializationTimeout, "#1");
			Assert.AreEqual (0x2000, be.ConnectionBufferSize, "#2");
			Assert.AreEqual (HostNameComparisonMode.StrongWildcard, be.HostNameComparisonMode, "#3");
			Assert.AreEqual (0x10000, be.MaxBufferSize, "#4");
			Assert.AreEqual (TimeSpan.FromMilliseconds (200), be.MaxOutputDelay, "#5");
			Assert.AreEqual (1, be.MaxPendingAccepts, "#6");
			Assert.AreEqual (10, be.MaxPendingConnections, "#7");
			Assert.AreEqual (TransferMode.Buffered, be.TransferMode, "#8");

			Assert.AreEqual (10, be.ListenBacklog, "#9");
			Assert.IsFalse (be.PortSharingEnabled, "#10");
			Assert.AreEqual ("net.tcp", be.Scheme, "#11");
			Assert.IsFalse (be.TeredoEnabled, "#12");
			TcpConnectionPoolSettings pool = be.ConnectionPoolSettings;
			Assert.IsNotNull (pool, "#13");
			Assert.AreEqual ("default", pool.GroupName, "#14");
			Assert.AreEqual (TimeSpan.FromSeconds (120), pool.IdleTimeout, "#15");
			Assert.AreEqual (TimeSpan.FromSeconds (300), pool.LeaseTimeout, "#16");
			Assert.AreEqual (10, pool.MaxOutboundConnectionsPerEndpoint, "#17");
		}

		[Test]
		public void CanBuildChannelFactory ()
		{
			TcpTransportBindingElement be =
				new TcpTransportBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (), empty_params);
			Assert.IsFalse (be.CanBuildChannelFactory<IRequestChannel> (ctx), "#1");
			Assert.IsFalse (be.CanBuildChannelFactory<IInputChannel> (ctx), "#2");
			Assert.IsFalse (be.CanBuildChannelFactory<IReplyChannel> (ctx), "#3");
			Assert.IsFalse (be.CanBuildChannelFactory<IOutputChannel> (ctx), "#4");

			Assert.IsFalse (be.CanBuildChannelFactory<IRequestSessionChannel> (ctx), "#5");
			Assert.IsFalse (be.CanBuildChannelFactory<IInputSessionChannel> (ctx), "#6");
			Assert.IsFalse (be.CanBuildChannelFactory<IReplySessionChannel> (ctx), "#7");
			Assert.IsFalse (be.CanBuildChannelFactory<IOutputSessionChannel> (ctx), "#8");

			// IServiceChannel is not supported
			Assert.IsFalse (be.CanBuildChannelFactory<IServiceChannel> (ctx), "#9");
			Assert.IsFalse (be.CanBuildChannelFactory<IClientChannel> (ctx), "#10");

			Assert.IsFalse (be.CanBuildChannelFactory<IDuplexChannel> (ctx), "#11");
			Assert.IsTrue (be.CanBuildChannelFactory<IDuplexSessionChannel> (ctx), "#12");
		}

		[Test]
		public void CanBuildChannelListener ()
		{
			TcpTransportBindingElement be =
				new TcpTransportBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (), empty_params);
			Assert.IsFalse (be.CanBuildChannelListener<IReplyChannel> (ctx), "#1");
			Assert.IsFalse (be.CanBuildChannelListener<IOutputChannel> (ctx), "#2");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestChannel> (ctx), "#3");
			Assert.IsFalse (be.CanBuildChannelListener<IInputChannel> (ctx), "#4");

			Assert.IsFalse (be.CanBuildChannelListener<IReplySessionChannel> (ctx), "#5");
			Assert.IsFalse (be.CanBuildChannelListener<IOutputSessionChannel> (ctx), "#6");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestSessionChannel> (ctx), "#7");
			Assert.IsFalse (be.CanBuildChannelListener<IInputSessionChannel> (ctx), "#8");

			// IServiceChannel is not supported
			Assert.IsFalse (be.CanBuildChannelListener<IServiceChannel> (ctx), "#9");
			Assert.IsFalse (be.CanBuildChannelListener<IClientChannel> (ctx), "#10");

			Assert.IsFalse (be.CanBuildChannelListener<IDuplexChannel> (ctx), "#11");
			Assert.IsTrue (be.CanBuildChannelListener<IDuplexSessionChannel> (ctx), "#12");
		}

		[Test]
		public void CanBuildChannelListener2 ()
		{
			TcpTransportBindingElement be =
				new TcpTransportBindingElement ();
			be.TransferMode = TransferMode.Streamed;
			BindingContext ctx = new BindingContext (
				new CustomBinding (), empty_params);
			Assert.IsTrue (be.CanBuildChannelListener<IReplyChannel> (ctx), "#1");
			Assert.IsFalse (be.CanBuildChannelListener<IOutputChannel> (ctx), "#2");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestChannel> (ctx), "#3");
			Assert.IsFalse (be.CanBuildChannelListener<IInputChannel> (ctx), "#4");

			Assert.IsFalse (be.CanBuildChannelListener<IReplySessionChannel> (ctx), "#5");
			Assert.IsFalse (be.CanBuildChannelListener<IOutputSessionChannel> (ctx), "#6");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestSessionChannel> (ctx), "#7");
			Assert.IsFalse (be.CanBuildChannelListener<IInputSessionChannel> (ctx), "#8");

			// IServiceChannel is not supported
			Assert.IsFalse (be.CanBuildChannelListener<IServiceChannel> (ctx), "#9");
			Assert.IsFalse (be.CanBuildChannelListener<IClientChannel> (ctx), "#10");

			Assert.IsFalse (be.CanBuildChannelListener<IDuplexChannel> (ctx), "#11");
			Assert.IsFalse (be.CanBuildChannelListener<IDuplexSessionChannel> (ctx), "#12");
		}

		[Test]
		public void GetPrpertyBindingDeliveryCapabilities ()
		{
			var be = new TcpTransportBindingElement ();
			var dc = be.GetProperty<IBindingDeliveryCapabilities> (new BindingContext (new CustomBinding (), new BindingParameterCollection ()));
			Assert.IsTrue (dc.AssuresOrderedDelivery, "#1");
			Assert.IsFalse (dc.QueuedDelivery, "#2");
		}

		[Test]
		public void GetPropertySecurityCapabilities ()
		{
			var b = new TcpTransportBindingElement ();
			var s = b.GetProperty<ISecurityCapabilities> (new BindingContext (new CustomBinding (), new BindingParameterCollection ()));
			Assert.IsNull (s, "#1");
		}

		[Test]
		public void SimpleDuplexBuffered () // sample[svc|cli]4.exe
		{
			ServiceHost host = new ServiceHost (typeof (Foo));
			var bindingS = new CustomBinding (new BindingElement [] {
				new BinaryMessageEncodingBindingElement (),
				new TcpTransportBindingElement () });
			bindingS.ReceiveTimeout = TimeSpan.FromSeconds (5);
			bindingS.OpenTimeout = TimeSpan.FromSeconds (20);
			int port = NetworkHelpers.FindFreePort ();
			host.AddServiceEndpoint (typeof (IFoo),
				bindingS, new Uri ("net.tcp://localhost:" + port));
			host.Description.Behaviors.Find<ServiceBehaviorAttribute> ().IncludeExceptionDetailInFaults = true;
			host.Open ();

			try {
				for (int i = 0; i < 2; i++) {
					var bindingC = new NetTcpBinding ();
					bindingC.Security.Mode = SecurityMode.None;
					IFooChannel proxy = new ChannelFactory<IFooChannel> (bindingC, new EndpointAddress ("net.tcp://localhost:" + port + "/")).CreateChannel ();
					proxy.Open ();
					try {
						Assert.AreEqual ("TEST FOR ECHO", proxy.Echo ("TEST FOR ECHO"), "#1");
						var sid = proxy.SessionId;
						Assert.AreEqual (3000, proxy.Add (1000, 2000), "#2");
						Assert.AreEqual (sid, proxy.SessionId, "#3");
					} finally {
						proxy.Close ();
					}
				}
			} catch (Exception ex) {
				Console.Error.WriteLine (ex);
			} finally {
				host.Close ();
			}
		}

		[Test]
		[Category ("NotWorking")]
		// FIXME: To enable this, we must fix the "request-reply TCP channel must close the connection" issue.
		// It is strange, but the standalone test just works.
		public void SimpleRequestReplyStreamed () // sample[svc|cli]5.exe
		{
			ServiceHost host = new ServiceHost (typeof (Foo));
			NetTcpBinding bindingS = new NetTcpBinding ();
			bindingS.TransferMode = TransferMode.Streamed;
			bindingS.Security.Mode = SecurityMode.None;
			bindingS.ReceiveTimeout = TimeSpan.FromSeconds (5);
			bindingS.OpenTimeout = TimeSpan.FromSeconds (20);
			int port = NetworkHelpers.FindFreePort ();
			host.AddServiceEndpoint (typeof (IFoo),
				bindingS, new Uri ("net.tcp://localhost:" + port));
			host.Description.Behaviors.Find<ServiceBehaviorAttribute> ().IncludeExceptionDetailInFaults = true;
			host.Open ();

			try {
				for (int i = 0; i < 2; i++) {
					var bindingC = new NetTcpBinding ();
					bindingS.TransferMode = TransferMode.Streamed;
					bindingC.Security.Mode = SecurityMode.None;
					IFooChannel proxy = new ChannelFactory<IFooChannel> (bindingC, new EndpointAddress ("net.tcp://localhost:" + port + "/")).CreateChannel ();
					proxy.Open ();
					try {
						Assert.AreEqual ("TEST FOR ECHO", proxy.Echo ("TEST FOR ECHO"), "#1");
						Assert.AreEqual (3000, proxy.Add (1000, 2000), "#2");
					} finally {
						proxy.Close ();
					}
				}
			} finally {
				host.Close ();
			}
		}

		[ServiceContract]
		public interface IFoo
		{
			[OperationContract]
			string Echo (string msg);

			[OperationContract]
			uint Add (uint v1, uint v2);
		}

		public interface IFooChannel : IFoo, IClientChannel
		{
		}

		class Foo : IFoo
		{
			public string Echo (string msg)
			{
				return msg;
			}
			
			public uint Add (uint v1, uint v2)
			{
				return v1 + v2;
			}
		}
	}
}
#endif