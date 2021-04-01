// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Profiler.Log;
using Xunit;

namespace MonoTests.Mono.Profiler.Log {

	public sealed class ProfilerTests {

		[Fact]
		public void BasicMetadataIsPresent ()
		{
			new ProfilerTestRun ("do-nothing", "jit").Run (events => {
				var domainName = events.OfType<AppDomainNameEvent> ().SingleOrDefault (e =>
					e.Name == "log-profiler-test.exe");

				Assert.NotNull (domainName);
				Assert.Equal (0, domainName.AppDomainId);
				Assert.Contains (events, ev =>
					ev is AppDomainLoadEvent e &&
					e.AppDomainId == domainName.AppDomainId);
				Assert.Contains (events, ev =>
					ev is ContextLoadEvent e &&
					e.AppDomainId == domainName.AppDomainId &&
					e.ContextId == 0);

				var imageLoad = events.OfType<ImageLoadEvent> ().SingleOrDefault (e =>
					Path.GetFileName (e.Name) == "log-profiler-test.exe");

				Assert.NotNull (imageLoad);
				Assert.Contains (events, ev =>
					ev is AssemblyLoadEvent e &&
					e.ImagePointer == imageLoad.ImagePointer &&
					new AssemblyName (e.Name).Name == "log-profiler-test");
				Assert.Contains (events, ev =>
					ev is ClassLoadEvent e &&
					e.ImagePointer == imageLoad.ImagePointer &&
					e.Name == "Mono.Profiling.Tests.DoNothingTest");

				var threadName = events.OfType<ThreadNameEvent> ().SingleOrDefault (e =>
					e.Name == "Main");

				Assert.NotNull (threadName);
				Assert.Contains (events, ev =>
					ev is ThreadStartEvent e &&
					e.ThreadId == threadName.ThreadId);

				Assert.Contains (events, ev =>
					ev is JitEvent e &&
					e.Name == "Mono.Profiling.Tests.DoNothingTest:Run ()");
				Assert.Contains (events, ev =>
					ev is JitHelperEvent e && e.Type == LogJitHelper.ExceptionHandling);
				Assert.Contains (events, ev =>
					ev is JitHelperEvent e && e.Type == LogJitHelper.Method);
			});
		}

		[Fact]
		public void OnDemandHeapshotTriggeredViaApi ()
		{
			new ProfilerTestRun ("runtime-api", "heapshot=ondemand").Run (events => {
				var classLoad = events.OfType<ClassLoadEvent> ().SingleOrDefault (ev =>
					ev.Name == "System.Object");

				Assert.NotNull (classLoad);

				var vtableLoad = events.OfType<VTableLoadEvent> ().SingleOrDefault (ev =>
					ev.ClassPointer == classLoad.ClassPointer);

				Assert.NotNull (vtableLoad);
				Assert.Equal (1, events.OfType<HeapBeginEvent> ().Count ());
				Assert.Equal (1, events.OfType<HeapEndEvent> ().Count ());
				AssertExtensions.GreaterThanOrEqualTo (events.OfType<HeapObjectEvent> ().Count (ev =>
					ev.VTablePointer == vtableLoad.VTablePointer), 1000);
			});
		}

		[Fact]
		public void OnDemandHeapshotNotTriggered ()
		{
			new ProfilerTestRun ("simple-allocation", "heapshot=ondemand").Run (events => {
				Assert.DoesNotContain (events, ev =>
					ev is HeapBeginEvent ||
					ev is HeapEndEvent ||
					ev is HeapObjectEvent ||
					ev is HeapRootsEvent);
			});
		}

		[Fact]
		public void HeapshotTriggeredOnShutdown ()
		{
			new ProfilerTestRun ("simple-allocation", "heapshot-on-shutdown").Run (events => {
				Assert.Equal (1, events.OfType<HeapBeginEvent> ().Count ());
			});
		}

		[Fact]
		public void AllocationEventsAreRecorded ()
		{
			new ProfilerTestRun ("simple-allocation", "gcalloc").Run (events => {
				var objClassLoad = events.OfType<ClassLoadEvent> ().SingleOrDefault (ev =>
					ev.Name == "System.Object");
				var intArrClassLoad = events.OfType<ClassLoadEvent> ().SingleOrDefault (ev =>
					ev.Name == "System.Int32[]");
				var excClassLoad = events.OfType<ClassLoadEvent> ().SingleOrDefault (ev =>
					ev.Name == "System.Exception");

				Assert.NotNull (objClassLoad);
				Assert.NotNull (intArrClassLoad);
				Assert.NotNull (excClassLoad);

				var objVTableLoad = events.OfType<VTableLoadEvent> ().SingleOrDefault (ev =>
					ev.ClassPointer == objClassLoad.ClassPointer);
				var intArrVTableLoad = events.OfType<VTableLoadEvent> ().SingleOrDefault (ev =>
					ev.ClassPointer == intArrClassLoad.ClassPointer);
				var excVTableLoad = events.OfType<VTableLoadEvent> ().SingleOrDefault (ev =>
					ev.ClassPointer == excClassLoad.ClassPointer);

				Assert.NotNull (objVTableLoad);
				Assert.NotNull (intArrVTableLoad);
				Assert.NotNull (excVTableLoad);

				AssertExtensions.GreaterThanOrEqualTo (events.OfType<AllocationEvent> ().Count (ev =>
					ev.VTablePointer == objVTableLoad.VTablePointer), 10000);
				AssertExtensions.GreaterThanOrEqualTo (events.OfType<AllocationEvent> ().Count (ev =>
					ev.VTablePointer == intArrVTableLoad.VTablePointer), 10000);
				AssertExtensions.GreaterThanOrEqualTo (events.OfType<AllocationEvent> ().Count (ev =>
					ev.VTablePointer == excVTableLoad.VTablePointer), 10000);
			});
		}

		[Fact]
		public void ExceptionEventsAreRecorded ()
		{
			new ProfilerTestRun ("exception-clause", "exception,gcalloc").Run (events => {
				var jit = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.ExceptionClauseTest:Run ()");

				Assert.NotNull (jit);

				var classLoad = events.OfType<ClassLoadEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.ExceptionClauseTest.CustomException");

				Assert.NotNull (classLoad);

				var vtableLoad = events.OfType<VTableLoadEvent> ().SingleOrDefault (ev =>
					ev.ClassPointer == classLoad.ClassPointer);

				Assert.NotNull (vtableLoad);

				var alloc = events.OfType<AllocationEvent> ().SingleOrDefault (ev =>
					ev.VTablePointer == vtableLoad.VTablePointer);

				Assert.Equal (2, events.OfType<ThrowEvent> ().Count (ev =>
					ev.ObjectPointer == alloc.ObjectPointer));
				Assert.Contains (events, ev =>
					ev is ExceptionClauseEvent e &&
					e.MethodPointer == jit.MethodPointer &&
					e.ObjectPointer == alloc.ObjectPointer &&
					e.Type == LogExceptionClause.Catch);
				Assert.Contains (events, ev =>
					ev is ExceptionClauseEvent e &&
					e.MethodPointer == jit.MethodPointer &&
					e.ObjectPointer == alloc.ObjectPointer &&
					e.Type == LogExceptionClause.Filter);
				Assert.Contains (events, ev =>
					ev is ExceptionClauseEvent e &&
					e.MethodPointer == jit.MethodPointer &&
					e.ObjectPointer == alloc.ObjectPointer &&
					e.Type == LogExceptionClause.Finally);
			});
		}

		[Fact]
		public void EventsHaveBacktraces ()
		{
			new ProfilerTestRun ("backtrace", "exception,gcalloc").Run (events => {
				var jitOne = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BacktraceTest:One ()");
				var jitTwo = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BacktraceTest:Two ()");
				var jitThree = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BacktraceTest:Three ()");
				var jitFour = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BacktraceTest:Four ()");

				Assert.NotNull (jitOne);
				Assert.NotNull (jitTwo);
				Assert.NotNull (jitThree);
				Assert.NotNull (jitFour);

				var classLoad = events.OfType<ClassLoadEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BacktraceTest.CustomException");

				Assert.NotNull (classLoad);

				var vtableLoad = events.OfType<VTableLoadEvent> ().SingleOrDefault (ev =>
					ev.ClassPointer == classLoad.ClassPointer);

				Assert.NotNull (vtableLoad);

				var alloc = events.OfType<AllocationEvent> ().SingleOrDefault (ev =>
					ev.VTablePointer == vtableLoad.VTablePointer);

				Assert.NotNull (alloc);
				AssertExtensions.GreaterThanOrEqualTo (alloc.Backtrace.Count, 6);
				Assert.Equal (jitOne.MethodPointer, alloc.Backtrace [alloc.Backtrace.Count - 6]);
				Assert.Equal (jitTwo.MethodPointer, alloc.Backtrace [alloc.Backtrace.Count - 5]);
				Assert.Equal (jitThree.MethodPointer, alloc.Backtrace [alloc.Backtrace.Count - 4]);
				Assert.Equal (jitFour.MethodPointer, alloc.Backtrace [alloc.Backtrace.Count - 3]);

				var raise = events.OfType<ThrowEvent> ().SingleOrDefault (ev =>
					ev.ObjectPointer == alloc.ObjectPointer);

				Assert.NotNull (raise);
				AssertExtensions.GreaterThanOrEqualTo (raise.Backtrace.Count, 4);
				Assert.Equal (jitOne.MethodPointer, raise.Backtrace [raise.Backtrace.Count - 4]);
				Assert.Equal (jitTwo.MethodPointer, raise.Backtrace [raise.Backtrace.Count - 3]);
				Assert.Equal (jitThree.MethodPointer, raise.Backtrace [raise.Backtrace.Count - 2]);
				Assert.Equal (jitFour.MethodPointer, raise.Backtrace [raise.Backtrace.Count - 1]);
			});
		}

		[Fact]
		public void BusySamplingWorks ()
		{
			new ProfilerTestRun ("busy-work", "sample").Run (events => {
				var jit = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BusyWorkTest:Work ()");

				Assert.NotNull (jit);
				AssertExtensions.GreaterThanOrEqualTo (events.OfType<SampleHitEvent> ().Count (ev =>
					ev.ManagedBacktrace.LastOrDefault () == jit.MethodPointer), 500);
			});
		}

		[Fact]
		public void RealTimeSamplingWorks ()
		{
			new ProfilerTestRun ("idle-sleep", "sample-real").Run (events => {
				AssertExtensions.GreaterThanOrEqualTo (events.OfType<SampleHitEvent> ().Count (), 2000);
			});
		}

		[Fact]
		public void ProcessTimeSamplingWorks ()
		{
			if (!RuntimeInformation.IsOSPlatform (OSPlatform.Linux))
				return;

			new ProfilerTestRun ("idle-sleep", "sample").Run (events => {
				AssertExtensions.LessThan (events.OfType<SampleHitEvent> ().Count (), 100);
			});

			new ProfilerTestRun ("busy-work", "sample").Run (events => {
				AssertExtensions.GreaterThanOrEqualTo (events.OfType<SampleHitEvent> ().Count (), 2500);
			});
		}

		[Fact]
		public void MonitorEventsAreRecorded ()
		{
			new ProfilerTestRun ("monitor-lock", "monitor").Run (events => {
				Assert.Contains (events, ev =>
					ev is MonitorEvent e &&
					e.Event == LogMonitorEvent.Done);

				var countCont = events.OfType<MonitorEvent> ().Count (ev =>
					ev.Event == LogMonitorEvent.Contention);
				var countFail = events.OfType<MonitorEvent> ().Count (ev =>
					ev.Event == LogMonitorEvent.Fail);

				AssertExtensions.GreaterThanOrEqualTo (countFail, 1000);
				AssertExtensions.GreaterThanOrEqualTo (countCont, 1000);
				AssertExtensions.GreaterThan (countCont, countFail);
			});
		}

		[Fact (Skip = "https://github.com/mono/mono/issues/7560")]
		public void UnmanagedSymbolsAreResolved ()
		{
			if (!RuntimeInformation.IsOSPlatform (OSPlatform.Linux) &&
			    !RuntimeInformation.IsOSPlatform (OSPlatform.OSX))
				return;

			new ProfilerTestRun ("pinvoke", "sample-real").Run (events => {
				Assert.Contains (events, ev =>
					ev is UnmanagedSymbolEvent e &&
					e.Name == "uname");
			});
		}

		[Fact]
		public void FinalizationEventsAreRecorded ()
		{
			new ProfilerTestRun ("finalization", "finalization").Run (events => {
				var countBegin = events.OfType<GCFinalizeBeginEvent> ().Count ();
				var countEnd = events.OfType<GCFinalizeEndEvent> ().Count ();

				AssertExtensions.GreaterThanOrEqualTo (countBegin, 10);
				Assert.Equal (countBegin, countEnd);

				var countObjBegin = events.OfType<GCFinalizeObjectBeginEvent> ().Count ();
				var countObjEnd = events.OfType<GCFinalizeObjectEndEvent> ().Count ();

				AssertExtensions.GreaterThanOrEqualTo (countObjBegin, 5000);
				Assert.Equal (countObjBegin, countObjEnd);
			});
		}

		[Fact]
		public void EnterLeaveEventsAreRecorded ()
		{
			new ProfilerTestRun ("backtrace", "calls").Run (events => {
				var jitRun = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BacktraceTest:Run ()");
				var jitOne = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BacktraceTest:One ()");
				var jitTwo = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BacktraceTest:Two ()");
				var jitThree = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BacktraceTest:Three ()");
				var jitFour = events.OfType<JitEvent> ().SingleOrDefault (ev =>
					ev.Name == "Mono.Profiling.Tests.BacktraceTest:Four ()");

				Assert.NotNull (jitRun);
				Assert.NotNull (jitOne);
				Assert.NotNull (jitTwo);
				Assert.NotNull (jitThree);
				Assert.NotNull (jitFour);

				var expected = new Queue<(Type, long)> ();

				expected.Enqueue ((typeof (EnterEvent), jitRun.MethodPointer));
				expected.Enqueue ((typeof (EnterEvent), jitOne.MethodPointer));
				expected.Enqueue ((typeof (EnterEvent), jitTwo.MethodPointer));
				expected.Enqueue ((typeof (EnterEvent), jitThree.MethodPointer));
				expected.Enqueue ((typeof (EnterEvent), jitFour.MethodPointer));
				expected.Enqueue ((typeof (ExceptionalLeaveEvent), jitFour.MethodPointer));
				expected.Enqueue ((typeof (ExceptionalLeaveEvent), jitThree.MethodPointer));
				expected.Enqueue ((typeof (ExceptionalLeaveEvent), jitTwo.MethodPointer));
				expected.Enqueue ((typeof (ExceptionalLeaveEvent), jitOne.MethodPointer));
				expected.Enqueue ((typeof (LeaveEvent), jitRun.MethodPointer));

				var addrs = new HashSet<long> (new[] { jitRun, jitOne, jitTwo, jitThree, jitFour }.Select (ev =>
					ev.MethodPointer));

				foreach (var ev in events.Where (ev =>
					ev is EnterEvent || ev is LeaveEvent || ev is ExceptionalLeaveEvent)) {
					long method;

					switch (ev) {
					case EnterEvent e:
						method = e.MethodPointer;
						break;
					case LeaveEvent e:
						method = e.MethodPointer;
						break;
					case ExceptionalLeaveEvent e:
						method = e.MethodPointer;
						break;
					default:
						throw new Exception ();
					}

					if (!addrs.Contains (method))
						continue;

					Assert.Equal (expected.Dequeue (), (ev.GetType (), method));
				}

				Assert.Equal (0, expected.Count);
			});
		}

		[Fact]
		public void EnterLeaveStackIsCorrect ()
		{
			new ProfilerTestRun ("backtrace", "calls").Run (events => {
				var stack = new Stack<long> ();

				foreach (var ev in events) {
					long leave = 0;

					switch (ev) {
					case EnterEvent e:
						stack.Push (e.MethodPointer);
						break;
					case LeaveEvent e:
						leave = e.MethodPointer;
						break;
					case ExceptionalLeaveEvent e:
						leave = e.MethodPointer;
						break;
					default:
						break;
					}

					if (leave != 0)
						Assert.Equal (stack.Pop (), leave);
				}

				Assert.Equal (0, stack.Count);
			});
		}

		[Fact]
		public void GCEventsAreRecorded ()
		{
			new ProfilerTestRun ("wasteful-allocation", "gc").Run (events => {
				AssertExtensions.GreaterThanOrEqualTo (events.OfType<GCResizeEvent> ().Count (), 500);
				AssertExtensions.GreaterThanOrEqualTo (events.OfType<GCEvent> ().Count (), 10000);
				Assert.Contains (events, ev =>
					ev is GCEvent e &&
					e.Type == LogGCEvent.PreStopWorld);
				Assert.Contains (events, ev =>
					ev is GCEvent e &&
					e.Type == LogGCEvent.PreStopWorldLocked);
				Assert.Contains (events, ev =>
					ev is GCEvent e &&
					e.Type == LogGCEvent.PostStopWorld);
				Assert.Contains (events, ev =>
					ev is GCEvent e &&
					e.Type == LogGCEvent.Begin);
				Assert.Contains (events, ev =>
					ev is GCEvent e &&
					e.Type == LogGCEvent.End);
				Assert.Contains (events, ev =>
					ev is GCEvent e &&
					e.Type == LogGCEvent.PreStartWorld);
				Assert.Contains (events, ev =>
					ev is GCEvent e &&
					e.Type == LogGCEvent.PostStartWorld);
				Assert.Contains (events, ev =>
					ev is GCEvent e &&
					e.Type == LogGCEvent.PostStartWorldUnlocked);
				Assert.Contains (events, ev =>
					ev is GCEvent e &&
					e.Generation == 0);
				Assert.Contains (events, ev =>
					ev is GCEvent e &&
					e.Generation == 1);
			});
		}

		[Fact]
		public void CounterSamplingWorks ()
		{
			new ProfilerTestRun ("idle-sleep", "counter").Run (events => {
				var descs = events.OfType<CounterDescriptionsEvent> ().SelectMany (ev =>
					ev.Descriptions);

				AssertExtensions.GreaterThanOrEqualTo (descs.Count (), 50);

				var samples = events.OfType<CounterSamplesEvent> ().SelectMany (ev =>
					ev.Samples);

				AssertExtensions.GreaterThanOrEqualTo (samples.Count (), 5);

				var indexes = new HashSet<long> (descs.Select (x => x.Index));

				Assert.All (samples, sample => indexes.Contains (sample.Index));
			});
		}

		[Fact]
		public void GCHandleEventsAreRecorded ()
		{
			new ProfilerTestRun ("gc-handle", "gcalloc,gchandle").Run (events => {
				foreach (var type in new[] { "Normal", "Pinned", "Weak", "WeakTrackResurrection" }) {
					var classLoad = events.OfType<ClassLoadEvent> ().SingleOrDefault (ev =>
						ev.Name == $"Mono.Profiling.Tests.GCHandleTest.{type}Class");

					Assert.NotNull (classLoad);

					var vtableLoad = events.OfType<VTableLoadEvent> ().SingleOrDefault (ev =>
						ev.ClassPointer == classLoad.ClassPointer);

					Assert.NotNull (vtableLoad);

					var alloc = events.OfType<AllocationEvent> ().SingleOrDefault (ev =>
						ev.VTablePointer == vtableLoad.VTablePointer);

					Assert.NotNull (alloc);

					var typeParsed = (LogGCHandleType) Enum.Parse (typeof (LogGCHandleType), type);
					var create = events.OfType<GCHandleCreationEvent> ().SingleOrDefault (ev =>
						ev.ObjectPointer == alloc.ObjectPointer &&
						ev.Type == typeParsed);

					Assert.NotNull (create);

					var delete = events.OfType<GCHandleDeletionEvent> ().SingleOrDefault (ev =>
						ev.Handle == create.Handle &&
						ev.Type == typeParsed);

					Assert.NotNull (delete);
				}
			});
		}

		[Fact]
		public void HeapshotDataIsValid ()
		{
			new ProfilerTestRun ("simple-allocation", "heapshot").Run (events => {
				var threadIds = new HashSet<long> (events.OfType<ThreadStartEvent> ().Select (ev =>
					ev.ThreadId));
				var domainIds = new HashSet<long> (events.OfType<AppDomainLoadEvent> ().Select (ev =>
					ev.AppDomainId));
				var contextIds = new HashSet<long> (events.OfType<ContextLoadEvent> ().Select (ev =>
					ev.ContextId));
				var vtables = new HashSet<long> (events.OfType<VTableLoadEvent> ().Select (ev =>
					ev.VTablePointer));
				var ranges = new Dictionary<long, long> ();
				var objects = new Dictionary<long, List<HeapObjectEvent.HeapObjectReference>> ();
				var roots = new List<HeapRootsEvent.HeapRoot> ();
				var inHeapshot = false;

				foreach (var ev in events) {
					switch (ev) {
					case HeapBeginEvent e:
						Assert.False (inHeapshot);
						inHeapshot = true;
						break;
					case HeapEndEvent e:
						Assert.True (inHeapshot);
						inHeapshot = false;

						foreach (var kvp in objects)
							Assert.All (kvp.Value, reference => objects.ContainsKey (reference.ObjectPointer));

						foreach (var root in roots) {
							Assert.Contains (ranges, kvp =>
								root.SlotPointer >= kvp.Key && root.SlotPointer < kvp.Key + kvp.Value);
							Assert.True (objects.ContainsKey (root.ObjectPointer));
						}

						objects.Clear ();
						roots.Clear ();
						break;
					case HeapRootRegisterEvent e:
						Assert.False (inHeapshot);

						switch (e.Source) {
						case LogHeapRootSource.Stack:
						case LogHeapRootSource.ThreadStatic:
						case LogHeapRootSource.Handle:
							Assert.NotEqual (0, e.Key);
							Assert.Contains (e.Key, threadIds);
							break;
						case LogHeapRootSource.Static:
							Assert.NotEqual (0, e.Key);
							Assert.Contains (e.Key, vtables);
							break;
						case LogHeapRootSource.ContextStatic:
							Assert.Contains (e.Key, contextIds);
							break;
						case LogHeapRootSource.Threading:
							if (e.Key != 0)
								Assert.Contains (e.Key, threadIds);
							break;
						case LogHeapRootSource.AppDomain:
							Assert.Contains (e.Key, domainIds);
							break;
						case LogHeapRootSource.Reflection:
							if (e.Key != 0)
								Assert.Contains (e.Key, vtables);
							break;
						}

						ranges [e.RootPointer] = e.RootSize;
						break;
					case HeapRootUnregisterEvent e:
						Assert.False (inHeapshot);
						Assert.True (ranges.Remove (e.RootPointer));
						break;
					case HeapObjectEvent e:
						Assert.True (inHeapshot);
						Assert.NotEqual (0, e.ObjectPointer);
						Assert.Contains (e.VTablePointer, vtables);
						Assert.True (e.Generation == 0 || e.Generation == 1);

						if (e.ObjectSize != 0)
							objects.Add (e.ObjectPointer, e.References.ToList ());
						else
							objects [e.ObjectPointer].AddRange (e.References);
						break;
					case HeapRootsEvent e:
						Assert.True (inHeapshot);
						roots.AddRange (e.Roots);
						break;
					default:
						break;
					}
				}
			});
		}
	}
}
