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
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class UdpTransportTest
	{
		[Test]
		public void DefaultSettings ()
		{
			var binding = new UdpDiscoveryEndpoint ().Binding;
			Assert.AreEqual (TimeSpan.FromMinutes (1), binding.SendTimeout, "#1");
			Assert.AreEqual (TimeSpan.FromMinutes (10), binding.ReceiveTimeout, "#2");
		}

		[Test]
		public void CanBuildChannelFactory ()
		{
			var binding = new UdpDiscoveryEndpoint ().Binding;
			Assert.IsFalse (binding.CanBuildChannelFactory<IRequestChannel> (), "#1");
			Assert.IsFalse (binding.CanBuildChannelFactory<IRequestSessionChannel> (), "#2");
			Assert.IsTrue (binding.CanBuildChannelFactory<IDuplexChannel> (), "#3");
			Assert.IsFalse (binding.CanBuildChannelFactory<IDuplexSessionChannel> (), "#4");
			Assert.IsFalse (binding.CanBuildChannelFactory<IOutputChannel> (), "#5");
			Assert.IsFalse (binding.CanBuildChannelFactory<IOutputSessionChannel> (), "#6");
		}

		[Test]
		public void CanBuildChannelListener ()
		{
			var binding = new UdpDiscoveryEndpoint ().Binding;
			Assert.IsFalse (binding.CanBuildChannelListener<IRequestChannel> (), "#1");
			Assert.IsFalse (binding.CanBuildChannelListener<IRequestSessionChannel> (), "#2");
			Assert.IsTrue (binding.CanBuildChannelListener<IDuplexChannel> (), "#3");
			Assert.IsFalse (binding.CanBuildChannelListener<IDuplexSessionChannel> (), "#4");
			Assert.IsFalse (binding.CanBuildChannelListener<IOutputChannel> (), "#5");
			Assert.IsFalse (binding.CanBuildChannelListener<IOutputSessionChannel> (), "#6");
		}
	}
}
