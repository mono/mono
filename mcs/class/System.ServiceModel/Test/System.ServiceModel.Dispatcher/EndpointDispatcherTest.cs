//
// EndpointDispatcherTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;
using SMMessage = System.ServiceModel.Channels.Message;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class EndpointDispatcherTest
	{
		[Test]
		public void Ctor ()
		{
			EndpointDispatcher d = new EndpointDispatcher (
				new EndpointAddress ("localhost:8080"),
				"test", "urn:foo");
			// properties at the Created state.
			Assert.IsTrue (d.AddressFilter is EndpointAddressMessageFilter, "#1");
			Assert.IsNull (d.ChannelDispatcher, "#2");
			Assert.IsNotNull (d.ContractFilter, "#3");
			Assert.AreEqual ("test", d.ContractName, "#4");
			Assert.AreEqual ("urn:foo", d.ContractNamespace, "#5");
			Assert.IsNotNull (d.DispatchRuntime, "#6");
			Assert.AreEqual (
				new EndpointAddress ("localhost:8080"),
				d.EndpointAddress, "#7");
			Assert.AreEqual (0, d.FilterPriority, "#8");

			Assert.IsNull (d.DispatchRuntime.OperationSelector, "#2-1");
			Assert.AreEqual (0, d.DispatchRuntime.Operations.Count, "#2-2");
		}

		[Test]
		public void MatchTest () {
			EndpointDispatcher d = new EndpointDispatcher (
				new EndpointAddress ("http://localhost:8000"), "test", "http://MonoTests.Tests");
			Message mess = Message.CreateMessage (MessageVersion.Default, "action1", (object)null);
			mess.Headers.To = new Uri ("http://localhost:8000");
			Assert.IsTrue (d.AddressFilter.Match (mess), "#1");
			mess.Headers.To = new Uri ("http://localhost:8001");
			Assert.IsFalse (d.AddressFilter.Match (mess), "#2");			
			mess.Headers.Action = "Fail";
			//MatchAllMessageFilter
			Assert.IsTrue (d.ContractFilter.Match (mess), "#3");

			d.ContractFilter = new ActionMessageFilter ("action1");
			Assert.IsFalse (d.ContractFilter.Match (mess), "#4");
			mess.Headers.Action = "action1";
			Assert.IsTrue (d.ContractFilter.Match (mess), "#5");			
		}

		[Test]
		public void ActionMessageFilterTest () {
			ServiceHost h = new ServiceHost (typeof (SpecificAction), new Uri ("http://localhost:8000"));
			EndpointDispatcher ed = new EndpointDispatcher (new EndpointAddress ("http://localhost:8000/address"),
							typeof (SpecificAction).FullName,
							typeof (SpecificAction).Namespace);
			Assert.IsTrue (ed.ContractFilter is MatchAllMessageFilter, "#1");
		}

		[Test]		
		public void DispatchRuntimeProperty () {
			ServiceHost h = new ServiceHost (typeof (SpecificAction), new Uri ("http://localhost:8000"));
			EndpointDispatcher ed = new EndpointDispatcher (new EndpointAddress ("http://localhost:8000/address"),
							typeof (SpecificAction).FullName,
							typeof (SpecificAction).Namespace);
			Assert.IsNotNull (ed.DispatchRuntime, "#1");
		}

		[ServiceContract]
		class SpecificAction
		{
			[OperationContract (Action = "Specific", ReplyAction = "*")]
			public SMMessage Get (SMMessage req) {
				return null;
			}
		}
	}
}
#endif
