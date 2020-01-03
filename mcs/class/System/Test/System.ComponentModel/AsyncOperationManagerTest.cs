//
// AsyncOperationManager.cs
//
// Author:
// 	Jonathan Pobst  <monkey@jpobst.com>
//
// Copyright (C) 2007 Novell, Inc.
//


using System;
using System.Threading;
using System.ComponentModel;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	[NUnit.Framework.CategoryAttribute ("NotWasm")]
	public class AsyncOperationManagerTest
	{
		[Test]
		public void SyncContext ()
		{
			SynchronizationContext sc1 = new SynchronizationContext ();
			SynchronizationContext sc2 = new SynchronizationContext ();

#if MONOTOUCH
			Assert.IsNotNull (SynchronizationContext.Current, "A1");
#else
			Assert.IsNull (SynchronizationContext.Current, "A1");
#endif
			Assert.IsNotNull (AsyncOperationManager.SynchronizationContext, "A2");
			Assert.IsNotNull (SynchronizationContext.Current, "A3");
			
			SynchronizationContext.SetSynchronizationContext (sc1);

			Assert.AreSame (sc1, SynchronizationContext.Current, "A4");
			Assert.AreSame (sc1, AsyncOperationManager.SynchronizationContext, "A5");
			
			AsyncOperationManager.SynchronizationContext = sc2;

			Assert.AreSame (sc2, SynchronizationContext.Current, "A6");
			Assert.AreSame (sc2, AsyncOperationManager.SynchronizationContext, "A7");
			
			SynchronizationContext.SetSynchronizationContext (null);

			Assert.IsNull (SynchronizationContext.Current, "A8");
			// This is a brand new one, not sc1 or sc2
			Assert.IsNotNull (AsyncOperationManager.SynchronizationContext, "A9");
			Assert.IsNotNull (SynchronizationContext.Current, "A10");
			
			AsyncOperationManager.SynchronizationContext = null;

			Assert.IsNull (SynchronizationContext.Current, "A11");
			// This is a brand new one, not sc1 or sc2
			Assert.IsNotNull (AsyncOperationManager.SynchronizationContext, "A12");
			Assert.IsNotNull (SynchronizationContext.Current, "A13");
		}
	}
}

