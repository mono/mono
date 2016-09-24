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
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using System.Xml;
using MonoTests.Helpers;
using NUnit.Framework;
using System.Text;
using System.Threading;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class DiscoveryClientTest
	{
		[Test]
		public void ContractInterfaceManaged ()
		{
			var client = new DiscoveryClient (new DiscoveryEndpoint ());
			var v11 = client.ChannelFactory.Endpoint;
			Assert.IsNotNull (v11, "v11");
			Assert.AreEqual ("DiscoveryProxy", v11.Name, "v11.Name");
			Assert.AreEqual (2, v11.Contract.Operations.Count, "v11.Operations.Count");
			Assert.IsNull (v11.Contract.CallbackContractType, "v11.CallbackContractType");
		}

		[Test]
		public void ContractInterfaceAdhoc ()
		{
			var client = new DiscoveryClient (new UdpDiscoveryEndpoint ());
			var v11 = client.ChannelFactory.Endpoint;
var cd = ContractDescription.GetContract (v11.Contract.ContractType);
			Assert.IsNotNull (v11, "v11");
			Assert.AreEqual ("CustomBinding_TargetService", v11.Name, "v11.Name");
			Assert.AreEqual (5, v11.Contract.Operations.Count, "v11.Operations.Count");
			Assert.IsNotNull (v11.Contract.CallbackContractType, "v11.CallbackContractType");
		}

		[Test]
		public void TestClientDiscovery()
		{
			var port = NetworkHelpers.FindFreePort();
			UdpClient server = new UdpClient(port);
			server.MulticastLoopback = true;
			server.JoinMulticastGroup(IPAddress.Parse("239.255.255.250"));
			server.ReceiveAsync().ContinueWith(r =>
			{
				string response = @"
				<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
					<soap:Header>
						<wsa:Action>http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/ProbeMatches</wsa:Action>
						<wsa:MessageID>urn:uuid:928b5829-d201-4cfa-ba73-c6e7e23f6797</wsa:MessageID>
						<wsa:To>http://www.w3.org/2005/08/addressing/anonymous</wsa:To>
						<wsa:RelatesTo>urn:uuid:83ca7486-6659-46e6-9c22-ca442be8a62a</wsa:RelatesTo>
					</soap:Header>
					<soap:Body>
						<ProbeMatches xmlns=""http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01"" xmlns:ns2=""http://www.w3.org/2005/08/addressing"">
							<ProbeMatch>
								<ns2:EndpointReference>
									<ns2:Address>urn:uuid:3ca2976d-7dfc-43aa-86d1-2abf46beb828</ns2:Address>
									<ns2:ReferenceParameters/>
								</ns2:EndpointReference>
								<Types xmlns:ns3=""http://www.onvif.org/ver10/network/wsdl"">ns3:NetworkVideoTransmitter</Types>
								<Scopes>onvif://www.onvif.org/type/Network_Video_Transmitter</Scopes>
								<XAddrs>http://192.168.1.101:8080/onvif/webservices/device_service</XAddrs>
								<MetadataVersion>1</MetadataVersion>
							</ProbeMatch>
						</ProbeMatches>
					</soap:Body>
				</soap:Envelope>";

				var bytes = Encoding.UTF8.GetBytes(response.Trim().Replace("\t", "").Replace("\n", ""));

				server.Send(bytes, bytes.Length, r.Result.RemoteEndPoint);
				server.Close();
			});
			Uri multicastUri = new Uri(string.Format("soap.udp://239.255.255.250:{0}/", port));
			UdpDiscoveryEndpoint ude = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscovery11, multicastUri);
			DiscoveryClient discoveryClient = new DiscoveryClient(ude);
			FindCriteria findCriteria = new FindCriteria();
			findCriteria.MaxResults = 1;
			findCriteria.Duration = TimeSpan.FromSeconds(5);
			var res = discoveryClient.Find(findCriteria);
			Assert.AreEqual(1, res.Endpoints.Count);
			Assert.AreEqual("urn:uuid:3ca2976d-7dfc-43aa-86d1-2abf46beb828", res.Endpoints[0].Address.ToString());
		}
	}
}
