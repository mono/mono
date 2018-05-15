//
// MsmqTransportBindingElementTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class MsmqTransportBindingElementTest
	{
		static BindingParameterCollection empty_params =
			new BindingParameterCollection ();

		[Test]
		public void DefaultValues ()
		{
			MsmqTransportBindingElement be =
				new MsmqTransportBindingElement ();
			Assert.AreEqual (8, be.MaxPoolSize, "#1");
			Assert.AreEqual (0x80000, be.MaxBufferPoolSize, "#2");
			Assert.AreEqual (QueueTransferProtocol.Native, be.QueueTransferProtocol, "#2");
			Assert.AreEqual ("net.msmq", be.Scheme, "#3");

			Assert.IsNull (be.CustomDeadLetterQueue, "#5");
			Assert.AreEqual (DeadLetterQueue.System, be.DeadLetterQueue, "#6");
			Assert.IsTrue (be.Durable, "#7");
			Assert.IsTrue (be.ExactlyOnce, "#8");
			Assert.AreEqual (0x10000, be.MaxReceivedMessageSize, "#9");
			Assert.AreEqual (2, be.MaxRetryCycles, "#10");
			Assert.AreEqual (ReceiveErrorHandling.Fault, be.ReceiveErrorHandling, "#11");
			Assert.AreEqual (5, be.ReceiveRetryCount, "#12");
			// hmm, it is documented as 10 minutes but ...
			Assert.AreEqual (TimeSpan.FromMinutes (30), be.RetryCycleDelay, "#13");
			Assert.AreEqual (TimeSpan.FromDays (1), be.TimeToLive, "#15");
			Assert.IsFalse (be.UseMsmqTracing, "#16");
			Assert.IsFalse (be.UseSourceJournal, "#17");
		}

		[Test]
		public void CanBuildChannelFactory ()
		{
			MsmqTransportBindingElement be =
				new MsmqTransportBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (), empty_params);
			Assert.IsFalse (be.CanBuildChannelFactory<IRequestChannel> (ctx), "#1");
			Assert.IsFalse (be.CanBuildChannelFactory<IInputChannel> (ctx), "#2");
			Assert.IsFalse (be.CanBuildChannelFactory<IReplyChannel> (ctx), "#3");
			Assert.IsTrue (be.CanBuildChannelFactory<IOutputChannel> (ctx), "#4");
			Assert.IsFalse (be.CanBuildChannelFactory<IRequestSessionChannel> (ctx), "#5");
			Assert.IsFalse (be.CanBuildChannelFactory<IInputSessionChannel> (ctx), "#6");
			Assert.IsFalse (be.CanBuildChannelFactory<IReplySessionChannel> (ctx), "#7");
			Assert.IsTrue (be.CanBuildChannelFactory<IOutputSessionChannel> (ctx), "#8");

			// IServiceChannel is not supported
			Assert.IsFalse (be.CanBuildChannelListener<IServiceChannel> (ctx), "#9");
		}

		[Test]
		public void CanBuildChannelListener ()
		{
			MsmqTransportBindingElement be =
				new MsmqTransportBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (), empty_params);
			Assert.IsFalse (be.CanBuildChannelListener<IReplyChannel> (ctx), "#1");
			Assert.IsFalse (be.CanBuildChannelListener<IOutputChannel> (ctx), "#2");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestChannel> (ctx), "#3");
			Assert.IsTrue (be.CanBuildChannelListener<IInputChannel> (ctx), "#4");
			Assert.IsFalse (be.CanBuildChannelListener<IReplySessionChannel> (ctx), "#5");
			Assert.IsFalse (be.CanBuildChannelListener<IOutputSessionChannel> (ctx), "#6");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestSessionChannel> (ctx), "#7");
			Assert.IsTrue (be.CanBuildChannelListener<IInputSessionChannel> (ctx), "#8");

			// IServiceChannel is not supported
			Assert.IsFalse (be.CanBuildChannelListener<IServiceChannel> (ctx), "#9");
		}

		[Test]
		public void BuildChannelFactory ()
		{
			MsmqTransportBindingElement be =
				new MsmqTransportBindingElement ();
			// Without settings them, it borks when MSMQ setup
			// does not support AD integration.
			be.MsmqTransportSecurity.MsmqAuthenticationMode =
				MsmqAuthenticationMode.None;
			be.MsmqTransportSecurity.MsmqProtectionLevel =
				ProtectionLevel.None;

			BindingContext ctx = new BindingContext (
				new CustomBinding (be),
				empty_params);
			// returns MsmqChannelFactory
			IChannelFactory<IOutputChannel> f =
				ctx.BuildInnerChannelFactory<IOutputChannel> ();
			f.Open (); // required
			IChannel c = f.CreateChannel (new EndpointAddress (
				"net.msmq://nosuchqueueexists"));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateChannelWithoutOpen ()
		{
			MsmqTransportBindingElement be =
				new MsmqTransportBindingElement ();
			// Without settings them, it borks when MSMQ setup
			// does not support AD integration.
			be.MsmqTransportSecurity.MsmqAuthenticationMode =
				MsmqAuthenticationMode.None;
			be.MsmqTransportSecurity.MsmqProtectionLevel =
				ProtectionLevel.None;

			BindingContext ctx = new BindingContext (
				new CustomBinding (be),
				empty_params);
			// returns MsmqChannelFactory
			IChannelFactory<IOutputChannel> f =
				ctx.BuildInnerChannelFactory<IOutputChannel> ();

			IChannel c = f.CreateChannel (new EndpointAddress (
				"net.msmq://nosuchqueueexists"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateChannelInvalidScheme ()
		{
			MsmqTransportBindingElement be =
				new MsmqTransportBindingElement ();
			// Without settings them, it borks when MSMQ setup
			// does not support AD integration.
			be.MsmqTransportSecurity.MsmqAuthenticationMode =
				MsmqAuthenticationMode.None;
			be.MsmqTransportSecurity.MsmqProtectionLevel =
				ProtectionLevel.None;

			BindingContext ctx = new BindingContext (
				new CustomBinding (be),
				empty_params);
			// returns MsmqChannelFactory
			IChannelFactory<IOutputChannel> f =
				ctx.BuildInnerChannelFactory<IOutputChannel> ();
			f.Open ();
			f.CreateChannel (new EndpointAddress ("stream:dummy"));
		}
	}
}
#endif
