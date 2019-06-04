//
// PeerTransportBindingElementTest.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class PeerTransportBindingElementTest
	{
		[Test]
		public void CanBuildChannelFactoryListener ()
		{
			var be = new PeerTransportBindingElement ();
			var binding = new CustomBinding (new HandlerTransportBindingElement (null));
			var ctx = new BindingContext (binding, new BindingParameterCollection ());
			Assert.IsFalse (be.CanBuildChannelFactory<IRequestChannel> (ctx), "#1");
			Assert.IsTrue (be.CanBuildChannelFactory<IOutputChannel> (ctx), "#2");
			Assert.IsTrue  (be.CanBuildChannelFactory<IDuplexChannel> (ctx), "#3");
			Assert.IsFalse (be.CanBuildChannelFactory<IRequestSessionChannel> (ctx), "#4");
			Assert.IsFalse (be.CanBuildChannelFactory<IOutputSessionChannel> (ctx), "#5"); // oh?
			Assert.IsFalse (be.CanBuildChannelFactory<IDuplexSessionChannel> (ctx), "#6"); // really?

			Assert.IsFalse (be.CanBuildChannelListener<IReplyChannel> (ctx), "#7");
			Assert.IsTrue (be.CanBuildChannelListener<IInputChannel> (ctx), "#8");
			Assert.IsTrue (be.CanBuildChannelListener<IDuplexChannel> (ctx), "#9");
			Assert.IsFalse (be.CanBuildChannelListener<IReplySessionChannel> (ctx), "#10");
			Assert.IsFalse (be.CanBuildChannelListener<IInputSessionChannel> (ctx), "#11"); // hrm...
			Assert.IsFalse (be.CanBuildChannelListener<IDuplexSessionChannel> (ctx), "#12"); // ...k.
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BuildRequestChannelFactory ()
		{
			// IRequestChannel is invalid
			PeerTransportBindingElement be =
				new PeerTransportBindingElement ();
			be.Security.Mode = SecurityMode.None;
			CustomBinding binding = new CustomBinding (
				new HandlerTransportBindingElement (null));
			BindingContext ctx = new BindingContext (
				binding, new BindingParameterCollection ());

			var f = be.BuildChannelFactory<IRequestChannel> (ctx);
			Assert.IsNotNull (f.GetProperty<IOnlineStatus> (), "#1");
			Assert.IsNotNull (f.GetProperty<PeerNode> (), "#2");
		}

		[Test]
		public void BuildOutputChannelFactory ()
		{
			PeerTransportBindingElement be =
				new PeerTransportBindingElement ();
			be.Security.Mode = SecurityMode.None;
			CustomBinding binding = new CustomBinding (
				new HandlerTransportBindingElement (null));
			BindingContext ctx = new BindingContext (
				binding, new BindingParameterCollection ());
			be.BuildChannelFactory<IOutputChannel> (ctx);
		}
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BuildReplyChannelListener ()
		{
			// IReplyChannel is invalid
			PeerTransportBindingElement be =
				new PeerTransportBindingElement ();
			be.Security.Mode = SecurityMode.None;
			CustomBinding binding = new CustomBinding (
				new HandlerTransportBindingElement (null));
			BindingContext ctx = new BindingContext (
				binding, new BindingParameterCollection ());
			be.BuildChannelListener<IReplyChannel> (ctx);
		}

		[Test]
		public void BuildInputChannelListener ()
		{
			PeerTransportBindingElement be =
				new PeerTransportBindingElement ();
			be.Security.Mode = SecurityMode.None;
			CustomBinding binding = new CustomBinding (
				new HandlerTransportBindingElement (null));
			BindingContext ctx = new BindingContext (
				binding, new BindingParameterCollection ());
			ctx.ListenUriBaseAddress = new Uri ("net.p2p:foobar");
			be.BuildChannelListener<IInputChannel> (ctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void InvalidListenIPAddress ()
		{
			PeerTransportBindingElement be =
				new PeerTransportBindingElement ();
			be.Security.Mode = SecurityMode.None;
			be.ListenIPAddress = IPAddress.Parse ("127.0.0.1");
			CustomBinding binding = new CustomBinding (
				new HandlerTransportBindingElement (null));
			BindingContext ctx = new BindingContext (
				binding, new BindingParameterCollection ());
			ctx.ListenUriBaseAddress = new Uri ("net.p2p:foobar");
			be.BuildChannelListener<IInputChannel> (ctx);
		}

		[Test]
		[Ignore ("It is documented that MaxBufferPoolSize must be greater than MaxReceivedMessageSize, but not really checked (at least here)")]
		public void MaxBufferPoolSizeTooSmall ()
		{
			PeerTransportBindingElement be =
				new PeerTransportBindingElement ();
			be.MaxBufferPoolSize = 0x1000;
			be.Security.Mode = SecurityMode.None;
			CustomBinding binding = new CustomBinding (
				new HandlerTransportBindingElement (null));
			BindingContext ctx = new BindingContext (
				binding, new BindingParameterCollection ());
			ctx.ListenUriBaseAddress = new Uri ("net.p2p:foobar");
			be.BuildChannelListener<IInputChannel> (ctx);
		}
	}
}
#endif
