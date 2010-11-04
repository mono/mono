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

#if NET_4_0
using System.Security.Authentication.ExtendedProtection;
#endif

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class HttpTransportBindingElementTest
	{
		static BindingParameterCollection empty_params =
			new BindingParameterCollection ();

		[Test]
		public void DefaultValues ()
		{
			HttpTransportBindingElement be =
				new HttpTransportBindingElement ();
			Assert.AreEqual (false, be.AllowCookies, "#1");
			Assert.AreEqual (AuthenticationSchemes.Anonymous,
				be.AuthenticationScheme, "#2");
			Assert.AreEqual (false, be.BypassProxyOnLocal, "#3");
			Assert.AreEqual (default (HostNameComparisonMode),
				be.HostNameComparisonMode, "#4");
			Assert.AreEqual (0x10000, be.MaxBufferSize, "#6");
			Assert.IsNull (be.ProxyAddress, "#7");
			Assert.AreEqual (AuthenticationSchemes.Anonymous,
				be.ProxyAuthenticationScheme, "#8");
			Assert.AreEqual (String.Empty, be.Realm, "#9");
			Assert.AreEqual ("http", be.Scheme, "#10");
			Assert.AreEqual (default (TransferMode),
				be.TransferMode, "#11");
			Assert.AreEqual (false,
				be.UnsafeConnectionNtlmAuthentication, "#12");
			Assert.AreEqual (true, be.UseDefaultWebProxy, "#13");
		}

		[Test]
		public void CanBuildChannelFactory ()
		{
			HttpTransportBindingElement be =
				new HttpTransportBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (), empty_params);
			Assert.IsTrue (be.CanBuildChannelFactory<IRequestChannel> (ctx), "#1");
			Assert.IsFalse (be.CanBuildChannelFactory<IInputChannel> (ctx), "#2");
			Assert.IsFalse (be.CanBuildChannelFactory<IReplyChannel> (ctx), "#3");
			Assert.IsFalse (be.CanBuildChannelFactory<IOutputChannel> (ctx), "#4");
			// seems like it does not support session channels by itself ?
			Assert.IsFalse (be.CanBuildChannelFactory<IRequestSessionChannel> (ctx), "#5");
			Assert.IsFalse (be.CanBuildChannelFactory<IInputSessionChannel> (ctx), "#6");
			Assert.IsFalse (be.CanBuildChannelFactory<IReplySessionChannel> (ctx), "#7");
			Assert.IsFalse (be.CanBuildChannelFactory<IOutputSessionChannel> (ctx), "#8");

			// IServiceChannel is not supported
			Assert.IsFalse (be.CanBuildChannelListener<IServiceChannel> (ctx), "#9");
		}

		[Test]
		public void CanBuildChannelListener ()
		{
			HttpTransportBindingElement be =
				new HttpTransportBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (), empty_params);
			Assert.IsTrue (be.CanBuildChannelListener<IReplyChannel> (ctx), "#1");
			Assert.IsFalse (be.CanBuildChannelListener<IOutputChannel> (ctx), "#2");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestChannel> (ctx), "#3");
			Assert.IsFalse (be.CanBuildChannelListener<IInputChannel> (ctx), "#4");
			// seems like it does not support session channels by itself ?
			Assert.IsFalse (be.CanBuildChannelListener<IReplySessionChannel> (ctx), "#5");
			Assert.IsFalse (be.CanBuildChannelListener<IOutputSessionChannel> (ctx), "#6");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestSessionChannel> (ctx), "#7");
			Assert.IsFalse (be.CanBuildChannelListener<IInputSessionChannel> (ctx), "#8");

			// IServiceChannel is not supported
			Assert.IsFalse (be.CanBuildChannelListener<IServiceChannel> (ctx), "#9");
		}

		[Test]
		public void BuildChannelFactory ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new HttpTransportBindingElement ()),
				empty_params);
			// returns HttpChannelFactory
			IChannelFactory<IRequestChannel> f =
				ctx.BuildInnerChannelFactory<IRequestChannel> ();
			f.Open (); // required
			IChannel c = f.CreateChannel (new EndpointAddress (
				"http://www.mono-project.com"));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateChannelWithoutOpen ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new HttpTransportBindingElement ()),
				empty_params);
			// returns HttpChannelFactory
			IChannelFactory<IRequestChannel> f =
				ctx.BuildInnerChannelFactory<IRequestChannel> ();
			IChannel c = f.CreateChannel (new EndpointAddress (
				"http://www.mono-project.com"));
		}

		[Test]
		public void BuildChannelFactoryTwoHttp ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new HttpTransportBindingElement (),
					new HttpTransportBindingElement ()),
				empty_params);
			ctx.BuildInnerChannelFactory<IRequestChannel> ();
		}

		[Test]
		public void BuildChannelFactoryHttpThenMessage ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new HttpTransportBindingElement (),
					new BinaryMessageEncodingBindingElement ()),
				empty_params);
			IChannelFactory<IRequestChannel> cf =
				ctx.BuildInnerChannelFactory<IRequestChannel> ();
			cf.Open ();
		}

		[Test]
		// with July CTP it still works ...
		public void BuildChannelFactoryHttpNoMessage ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new HttpTransportBindingElement ()),
				empty_params);
			IChannelFactory<IRequestChannel> cf =
				ctx.BuildInnerChannelFactory<IRequestChannel> ();
			cf.Open ();
		}

		[Test]
		public void BuildChannelFactoryIgnoresRemaining ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new HttpTransportBindingElement (),
					new InvalidBindingElement ()),
				empty_params);
			ctx.BuildInnerChannelFactory<IRequestChannel> ();
		}

		// Disable this test anytime when HttpTransportBindingElement.BuildChannelFactory() doesn't return ChannelFactoryBase`1 anymore. It's not an API requirement.
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void BuildChannelFactory_CreateChannelNullVia ()
		{
			var ctx = new BindingContext (new CustomBinding (), empty_params);
			var cf = new HttpTransportBindingElement ().BuildChannelFactory<IRequestChannel> (ctx);
			Assert.IsTrue (cf is ChannelFactoryBase<IRequestChannel>, "#1");
			cf.Open ();
			cf.CreateChannel (new EndpointAddress ("http://localhost:8080"), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateChannelInvalidScheme ()
		{
			IChannelFactory<IRequestChannel> f = new BasicHttpBinding ().BuildChannelFactory<IRequestChannel> (new BindingParameterCollection ());
			f.Open ();
			f.CreateChannel (new EndpointAddress ("stream:dummy"));
		}

		[Test]
		public void BuildChannelListenerWithoutListenUri ()
		{
			new BasicHttpBinding ().BuildChannelListener<IReplyChannel> (new BindingParameterCollection ());
		}

		// when AddressingVersion is None (in MessageVersion), then
		// EndpointAddress.Uri and via URIs must match.
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void EndpointAddressAndViaMustMatchOnAddressingNone ()
		{
			try {
				var ch = ChannelFactory<IFoo>.CreateChannel (new BasicHttpBinding (), new EndpointAddress ("http://localhost:37564/"), new Uri ("http://localhost:8080/HogeService"));
				((ICommunicationObject) ch).Close ();
			} catch (TargetInvocationException) {
				// we throw this exception so far. Since it is
				// very internal difference (channel is created
				// inside ClientRuntimeChannel.ctor() while .NET
				// does it in ChannelFactory<T>.CreateChannel(),
				// there is no point of treating it as failure).
				throw new ArgumentException ();
			}
		}

		[Test]
		public void GetPropertyMessageVersion ()
		{
			var be = new HttpTransportBindingElement ();
			var mv = be.GetProperty<MessageVersion> (new BindingContext (new CustomBinding (), empty_params));
			Assert.AreEqual (MessageVersion.Soap12WSAddressing10, mv, "#1");
		}

		[Test]
		public void GetPrpertyBindingDeliveryCapabilities ()
		{
			var be = new HttpTransportBindingElement ();
			var dc = be.GetProperty<IBindingDeliveryCapabilities> (new BindingContext (new CustomBinding (), empty_params));
			Assert.IsFalse (dc.AssuresOrderedDelivery, "#1");
			Assert.IsFalse (dc.QueuedDelivery, "#2");
		}

		[Test]
		public void GetPrpertySecurityCapabilities ()
		{
			var be = new HttpTransportBindingElement ();
			var sec = be.GetProperty<ISecurityCapabilities> (new BindingContext (new CustomBinding (), empty_params));
			Assert.IsNotNull (sec, "#1.1");
			Assert.AreEqual (ProtectionLevel.None, sec.SupportedRequestProtectionLevel, "#1.2");
			Assert.AreEqual (ProtectionLevel.None, sec.SupportedResponseProtectionLevel, "#1.3");
			Assert.IsFalse (sec.SupportsClientAuthentication, "#1.4");
			Assert.IsFalse (sec.SupportsClientWindowsIdentity, "#1.5");
			Assert.IsFalse (sec.SupportsServerAuthentication , "#1.6");

			be = new HttpTransportBindingElement ();
			be.AuthenticationScheme = AuthenticationSchemes.Negotiate;
			sec = be.GetProperty<ISecurityCapabilities> (new BindingContext (new CustomBinding (), empty_params));
			Assert.IsNotNull (sec, "#2.1");
			Assert.AreEqual (ProtectionLevel.None, sec.SupportedRequestProtectionLevel, "#2.2");
			Assert.AreEqual (ProtectionLevel.None, sec.SupportedResponseProtectionLevel, "#2.3");
			Assert.IsTrue (sec.SupportsClientAuthentication, "#2.4");
			Assert.IsTrue (sec.SupportsClientWindowsIdentity, "#2.5");
			Assert.IsTrue (sec.SupportsServerAuthentication , "#2.6");

			// almost the same, only differ at SupportsServerAuth
			be = new HttpTransportBindingElement ();
			be.AuthenticationScheme = AuthenticationSchemes.Ntlm;
			sec = be.GetProperty<ISecurityCapabilities> (new BindingContext (new CustomBinding (), empty_params));
			Assert.IsNotNull (sec, "#3.1");
			Assert.AreEqual (ProtectionLevel.None, sec.SupportedRequestProtectionLevel, "#3.2");
			Assert.AreEqual (ProtectionLevel.None, sec.SupportedResponseProtectionLevel, "#3.3");
			Assert.IsTrue (sec.SupportsClientAuthentication, "#3.4");
			Assert.IsTrue (sec.SupportsClientWindowsIdentity, "#3.5");
			Assert.IsFalse (sec.SupportsServerAuthentication , "#3.6");

			be = new HttpTransportBindingElement ();
			be.AuthenticationScheme = AuthenticationSchemes.Basic;
			sec = be.GetProperty<ISecurityCapabilities> (new BindingContext (new CustomBinding (), empty_params));
			Assert.IsNotNull (sec, "#4.1");
			Assert.AreEqual (ProtectionLevel.None, sec.SupportedRequestProtectionLevel, "#4.2");
			Assert.AreEqual (ProtectionLevel.None, sec.SupportedResponseProtectionLevel, "#4.3");
			Assert.IsTrue (sec.SupportsClientAuthentication, "#4.4");
			Assert.IsTrue (sec.SupportsClientWindowsIdentity, "#4.5");
			Assert.IsFalse (sec.SupportsServerAuthentication , "#4.6");

			be = new HttpTransportBindingElement ();
			be.AuthenticationScheme = AuthenticationSchemes.Digest;
			sec = be.GetProperty<ISecurityCapabilities> (new BindingContext (new CustomBinding (), empty_params));
			Assert.IsNotNull (sec, "#5.1");
			Assert.AreEqual (ProtectionLevel.None, sec.SupportedRequestProtectionLevel, "#5.2");
			Assert.AreEqual (ProtectionLevel.None, sec.SupportedResponseProtectionLevel, "#5.3");
			Assert.IsTrue (sec.SupportsClientAuthentication, "#5.4");
			Assert.IsTrue (sec.SupportsClientWindowsIdentity, "#5.5");
			Assert.IsFalse (sec.SupportsServerAuthentication , "#5.6");
		}

		#region contracts

		[ServiceContract]
		interface IFoo
		{
			[OperationContract]
			string DoWork (string s1, string s2);
		}

		#endregion

		#region connection test

		string svcret;

		[Test]
		[Ignore ("It somehow fails...")]
		// It is almost identical to http-low-level-binding
		public void LowLevelHttpConnection ()
		{
			HttpTransportBindingElement lel =
				new HttpTransportBindingElement ();

			// Service
			BindingContext lbc = new BindingContext (
				new CustomBinding (),
				new BindingParameterCollection (),
				new Uri ("http://localhost:37564"),
				String.Empty, ListenUriMode.Explicit);
			listener = lel.BuildChannelListener<IReplyChannel> (lbc);

			try {

			listener.Open ();

			svcret = "";

			Thread svc = new Thread (delegate () {
				try {
					svcret = LowLevelHttpConnection_SetupService ();
				} catch (Exception ex) {
					svcret = ex.ToString ();
				}
			});
			svc.Start ();

			// Client code goes here.

			HttpTransportBindingElement el =
				new HttpTransportBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (),
				new BindingParameterCollection ());
			IChannelFactory<IRequestChannel> factory =
				el.BuildChannelFactory<IRequestChannel> (ctx);

			factory.Open ();

			IRequestChannel request = factory.CreateChannel (
				new EndpointAddress ("http://localhost:37564"));

			request.Open ();

			try {
			try {
				Message reqmsg = Message.CreateMessage (
					MessageVersion.Default, "Echo");
				// sync version does not work here.
				Message msg = request.Request (reqmsg, TimeSpan.FromSeconds (5));

				using (XmlWriter w = XmlWriter.Create (TextWriter.Null)) {
					msg.WriteMessage (w);
				}

				if (svcret != null)
					Assert.Fail (svcret.Length > 0 ? svcret : "service code did not finish until this test expected.");
			} finally {
				if (request.State == CommunicationState.Opened)
					request.Close ();
			}
			} finally {
				if (factory.State == CommunicationState.Opened)
					factory.Close ();
			}
			} finally {
				if (listener.State == CommunicationState.Opened)
					listener.Close ();
			}
		}

		IChannelListener<IReplyChannel> listener;

		string LowLevelHttpConnection_SetupService ()
		{
			IReplyChannel reply = listener.AcceptChannel ();
			reply.Open ();
			if (!reply.WaitForRequest (TimeSpan.FromSeconds (10)))
				return "No request reached here.";

			svcret = "Receiving request ...";
			RequestContext ctx = reply.ReceiveRequest ();
			if (ctx == null)
				return "No request context returned.";

			svcret = "Starting reply ...";
			ctx.Reply (Message.CreateMessage (MessageVersion.Default, "Ack"));
			return null; // OK
		}

		#endregion

		#region metadata

		[Test]
		public void ExportPolicyDefault ()
		{
			IPolicyExportExtension binding_element = new HttpTransportBindingElement ();
			PolicyConversionContext conversion_context = new CustomPolicyConversionContext ();
			binding_element.ExportPolicy (new WsdlExporter (), conversion_context);

			PolicyAssertionCollection binding_assertions = conversion_context.GetBindingAssertions ();
			BindingElementCollection binding_elements = conversion_context.BindingElements;
			Assert.AreEqual (1, binding_assertions.Count, "#A0");
			Assert.AreEqual (0, binding_elements.Count, "#A1");

			// wsaw:UsingAddressing
			XmlNode using_addressing_node = FindAssertion (binding_assertions, "wsaw:UsingAddressing");
			Assert.AreEqual (true, using_addressing_node != null, "#B0");
			Assert.AreEqual ("UsingAddressing", using_addressing_node.LocalName, "#B1");
			Assert.AreEqual ("http://www.w3.org/2006/05/addressing/wsdl", using_addressing_node.NamespaceURI, "#B2");
			Assert.AreEqual (0, using_addressing_node.Attributes.Count, "#B3");
			Assert.AreEqual (0, using_addressing_node.ChildNodes.Count, "#B4");
			Assert.AreEqual (String.Empty, using_addressing_node.InnerText, "#B5");
		}

		[Test]
		public void ExportPolicy ()
		{
			HttpTransportBindingElement http_binding_element = new HttpTransportBindingElement ();

			//
			// Specify some non-default values
			//
			http_binding_element.AllowCookies = !http_binding_element.AllowCookies;
			http_binding_element.AuthenticationScheme = AuthenticationSchemes.Ntlm;
			http_binding_element.BypassProxyOnLocal = !http_binding_element.BypassProxyOnLocal;
			http_binding_element.HostNameComparisonMode = HostNameComparisonMode.WeakWildcard;
			http_binding_element.KeepAliveEnabled = !http_binding_element.KeepAliveEnabled;
			http_binding_element.ManualAddressing = !http_binding_element.ManualAddressing;
			http_binding_element.MaxBufferPoolSize = http_binding_element.MaxBufferPoolSize / 2;
			http_binding_element.MaxBufferSize = http_binding_element.MaxBufferSize / 2;
			http_binding_element.MaxReceivedMessageSize = http_binding_element.MaxReceivedMessageSize / 2;
			http_binding_element.ProxyAddress = new Uri ("http://proxyaddress.com");
			http_binding_element.ProxyAuthenticationScheme = AuthenticationSchemes.Basic;
			http_binding_element.Realm = "RandomRealm";
			http_binding_element.TransferMode = TransferMode.Streamed;
			http_binding_element.UnsafeConnectionNtlmAuthentication = !http_binding_element.UnsafeConnectionNtlmAuthentication;
			http_binding_element.UseDefaultWebProxy = !http_binding_element.UseDefaultWebProxy;
#if NET_4_0
			http_binding_element.DecompressionEnabled = !http_binding_element.DecompressionEnabled;
			http_binding_element.ExtendedProtectionPolicy = new ExtendedProtectionPolicy (PolicyEnforcement.WhenSupported);
#endif

			// 
			// Actual call to ExportPolicy
			//
			IPolicyExportExtension binding_element = http_binding_element as IPolicyExportExtension;
			PolicyConversionContext conversion_context = new CustomPolicyConversionContext ();
			binding_element.ExportPolicy (new WsdlExporter (), conversion_context);

			PolicyAssertionCollection binding_assertions = conversion_context.GetBindingAssertions ();
			BindingElementCollection binding_elements = conversion_context.BindingElements;
			Assert.AreEqual (2, binding_assertions.Count, "#A0");
			Assert.AreEqual (0, binding_elements.Count, "#A1");

			// AuthenticationScheme - the only property that causes information to be exported.
			XmlNode authentication_node = FindAssertion (binding_assertions, "http:NtlmAuthentication");
			Assert.AreEqual (true, authentication_node != null, "#B0");
			Assert.AreEqual ("NtlmAuthentication", authentication_node.LocalName, "#B1");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/06/2004/policy/http", authentication_node.NamespaceURI, "#B2");
			Assert.AreEqual (String.Empty, authentication_node.InnerText, "#B3");
			Assert.AreEqual (0, authentication_node.Attributes.Count, "#B4");
		}

		// For some reason PolicyAssertionCollection.Find is not working as expected,
		// so do the lookup manually.
		XmlNode FindAssertion (PolicyAssertionCollection assertionCollection, string name)
		{
			foreach (XmlNode node in assertionCollection)
				if (node.Name == name)
					return node;

			return null;
		}

		#endregion
    }
}
