// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Mono.Profiler.Log {

	public sealed class AppDomainLoadEvent : LogEvent {

		public long AppDomainId { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class AppDomainUnloadEvent : LogEvent {

		public long AppDomainId { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class AppDomainNameEvent : LogEvent {

		public long AppDomainId { get; internal set; }

		public string Name { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ContextLoadEvent : LogEvent {

		public long ContextId { get; internal set; }

		public long AppDomainId { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ContextUnloadEvent : LogEvent {

		public long ContextId { get; internal set; }

		public long AppDomainId { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ThreadStartEvent : LogEvent {

		public long ThreadId { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ThreadEndEvent : LogEvent {

		public long ThreadId { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ThreadNameEvent : LogEvent {

		public long ThreadId { get; internal set; }

		public string Name { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ImageLoadEvent : LogEvent {

		public long ImagePointer { get; internal set; }

		public string Name { get; internal set; }

		public Guid ModuleVersionId { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ImageUnloadEvent : LogEvent {

		public long ImagePointer { get; internal set; }

		public string Name { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class AssemblyLoadEvent : LogEvent {

		public long AssemblyPointer { get; internal set; }

		public long ImagePointer { get; internal set; }

		public string Name { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class AssemblyUnloadEvent : LogEvent {

		public long AssemblyPointer { get; internal set; }

		public long ImagePointer { get; internal set; }

		public string Name { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ClassLoadEvent : LogEvent {

		public long ClassPointer { get; internal set; }

		public long ImagePointer { get; internal set; }

		public string Name { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class VTableLoadEvent : LogEvent {

		public long VTablePointer { get; internal set; }

		public long AppDomainId { get; internal set; }

		public long ClassPointer { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class JitEvent : LogEvent {

		public long MethodPointer { get; internal set; }

		public long CodePointer { get; internal set; }

		public long CodeSize { get; internal set; }

		public string Name { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class JitHelperEvent : LogEvent {

		public LogJitHelper Type { get; internal set; }

		public long BufferPointer { get; internal set; }

		public long BufferSize { get; internal set; }

		public string Name { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class AllocationEvent : LogEvent {

		[Obsolete ("This field is no longer produced.")]
		public long ClassPointer { get; internal set; }

		public long VTablePointer { get; internal set; }

		public long ObjectPointer { get; internal set; }

		public long ObjectSize { get; internal set; }

		public IReadOnlyList<long> Backtrace { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class HeapBeginEvent : LogEvent {

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class HeapEndEvent : LogEvent {

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class HeapObjectEvent : LogEvent {

		public struct HeapObjectReference {

			public long Offset { get; internal set; }

			public long ObjectPointer { get; internal set; }
		}

		public long ObjectPointer { get; internal set; }

		[Obsolete ("This field is no longer produced.")]
		public long ClassPointer { get; internal set; }

		public long VTablePointer { get; internal set; }

		public long ObjectSize { get; internal set; }

		public int Generation { get; internal set; }

		public IReadOnlyList<HeapObjectReference> References { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class HeapRootsEvent : LogEvent {

		public struct HeapRoot {

			public long SlotPointer { get; internal set; }

			public long ObjectPointer { get; internal set; }

			[Obsolete ("This field is no longer produced.")]
			public LogHeapRootAttributes Attributes { get; internal set; }

			[Obsolete ("This field is no longer produced.")]
			public long ExtraInfo { get; internal set; }
		}

		[Obsolete ("This field is no longer produced.")]
		public long MaxGenerationCollectionCount { get; internal set; }

		public IReadOnlyList<HeapRoot> Roots { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class HeapRootRegisterEvent : LogEvent {

		public long RootPointer { get; internal set; }

		public long RootSize { get; internal set; }

		public LogHeapRootSource Source { get; internal set; }

		public long Key { get; internal set; }

		public string Name { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class HeapRootUnregisterEvent : LogEvent {

		public long RootPointer { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class GCEvent : LogEvent {

		public LogGCEvent Type { get; internal set; }

		public int Generation { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class GCResizeEvent : LogEvent {

		public long NewSize { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class GCMoveEvent : LogEvent {

		public IReadOnlyList<long> OldObjectPointers { get; internal set; }

		public IReadOnlyList<long> NewObjectPointers { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class GCHandleCreationEvent : LogEvent {

		public LogGCHandleType Type { get; internal set; }

		public long Handle { get; internal set; }

		public long ObjectPointer { get; internal set; }

		public IReadOnlyList<long> Backtrace { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class GCHandleDeletionEvent : LogEvent {

		public LogGCHandleType Type { get; internal set; }

		public long Handle { get; internal set; }

		public IReadOnlyList<long> Backtrace { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class GCFinalizeBeginEvent : LogEvent {

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class GCFinalizeEndEvent : LogEvent {

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class GCFinalizeObjectBeginEvent : LogEvent {

		public long ObjectPointer { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class GCFinalizeObjectEndEvent : LogEvent {

		public long ObjectPointer { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ThrowEvent : LogEvent {

		public long ObjectPointer { get; internal set; }

		public IReadOnlyList<long> Backtrace { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ExceptionClauseEvent : LogEvent {

		public LogExceptionClause Type { get; internal set; }

		public long Index { get; internal set; }

		public long MethodPointer { get; internal set; }

		public long ObjectPointer { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class EnterEvent : LogEvent {

		public long MethodPointer { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class LeaveEvent : LogEvent {

		public long MethodPointer { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class ExceptionalLeaveEvent : LogEvent {

		public long MethodPointer { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class MonitorEvent : LogEvent {

		public LogMonitorEvent Event { get; internal set; }

		public long ObjectPointer { get; internal set; }

		public IReadOnlyList<long> Backtrace { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class SampleHitEvent : LogEvent {

		public long ThreadId { get; internal set; }

		public IReadOnlyList<long> UnmanagedBacktrace { get; internal set; }

		public IReadOnlyList<long> ManagedBacktrace { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class CounterSamplesEvent : LogEvent {

		public struct CounterSample {

			public long Index { get; internal set; }

			public LogCounterType Type { get; internal set; }

			public object Value { get; internal set; }
		}

		public IReadOnlyList<CounterSample> Samples { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class CounterDescriptionsEvent : LogEvent {

		public struct CounterDescription {

			public LogCounterSection Section { get; internal set; }

			public string SectionName { get; internal set; }

			public string CounterName { get; internal set; }

			public LogCounterType Type { get; internal set; }

			public LogCounterUnit Unit { get; internal set; }

			public LogCounterVariance Variance { get; internal set; }

			public long Index { get; internal set; }
		}

		public IReadOnlyList<CounterDescription> Descriptions { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	[Obsolete ("This event is no longer produced.")]
	public sealed class UnmanagedBinaryEvent : LogEvent {

		public long SegmentPointer { get; internal set; }

		public long SegmentOffset { get; internal set; }

		public long SegmentSize { get; internal set; }

		public string FileName { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class UnmanagedSymbolEvent : LogEvent {

		public long CodePointer { get; internal set; }

		public long CodeSize { get; internal set; }

		public string Name { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class SynchronizationPointEvent : LogEvent {

		public LogSynchronizationPoint Type { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}

	public sealed class AotIdEvent : LogEvent {

		public Guid AotId { get; internal set; }

		internal override void Accept (LogEventVisitor visitor)
		{
			visitor.Visit (this);
		}
	}
}
