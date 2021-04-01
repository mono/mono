// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Mono.Profiler.Log {

	// mono/profiler/log.h : TYPE_*
	enum LogEventType {
		Allocation = 0,
		GC = 1,
		Metadata = 2,
		Method = 3,
		Exception = 4,
		Monitor = 5,
		Heap = 6,
		Sample = 7,
		Runtime = 8,
		Meta = 10,

		AllocationNoBacktrace = 0 << 4,
		AllocationBacktrace = 1 << 4,

		GCEvent = 1 << 4,
		GCResize = 2 << 4,
		GCMove = 3 << 4,
		GCHandleCreationNoBacktrace = 4 << 4,
		GCHandleDeletionNoBacktrace = 5 << 4,
		GCHandleCreationBacktrace = 6 << 4,
		GCHandleDeletionBacktrace = 7 << 4,
		GCFinalizeBegin = 8 << 4,
		GCFinalizeEnd = 9 << 4,
		GCFinalizeObjectBegin = 10 << 4,
		GCFinalizeObjectEnd = 11 << 4,

		MetadataExtra = 0 << 4,
		MetadataEndLoad = 2 << 4,
		MetadataEndUnload = 4 << 4,

		MethodLeave = 1 << 4,
		MethodEnter = 2 << 4,
		MethodLeaveExceptional = 3 << 4,
		MethodJit = 4 << 4,

		ExceptionThrowNoBacktrace = 0 << 7,
		ExceptionThrowBacktrace = 1 << 7,
		ExceptionClause = 1 << 4,

		MonitorNoBacktrace = 0 << 7,
		MonitorBacktrace = 1 << 7,

		HeapBegin = 0 << 4,
		HeapEnd = 1 << 4,
		HeapObject = 2 << 4,
		HeapRoots = 3 << 4,
		HeapRootRegister = 4 << 4,
		HeapRootUnregister = 5 << 4,

		SampleHit = 0 << 4,
		SampleUnmanagedSymbol = 1 << 4,
		SampleUnmanagedBinary = 2 << 4,
		SampleCounterDescriptions = 3 << 4,
		SampleCounters = 4 << 4,

		RuntimeJitHelper = 1 << 4,

		MetaSynchronizationPoint = 0 << 4,
		MetaAotId = 1 << 4,
	}

	// mono/profiler/log.h : TYPE_*
	enum LogMetadataType {
		Class = 1,
		Image = 2,
		Assembly = 3,
		AppDomain = 4,
		Thread = 5,
		Context = 6,
		VTable = 7,
	}

	// mono/utils/mono-counters.h : MONO_COUNTER_*
	public enum LogCounterType {
		Int32 = 0,
		UInt32 = 1,
		Word = 2,
		Int64 = 3,
		UInt64 = 4,
		Double = 5,
		String = 6,
		Interval = 7,
	}

	// mono/utils/mono-counters.h : MONO_COUNTER_*
	public enum LogCounterSection {
		Jit = 1 << 8,
		GC = 1 << 9,
		Metadata = 1 << 10,
		Generics = 1 << 11,
		Security = 1 << 12,
		Runtime = 1 << 13,
		System = 1 << 14,
		User = 1 << 15,
		Profiler = 1 << 16,
	}

	// mono/utils/mono-counters.h : MONO_COUNTER_*
	public enum LogCounterUnit {
		Raw = 0 << 24,
		Bytes = 1 << 24,
		Time = 2 << 24,
		Count = 3 << 24,
		Percentage = 4 << 24,
	}

	// mono/utils/mono-counters.h : MONO_COUNTER_*
	public enum LogCounterVariance {
		Monotonic = 1 << 28,
		Constant = 1 << 29,
		Variable = 1 << 30,
	}

	// mono/metadata/profiler.h : MonoProfilerCodeBufferType
	public enum LogJitHelper {
		Method = 0,
		[Obsolete ("This value is no longer produced.")]
		MethodTrampoline = 1,
		UnboxTrampoline = 2,
		ImtTrampoline = 3,
		GenericsTrampoline = 4,
		SpecificTrampoline = 5,
		Helper = 6,
		[Obsolete ("This value is no longer produced.")]
		Monitor = 7,
		DelegateInvoke = 8,
		ExceptionHandling = 9,
	}

	// mono/metadata/profiler.h : MonoProfilerGCRootType
	[Flags]
	[Obsolete ("The event field using this enum is no longer produced.")]
	public enum LogHeapRootAttributes {
		Pinning = 1 << 8,
		WeakReference = 2 << 8,
		Interior = 4 << 8,

		Stack = 1 << 0,
		Finalizer = 1 << 1,
		Handle = 1 << 2,
		Other = 1 << 3,
		Miscellaneous = 1 << 4,

		TypeMask = 0xff,
	}

	// mono/metadata/mono-gc.h : MonoGCRootSource
	public enum LogHeapRootSource {
		External = 0,
		Stack = 1,
		FinalizerQueue = 2,
		Static = 3,
		ThreadStatic = 4,
		ContextStatic = 5,
		GCHandle = 6,
		Jit = 7,
		Threading = 8,
		AppDomain = 9,
		Reflection = 10,
		Marshal = 11,
		ThreadPool = 12,
		Debugger = 13,
		Handle = 14,
		Ephemeron = 15,
		Toggleref = 16,
	}

	// mono/profiler/log.h : MonoProfilerMonitorEvent
	public enum LogMonitorEvent {
		Contention = 1,
		Done = 2,
		Fail = 3,
	}

	// mono/metadata/metadata.h : MonoExceptionEnum
	public enum LogExceptionClause {
		Catch = 0,
		Filter = 1,
		Finally = 2,
		Fault = 4,
	}

	// mono/metadata/profiler.h : MonoProfilerGCEvent
	public enum LogGCEvent {
		PreStopWorld = 6,
		PreStopWorldLocked = 10,
		PostStopWorld = 7,
		Begin = 0,
		End = 5,
		PreStartWorld = 8,
		PostStartWorld = 9,
		PostStartWorldUnlocked = 11,
		// Following are v13 and older only
		[Obsolete ("This event is no longer produced.")]
		MarkBegin = 1,
		[Obsolete ("This event is no longer produced.")]
		MarkEnd = 2,
		[Obsolete ("This event is no longer produced.")]
		ReclaimBegin = 3,
		[Obsolete ("This event is no longer produced.")]
		ReclaimEnd = 4
	}

	// mono/metadata/mono-gc.h : MonoGCHandleType
	public enum LogGCHandleType {
		Weak = 0,
		WeakTrackResurrection = 1,
		Normal = 2,
		Pinned = 3,
	}

	// mono/profiler/log.h : MonoProfilerSyncPointType
	public enum LogSynchronizationPoint {
		Periodic = 0,
		WorldStop = 1,
		WorldStart = 2,
	}

	// mono/metadata/profiler.h : MonoProfilerSampleMode
	public enum LogSampleMode {
		None = 0,
		Process = 1,
		Real = 2,
	}

	// mono/profiler/log.h : MonoProfilerHeapshotMode
	public enum LogHeapshotMode {
		None = 0,
		EveryMajor = 1,
		OnDemand = 2,
		Milliseconds = 3,
		Collections = 4,
	}
}
