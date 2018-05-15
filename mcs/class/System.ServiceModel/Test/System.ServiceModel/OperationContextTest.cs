//
// OperationContextTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
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
using System.IO;
using System.IdentityModel.Claims;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class OperationContextTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullConstructor ()
		{
			new OperationContext (null);
		}

		[Test]
		public void UserContextProperties ()
		{
			var ch = ChannelFactory<IFooChannel>.CreateChannel (new BasicHttpBinding (), new EndpointAddress ("http://localhost:" + NetworkHelpers.FindFreePort ()));
			var o = new OperationContext (ch);

			// FIXME: this is strange. It should return non-null value.
			// Assert.IsNull(o.Channel, "#1");

			Assert.IsNull (o.EndpointDispatcher, "#2");
			Assert.IsNull (o.Host, "#3");

			Assert.IsFalse (o.HasSupportingTokens, "#4");

			Assert.IsNull (o.IncomingMessageHeaders, "#5");
			Assert.IsNull (o.IncomingMessageProperties, "#6");
			Assert.IsNotNull (o.OutgoingMessageHeaders, "#7");
			Assert.IsNotNull (o.OutgoingMessageProperties, "#8");

			Assert.IsNull (o.InstanceContext, "#9");
			Assert.IsTrue (o.IsUserContext, "#10");
			Assert.IsNull (o.RequestContext, "#11");
			Assert.IsNull (o.ServiceSecurityContext, "#12");
			Assert.IsNull (o.SessionId, "#13");
			Assert.IsNull (o.SupportingTokens, "#14");
		}

		[ServiceContract]
		interface IFoo
		{
			[OperationContract]
			string Echo (string input);
		}

		interface IFooChannel : IFoo, IClientChannel
		{
		}

		[Test]
		public void Current ()
		{
			Assert.IsNull (OperationContext.Current, "Current-1");

			try {
				var ch = ChannelFactory<IFooChannel>.CreateChannel (new BasicHttpBinding (), new EndpointAddress ("http://localhost:" + NetworkHelpers.FindFreePort ()));
				OperationContext.Current = new OperationContext (ch);
				Assert.IsNotNull (OperationContext.Current, "Current-2");
			}
			finally {
				OperationContext.Current = null;
				Assert.IsNull (OperationContext.Current, "Current-3");
			}
		}
	}
}
#endif

