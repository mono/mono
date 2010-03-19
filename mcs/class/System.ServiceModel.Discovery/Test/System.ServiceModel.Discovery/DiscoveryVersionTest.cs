using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class DiscoveryVersionTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullName ()
		{
			DiscoveryVersion.FromName (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void EmptyName ()
		{
			DiscoveryVersion.FromName (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InvalidName ()
		{
			DiscoveryVersion.FromName ("foobar");
		}

		[Test]
		public void ValidName ()
		{
			DiscoveryVersion.FromName ("WSDiscovery11");
			DiscoveryVersion.FromName ("WSDiscoveryApril2005");
			DiscoveryVersion.FromName ("WSDiscoveryCD1");
		}
	}
}
