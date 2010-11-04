//
// ConnectionOrientedTransportBindingElementTest.cs
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class ConnectionOrientedTransportBindingElementTest
	{
		//
		// We use NamedPipeTransportBindingElement to access the impl of ExportPolicy
		//
		[Test]
		public void ExportPolicyDefault ()
		{
			ConnectionOrientedTransportBindingElement binding_element = new NamedPipeTransportBindingElement ();
			IPolicyExportExtension export_extension = binding_element as IPolicyExportExtension;
			PolicyConversionContext conversion_context = new CustomPolicyConversionContext ();
			export_extension.ExportPolicy (new WsdlExporter (), conversion_context);

			PolicyAssertionCollection binding_assertions = conversion_context.GetBindingAssertions ();
			BindingElementCollection binding_elements = conversion_context.BindingElements;
			Assert.AreEqual (2, binding_assertions.Count, "#A0");
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
		}

		// 
		// Non-default values
		//
		[Test]
		public void ExportPolicy ()
		{
			ConnectionOrientedTransportBindingElement binding_element = new NamedPipeTransportBindingElement ();
			binding_element.ChannelInitializationTimeout = TimeSpan.FromSeconds (3);
			binding_element.ConnectionBufferSize = binding_element.ConnectionBufferSize / 2;
			binding_element.HostNameComparisonMode = HostNameComparisonMode.WeakWildcard;
			binding_element.ManualAddressing = !binding_element.ManualAddressing;
			binding_element.MaxBufferSize = binding_element.MaxBufferSize / 2;
			binding_element.MaxBufferPoolSize = binding_element.MaxBufferPoolSize / 2;
			binding_element.MaxOutputDelay = TimeSpan.FromSeconds (3);
			binding_element.MaxPendingAccepts = 3;
			binding_element.MaxPendingConnections = 15;
			binding_element.MaxReceivedMessageSize = binding_element.MaxReceivedMessageSize / 2;
			binding_element.TransferMode = TransferMode.Streamed; // Causes an assertion with Streamed* values

			IPolicyExportExtension export_extension = binding_element as IPolicyExportExtension;
			PolicyConversionContext conversion_context = new CustomPolicyConversionContext ();
			export_extension.ExportPolicy (new WsdlExporter (), conversion_context);

			PolicyAssertionCollection binding_assertions = conversion_context.GetBindingAssertions ();
			BindingElementCollection binding_elements = conversion_context.BindingElements;
			Assert.AreEqual (3, binding_assertions.Count, "#A0");
			Assert.AreEqual (0, binding_elements.Count, "#A1");

			// msf:Streamed
			XmlNode streamed_node = FindAssertion (binding_assertions, "msf:Streamed");
			Assert.AreEqual (true, streamed_node != null, "#B0");
			Assert.AreEqual ("Streamed", streamed_node.LocalName, "#B1");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/framing/policy", streamed_node.NamespaceURI, "#B2");
			Assert.AreEqual (String.Empty, streamed_node.InnerText, "#B3");
			Assert.AreEqual (0, streamed_node.Attributes.Count, "#B4");
			Assert.AreEqual (0, streamed_node.ChildNodes.Count, "#B5");
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
	}
}

