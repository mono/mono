//
// ServiceElementTest.cs
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
using System.ServiceModel.Configuration;
using System.Configuration;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class ServiceElementTest
	{
		[Test]
		public void ReadConfiguration () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/service")).GetSectionGroup ("system.serviceModel");
			ServiceElement service = config.Services.Services [0];

			Assert.AreEqual ("ServiceType", service.Name, "Name");
			Assert.AreEqual ("", service.BehaviorConfiguration, "BehaviorConfiguration");
			Assert.AreEqual (3, service.Endpoints.Count, "Endpoints.Count");

			Assert.AreEqual ("basicHttp", service.Endpoints [0].Name, "Endpoints [0].Name");
			Assert.AreEqual ("basicHttpBinding", service.Endpoints [0].Binding, "Endpoints [0].Binding");
			Assert.AreEqual ("HttpServiceContract", service.Endpoints [0].Contract, "Endpoints [0].Contract");
			Assert.AreEqual ("/rooted.path", service.Endpoints [0].Address.OriginalString, "Endpoints [0].Address");

			Assert.AreEqual (1, service.Endpoints [0].Headers.Headers.Count, "Endpoints [0].Headers.Headers.Count");
			Assert.AreEqual ("Tag", service.Endpoints [0].Headers.Headers [0].Name, "Endpoints [0].Headers.Headers[0].Name");
			Assert.AreEqual ("", service.Endpoints [0].Headers.Headers [0].Namespace, "Endpoints [0].Headers.Headers[0].Namespace");

			Assert.AreEqual ("", service.Endpoints [1].Name, "Endpoints [1].Name");
			Assert.AreEqual ("wsHttpBinding", service.Endpoints [1].Binding, "Endpoints [1].Binding");
			Assert.AreEqual ("WSServiceContract", service.Endpoints [1].Contract, "Endpoints [1].Contract");
			Assert.AreEqual ("http://other.endpoint.com", service.Endpoints [1].Address.OriginalString, "Endpoints [1].Address");

			Assert.AreEqual ("netTcp", service.Endpoints [2].Name, "Endpoints [2].Name");
			Assert.AreEqual ("netTcpBinding", service.Endpoints [2].Binding, "Endpoints [2].Binding");
			Assert.AreEqual ("TcpServiceContract", service.Endpoints [2].Contract, "Endpoints [2].Contract");
			Assert.AreEqual ("path", service.Endpoints [2].Address.OriginalString, "Endpoints [2].Address");

			Assert.AreEqual (new TimeSpan (0, 0, 20), service.Host.Timeouts.CloseTimeout, "Host.Timeouts.CloseTimeout");
			Assert.AreEqual (new TimeSpan (0, 2, 0), service.Host.Timeouts.OpenTimeout, "Host.Timeouts.OpenTimeout");

			Assert.AreEqual (2, service.Host.BaseAddresses.Count, "Host.BaseAddresses.Count");
			Assert.AreEqual ("http://endpoint.com/some.path", service.Host.BaseAddresses [0].BaseAddress, "Host.BaseAddresses[0].BaseAddress");
			Assert.AreEqual ("net.tcp://endpoint.com", service.Host.BaseAddresses [1].BaseAddress, "Host.BaseAddresses[1].BaseAddress");
		}

		[Test]
		public void ServiceEndpointCollection () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/service")).GetSectionGroup ("system.serviceModel");
			ServiceElement service = config.Services.Services [1];

			Assert.AreEqual (3, service.Endpoints.Count, "Count");
		}

	}
}
#endif
