//
// NetPeerTcpBindingTest.cs
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
using System.Collections.ObjectModel;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Security;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class NetPeerTcpBindingTest
	{
		[Test]
		public void DefaultValues ()
		{
			if (!NetPeerTcpBinding.IsPnrpAvailable)
				Assert.Ignore ("PNRP is not available."); // yes, we actually don't test it.

			var n = new NetPeerTcpBinding ();
			Assert.AreEqual (EnvelopeVersion.Soap12, n.EnvelopeVersion, "#1");
			Assert.IsNull (n.ListenIPAddress, "#2");
			Assert.AreEqual (0x10000, n.MaxReceivedMessageSize, "#3");
			Assert.AreEqual (0, n.Port, "#4");
			Assert.IsNotNull (n.ReaderQuotas, "#5");

			var bec = n.CreateBindingElements ();
			Assert.IsNotNull (bec.Find<PnrpPeerResolverBindingElement> (), "#bec0");
			Assert.IsNotNull (bec.Find<BinaryMessageEncodingBindingElement> (), "#bec1");
			Assert.AreEqual (3, bec.Count, "#bec2");

			var tr = bec.Find<PeerTransportBindingElement> ();
			Assert.IsNotNull (tr, "#tr1");
		}

		[Test]
		public void DefaultValuesForCustom ()
		{
			var n = new NetPeerTcpBinding ();
			n.Resolver.Mode = PeerResolverMode.Custom;
			n.Resolver.Custom.Resolver = new DummyResolver ();

			Assert.AreEqual (EnvelopeVersion.Soap12, n.EnvelopeVersion, "#1");
			Assert.IsNull (n.ListenIPAddress, "#2");
			Assert.AreEqual (0x10000, n.MaxReceivedMessageSize, "#3");
			Assert.AreEqual (0, n.Port, "#4");
			Assert.IsNotNull (n.ReaderQuotas, "#5");

			Assert.IsFalse (((IBindingRuntimePreferences) n).ReceiveSynchronously, "#6");

			var bec = n.CreateBindingElements ();
			Assert.IsNotNull (bec.Find<PeerCustomResolverBindingElement> (), "#bec0");
			Assert.IsNotNull (bec.Find<BinaryMessageEncodingBindingElement> (), "#bec1");
			Assert.AreEqual (3, bec.Count, "#bec2");

			var tr = bec.Find<PeerTransportBindingElement> ();
			Assert.IsNotNull (tr, "#tr1");
		}
	}

	class DummyResolver : PeerResolver
	{
		public override bool CanShareReferrals {
			get { throw new NotImplementedException (); }
		}

		public override object Register (string meshId,
			PeerNodeAddress nodeAddress, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public override ReadOnlyCollection<PeerNodeAddress> Resolve (
			string meshId, int maxAddresses, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public override void Unregister (object registrationId,
			TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public override void Update (object registrationId,
			PeerNodeAddress updatedNodeAddress, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
