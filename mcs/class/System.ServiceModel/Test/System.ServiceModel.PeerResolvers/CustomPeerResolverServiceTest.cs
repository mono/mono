// 
// CustomPeerResolverServiceTest.cs
// 
// Author: 
//	 Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.PeerResolvers;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ServiceModel.PeerResolvers
{
	[TestFixture]
	public class CustomPeerResolverServiceTest
	{
		private CustomPeerResolverService cprs;

		[SetUp]
		protected void SetUp ()
		{
			cprs = new CustomPeerResolverService ();
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		public void CloseTest ()
		{
			cprs.Open ();
			cprs.Close ();
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof (InvalidOperationException))]
		public void CloseTest1 ()
		{
			cprs.Close ();
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetServiceSettingsTest ()
		{
			ServiceSettingsResponseInfo ssri;

			ssri = cprs.GetServiceSettings ();
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		public void OpenTest ()
		{
			cprs.Open ();
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof (ArgumentException))]
		public void OpenTest1 ()
		{
			cprs.CleanupInterval = TimeSpan.Zero;
			cprs.Open ();
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof (ArgumentException))]
		public void OpenTest2 ()
		{
			cprs.RefreshInterval = TimeSpan.Zero;
			cprs.Open ();
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof (ArgumentException))]
		public void OpenTest3 ()
		{
			cprs.CleanupInterval = TimeSpan.Zero;
			cprs.RefreshInterval = TimeSpan.Zero;
			cprs.Open ();
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof (InvalidOperationException))]
		public void OpenTest4 ()
		{
			cprs.Open ();
			cprs.Open ();
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof (ArgumentException))]
		public void RefreshTest ()
		{
			cprs.Refresh (null);
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
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
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof( ArgumentException))]
		public void RegisterTest ()
		{
			cprs.Register (null);
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
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
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof (ArgumentException))]
		public void ResolveTest ()
		{
			cprs.Resolve (null);
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
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
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof (ArgumentException))]
		public void UnregisterTest ()
		{
			cprs.Unregister (null);
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
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
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
		[ExpectedException (typeof (ArgumentException))]
		public void UpdateTest ()
		{
			cprs.Update (null);
		}

		[Test]
		[Category ("NotWorking")] // It somehow stopped working properly recently in Nov. 2009, not sure where the source of the problem lies.
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
