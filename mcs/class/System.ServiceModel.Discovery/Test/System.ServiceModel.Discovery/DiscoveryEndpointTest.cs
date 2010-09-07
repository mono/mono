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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class DiscoveryEndpointTest
	{
		[Test]
		public void DefaultValues ()
		{
			var de = new DiscoveryEndpoint ();
			Assert.AreEqual (DiscoveryVersion.WSDiscovery11, de.DiscoveryVersion, "#1");
			Assert.AreEqual (ServiceDiscoveryMode.Managed, de.DiscoveryMode, "#2");
			Assert.AreEqual (TimeSpan.Zero, de.MaxResponseDelay, "#3");
			var cd = de.Contract;
			Assert.IsNotNull (cd, "#11"); // some version-dependent internal type.
			Assert.AreEqual ("http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01", cd.Namespace, "#11-2");
			Assert.AreEqual ("DiscoveryProxy", cd.Name, "#11-3");
			Assert.AreEqual (2, cd.Operations.Count, "#11-4");
			var oper = cd.Operations.FirstOrDefault (od => !od.IsOneWay && od.Messages.Any (md => md.Action == "http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/Probe"));
			Assert.IsNotNull (oper, "#11-5");
			Assert.IsNotNull (oper.Messages.Any (md => md.Body.ReturnValue != null && md.Body.ReturnValue.Type == typeof (void)), "#11-5-2"); // return type is void
			Assert.IsTrue (cd.Operations.Any (od => !od.IsOneWay && od.Messages.Any (md => md.Action == "http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/Resolve")), "#11-6");
			Assert.IsNull (de.Binding, "#12");
			Assert.IsNull (de.Address, "#13");
			Assert.IsNull (de.ListenUri, "#14");
		}

		// Adhoc mode gives *different* ServiceContract than Managed mode.
		// Adhoc one is one-way based, while Managed one is bidirectional.
		[Test]
		public void AdhocDefaultValues ()
		{
			var de = new DiscoveryEndpoint (DiscoveryVersion.WSDiscoveryCD1, ServiceDiscoveryMode.Adhoc);
			Assert.AreEqual (DiscoveryVersion.WSDiscoveryCD1, de.DiscoveryVersion, "#1");
			Assert.AreEqual (ServiceDiscoveryMode.Adhoc, de.DiscoveryMode, "#2");
			Assert.AreEqual (TimeSpan.Zero, de.MaxResponseDelay, "#3");
			var cd = de.Contract;
			Assert.IsNotNull (cd, "#11"); // some version-dependent internal type.
			Assert.AreEqual ("http://docs.oasis-open.org/ws-dd/ns/discovery/2008/09", cd.Namespace, "#11-2");
			Assert.AreEqual ("TargetService", cd.Name, "#11-3");
			Assert.AreEqual (5, cd.Operations.Count, "#11-4");
			Assert.IsTrue (cd.Operations.Any (od => od.IsOneWay && od.Messages.Any (md => md.Action == "http://docs.oasis-open.org/ws-dd/ns/discovery/2008/09/Probe")), "#11-5");
			Assert.IsTrue (cd.Operations.Any (od => od.IsOneWay && od.Messages.Any (md => md.Action == "http://docs.oasis-open.org/ws-dd/ns/discovery/2008/09/Resolve")), "#11-6");
			Assert.IsTrue (cd.Operations.Any (od => od.IsOneWay && od.Messages.Any (md => md.Action == "http://docs.oasis-open.org/ws-dd/ns/discovery/2008/09/ProbeMatches")), "#11-7");
			Assert.IsTrue (cd.Operations.Any (od => od.IsOneWay && od.Messages.Any (md => md.Action == "http://docs.oasis-open.org/ws-dd/ns/discovery/2008/09/ResolveMatches")), "#11-8");
			Assert.IsTrue (cd.Operations.Any (od => od.IsOneWay && od.Messages.Any (md => md.Action == "http://docs.oasis-open.org/ws-dd/ns/discovery/2008/09/Hello")), "#11-9");
			Assert.IsNull (de.Binding, "#12");
			Assert.IsNull (de.Address, "#13");
			Assert.IsNull (de.ListenUri, "#14");
		}
	}
}
