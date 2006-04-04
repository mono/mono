//
// StopwatchTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
//
#if NET_2_0
using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class StopwatchTest
	{
		[Test]
		public void TestSimple ()
		{
			// It starts at started state.
			Stopwatch sw = Stopwatch.StartNew ();
			Thread.Sleep (1000);
			sw.Stop ();
			long ticks = sw.ElapsedTicks;
			Assert.IsTrue (sw.ElapsedMilliseconds > 100, "#1");
			Thread.Sleep (1000);
			// do not increment resuts
			Assert.AreEqual (ticks, sw.ElapsedTicks, "#2");
			sw.Start ();
			Thread.Sleep (1000);
			// increment results
			Assert.IsTrue (sw.ElapsedTicks > ticks, "#3");
			ticks = sw.ElapsedTicks;
			sw.Stop ();
			Assert.IsTrue (sw.ElapsedTicks >= ticks, "#4");
			sw.Reset ();
			Assert.AreEqual (0, sw.ElapsedTicks, "#5");
			sw.Start ();
			Thread.Sleep (1000);
			Assert.IsTrue (sw.ElapsedTicks > 100, "#5");
			// This test is not strict but would mostly work.
			Assert.IsTrue (ticks > sw.ElapsedTicks, "#6");
			sw.Stop ();
		}
	}
}
#endif
