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
		Coverage = 9,
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

		SampleHit = 0 << 4,
		SampleUnmanagedSymbol = 1 << 4,
		SampleUnmanagedBinary = 2 << 4,
		SampleCounterDescriptions = 3 << 4,
		SampleCounters = 4 << 4,

		RuntimeJitHelper = 1 << 4,

		CoverageAssembly = 0 << 4,
		CoverageMethod = 1 << 4,
		CoverageStatement = 2 << 4,
		CoverageClass = 3 << 4,

		MetaSynchronizationPoint = 0 << 4,
	}

	// mono/profiler/log.h : TYPE_*
	enum LogMetadataType {
		Class = 1,
		Image = 2,
		Assembly = 3,
		AppDomain = 4,
		Thread = 5,
		Context = 6,
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
		Unknown = 0,
		Method = 1,
		MethodTrampoline = 2,
		UnboxTrampoline = 3,
		ImtTrampoline = 4,
		GenericsTrampoline = 5,
		SpecificTrampoline = 6,
		Helper = 7,
		Monitor = 8,
		DelegateInvoke = 9,
		ExceptionHandling = 10,
	}

	// mono/profiler/log.h : SAMPLE_*
	public enum LogSampleHitType {
		Cycles = 1,
		Instructions = 2,
		CacheMisses = 3,
		CacheHits = 4,
		Branches = 5,
		BranchMisses = 6,
	}

	// mono/metadata/profiler.h : MonoProfileGCRootType
	[Flags]
	public enum LogHeapRootAttributes {
		Pinning = 1 << 8,
		WeakReference = 2 << 8,
		Interior = 4 << 8,

		Stack = 0,
		Finalizer = 1,
		Handle = 2,
		Other = 3,
		Miscellaneous = 4,

		TypeMask = 0xff,
	}

	// mono/metadata/profiler.h : MonoProfilerMonitorEvent
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

	// mono/metadata/profiler.h : MonoGCEvent
	public enum LogGCEvent {
		Begin = 0,
		MarkBegin = 1,
		MarkEnd = 2,
		ReclaimBegin = 3,
		ReclaimEnd = 4,
		End = 5,
		PreStopWorld = 6,
		PostStopWorld = 7,
		PreStartWorld = 8,
		PostStartWorld = 9,
		PreStopWorldLocked = 10,
		PostStartWorldUnlocked = 11,
	}

	// mono/sgen/gc-internal-agnostic.h : GCHandleType
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
}
