//
// MetadataResolverTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class MetadataExchangeBindingsTest
	{
		Uri CreateUri (string uriString)
		{
			var uri = new Uri (uriString);
			var l = new TcpListener (uri.Port);
			l.Start ();
			l.Stop ();
			return uri;
		}

		[Test]
		public void CreateMexHttpBinding ()
		{
			var b = MetadataExchangeBindings.CreateMexHttpBinding () as WSHttpBinding;
			Assert.IsNotNull (b, "#1");
			Assert.AreEqual (SecurityMode.None, b.Security.Mode, "#2");
			Assert.IsFalse (b.TransactionFlow, "#3");
			Assert.IsFalse (b.ReliableSession.Enabled, "#4");
			Assert.IsFalse (b.CreateBindingElements ().Any (be => be is SecurityBindingElement), "#b1");
			Assert.IsTrue (b.CreateBindingElements ().Any (be => be is TransactionFlowBindingElement), "#b2");
			Assert.IsFalse (b.CreateBindingElements ().Any (be => be is ReliableSessionBindingElement), "#b3");
			Assert.IsTrue (new TransactionFlowBindingElement ().TransactionProtocol == TransactionProtocol.Default, "#x1");
			Assert.AreEqual (MessageVersion.Soap12WSAddressing10, b.MessageVersion, "#5");
			Assert.AreEqual (MessageVersion.Soap12WSAddressing10, b.GetProperty<MessageVersion> (new BindingParameterCollection ()), "#6");

			var host = new ServiceHost (typeof (MetadataExchange));
			host.AddServiceEndpoint (typeof (IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding (), CreateUri ("http://localhost:8080"));
			host.Open ();
			try {
				// it still does not rewrite MessageVersion.None. It's rather likely ServiceMetadataExtension which does overwriting.
				Assert.AreEqual (MessageVersion.Soap12WSAddressing10, ((ChannelDispatcher) host.ChannelDispatchers [0]).MessageVersion, "#7");
			} finally {
				host.Close ();
			}
		}
		
		public class MetadataExchange : IMetadataExchange
		{
			public Message Get (Message request)
			{
				throw new Exception ();
			}
			
			public IAsyncResult BeginGet (Message request, AsyncCallback callback, object state)
			{
				throw new Exception ();
			}
			
			public Message EndGet (IAsyncResult result)
			{
				throw new Exception ();
			}
		}
	}
}
