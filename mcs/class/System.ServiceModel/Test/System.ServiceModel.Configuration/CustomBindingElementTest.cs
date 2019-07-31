//
// AddressHeaderCollectionElementTest.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;
using NUnit.Framework;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;
using System.Net;
using System.ServiceModel;
using System.Net.Security;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class CustomBindingElementTest
	{
		CustomBindingCollectionElement OpenConfig () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/customBinding")).GetSectionGroup ("system.serviceModel");
			Assert.AreEqual (7, config.Bindings.CustomBinding.Bindings.Count, "CustomBinding count");
			return config.Bindings.CustomBinding;
		}

		T GetElement<T> (int index) where T : BindingElementExtensionElement {
			CustomBindingElement binding = OpenConfig ().Bindings [index];
			T element = (T) binding [typeof (T)];
			Assert.IsNotNull (element, typeof (T).Name + " is not exist in collection.");
			return element;
		}

		[Test]
		public void CustomBindingElement () {
			CustomBindingElement binding = OpenConfig ().Bindings [0];

			Assert.AreEqual ("CustomBinding_1", binding.Name, "Name");
			Assert.AreEqual (new TimeSpan (0, 2, 0), binding.CloseTimeout, "CloseTimeout");
			Assert.AreEqual (new TimeSpan (0, 2, 0), binding.OpenTimeout, "OpenTimeout");
			Assert.AreEqual (new TimeSpan (0, 20, 0), binding.ReceiveTimeout, "ReceiveTimeout");
			Assert.AreEqual (new TimeSpan (0, 2, 0), binding.SendTimeout, "SendTimeout");

		}

		[Test]
		public void BinaryMessageEncodingElement () {

			BinaryMessageEncodingElement binaryMessageEncoding = GetElement<BinaryMessageEncodingElement> (0);

			Assert.AreEqual (typeof (BinaryMessageEncodingBindingElement), binaryMessageEncoding.BindingElementType, "BindingElementType");
			Assert.AreEqual ("binaryMessageEncoding", binaryMessageEncoding.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (128, binaryMessageEncoding.MaxReadPoolSize, "MaxReadPoolSize");
			Assert.AreEqual (1024, binaryMessageEncoding.MaxSessionSize, "MaxSessionSize");
			Assert.AreEqual (32, binaryMessageEncoding.MaxWritePoolSize, "MaxWritePoolSize");
			Assert.AreEqual (1024, binaryMessageEncoding.ReaderQuotas.MaxArrayLength, "ReaderQuotas.MaxArrayLength");
			Assert.AreEqual (1024, binaryMessageEncoding.ReaderQuotas.MaxBytesPerRead, "ReaderQuotas.MaxBytesPerRead");
			Assert.AreEqual (1024, binaryMessageEncoding.ReaderQuotas.MaxDepth, "ReaderQuotas.MaxDepth");
			Assert.AreEqual (1024, binaryMessageEncoding.ReaderQuotas.MaxNameTableCharCount, "ReaderQuotas.MaxNameTableCharCount");
			Assert.AreEqual (1024, binaryMessageEncoding.ReaderQuotas.MaxStringContentLength, "ReaderQuotas.MaxStringContentLength");
		}

		[Test]
		public void CompositeDuplexElement () {
			CompositeDuplexElement element = GetElement<CompositeDuplexElement> (0);

			Assert.AreEqual (typeof (CompositeDuplexBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("compositeDuplex", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual ("http://client.base.address", element.ClientBaseAddress.OriginalString, "ClientBaseAddress");
		}

		[Test]
		public void OneWayElement () {
			OneWayElement element = GetElement<OneWayElement> (0);

			Assert.AreEqual (typeof (OneWayBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("oneWay", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (5, element.MaxAcceptedChannels, "MaxAcceptedChannels");
			Assert.AreEqual (true, element.PacketRoutable, "PacketRoutable");

			Assert.AreEqual (new TimeSpan (0, 1, 0), element.ChannelPoolSettings.IdleTimeout, "ChannelPoolSettings.IdleTimeout");
			Assert.AreEqual (new TimeSpan (0, 12, 0), element.ChannelPoolSettings.LeaseTimeout, "ChannelPoolSettings.LeaseTimeout");
			Assert.AreEqual (5, element.ChannelPoolSettings.MaxOutboundChannelsPerEndpoint, "ChannelPoolSettings.MxOutboundChannelsPerEndpoint");
		}

		[Test]
		public void HttpTransportElement () {
			HttpTransportElement element = GetElement<HttpTransportElement> (0);

			Assert.AreEqual (typeof (HttpTransportBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("httpTransport", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.ManualAddressing, "ManualAddressing");
			Assert.AreEqual (262144, element.MaxBufferPoolSize, "MaxBufferPoolSize");
			Assert.AreEqual (32768, element.MaxReceivedMessageSize, "MaxReceivedMessageSize");
			Assert.AreEqual (true, element.AllowCookies, "AllowCookies");
			Assert.AreEqual (AuthenticationSchemes.None, element.AuthenticationScheme, "AuthenticationScheme");
			Assert.AreEqual (true, element.BypassProxyOnLocal, "BypassProxyOnLocal");
			Assert.AreEqual (HostNameComparisonMode.Exact, element.HostNameComparisonMode, "HostNameComparisonMode");
			Assert.AreEqual (false, element.KeepAliveEnabled, "KeepAliveEnabled");
			Assert.AreEqual (32768, element.MaxBufferSize, "MaxBufferSize");
			Assert.AreEqual ("http://proxy.address", element.ProxyAddress.OriginalString, "ProxyAddress");
			Assert.AreEqual (AuthenticationSchemes.None, element.ProxyAuthenticationScheme, "ProxyAuthenticationScheme");
			Assert.AreEqual ("Realm", element.Realm, "Realm");
			Assert.AreEqual (TransferMode.Streamed, element.TransferMode, "TransferMode");
			Assert.AreEqual (true, element.UnsafeConnectionNtlmAuthentication, "UnsafeConnectionNtlmAuthentication");
			Assert.AreEqual (false, element.UseDefaultWebProxy, "UseDefaultWebProxy");
		}

		[Test]
		public void HttpsTransportElement () {
			HttpsTransportElement element = GetElement<HttpsTransportElement> (1);

			Assert.AreEqual (typeof (HttpsTransportBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("httpsTransport", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.ManualAddressing, "ManualAddressing");
			Assert.AreEqual (262144, element.MaxBufferPoolSize, "MaxBufferPoolSize");
			Assert.AreEqual (32768, element.MaxReceivedMessageSize, "MaxReceivedMessageSize");
			Assert.AreEqual (true, element.AllowCookies, "AllowCookies");
			Assert.AreEqual (AuthenticationSchemes.None, element.AuthenticationScheme, "AuthenticationScheme");
			Assert.AreEqual (true, element.BypassProxyOnLocal, "BypassProxyOnLocal");
			Assert.AreEqual (HostNameComparisonMode.Exact, element.HostNameComparisonMode, "HostNameComparisonMode");
			Assert.AreEqual (true, element.KeepAliveEnabled, "KeepAliveEnabled");
			Assert.AreEqual (32768, element.MaxBufferSize, "MaxBufferSize");
			Assert.AreEqual ("https://proxy.address", element.ProxyAddress.OriginalString, "ProxyAddress");
			Assert.AreEqual (AuthenticationSchemes.None, element.ProxyAuthenticationScheme, "ProxyAuthenticationScheme");
			Assert.AreEqual ("Realm", element.Realm, "Realm");
			Assert.AreEqual (TransferMode.Streamed, element.TransferMode, "TransferMode");
			Assert.AreEqual (true, element.UnsafeConnectionNtlmAuthentication, "UnsafeConnectionNtlmAuthentication");
			Assert.AreEqual (false, element.UseDefaultWebProxy, "UseDefaultWebProxy");
		}

		[Test]
		public void PnrpPeerResolverElement () {
			PnrpPeerResolverElement element = GetElement<PnrpPeerResolverElement> (0);

			Assert.AreEqual (typeof (PnrpPeerResolverBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("pnrpPeerResolver", element.ConfigurationElementName, "ConfigurationElementName");
		}

		[Test]
		public void PrivacyNoticeElement () {
			PrivacyNoticeElement element = GetElement<PrivacyNoticeElement> (0);

			Assert.AreEqual (typeof (PrivacyNoticeBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("privacyNoticeAt", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual ("http://url", element.Url.OriginalString, "Url");
			Assert.AreEqual (5, element.Version, "Version");
		}

		[Test]
		public void ReliableSessionElement () {
			ReliableSessionElement element = GetElement<ReliableSessionElement> (0);

			Assert.AreEqual (typeof (ReliableSessionBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("reliableSession", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (TimeSpan.Parse ("00:00:00.4000000"), element.AcknowledgementInterval, "AcknowledgementInterval");
			Assert.AreEqual (false, element.FlowControlEnabled, "FlowControlEnabled");
			Assert.AreEqual (new TimeSpan (0, 15, 0), element.InactivityTimeout, "InactivityTimeout");
			Assert.AreEqual (8, element.MaxPendingChannels, "MaxPendingChannels");
			Assert.AreEqual (16, element.MaxRetryCount, "MaxRetryCount");
			Assert.AreEqual (16, element.MaxTransferWindowSize, "MaxTransferWindowSize");
			Assert.AreEqual (false, element.Ordered, "Ordered");
			Assert.AreEqual (ReliableMessagingVersion.WSReliableMessaging11, element.ReliableMessagingVersion, "ReliableMessagingVersion");
		}

		[Test]
		public void SecurityElement () {
			SecurityElement element = GetElement<SecurityElement> (0);

			Assert.AreEqual (typeof (SecurityBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("security", element.ConfigurationElementName, "ConfigurationElementName");

			// TODO
		}

		[Test]
		public void SslStreamSecurityElement () {
			SslStreamSecurityElement element = GetElement<SslStreamSecurityElement> (0);

			Assert.AreEqual (typeof (SslStreamSecurityBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("sslStreamSecurity", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.RequireClientCertificate, "RequireClientCertificate");
		}

		[Test]
		public void TransactionFlowElement () {
			TransactionFlowElement element = GetElement<TransactionFlowElement> (0);

			Assert.AreEqual (typeof (TransactionFlowBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("transactionFlow", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (TransactionProtocol.WSAtomicTransactionOctober2004, element.TransactionProtocol, "TransactionProtocol");
		}

		[Test]
		public void UseManagedPresentationElement () {
			UseManagedPresentationElement element = GetElement<UseManagedPresentationElement> (0);

			Assert.AreEqual (typeof (UseManagedPresentationBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("useManagedPresentation", element.ConfigurationElementName, "ConfigurationElementName");
		}

		[Test]
		public void WindowsStreamSecurityElement () {
			WindowsStreamSecurityElement element = GetElement<WindowsStreamSecurityElement> (1);

			Assert.AreEqual (typeof (WindowsStreamSecurityBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("windowsStreamSecurity", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (ProtectionLevel.None, element.ProtectionLevel, "ProtectionLevel");
		}

		[Test]
		public void TextMessageEncodingElement () {
			TextMessageEncodingElement element = GetElement<TextMessageEncodingElement> (1);

			Assert.AreEqual (typeof (TextMessageEncodingBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("textMessageEncoding", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (128, element.MaxReadPoolSize, "MaxReadPoolSize");
			Assert.AreEqual (Encoding.UTF32, element.WriteEncoding, "WriteEncoding");
			Assert.AreEqual (MessageVersion.Soap11WSAddressingAugust2004, element.MessageVersion, "MessageVersion");
			Assert.AreEqual (32, element.MaxWritePoolSize, "MaxWritePoolSize");
			Assert.AreEqual (128, element.ReaderQuotas.MaxArrayLength, "ReaderQuotas.MaxArrayLength");
			Assert.AreEqual (128, element.ReaderQuotas.MaxBytesPerRead, "ReaderQuotas.MaxBytesPerRead");
			Assert.AreEqual (128, element.ReaderQuotas.MaxDepth, "ReaderQuotas.MaxDepth");
			Assert.AreEqual (128, element.ReaderQuotas.MaxNameTableCharCount, "ReaderQuotas.MaxNameTableCharCount");
			Assert.AreEqual (128, element.ReaderQuotas.MaxStringContentLength, "ReaderQuotas.MaxStringContentLength");
		}

		[Test]
		public void MtomMessageEncodingElement () {
			MtomMessageEncodingElement element = GetElement<MtomMessageEncodingElement> (2);

			Assert.AreEqual (typeof (MtomMessageEncodingBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("mtomMessageEncoding", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (128, element.MaxReadPoolSize, "MaxReadPoolSize");
			Assert.AreEqual (32, element.MaxWritePoolSize, "MaxWritePoolSize");
			Assert.AreEqual (32768, element.MaxBufferSize, "MaxBufferSize");
			Assert.AreEqual (Encoding.UTF32, element.WriteEncoding, "WriteEncoding");
			Assert.AreEqual (MessageVersion.Soap11WSAddressingAugust2004, element.MessageVersion, "MessageVersion");
			Assert.AreEqual (256, element.ReaderQuotas.MaxArrayLength, "ReaderQuotas.MaxArrayLength");
			Assert.AreEqual (256, element.ReaderQuotas.MaxBytesPerRead, "ReaderQuotas.MaxBytesPerRead");
			Assert.AreEqual (256, element.ReaderQuotas.MaxDepth, "ReaderQuotas.MaxDepth");
			Assert.AreEqual (256, element.ReaderQuotas.MaxNameTableCharCount, "ReaderQuotas.MaxNameTableCharCount");
			Assert.AreEqual (256, element.ReaderQuotas.MaxStringContentLength, "ReaderQuotas.MaxStringContentLength");
		}

		[Test]
		public void MsmqIntegrationElement () {
			MsmqIntegrationElement element = GetElement<MsmqIntegrationElement> (2);

			Assert.AreEqual (typeof (global::System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("msmqIntegration", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.ManualAddressing, "ManualAddressing");
			Assert.AreEqual (262144, element.MaxBufferPoolSize, "MaxBufferPoolSize");
			Assert.AreEqual (32768, element.MaxReceivedMessageSize, "MaxReceivedMessageSize");
			Assert.AreEqual ("http://CustomDeadLetterQueue", element.CustomDeadLetterQueue.OriginalString, "CustomDeadLetterQueue");
			Assert.AreEqual (DeadLetterQueue.Custom, element.DeadLetterQueue, "DeadLetterQueue");
			Assert.AreEqual (false, element.Durable, "Durable");
			Assert.AreEqual (false, element.ExactlyOnce, "ExactlyOnce");
			Assert.AreEqual (3, element.MaxRetryCycles, "MaxRetryCycles");
			Assert.AreEqual (ReceiveErrorHandling.Drop, element.ReceiveErrorHandling, "ReceiveErrorHandling");
			Assert.AreEqual (10, element.ReceiveRetryCount, "ReceiveRetryCount");
			Assert.AreEqual (new TimeSpan (0, 15, 0), element.RetryCycleDelay, "RetryCycleDelay");
			Assert.AreEqual (TimeSpan.Parse ("1.12:00:00"), element.TimeToLive, "TimeToLive");
			Assert.AreEqual (true, element.UseSourceJournal, "UseSourceJournal");
			Assert.AreEqual (true, element.UseMsmqTracing, "UseMsmqTracing");
			Assert.AreEqual (global::System.ServiceModel.MsmqIntegration.MsmqMessageSerializationFormat.Binary, element.SerializationFormat, "SerializationFormat");
			Assert.AreEqual (MsmqAuthenticationMode.Certificate, element.MsmqTransportSecurity.MsmqAuthenticationMode, "MsmqTransportSecurity.MsmqAuthenticationMode");
			Assert.AreEqual (MsmqEncryptionAlgorithm.Aes, element.MsmqTransportSecurity.MsmqEncryptionAlgorithm, "MsmqTransportSecurity.MsmqEncryptionAlgorithm");
			Assert.AreEqual (ProtectionLevel.EncryptAndSign, element.MsmqTransportSecurity.MsmqProtectionLevel, "MsmqTransportSecurity.MsmqProtectionLevel");
			Assert.AreEqual (MsmqSecureHashAlgorithm.Sha256, element.MsmqTransportSecurity.MsmqSecureHashAlgorithm, "MsmqTransportSecurity.MsmqSecureHashAlgorithm");
		}

		[Test]
		public void MsmqTransportElement () {
			MsmqTransportElement element = GetElement<MsmqTransportElement> (3);

			Assert.AreEqual (typeof (MsmqTransportBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("msmqTransport", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.ManualAddressing, "ManualAddressing");
			Assert.AreEqual (262144, element.MaxBufferPoolSize, "MaxBufferPoolSize");
			Assert.AreEqual (262144, element.MaxReceivedMessageSize, "MaxReceivedMessageSize");
			Assert.AreEqual ("http://CustomDeadLetterQueue", element.CustomDeadLetterQueue.OriginalString, "CustomDeadLetterQueue");
			Assert.AreEqual (DeadLetterQueue.Custom, element.DeadLetterQueue, "DeadLetterQueue");
			Assert.AreEqual (false, element.Durable, "Durable");
			Assert.AreEqual (false, element.ExactlyOnce, "ExactlyOnce");
			Assert.AreEqual (3, element.MaxRetryCycles, "MaxRetryCycles");
			Assert.AreEqual (ReceiveErrorHandling.Drop, element.ReceiveErrorHandling, "ReceiveErrorHandling");
			Assert.AreEqual (9, element.ReceiveRetryCount, "ReceiveRetryCount");
			Assert.AreEqual (new TimeSpan (0, 15, 0), element.RetryCycleDelay, "RetryCycleDelay");
			Assert.AreEqual (TimeSpan.Parse ("1.12:00:00"), element.TimeToLive, "TimeToLive");
			Assert.AreEqual (true, element.UseSourceJournal, "UseSourceJournal");
			Assert.AreEqual (true, element.UseMsmqTracing, "UseMsmqTracing");
			Assert.AreEqual (MsmqAuthenticationMode.Certificate, element.MsmqTransportSecurity.MsmqAuthenticationMode, "MsmqTransportSecurity.MsmqAuthenticationMode");
			Assert.AreEqual (MsmqEncryptionAlgorithm.Aes, element.MsmqTransportSecurity.MsmqEncryptionAlgorithm, "MsmqTransportSecurity.MsmqEncryptionAlgorithm");
			Assert.AreEqual (ProtectionLevel.EncryptAndSign, element.MsmqTransportSecurity.MsmqProtectionLevel, "MsmqTransportSecurity.MsmqProtectionLevel");
			Assert.AreEqual (MsmqSecureHashAlgorithm.Sha256, element.MsmqTransportSecurity.MsmqSecureHashAlgorithm, "MsmqTransportSecurity.MsmqSecureHashAlgorithm");
		}

		[Test]
		public void NamedPipeTransportElement () {
			NamedPipeTransportElement element = GetElement<NamedPipeTransportElement> (4);

			Assert.AreEqual (typeof (NamedPipeTransportBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("namedPipeTransport", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.ManualAddressing, "ManualAddressing");
			Assert.AreEqual (262144, element.MaxBufferPoolSize, "MaxBufferPoolSize");
			Assert.AreEqual (32768, element.MaxBufferSize, "MaxBufferSize");
			Assert.AreEqual (32768, element.MaxReceivedMessageSize, "MaxReceivedMessageSize");
			Assert.AreEqual (4096, element.ConnectionBufferSize, "ConnectionBufferSize");
			Assert.AreEqual (HostNameComparisonMode.Exact, element.HostNameComparisonMode, "HostNameComparisonMode");
			Assert.AreEqual (new TimeSpan (0, 0, 20), element.ChannelInitializationTimeout, "ChannelInitializationTimeout");
			Assert.AreEqual (5, element.MaxPendingConnections, "MaxPendingConnections");
			Assert.AreEqual (TimeSpan.Parse ("00:00:01.2000000"), element.MaxOutputDelay, "MaxOutputDelay");
			Assert.AreEqual (3, element.MaxPendingAccepts, "MaxPendingAccepts");
			Assert.AreEqual (TransferMode.Streamed, element.TransferMode, "MaxPendingAccepts");

			Assert.AreEqual ("GroupName", element.ConnectionPoolSettings.GroupName, "ConnectionPoolSettings.GroupName");
			Assert.AreEqual (new TimeSpan (0, 6, 0), element.ConnectionPoolSettings.IdleTimeout, "ConnectionPoolSettings.IdleTimeout");
			Assert.AreEqual (20, element.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint, "ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint");
		}

		[Test]
		public void TcpTransportElement () {
			TcpTransportElement element = GetElement<TcpTransportElement> (5);

			Assert.AreEqual (typeof (TcpTransportBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("tcpTransport", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.ManualAddressing, "ManualAddressing");
			Assert.AreEqual (262144, element.MaxBufferPoolSize, "MaxBufferPoolSize");
			Assert.AreEqual (32768, element.MaxBufferSize, "MaxBufferSize");
			Assert.AreEqual (32768, element.MaxReceivedMessageSize, "MaxReceivedMessageSize");
			Assert.AreEqual (4096, element.ConnectionBufferSize, "ConnectionBufferSize");
			Assert.AreEqual (HostNameComparisonMode.Exact, element.HostNameComparisonMode, "HostNameComparisonMode");
			Assert.AreEqual (new TimeSpan (0, 0, 15), element.ChannelInitializationTimeout, "ChannelInitializationTimeout");
			Assert.AreEqual (20, element.MaxPendingConnections, "MaxPendingConnections");
			Assert.AreEqual (TimeSpan.Parse ("00:00:01.2000000"), element.MaxOutputDelay, "MaxOutputDelay");
			Assert.AreEqual (3, element.MaxPendingAccepts, "MaxPendingAccepts");
			Assert.AreEqual (TransferMode.Streamed, element.TransferMode, "MaxPendingAccepts");
			Assert.AreEqual (20, element.ListenBacklog, "ListenBacklog");
			Assert.AreEqual (true, element.PortSharingEnabled, "PortSharingEnabled");
			Assert.AreEqual (true, element.TeredoEnabled, "TeredoEnabled");

			Assert.AreEqual ("GroupName", element.ConnectionPoolSettings.GroupName, "ConnectionPoolSettings.GroupName");
			Assert.AreEqual (new TimeSpan (0, 15, 0), element.ConnectionPoolSettings.LeaseTimeout, "ConnectionPoolSettings.LeaseTimeout");
			Assert.AreEqual (new TimeSpan (0, 2, 30), element.ConnectionPoolSettings.IdleTimeout, "ConnectionPoolSettings.IdleTimeout");
			Assert.AreEqual (30, element.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint, "ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint");
		}

		[Test]
		public void PeerTransportElement () {
			PeerTransportElement element = GetElement<PeerTransportElement> (6);

			Assert.AreEqual (typeof (PeerTransportBindingElement), element.BindingElementType, "BindingElementType");
			Assert.AreEqual ("peerTransport", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (262144, element.MaxBufferPoolSize, "MaxBufferPoolSize");
			Assert.AreEqual (32768, element.MaxReceivedMessageSize, "MaxReceivedMessageSize");
			Assert.AreEqual (IPAddress.Parse ("192.168.0.1"), element.ListenIPAddress, "ListenIPAddress");
			Assert.AreEqual (88, element.Port, "Port");

			Assert.AreEqual (SecurityMode.Message, element.Security.Mode, "Security.Mode");
			Assert.AreEqual (PeerTransportCredentialType.Certificate, element.Security.Transport.CredentialType, "Security.Transport.CredentialType");
		}
	}
}
#endif