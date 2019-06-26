//
// BasicHttpBindingTest.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using NUnit.Framework;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.Text;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class BasicHttpBindingTest
	{
		[Test]
		public void DefaultValues ()
		{
			BasicHttpBinding b = new BasicHttpBinding ();
			DefaultValues (b);

			// BasicHttpSecurity
			BasicHttpSecurity sec = b.Security;
			Assert.IsNotNull (sec, "#2-1");
			Assert.AreEqual (BasicHttpSecurityMode.None, sec.Mode, "#2-2");
			BasicHttpMessageSecurity msg = sec.Message;
			Assert.IsNotNull (msg, "#2-3-1");
			Assert.AreEqual (SecurityAlgorithmSuite.Default, msg.AlgorithmSuite, "#2-3-2");
			Assert.AreEqual (BasicHttpMessageCredentialType.UserName, msg.ClientCredentialType, "#2-3-3");
			HttpTransportSecurity trans = sec.Transport;
			Assert.IsNotNull (trans, "#2-4-1");
			Assert.AreEqual (HttpClientCredentialType.None, trans.ClientCredentialType, "#2-4-2");
			Assert.AreEqual (HttpProxyCredentialType.None, trans.ProxyCredentialType, "#2-4-3");
			Assert.AreEqual ("", trans.Realm, "#2-4-4");

			// Binding elements
			BindingElementCollection bec = b.CreateBindingElements ();
			Assert.AreEqual (2, bec.Count, "#5-1");
			Assert.AreEqual (typeof (TextMessageEncodingBindingElement),
				bec [0].GetType (), "#5-2");
			Assert.AreEqual (typeof (HttpTransportBindingElement),
				bec [1].GetType (), "#5-3");
		}

		[Test]
		public void DefaultValueSecurityModeMessage ()
		{
			BasicHttpBinding b = new BasicHttpBinding (BasicHttpSecurityMode.Message);
			b.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.Certificate;
			DefaultValues (b);

			// BasicHttpSecurity
			BasicHttpSecurity sec = b.Security;
			Assert.IsNotNull (sec, "#2-1");
			Assert.AreEqual (BasicHttpSecurityMode.Message, sec.Mode, "#2-2");
			BasicHttpMessageSecurity msg = sec.Message;
			Assert.IsNotNull (msg, "#2-3-1");
			Assert.AreEqual (SecurityAlgorithmSuite.Default, msg.AlgorithmSuite, "#2-3-2");
			Assert.AreEqual (BasicHttpMessageCredentialType.Certificate, msg.ClientCredentialType, "#2-3-3");
			HttpTransportSecurity trans = sec.Transport;
			Assert.IsNotNull (trans, "#2-4-1");
			Assert.AreEqual (HttpClientCredentialType.None, trans.ClientCredentialType, "#2-4-2");
			Assert.AreEqual (HttpProxyCredentialType.None, trans.ProxyCredentialType, "#2-4-3");
			Assert.AreEqual ("", trans.Realm, "#2-4-4");

			// Binding elements
			BindingElementCollection bec = b.CreateBindingElements ();
			Assert.AreEqual (3, bec.Count, "#5-1");
			Assert.AreEqual (typeof (AsymmetricSecurityBindingElement),
				bec [0].GetType (), "#5-2");
			Assert.AreEqual (typeof (TextMessageEncodingBindingElement),
				bec [1].GetType (), "#5-3");
			Assert.AreEqual (typeof (HttpTransportBindingElement),
				bec [2].GetType (), "#5-4");
		}

		void DefaultValues (BasicHttpBinding b)
		{
			Assert.AreEqual (false, b.BypassProxyOnLocal, "#1");
			Assert.AreEqual (HostNameComparisonMode.StrongWildcard,
				b.HostNameComparisonMode, "#2");
			Assert.AreEqual (0x80000, b.MaxBufferPoolSize, "#3");
			Assert.AreEqual (0x10000, b.MaxBufferSize, "#4");
			Assert.AreEqual (0x10000, b.MaxReceivedMessageSize, "#5");
			Assert.AreEqual (WSMessageEncoding.Text, b.MessageEncoding, "#6");
			Assert.IsNull (b.ProxyAddress, "#7");
			// FIXME: test b.ReaderQuotas
			Assert.AreEqual ("http", b.Scheme, "#8");
			Assert.AreEqual (EnvelopeVersion.Soap11, b.EnvelopeVersion, "#9");
			Assert.AreEqual (65001, b.TextEncoding.CodePage, "#10"); // utf-8
			Assert.AreEqual (TransferMode.Buffered, b.TransferMode, "#11");
			Assert.AreEqual (true, b.UseDefaultWebProxy, "#12");

/*
			// Interfaces
			IBindingDeliveryCapabilities ib = (IBindingDeliveryCapabilities ) b;
			Assert.AreEqual (false, ib.AssuresOrderedDelivery, "#2-1");
			Assert.AreEqual (false, ib.QueuedDelivery, "#2-3");

			IBindingMulticastCapabilities imc = (IBindingMulticastCapabilities) b;
			Assert.AreEqual (false, imc.IsMulticast, "#2.2-1");

			IBindingRuntimePreferences ir =
				(IBindingRuntimePreferences) b;
			Assert.AreEqual (false, ir.ReceiveSynchronously, "#3-1");

			ISecurityCapabilities ic = b as ISecurityCapabilities;
			Assert.AreEqual (ProtectionLevel.None,
				ic.SupportedRequestProtectionLevel, "#4-1");
			Assert.AreEqual (ProtectionLevel.None,
				ic.SupportedResponseProtectionLevel, "#4-2");
			Assert.AreEqual (false, ic.SupportsClientAuthentication, "#4-3");
			Assert.AreEqual (false, ic.SupportsClientWindowsIdentity, "#4-4");
			Assert.AreEqual (false, ic.SupportsServerAuthentication, "#4-5");
*/
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DefaultValueSecurityModeMessageError ()
		{
			BasicHttpBinding b = new BasicHttpBinding (BasicHttpSecurityMode.Message);
			// "BasicHttp binding requires that BasicHttpBinding.Security.Message.ClientCredentialType be equivalent to the BasicHttpMessageCredentialType.Certificate credential type for secure messages. Select Transport or TransportWithMessageCredential security for UserName credentials."
			b.CreateBindingElements ();
		}

		[Test]
		public void MessageEncoding ()
		{
			BasicHttpBinding b = new BasicHttpBinding ();
			foreach (BindingElement be in b.CreateBindingElements ()) {
				MessageEncodingBindingElement mbe =
					be as MessageEncodingBindingElement;
				if (mbe != null) {
					MessageEncoderFactory f = mbe.CreateMessageEncoderFactory ();
					MessageEncoder e = f.Encoder;

					Assert.AreEqual (typeof (TextMessageEncodingBindingElement), mbe.GetType (), "#1-1");
					Assert.AreEqual (MessageVersion.Soap11, f.MessageVersion, "#2-1");
					Assert.AreEqual ("text/xml; charset=utf-8", e.ContentType, "#3-1");
					Assert.AreEqual ("text/xml", e.MediaType, "#3-2");
					return;
				}
			}
			Assert.Fail ("No message encodiing binding element.");
		}

		[Test]
		public void ApplyConfiguration ()
		{
			BasicHttpBinding b = CreateBindingFromConfig ();

			Assert.AreEqual (true, b.AllowCookies, "#1");
			Assert.AreEqual (true, b.BypassProxyOnLocal, "#2");
			Assert.AreEqual (HostNameComparisonMode.Exact, b.HostNameComparisonMode, "#3");
			Assert.AreEqual (262144, b.MaxBufferPoolSize, "#4");
			Assert.AreEqual (32768, b.MaxBufferSize, "#5");
			Assert.AreEqual (32768, b.MaxReceivedMessageSize, "#6");
			Assert.AreEqual ("proxy", b.ProxyAddress.ToString (), "#7");
			Assert.AreEqual (Encoding.Unicode, b.TextEncoding, "#7");
			Assert.AreEqual (TransferMode.Streamed, b.TransferMode, "#7");
		}

		[Test]
		public void CreateBindingElements ()
		{
			BasicHttpBinding b = CreateBindingFromConfig ();

			// Binding elements
			BindingElementCollection bec = b.CreateBindingElements ();
			Assert.AreEqual (2, bec.Count, "#1");
			Assert.AreEqual (typeof (TextMessageEncodingBindingElement),
				bec [0].GetType (), "#2");
			Assert.AreEqual (typeof (HttpTransportBindingElement),
				bec [1].GetType (), "#3");
		}

		[Test]
		public void Elements_MessageEncodingBindingElement ()
		{
			BasicHttpBinding b = CreateBindingFromConfig ();
			BindingElementCollection bec = b.CreateBindingElements ();
			TextMessageEncodingBindingElement m =
				(TextMessageEncodingBindingElement) bec [0];

			Assert.AreEqual (64, m.MaxReadPoolSize, "#1");
			Assert.AreEqual (16, m.MaxWritePoolSize, "#2");
			Assert.AreEqual (4096, m.ReaderQuotas.MaxArrayLength, "#3");
			Assert.AreEqual (8192, m.ReaderQuotas.MaxBytesPerRead, "#4");
			Assert.AreEqual (64, m.ReaderQuotas.MaxDepth, "#5");
			Assert.AreEqual (8192, m.ReaderQuotas.MaxNameTableCharCount, "#6");
			Assert.AreEqual (16384, m.ReaderQuotas.MaxStringContentLength, "#7");
			Assert.AreEqual (Encoding.Unicode, m.WriteEncoding, "#8");
		}

		[Test]
		public void Elements_TransportBindingElement ()
		{
			BasicHttpBinding b = CreateBindingFromConfig ();
			BindingElementCollection bec = b.CreateBindingElements ();
			HttpTransportBindingElement t =
				(HttpTransportBindingElement) bec [1];

			Assert.AreEqual (true, t.AllowCookies, "#1");
			Assert.AreEqual (AuthenticationSchemes.Anonymous, t.AuthenticationScheme, "#2");
			Assert.AreEqual (true, t.BypassProxyOnLocal, "#3");
			Assert.AreEqual (HostNameComparisonMode.Exact, t.HostNameComparisonMode, "#4");
			Assert.AreEqual (true, t.KeepAliveEnabled, "#5");
			Assert.AreEqual (false, t.ManualAddressing, "#6");
			Assert.AreEqual (262144, t.MaxBufferPoolSize, "#7");
			Assert.AreEqual (32768, t.MaxBufferSize, "#8");
			Assert.AreEqual (32768, t.MaxReceivedMessageSize, "#9");
			Assert.AreEqual ("proxy", t.ProxyAddress.ToString (), "#10");
			Assert.AreEqual (AuthenticationSchemes.Anonymous, t.ProxyAuthenticationScheme, "#11");
			Assert.AreEqual ("", t.Realm, "#12");
			Assert.AreEqual ("http", t.Scheme, "#13");
			Assert.AreEqual (TransferMode.Streamed, t.TransferMode, "#14");
			Assert.AreEqual (false, t.UnsafeConnectionNtlmAuthentication, "#15");
			Assert.AreEqual (false, t.UseDefaultWebProxy, "#16");
		}

		[Test]
		public void SecurityMode ()
		{
			// hmm, against my expectation, those modes does not give Http(s)TransportBindingElement property differences..
			var modes = new HttpClientCredentialType [] {HttpClientCredentialType.None, HttpClientCredentialType.Basic, HttpClientCredentialType.Digest, HttpClientCredentialType.Ntlm, HttpClientCredentialType.Windows, HttpClientCredentialType.Certificate};
			foreach (var m in modes) {
				var b = new BasicHttpBinding ();
				b.Security.Mode = BasicHttpSecurityMode.Transport;
				b.Security.Transport.ClientCredentialType = m;
				var bec = b.CreateBindingElements ();
				Assert.AreEqual (2, bec.Count, "#1." + m);
				Assert.IsTrue (bec [1] is HttpsTransportBindingElement, "#2." + m);
				var tbe = (HttpsTransportBindingElement) bec [1];
				if (m == HttpClientCredentialType.Certificate)
					Assert.IsTrue (tbe.RequireClientCertificate, "#3." + m);
				else
					Assert.IsFalse (tbe.RequireClientCertificate, "#3." + m);
			}
		}

		[Test]
		public void SecurityMode2 ()
		{
			var modes = new HttpClientCredentialType [] {HttpClientCredentialType.None, HttpClientCredentialType.Basic, HttpClientCredentialType.Digest, HttpClientCredentialType.Ntlm, HttpClientCredentialType.Windows, HttpClientCredentialType.Certificate};
			foreach (var m in modes) {
				var b = new BasicHttpBinding ();
				b.Security.Mode = BasicHttpSecurityMode.TransportWithMessageCredential; // gives WS-Security message security.
				b.Security.Transport.ClientCredentialType = m;
				var bec = b.CreateBindingElements ();
				Assert.AreEqual (3, bec.Count, "#1." + m);
				Assert.IsTrue (bec [0] is TransportSecurityBindingElement, "#2." + m);
				Assert.IsTrue (bec [2] is HttpsTransportBindingElement, "#3." + m);
				var tbe = (HttpsTransportBindingElement) bec [2];
				Assert.IsFalse (tbe.RequireClientCertificate, "#4." + m);
			}
		}

		[Test]
		public void SecurityMode3 ()
		{
			var modes = new HttpClientCredentialType [] {HttpClientCredentialType.None, HttpClientCredentialType.Basic, HttpClientCredentialType.Digest, HttpClientCredentialType.Ntlm, HttpClientCredentialType.Windows};
			var auths = new AuthenticationSchemes [] { AuthenticationSchemes.Anonymous, AuthenticationSchemes.Basic, AuthenticationSchemes.Digest, AuthenticationSchemes.Ntlm, AuthenticationSchemes.Negotiate }; // specifically, none->anonymous, and windows->negotiate
			for (int i = 0; i < modes.Length; i++) {
				var m = modes [i];
				var b = new BasicHttpBinding ();
				b.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly; // gives WS-Security message security.
				b.Security.Transport.ClientCredentialType = m;
				var bec = b.CreateBindingElements ();
				Assert.AreEqual (2, bec.Count, "#1." + m);
				Assert.IsTrue (bec [1] is HttpTransportBindingElement, "#2." + m);
				var tbe = (HttpTransportBindingElement) bec [1];
				Assert.AreEqual (auths [i], tbe.AuthenticationScheme, "#3." + m);
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SecurityMode4 ()
		{
			var b = new BasicHttpBinding ();
			b.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly; // gives WS-Security message security.
			b.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
			var bec = b.CreateBindingElements ();
		}

		private BasicHttpBinding CreateBindingFromConfig ()
		{
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/basicHttpBinding")).GetSectionGroup ("system.serviceModel");
			BindingsSection section = (BindingsSection) config.Bindings;
			BasicHttpBindingElement el = section.BasicHttpBinding.Bindings ["BasicHttpBinding2_Service"];

			BasicHttpBinding b = new BasicHttpBinding ();
			el.ApplyConfiguration (b);

			return b;
		}
	}
}
#endif
