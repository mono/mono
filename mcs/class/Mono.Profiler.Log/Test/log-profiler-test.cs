// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Profiler.Log;

namespace Mono.Profiling.Tests {

	static class Program {

		static int Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: log-profiler-test <test name>");
				return 1;
			}

			if (!_tests.TryGetValue (args [0], out var test)) {
				Console.WriteLine ("Unknown test name: '{0}'", args [0]);
				return 1;
			}

			test.Run ();
			return 0;
		}

		static readonly IReadOnlyDictionary<string, ProfilerTest> _tests = new Dictionary<string, ProfilerTest> {
			["do-nothing"] = new DoNothingTest (),
			["busy-work"] = new BusyWorkTest (),
			["idle-sleep"] = new IdleSleepTest (),
			["simple-allocation"] = new SimpleAllocationTest (),
			["wasteful-allocation"] = new WastefulAllocationTest (),
			["runtime-api"] = new RuntimeApiTest (),
			["exception-clause"] = new ExceptionClauseTest (),
			["monitor-lock"] = new MonitorLockTest (),
			["backtrace"] = new BacktraceTest (),
			["gc-handle"] = new GCHandleTest (),
			["finalization"] = new FinalizationTest (),
			["pinvoke"] = new PInvokeTest (),
		};
	}

	abstract class ProfilerTest {

		public abstract void Run ();
	}

	sealed class DoNothingTest : ProfilerTest {

		public override void Run ()
		{
		}
	}

	sealed class BusyWorkTest : ProfilerTest {

		int _value;

		public override void Run ()
		{
			var threads = new Thread [4];

			for (var i = 0; i < threads.Length; i++) {
				threads [i] = new Thread (Work) {
					Name = "BusyWork" + i,
				};
			}

			foreach (var thread in threads)
				thread.Start ();

			foreach (var thread in threads)
				thread.Join ();
		}

		void Work ()
		{
			for (var i = 0; i < 500000000; i++)
				_value /= (i + 100) * 1000;
		}
	}

	sealed class IdleSleepTest : ProfilerTest {

		public override void Run ()
		{
			var threads = new Thread [4];

			for (var i = 0; i < threads.Length; i++) {
				threads [i] = new Thread (Sleep) {
					Name = "IdleSleep" + i,
				};
			}

			foreach (var thread in threads)
				thread.Start ();

			foreach (var thread in threads)
				thread.Join ();
		}

		static void Sleep ()
		{
			Thread.Sleep (5000);
		}
	}

	sealed class SimpleAllocationTest : ProfilerTest {

		readonly (object, int[], Exception)[] _objects = new (object, int[], Exception) [10000];

		public override void Run ()
		{
			for (var i = 0; i < _objects.Length; i++)
				_objects [i] = (new object (), new int [i], new Exception ());
		}
	}

	sealed class WastefulAllocationTest : ProfilerTest {

		public override void Run ()
		{
			for (var i = 0; i < 100000; i++) {
				var dummy = new int [i];
			}
		}
	}

	sealed class RuntimeApiTest : ProfilerTest {

		readonly object[] _array = new object [1000];

		public override void Run ()
		{
			if (!LogProfiler.IsAttached)
				throw new Exception ("Where's the log profiler?");

			for (var i = 0; i < _array.Length; i++)
				_array [i] = new object ();

			LogProfiler.TriggerHeapshot ();

			Thread.Sleep (5000);
		}
	}

	sealed class ExceptionClauseTest : ProfilerTest {

		sealed class CustomException : Exception {
		}

		public override void Run ()
		{
			var ex = new CustomException ();

			try {
				throw ex;
			} catch (Exception e) when (e is CustomException) {
			}

			try {
				throw ex;
			} catch (Exception) {
			} finally {
				Dummy ();
			}
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		static void Dummy ()
		{
		}
	}

	sealed class MonitorLockTest : ProfilerTest {

		readonly object _lock = new object ();

		public override void Run ()
		{
			var thread = new Thread (TryLock) {
				Name = "MonitorLock1",
			};

			Monitor.Enter (_lock);
			thread.Start ();
			Thread.Sleep (10000);
			Monitor.Exit (_lock);
			thread.Join ();

			thread = new Thread (Lock) {
				Name = "MonitorLock2",
			};

			thread.Start ();
			Lock ();
			thread.Join ();
		}

		void TryLock ()
		{
			while (!Monitor.TryEnter (_lock, 1));

			Monitor.Exit (_lock);
		}

		void Lock ()
		{
			for (var i = 0; i < 1000000; i++) {
				lock (_lock) {
				}
			}
		}
	}

	sealed class BacktraceTest : ProfilerTest {

		sealed class CustomException : Exception {
		}

		public override void Run ()
		{
			try {
				One ();
			} catch (CustomException) {
			}
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		static void One ()
		{
			Two ();
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		static void Two ()
		{
			Three ();
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		static void Three ()
		{
			Four ();
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		static void Four ()
		{
			throw new CustomException ();
		}
	}

	sealed class GCHandleTest : ProfilerTest {

		sealed class NormalClass {
		}

		sealed class PinnedClass {
		}

		sealed class WeakClass {
		}

		sealed class WeakTrackResurrectionClass {
		}

		public override void Run ()
		{
			var normal = GCHandle.Alloc (new NormalClass (), GCHandleType.Normal);
			normal.Free ();

			var pinned = GCHandle.Alloc (new PinnedClass (), GCHandleType.Pinned);
			pinned.Free ();

			var weak = GCHandle.Alloc (new WeakClass (), GCHandleType.Weak);
			weak.Free ();

			var weakTrack = GCHandle.Alloc (new WeakTrackResurrectionClass (), GCHandleType.WeakTrackResurrection);
			weakTrack.Free ();
		}
	}

	sealed class FinalizationTest : ProfilerTest {

		sealed class FinalizableClass {

			static int _finalized;

			~FinalizableClass ()
			{
				_finalized++;
			}
		}

		public override void Run ()
		{
			for (var i = 0; i < 10000; i++) {
				new FinalizableClass ();

				if (i % 1000 == 0) {
					GC.Collect ();
					GC.WaitForPendingFinalizers ();
				}
			}
		}
	}

	sealed class PInvokeTest : ProfilerTest {

		[DllImport ("libc", EntryPoint = "uname")]
		static extern int KernelName (IntPtr buf);

		public override void Run ()
		{
			for (var i = 0; i < 10000000; i++)
				KernelName (IntPtr.Zero);
		}
	}
}
