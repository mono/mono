//
// HttpTransportBindingElementTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class HttpsTransportBindingElementTest
	{
#if !MOBILE && !XAMMAC_4_5
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BuildChannelFactoryForHttpEndpoint ()
		{
			var b = new BasicHttpBinding ();
			b.Security.Mode = BasicHttpSecurityMode.Transport;
			var cf = b.BuildChannelFactory<IRequestChannel> ();
			cf.Open ();
			cf.CreateChannel (new EndpointAddress ("http://localhost:8080"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BuildChannelListenerForHttpEndpoint ()
		{
			var b = new BasicHttpBinding ();
			b.Security.Mode = BasicHttpSecurityMode.Transport;
			b.BuildChannelListener<IReplyChannel> (new Uri ("http://localhost:8080"));
		}
#endif
		[Test]
		public void GetProperty ()
		{
			var b = new HttpsTransportBindingElement ();
			var s = b.GetProperty<ISecurityCapabilities> (new BindingContext (new CustomBinding (), new BindingParameterCollection ()));
			Assert.IsNotNull (s, "#1");
			Assert.AreEqual (ProtectionLevel.EncryptAndSign, s.SupportedRequestProtectionLevel, "#2");
			Assert.AreEqual (ProtectionLevel.EncryptAndSign, s.SupportedResponseProtectionLevel, "#3");
			Assert.IsFalse (s.SupportsClientAuthentication, "#4");
			Assert.IsFalse (s.SupportsClientWindowsIdentity, "#5");
			Assert.IsTrue (s.SupportsServerAuthentication, "#6");

			b.RequireClientCertificate = true;
			s = b.GetProperty<ISecurityCapabilities> (new BindingContext (new CustomBinding (), new BindingParameterCollection ()));
			Assert.IsTrue (s.SupportsClientAuthentication, "#7");
			Assert.IsTrue (s.SupportsClientWindowsIdentity, "#8");
		}
	}
}
