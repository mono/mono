//
// PeerNodeAddressTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class PeerNodeAddressTest
	{
		[Test]
		public void ReadWriteXml ()
		{
			string nas = @"<PeerNodeAddress xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.microsoft.com/net/2006/05/peer'><EndpointAddress xmlns:b='http://www.w3.org/2005/08/addressing'><Address xmlns='http://www.w3.org/2005/08/addressing'>net.tcp://atsushi-pc:37564/PeerChannelEndpoints/b2b137a4-3fdf-4366-a9e4-70d0ab6f2bff</Address></EndpointAddress><IPAddresses xmlns:b='http://schemas.datacontract.org/2004/07/System.Net'></IPAddresses></PeerNodeAddress>";

			var ser = new DataContractSerializer (typeof (PeerNodeAddress));
			var na = (PeerNodeAddress) ser.ReadObject (XmlReader.Create (new StringReader (nas)));
			Assert.IsNotNull (na.EndpointAddress, "#1");
			Assert.IsNotNull (na.EndpointAddress.Uri, "#2");
		}
	}
}
#endif
