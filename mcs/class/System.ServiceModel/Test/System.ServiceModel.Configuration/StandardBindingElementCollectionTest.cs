//
// StandardBindingElementCollectionTest.cs
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
using NUnit.Framework;
using System;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Text;
using System.ServiceModel.Security;
using System.ServiceModel;
using System.Net.Security;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class StandardBindingElementCollectionTest
	{
		[Test]
		public void BasicHttpBinding () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/basicHttpBinding")).GetSectionGroup ("system.serviceModel");

			BasicHttpBindingCollectionElement basicHttpBinding = config.Bindings.BasicHttpBinding;
			Assert.AreEqual (2, basicHttpBinding.Bindings.Count, "count");

			BasicHttpBindingElement binding = basicHttpBinding.Bindings [0];
			Assert.AreEqual ("BasicHttpBinding_Service", binding.Name, "Name");
			Assert.AreEqual (Encoding.UTF8, binding.TextEncoding, "Name");
			Assert.AreEqual (SecurityAlgorithmSuite.Default, binding.Security.Message.AlgorithmSuite, "Name");
		}

		[Test]
		public void NetTcpBinding () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/netTcpBinding")).GetSectionGroup ("system.serviceModel");

			NetTcpBindingCollectionElement netTcpBinding = config.Bindings.NetTcpBinding;
			Assert.AreEqual (1, netTcpBinding.Bindings.Count, "count");

			NetTcpBindingElement binding = netTcpBinding.Bindings [0];
			Assert.AreEqual ("NetTcpBinding_IHelloWorldService", binding.Name, "Name");
			Assert.AreEqual (TransactionProtocol.OleTransactions, binding.TransactionProtocol, "TransactionProtocol");
			Assert.AreEqual (SecurityMode.Transport, binding.Security.Mode, "Security.Mode");
			Assert.AreEqual (MessageCredentialType.Windows, binding.Security.Message.ClientCredentialType, "Security.Message.ClientCredentialType");
			Assert.AreEqual (ProtectionLevel.EncryptAndSign, binding.Security.Transport.ProtectionLevel, "Security.Transport.ProtectionLevel");
			Assert.AreEqual (TcpClientCredentialType.Windows, binding.Security.Transport.ClientCredentialType, "Security.Transport.ProtectionLevel");
		}

		[Test]
		public void WSHttpBinding () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/wsHttpBinding")).GetSectionGroup ("system.serviceModel");

			WSHttpBindingCollectionElement wsHttpBinding = config.Bindings.WSHttpBinding;
			Assert.AreEqual (1, wsHttpBinding.Bindings.Count, "count");

			WSHttpBindingElement binding = wsHttpBinding.Bindings [0];
			Assert.AreEqual ("WSHttpBinding_IHelloWorldService", binding.Name, "Name");
			Assert.AreEqual (HostNameComparisonMode.StrongWildcard, binding.HostNameComparisonMode, "HostNameComparisonMode");
			Assert.AreEqual (SecurityMode.Message, binding.Security.Mode, "Security.Mode");
			Assert.AreEqual (MessageCredentialType.Windows, binding.Security.Message.ClientCredentialType, "Security.Message.ClientCredentialType");
			Assert.AreEqual (HttpProxyCredentialType.None, binding.Security.Transport.ProxyCredentialType, "Security.Transport.ProtectionLevel");
			Assert.AreEqual (HttpClientCredentialType.Windows, binding.Security.Transport.ClientCredentialType, "Security.Transport.ProtectionLevel");
		}

		[Test]
		public void CollectionType () {
			StandardBindingElementCollection<BasicHttpBindingElement> coll = new StandardBindingElementCollection<BasicHttpBindingElement> ();
			Assert.AreEqual (ConfigurationElementCollectionType.AddRemoveClearMap, coll.CollectionType, "CollectionType");
		}
	}
}
#endif
