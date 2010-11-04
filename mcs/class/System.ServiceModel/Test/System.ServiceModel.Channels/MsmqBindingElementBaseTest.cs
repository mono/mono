//
// MsmqBindingElementBaseTest.cs
//
// Author:
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
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
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class MsmqBindingElementBaseTest
	{
		[Test]
		public void ExportPolicyDefault ()
		{
			MsmqBindingElementBase binding_element = new MsmqTransportBindingElement ();
			IPolicyExportExtension export_extension = binding_element as IPolicyExportExtension;
			PolicyConversionContext conversion_context = new CustomPolicyConversionContext ();
			export_extension.ExportPolicy (new WsdlExporter (), conversion_context);

			PolicyAssertionCollection binding_assertions = conversion_context.GetBindingAssertions ();
			BindingElementCollection binding_elements = conversion_context.BindingElements;
			Assert.AreEqual (4, binding_assertions.Count, "#A0");
			Assert.AreEqual (0, binding_elements.Count, "#A1");

			// wsaw:UsingAddressing
			XmlNode using_addressing_node = FindAssertion (binding_assertions, "wsaw:UsingAddressing");
			Assert.AreEqual (true, using_addressing_node != null, "#B0");
			Assert.AreEqual ("UsingAddressing", using_addressing_node.LocalName, "#B1");
			Assert.AreEqual ("http://www.w3.org/2006/05/addressing/wsdl", using_addressing_node.NamespaceURI, "#B2");
			Assert.AreEqual (String.Empty, using_addressing_node.InnerText, "#B3");
			Assert.AreEqual (0, using_addressing_node.Attributes.Count, "#B4");
			Assert.AreEqual (0, using_addressing_node.ChildNodes.Count, "#B5");

			// msb:BinaryEncoding
			XmlNode binary_encoding_node = FindAssertion (binding_assertions, "msb:BinaryEncoding");
			Assert.AreEqual (true, binary_encoding_node != null, "#C0");
			Assert.AreEqual ("BinaryEncoding", binary_encoding_node.LocalName, "#C1");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/06/2004/mspolicy/netbinary1", binary_encoding_node.NamespaceURI, "#C2");
			Assert.AreEqual (String.Empty, binary_encoding_node.InnerText, "#C3");
			Assert.AreEqual (0, binary_encoding_node.Attributes.Count, "#C4");
			Assert.AreEqual (0, binary_encoding_node.ChildNodes.Count, "#C5");

			// msmq:Authenticated
			XmlNode authenticated_node = FindAssertion (binding_assertions, "msmq:Authenticated");
			Assert.AreEqual (true, authenticated_node != null, "#D0");
			Assert.AreEqual ("Authenticated", authenticated_node.LocalName, "#D1");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq", authenticated_node.NamespaceURI, "#D2");
			Assert.AreEqual (String.Empty, authenticated_node.InnerText, "#D3");
			Assert.AreEqual (0, authenticated_node.Attributes.Count, "#D4");
			Assert.AreEqual (0, authenticated_node.ChildNodes.Count, "#D5");

			// msmq:WindowsDomain
			XmlNode domain_node = FindAssertion (binding_assertions, "msmq:WindowsDomain");
			Assert.AreEqual (true, domain_node != null, "#E0");
			Assert.AreEqual ("WindowsDomain", domain_node.LocalName, "#E1");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq", domain_node.NamespaceURI, "#E2");
			Assert.AreEqual (String.Empty, domain_node.InnerText, "#E3");
			Assert.AreEqual (0, domain_node.Attributes.Count, "#E4");
			Assert.AreEqual (0, domain_node.ChildNodes.Count, "#E5");
		}

		[Test]
		public void ExportPolicy ()
		{
			MsmqBindingElementBase binding_element = new MsmqTransportBindingElement ();
			binding_element.CustomDeadLetterQueue = new Uri ("msmq://custom");
			binding_element.DeadLetterQueue = DeadLetterQueue.Custom;
			binding_element.Durable = !binding_element.Durable; // Volatile
			binding_element.ExactlyOnce = !binding_element.ExactlyOnce; // BestEffort
			binding_element.ManualAddressing = !binding_element.ManualAddressing;
			binding_element.MaxBufferPoolSize = binding_element.MaxBufferPoolSize / 2;
			binding_element.MaxReceivedMessageSize = binding_element.MaxReceivedMessageSize / 2;
			binding_element.MaxRetryCycles = binding_element.MaxRetryCycles / 2;
			binding_element.ReceiveRetryCount = 10;
			binding_element.ReceiveErrorHandling = ReceiveErrorHandling.Reject;
			binding_element.RetryCycleDelay = TimeSpan.FromSeconds (5);
			binding_element.TimeToLive = TimeSpan.FromSeconds (60);
			binding_element.UseMsmqTracing = !binding_element.UseMsmqTracing;
			binding_element.UseSourceJournal = !binding_element.UseSourceJournal;
#if NET_4_0
			// This ones haven't been implemented yet, so comment them for now.
			//binding_element.ReceiveContextEnabled = !binding_element.ReceiveContextEnabled;
			//binding_element.ValidityDuration = TimeSpan.FromSeconds (30);
#endif

			binding_element.MsmqTransportSecurity.MsmqAuthenticationMode = MsmqAuthenticationMode.Certificate;
			binding_element.MsmqTransportSecurity.MsmqEncryptionAlgorithm = MsmqEncryptionAlgorithm.Aes;
			binding_element.MsmqTransportSecurity.MsmqProtectionLevel = ProtectionLevel.EncryptAndSign;
			binding_element.MsmqTransportSecurity.MsmqSecureHashAlgorithm = MsmqSecureHashAlgorithm.Sha256;

			IPolicyExportExtension export_extension = binding_element as IPolicyExportExtension;
			PolicyConversionContext conversion_context = new CustomPolicyConversionContext ();
			export_extension.ExportPolicy (new WsdlExporter (), conversion_context);

			PolicyAssertionCollection binding_assertions = conversion_context.GetBindingAssertions ();
			BindingElementCollection binding_elements = conversion_context.BindingElements;
			Assert.AreEqual (5, binding_assertions.Count, "#A0");
			Assert.AreEqual (0, binding_elements.Count, "#A1");

			// msmq:MsmqVolatile
			XmlNode volatile_node = FindAssertion (binding_assertions, "msmq:MsmqVolatile");
			Assert.AreEqual (true, volatile_node != null, "#B0");
			Assert.AreEqual ("MsmqVolatile", volatile_node.LocalName, "#B1");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq", volatile_node.NamespaceURI, "#B2");
			Assert.AreEqual (String.Empty, volatile_node.InnerText, "#B3");
			Assert.AreEqual (0, volatile_node.Attributes.Count, "#B4");
			Assert.AreEqual (0, volatile_node.ChildNodes.Count, "#B5");

			// msmq:MsmqBestEffort
			XmlNode best_effort_node = FindAssertion (binding_assertions, "msmq:MsmqBestEffort");
			Assert.AreEqual (true, best_effort_node != null, "#C0");
			Assert.AreEqual ("MsmqBestEffort", best_effort_node.LocalName, "#C1");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq", best_effort_node.NamespaceURI, "#C2");
			Assert.AreEqual (String.Empty, best_effort_node.InnerText, "#C3");
			Assert.AreEqual (0, best_effort_node.Attributes.Count, "#C4");
			Assert.AreEqual (0, best_effort_node.ChildNodes.Count, "#C5");

			// Setting MsmqTransportSecurity.MsmqAuthenticationMode to a value other than WindowsDomain
			// causes the removal of the WindowsDomain policy.
			XmlNode domain_node = FindAssertion (binding_assertions, "msmq:WindowsDomain");
			Assert.AreEqual (true, domain_node == null, "#D0");
		}

		XmlNode FindAssertion (PolicyAssertionCollection assertionCollection, string name)
		{
			foreach (XmlNode node in assertionCollection)
				if (node.Name == name)
					return node;

			return null;
		}
	}
}

