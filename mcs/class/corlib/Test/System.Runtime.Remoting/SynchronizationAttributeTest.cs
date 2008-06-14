//
// MonoTests.System.Runtime.Remoting.SynchronizationAttributeTest.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
//

using System;
using System.Threading;
using System.Runtime.Remoting.Contexts;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Remoting
{
	enum SynchRes { SameSync, NewSync, NoSync }

	class SincroBase: ContextBoundObject
	{
		public int idx = 0;

		public bool CheckConcurrency ()
		{
			int t = idx;
			for (int n=0; n<40; n++)
			{
				idx++;
				Thread.Sleep (25);
			}
			return (t+40 != idx);
		}
		
		public bool CheckUnlockedConcurrency ()
		{
			Lock (false);
			return CheckConcurrency ();
		}
		
		public SynchRes CheckContext (Context ctx)
		{
			object otherp = ctx.GetProperty ("Synchronization");
			object thisp = Thread.CurrentContext.GetProperty ("Synchronization");

			if (thisp == null) return SynchRes.NoSync;
			if (thisp == otherp) return SynchRes.SameSync;
			return SynchRes.NewSync;
		}

		public SynchRes CheckContextTransition (Type type)
		{
			SincroBase bob = (SincroBase)Activator.CreateInstance (type);
			return bob.CheckContext (Thread.CurrentContext);
		}

		public bool CheckCalloutConcurrency (SincroBase bob)
		{
			bool res = bob.CheckConcurrency ();
			return res;
		}

		public void CheckLock1 ()
		{
			Thread.Sleep (2000);
			Lock (false);
			Thread.Sleep (6000);
		}

		public void CheckLock2 ()
		{
			Thread.Sleep (1000);
			Lock (true);
			Thread.Sleep (2000);
		}

		public void Lock (bool b)
		{
			SynchronizationAttribute thisp = (SynchronizationAttribute) Thread.CurrentContext.GetProperty ("Synchronization");
			thisp.Locked = b;
		}

		public bool GetLocked ()
		{
			SynchronizationAttribute thisp = (SynchronizationAttribute) Thread.CurrentContext.GetProperty ("Synchronization");
			return thisp.Locked;
		}
		
		public bool CheckMonitorWait (bool exitContext)
		{
			lock (this)
			{
				return Monitor.Wait (this, 1000, exitContext);
			}
		}
		
		public void CheckMonitorPulse ()
		{
			lock (this)
			{
				Monitor.Pulse (this);
			}
		}
	}

	[Synchronization (SynchronizationAttribute.SUPPORTED)]
	class SincroSupported: SincroBase
	{
	}

	[Synchronization (SynchronizationAttribute.REQUIRED)]
	class SincroRequired: SincroBase
	{
	}

	[Synchronization (SynchronizationAttribute.REQUIRES_NEW)]
	class SincroRequiresNew: SincroBase
	{
		public bool TestCallback ()
		{
			SincroNotSupported bob = new SincroNotSupported ();
			return bob.CallBack (this);
		}
	}

	[Synchronization (SynchronizationAttribute.NOT_SUPPORTED)]
	class SincroNotSupported: SincroBase
	{
		public bool CallBack (SincroRequiresNew bob)
		{
			return bob.CheckConcurrency ();
		}
	}

	[Synchronization (SynchronizationAttribute.REQUIRES_NEW, true)]
	class SincroRequiresNewReentrant: SincroBase
	{
	}

	[TestFixture]
	public class SynchronizationAttributeTest: Assertion
	{
		SincroRequiresNew sincob = new SincroRequiresNew ();
		SincroNotSupported notsup = new SincroNotSupported ();
		SincroRequiresNewReentrant reentrant = new SincroRequiresNewReentrant ();
		SincroRequiresNew notreentrant = new SincroRequiresNew ();
		bool otResult;

		[Test]
		public void TestSynchronization ()
		{
			Thread tr = new Thread (new ThreadStart (FirstSyncThread));
			tr.Start ();
			Thread.Sleep (200);
			SecondSyncThread ();
			
			tr.Join ();
			Assert ("Concurrency detected in FirstSyncThread", !otResult);
		}

		void FirstSyncThread ()
		{
			otResult = sincob.CheckConcurrency ();
		}

		void SecondSyncThread ()
		{
			bool concurrent = sincob.CheckConcurrency ();
			Assert ("Concurrency detected", !concurrent);
		}

		[Test]
		public void TestSupported ()
		{
			SincroRequiresNew ob = new SincroRequiresNew ();
			SynchRes res = ob.CheckContextTransition (typeof(SincroSupported));
			Assert ("Synchronizaton context expected", res == SynchRes.SameSync);

			SincroSupported ob2 = new SincroSupported ();
			res = ob2.CheckContext (Thread.CurrentContext);
			Assert ("Synchronizaton context not expected", res == SynchRes.NoSync);
		}

		[Test]
		public void TestRequired ()
		{
			SincroRequiresNew ob = new SincroRequiresNew ();
			SynchRes res = ob.CheckContextTransition (typeof(SincroRequired));
			Assert ("Synchronizaton context expected 1", res == SynchRes.SameSync);

			SincroRequired ob2 = new SincroRequired ();
			res = ob2.CheckContext (Thread.CurrentContext);
			Assert ("Synchronizaton context expected 2", res == SynchRes.NewSync);
		}

		[Test]
		public void TestRequiresNew ()
		{
			SincroRequiresNew ob = new SincroRequiresNew ();
			SynchRes res = ob.CheckContextTransition (typeof(SincroRequiresNew));
			Assert ("New synchronizaton context expected", res == SynchRes.NewSync);

			SincroRequiresNew ob2 = new SincroRequiresNew ();
			res = ob2.CheckContext (Thread.CurrentContext);
			Assert ("Synchronizaton context not expected", res == SynchRes.NewSync);
		}

		[Test]
		public void TestNotSupported ()
		{
			SincroRequiresNew ob = new SincroRequiresNew ();
			SynchRes res = ob.CheckContextTransition (typeof(SincroNotSupported));
			Assert ("Synchronizaton context not expected 1", res == SynchRes.NoSync);

			SincroNotSupported ob2 = new SincroNotSupported ();
			res = ob2.CheckContext (Thread.CurrentContext);
			Assert ("Synchronizaton context not expected 2", res == SynchRes.NoSync);
		}

		[Test]
		public void TestLocked1 ()
		{
			sincob.Lock (false);
			Thread tr = new Thread (new ThreadStart (FirstSyncThread));
			tr.Start ();
			Thread.Sleep (200);
			SecondSyncThread ();
			
			tr.Join ();
			Assert ("Concurrency detected in FirstSyncThread", !otResult);
		}

		[Test]
		public void TestLocked2 ()
		{
			Thread tr = new Thread (new ThreadStart (FirstNotSyncThread));
			tr.Start ();
			Thread.Sleep (200);
			SecondNotSyncThread ();
			
			tr.Join ();
			Assert ("Concurrency not detected in FirstReentryThread", otResult);
		}

		void FirstNotSyncThread ()
		{
			otResult = sincob.CheckUnlockedConcurrency ();
		}

		void SecondNotSyncThread ()
		{
			bool concurrent = sincob.CheckConcurrency ();
			Assert ("Concurrency not detected", concurrent);
		}

		[Test]
		public void TestLocked3 ()
		{
			Thread tr = new Thread (new ThreadStart (Lock1Thread));
			tr.Start ();
			Thread.Sleep (200);
			Lock2Thread ();
		}

		void Lock1Thread ()
		{
			sincob.CheckLock1 ();
		}

		void Lock2Thread ()
		{
			sincob.CheckLock2 ();
		}

		[Test]
		public void TestReentry ()
		{
			Thread tr = new Thread (new ThreadStart (FirstReentryThread));
			tr.Start ();
			Thread.Sleep (200);
			SecondReentryThread ();
			
			tr.Join ();
			Assert ("Concurrency not detected in FirstReentryThread", otResult);
		}

		void FirstReentryThread ()
		{
			otResult = reentrant.CheckCalloutConcurrency (notsup);
		}

		void SecondReentryThread ()
		{
			bool concurrent = reentrant.CheckCalloutConcurrency (notsup);
			Assert ("Concurrency not detected", concurrent);
		}

		[Test]
		public void TestNoReentry ()
		{
			Thread tr = new Thread (new ThreadStart (FirstNoReentryThread));
			tr.Start ();
			Thread.Sleep (200);
			SecondNoReentryThread ();
			
			tr.Join ();
			Assert ("Concurrency detected in FirstNoReentryThread", !otResult);
		}

		void FirstNoReentryThread ()
		{
			otResult = notreentrant.CheckCalloutConcurrency (notsup);
		}

		void SecondNoReentryThread ()
		{
			bool concurrent = notreentrant.CheckCalloutConcurrency (notsup);
			Assert ("Concurrency detected", !concurrent);
		}

		[Test]
		public void TestCallback ()
		{
			Thread tr = new Thread (new ThreadStart (CallbackThread));
			tr.Start ();
			Thread.Sleep (200);
			bool concurrent = notreentrant.CheckConcurrency ();
			Assert ("Concurrency detected", !concurrent);
			notreentrant.CheckContext (Thread.CurrentContext);
			
			tr.Join ();
			Assert ("Concurrency detected in CallbackThread", !otResult);
		}

		void CallbackThread ()
		{
			otResult = notreentrant.TestCallback ();
		}
		
		[Test]
		[Category("NotDotNet")]
		public void TestMonitorWait ()
		{
			Thread tr = new Thread (new ThreadStart (DoMonitorPulse));
			tr.Start ();
			
			bool r = sincob.CheckMonitorWait (true);
			Assert ("Wait timeout", r);
			
			r = tr.Join (1000);
			Assert ("Join timeout", r);
			
			tr = new Thread (new ThreadStart (DoMonitorPulse));
			tr.Start ();
			
			r = sincob.CheckMonitorWait (false);
			Assert ("Expected wait timeout", !r);
			
			r = tr.Join (1000);
			Assert ("Join timeout 2", r);
		}

		void DoMonitorPulse ()
		{
			Thread.Sleep (100);
			sincob.CheckMonitorPulse ();
		}
	}
}
