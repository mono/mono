// 
// CustomPeerResolverServiceTest.cs
// 
// Author: 
//	 Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.PeerResolvers;
using System.Text;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.PeerResolvers
{
	[TestFixture]
	public class CustomPeerResolverServiceTest
	{
		private CustomPeerResolverService cprs;

		[SetUp]
		protected void SetUp ()
		{
			var port = NetworkHelpers.FindFreePort ();
			Environment.SetEnvironmentVariable ("MONO_CUSTOMPEERRESOLVERSERVICE_PORT", port.ToString ());
			cprs = new CustomPeerResolverService ();
		}

		[Test]
		[Category ("NotWorking")]
		public void CloseTest ()
		{
			cprs.Open ();
			cprs.Close ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CloseTest1 ()
		{
			cprs.Close ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetServiceSettingsTest ()
		{
			ServiceSettingsResponseInfo ssri;

			ssri = cprs.GetServiceSettings ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void OpenTest1 ()
		{
			cprs.CleanupInterval = TimeSpan.Zero;
			cprs.Open ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void OpenTest2 ()
		{
			cprs.RefreshInterval = TimeSpan.Zero;
			cprs.Open ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void OpenTest3 ()
		{
			cprs.CleanupInterval = TimeSpan.Zero;
			cprs.RefreshInterval = TimeSpan.Zero;
			cprs.Open ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void OpenTest4 ()
		{
			cprs.Open ();
			try {
				cprs.Open ();
			} finally {
				cprs.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RefreshTest ()
		{
			cprs.Refresh (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void RefreshTest1 ()
		{
			cprs.Refresh (new RefreshInfo ());
		}

		//[Test]
		//public void RefreshTest2 ()
		//{
		//	cprs.Open ();
		//	cprs.Refresh(new RefreshInfo ("foo", new Guid ()));
		//}

		[Test]
		[ExpectedException (typeof( ArgumentException))]
		public void RegisterTest ()
		{
			cprs.Register (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void RegisterTest1 ()
		{
			cprs.Register (new RegisterInfo ());
		}

		//[Test]
		//public void RegisterTest2 ()
		//{
		//	cprs.Open ();
		//	cprs.Register(new RegisterInfo ());
		//}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ResolveTest ()
		{
			cprs.Resolve (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ResolveTest1 ()
		{
			cprs.Resolve (new ResolveInfo ());
		}

		//[Test]
		//public void ResolveTest2 ()
		//{
		//	cprs.Open ();
		//	cprs.Resolve (new ResolveInfo ());
		//}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnregisterTest ()
		{
			cprs.Unregister (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void UnregisterTest1 ()
		{
			cprs.Unregister (new UnregisterInfo ());
		}

		//[Test]
		//public void UnregisterTest2 ()
		//{
		//	cprs.Open ();
		//	cprs.Unregister (new UnregisterInfo ());
		//}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UpdateTest ()
		{
			cprs.Update (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void UpdateTest1 ()
		{
			cprs.Update (new UpdateInfo ());
		}

		//[Test]
		//public void UpdateTest2 ()
		//{
		//	cprs.Open ();
		//	cprs.Update (new UpdateInfo ());
		//}

		[Test]
		public void Contract ()
		{
			var cd = ContractDescription.GetContract (typeof (IPeerResolverContract));
			Assert.IsNull (cd.CallbackContractType, "#1");
			Assert.AreEqual (typeof (IPeerResolverContract), cd.ContractType, "#2");
			Assert.AreEqual (SessionMode.Allowed, cd.SessionMode, "#3");
		}
	}
}
#endif
